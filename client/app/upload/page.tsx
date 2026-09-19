"use client";

import { Suspense, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { VideoUploadFlow } from "@/features/video-upload/ui/video-upload-flow";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";

const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

function UploadPageContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const contextIdFromUrl = searchParams.get("contextId") ?? "";
  const videoAssetIdFromUrl = searchParams.get("videoAssetId");
  const [contextId, setContextId] = useState(
    guidPattern.test(contextIdFromUrl) ? contextIdFromUrl : ""
  );
  const hasValidContextId = guidPattern.test(contextId);
  const persistedVideoAssetId = videoAssetIdFromUrl && guidPattern.test(videoAssetIdFromUrl)
    ? videoAssetIdFromUrl
    : null;

  const updateVideoAssetInUrl = (videoAssetId: string | null) => {
    const nextSearchParams = new URLSearchParams(searchParams.toString());

    if (hasValidContextId) nextSearchParams.set("contextId", contextId);
    if (videoAssetId) nextSearchParams.set("videoAssetId", videoAssetId);
    else nextSearchParams.delete("videoAssetId");

    const query = nextSearchParams.toString();
    router.replace(query ? `/upload?${query}` : "/upload", { scroll: false });
  };

  return (
    <section className="mx-auto max-w-xl space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">File Service upload check</h1>
        <p className="mt-2 text-sm text-muted-foreground">
          Upload a video, then follow its background processing progress in real time.
        </p>
      </div>

      <div className="space-y-2">
        <Label htmlFor="context-id">Location ID</Label>
        <Input
          id="context-id"
          value={contextId}
          onChange={(event) => {
            setContextId(event.target.value.trim());
          }}
          placeholder="00000000-0000-0000-0000-000000000000"
          aria-invalid={contextId.length > 0 && !hasValidContextId}
        />
        {contextId.length > 0 && !hasValidContextId && (
          <p className="text-sm text-destructive">Enter a valid location GUID.</p>
        )}
      </div>

      <VideoUploadFlow
        key={persistedVideoAssetId ?? contextId}
        disabled={!hasValidContextId}
        context="location"
        contextId={contextId}
        initialVideoAssetId={persistedVideoAssetId}
        onVideoAssetChange={updateVideoAssetInUrl}
      />
    </section>
  );
}

export default function UploadPage() {
  return (
    <Suspense fallback={<p className="text-sm text-muted-foreground">Loading upload flow…</p>}>
      <UploadPageContent />
    </Suspense>
  );
}
