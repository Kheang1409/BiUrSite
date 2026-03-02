"use client";

import React, { memo } from "react";
import { Button, Textarea } from "@/components/ui/atoms";

interface PostEditFormProps {
  content: string;
  onContentChange: (value: string) => void;
  removeImage: boolean;
  imageUrl: string | null;
  isSaving: boolean;
  onCancel: () => void;
  onSave: () => void;
  onRemoveImage: () => void;
  onUndoRemove: () => void;
  onAddPhotoClick: () => void;
  hasExistingImage: boolean;
}

export const PostEditForm = memo(function PostEditForm({
  content,
  onContentChange,
  removeImage,
  imageUrl,
  isSaving,
  onCancel,
  onSave,
  onRemoveImage,
  onUndoRemove,
  onAddPhotoClick,
  hasExistingImage,
}: PostEditFormProps) {
  return (
    <div className="py-4 space-y-3">
      <Textarea
        value={content}
        onChange={(e) => onContentChange(e.target.value)}
        placeholder="Edit your post..."
        rows={3}
        maxLength={500}
        showCharCount
      />

      <div className="space-y-2">
        {imageUrl && !removeImage && (
          <div className="relative rounded-lg overflow-hidden border border-gray-200 dark:border-white/10">
            <img
              src={imageUrl}
              alt="Post image"
              className="w-full max-h-64 object-cover"
            />
            <button
              type="button"
              onClick={onRemoveImage}
              className="absolute top-2 right-2 w-8 h-8 rounded-full grid place-items-center bg-black/60 hover:bg-black/70 text-white border border-white/15 backdrop-blur-sm focus:outline-none focus-visible:ring-2 focus-visible:ring-white/20"
              aria-label="Remove photo"
              title="Remove photo"
            >
              <svg
                width="16"
                height="16"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
                aria-hidden="true"
              >
                <path d="M18 6L6 18" />
                <path d="M6 6l12 12" />
              </svg>
            </button>
          </div>
        )}

        {removeImage && (
          <div className="flex items-center justify-between rounded-lg border border-gray-200 dark:border-white/10 bg-gray-100 dark:bg-white/5 px-3 py-2">
            <p className="text-xs text-gray-600 dark:text-white/80">
              Image will be removed when you save.
            </p>
            <button
              type="button"
              className="text-xs text-primary-1 hover:underline"
              onClick={onUndoRemove}
            >
              Undo
            </button>
          </div>
        )}

        <div className="flex items-center justify-between gap-3">
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-lg px-3 py-2 text-sm text-gray-700 dark:text-white bg-gray-100 dark:bg-white/5 hover:bg-gray-200 dark:hover:bg-white/10 border border-gray-200 dark:border-white/10 transition-colors"
            onClick={onAddPhotoClick}
          >
            <svg
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
              aria-hidden="true"
            >
              <path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V7a2 2 0 0 1 2-2h3l2-3h8l2 3h3a2 2 0 0 1 2 2z" />
              <circle cx="12" cy="13" r="4" />
            </svg>
            <span>
              {imageUrl || hasExistingImage ? "Change photo" : "Add photo"}
            </span>
          </button>

          {imageUrl && !removeImage && (
            <span className="text-xs text-muted">
              Selecting a new photo will replace the current one.
            </span>
          )}
        </div>
      </div>

      <div className="flex gap-2 justify-end">
        <Button variant="secondary" size="sm" onClick={onCancel}>
          Cancel
        </Button>
        <Button size="sm" isLoading={isSaving} onClick={onSave}>
          Save Changes
        </Button>
      </div>
    </div>
  );
});
