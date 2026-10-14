import NextAuth, { customFetch } from "next-auth";
import { refreshAccessToken } from "@/features/auth/server/refresh-access-token";

type DirectoryProfile = {
  sub: string;
  name?: string;
  email?: string;
  role?: string | string[];
};

const issuer = process.env.OIDC_ISSUER ?? "http://localhost:5200";
const internalUrl = process.env.OIDC_INTERNAL_URL ?? issuer;
const issuerOrigin = new URL(issuer).origin;

// Discovery advertises public URLs. Only the server-side HTTP transport uses Docker DNS.
const fetchFromAuthService: typeof fetch = (input, init) => {
  const requested = new URL(
    typeof input === "string" || input instanceof URL ? input : input.url,
  );

  if (requested.origin !== issuerOrigin) return fetch(input, init);

  const internal = new URL(requested.pathname + requested.search, internalUrl);
  return input instanceof Request
    ? fetch(new Request(internal, input), init)
    : fetch(internal, init);
};

export const { handlers, auth, signIn, signOut } = NextAuth({
  basePath: "/api/client-auth",
  trustHost: true,
  session: { strategy: "jwt" },
  providers: [
    {
      id: "directory",
      name: "Directory Service",
      type: "oidc",
      issuer,
      clientId: process.env.OIDC_WEB_CLIENT_ID ?? "directory-web",
      clientSecret: process.env.OIDC_WEB_CLIENT_SECRET,
      authorization: {
        url: new URL("/connect/authorize", issuer).href,
        params: {
          scope: "openid profile email roles offline_access directory_api",
        },
      },
      checks: ["pkce", "state", "nonce"],
      [customFetch]: fetchFromAuthService,
      profile(profile: DirectoryProfile) {
        return {
          id: profile.sub,
          name: profile.name,
          email: profile.email,
        };
      },
    },
  ],
  callbacks: {
    async jwt({ token, account, profile }) {
      if (account) {
        token.accessToken = account.access_token;
        token.refreshToken = account.refresh_token;
        token.expiresAt = account.expires_at;
        const roles = (profile as DirectoryProfile | undefined)?.role;
        token.roles = Array.isArray(roles) ? roles : roles ? [roles] : [];
        return token;
      }

      if (!token.expiresAt || Date.now() < (token.expiresAt - 60) * 1000) {
        return token;
      }

      if (!token.refreshToken) return null;

      const refreshed = await refreshAccessToken(token.refreshToken);
      if (refreshed.kind === "invalid_grant") return null;
      if (refreshed.kind === "temporary_failure") {
        token.refreshError = "temporary_failure";
        return token;
      }

      token.accessToken = refreshed.accessToken;
      token.refreshToken = refreshed.refreshToken;
      token.expiresAt = refreshed.expiresAt;
      delete token.refreshError;
      return token;
    },
    async session({ session, token }) {
      session.accessToken = token.accessToken;
      session.roles = token.roles ?? [];
      session.refreshError = token.refreshError;
      return session;
    },
  },
});

declare module "next-auth" {
  interface Session {
    accessToken?: string;
    roles: string[];
    refreshError?: "temporary_failure";
  }
}

declare module "@auth/core/jwt" {
  interface JWT {
    accessToken?: string;
    refreshToken?: string;
    expiresAt?: number;
    roles?: string[];
    refreshError?: "temporary_failure";
  }
}
