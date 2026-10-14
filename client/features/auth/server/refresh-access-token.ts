import "server-only";

import { createHash } from "node:crypto";

export type RefreshResult =
  | { kind: "ok"; accessToken: string; refreshToken: string; expiresAt: number }
  | { kind: "invalid_grant" }
  | { kind: "temporary_failure" };

const reuseWindowMs = 2 * 60 * 1000;
const inFlight = new Map<string, { result: Promise<RefreshResult>; expiresAt: number }>();

export function refreshAccessToken(refreshToken: string): Promise<RefreshResult> {
  const now = Date.now();
  for (const [key, entry] of inFlight) {
    if (entry.expiresAt <= now) inFlight.delete(key);
  }

  // An old encrypted session cookie can arrive from another tab while rotation is in progress.
  const key = createHash("sha256").update(refreshToken).digest("hex");
  const previous = inFlight.get(key);
  if (previous) return previous.result;

  const result = exchangeRefreshToken(refreshToken).then((value) => {
    if (value.kind === "temporary_failure") inFlight.delete(key);
    return value;
  });
  inFlight.set(key, { result, expiresAt: now + reuseWindowMs });
  return result;
}

async function exchangeRefreshToken(refreshToken: string): Promise<RefreshResult> {
  const internalUrl = process.env.OIDC_INTERNAL_URL ?? process.env.OIDC_ISSUER;
  const clientId = process.env.OIDC_WEB_CLIENT_ID;
  const clientSecret = process.env.OIDC_WEB_CLIENT_SECRET;
  if (!internalUrl || !clientId || !clientSecret) {
    throw new Error("OIDC frontend client configuration is incomplete.");
  }

  try {
    const response = await fetch(new URL("/connect/token", internalUrl), {
      method: "POST",
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      body: new URLSearchParams({
        grant_type: "refresh_token",
        client_id: clientId,
        client_secret: clientSecret,
        refresh_token: refreshToken,
      }),
      cache: "no-store",
      signal: AbortSignal.timeout(10_000),
    });

    const body = (await response.json()) as Record<string, unknown>;
    if (response.status === 400 && body.error === "invalid_grant") {
      return { kind: "invalid_grant" };
    }

    if (
      !response.ok ||
      typeof body.access_token !== "string" ||
      typeof body.expires_in !== "number" ||
      body.expires_in <= 0
    ) {
      return { kind: "temporary_failure" };
    }

    return {
      kind: "ok",
      accessToken: body.access_token,
      refreshToken: typeof body.refresh_token === "string"
        ? body.refresh_token
        : refreshToken,
      expiresAt: Math.floor(Date.now() / 1000) + body.expires_in,
    };
  } catch {
    return { kind: "temporary_failure" };
  }
}
