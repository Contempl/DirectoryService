"use client";

import { useState } from "react";
import { Upload } from "lucide-react";
import { VideoUploadDialog } from "@/features/video-upload/ui/video-upload-dialog";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";

const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

export default function UploadPage() {
  const [contextId, setContextId] = useState("");
  const [uploadedAssetId, setUploadedAssetId] = useState<string | null>(null);
  const hasValidContextId = guidPattern.test(contextId);

  return (
    <section className="mx-auto max-w-xl space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">File Service upload check</h1>
        <p className="mt-2 text-sm text-muted-foreground">
          Minimal flow: initiate in File Service, upload directly to storage, then complete.
        </p>
      </div>

      <div className="space-y-2">
        <Label htmlFor="context-id">Location ID</Label>
        <Input
          id="context-id"
          value={contextId}
          onChange={(event) => {
            setContextId(event.target.value.trim());
            setUploadedAssetId(null);
          }}
          placeholder="00000000-0000-0000-0000-000000000000"
          aria-invalid={contextId.length > 0 && !hasValidContextId}
        />
        {contextId.length > 0 && !hasValidContextId && (
          <p className="text-sm text-destructive">Enter a valid location GUID.</p>
        )}
      </div>

      <VideoUploadDialog
        context="location"
        contextId={contextId}
        onSuccess={setUploadedAssetId}
        trigger={
          <Button disabled={!hasValidContextId}>
            <Upload className="mr-2 h-4 w-4" />
            Select video
          </Button>
        }
      />

      {uploadedAssetId && (
        <div className="rounded-md border p-4 text-sm">
          <p className="font-medium">Upload accepted</p>
          <p className="mt-1 break-all text-muted-foreground">Asset ID: {uploadedAssetId}</p>
        </div>
      )}
    </section>
  );
}
