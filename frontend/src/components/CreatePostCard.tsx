"use client";

import { useEffect, useRef, useState } from "react";
import { useMutation, useQuery } from "@apollo/client";
import {
  CREATE_POST_MUTATION,
  ME_QUERY,
  USER_QUERY,
} from "@/lib/graphql/queries";
import { useAuth } from "@/hooks/useAuth";
import { Card, Button, Textarea } from "@/components/ui/atoms";
import { Avatar } from "@/components/ui/atoms";
import { FileUtils } from "@/utils/helpers";
import { Post as PostType } from "@/types";

interface CreatePostCardProps {
  onPostCreated?: (created?: PostType, localPreviewUrl?: string | null) => void;
}

export function CreatePostCard({ onPostCreated }: CreatePostCardProps) {
  const { isAuthenticated, userId, username } = useAuth();
  const [content, setContent] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const [imageBytes, setImageBytes] = useState<number[] | null>(null);
  const [imagePreviewUrl, setImagePreviewUrl] = useState<string | null>(null);
  const [imageMime, setImageMime] = useState<string | null>(null);

  useEffect(() => {
    return () => {
      if (imagePreviewUrl) URL.revokeObjectURL(imagePreviewUrl);
    };
  }, [imagePreviewUrl]);

  const { data: meData } = useQuery(ME_QUERY, {
    skip: !isAuthenticated,
    fetchPolicy: "cache-and-network",
  });

  const { data: userData } = useQuery(USER_QUERY, {
    variables: { id: userId },
    skip: !userId,
    fetchPolicy: "cache-and-network",
  });

  const me = meData?.me;
  const meFallback = userData?.user;
  const avatarSrc = me?.profile || meFallback?.profile || undefined;
  const avatarInitials =
    me?.username || meFallback?.username || username || "ME";

  const [createPost] = useMutation(CREATE_POST_MUTATION);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!content.trim() && !imageBytes) {
      setError("Post must contain text or image");
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      const res: any = await createPost({
        variables: {
          text: content,
          data: imageBytes,
        },
      });

      const created = res?.data?.createPost as PostType | undefined;

      const localPreviewUrl = imageBytes
        ? URL.createObjectURL(
            new Blob([Uint8Array.from(imageBytes)], {
              type: imageMime || "image/jpeg",
            }),
          )
        : null;

      setContent("");
      setError(null);
      setImagePreviewUrl(null);
      setImageBytes(null);
      setImageMime(null);
      if (fileInputRef.current) fileInputRef.current.value = "";
      onPostCreated?.(
        localPreviewUrl
          ? ({ ...created, imageUrl: localPreviewUrl } as PostType)
          : created,
        localPreviewUrl,
      );
      setIsLoading(false);
    } catch (error) {
      console.error("Failed to create post:", error);
      setError("Failed to create post. Please try again.");
      setIsLoading(false);
    }
  };

  const handleFileSelected = async (file?: File) => {
    if (!file) return;
    try {
      const bytes = await FileUtils.fileToByteArray(file);
      if (imagePreviewUrl) URL.revokeObjectURL(imagePreviewUrl);
      setImagePreviewUrl(URL.createObjectURL(file));
      setImageBytes(bytes);
      setImageMime(file.type || "image/jpeg");
    } catch {
      setError("Failed to read image. Please try another file.");
    }
  };

  const handleRemoveImage = () => {
    if (imagePreviewUrl) URL.revokeObjectURL(imagePreviewUrl);
    setImagePreviewUrl(null);
    setImageBytes(null);
    setImageMime(null);
    if (fileInputRef.current) fileInputRef.current.value = "";
  };

  if (!isAuthenticated) {
    return null;
  }

  const charCount = content.length;
  const maxLength = 500;
  const isNearLimit = charCount > maxLength * 0.9;

  return (
    <Card className="mb-6 animate-fadeIn" variant="elevated">
      <div className="flex gap-4 mb-4">
        <Avatar initials={avatarInitials} src={avatarSrc} size="md" />
        <div className="flex-1">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
            What's on your mind?
          </h2>
          <p className="text-xs text-muted">
            Share your thoughts with the community
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        <Textarea
          value={content}
          onChange={(e) => setContent(e.target.value)}
          placeholder="Share your thoughts..."
          rows={1}
          maxLength={maxLength}
          showCharCount
          disabled={isLoading}
          error={error || undefined}
        />

        {imagePreviewUrl && (
          <div className="relative">
            <img
              src={imagePreviewUrl}
              alt="Selected post"
              className="w-full max-h-80 object-cover rounded-[10px] border border-gray-200 dark:border-white/10"
            />
            <button
              type="button"
              onClick={handleRemoveImage}
              disabled={isLoading}
              className="absolute top-2 right-2 w-9 h-9 rounded-full bg-black/60 hover:bg-black/70 text-white grid place-items-center border border-white/20"
              aria-label="Remove image"
              title="Remove image"
            >
              ✕
            </button>
          </div>
        )}

        <input
          ref={fileInputRef}
          type="file"
          accept="image/*"
          className="hidden"
          onChange={(e) => handleFileSelected(e.target.files?.[0])}
        />

        <div className="flex justify-between items-center">
          <div
            className={`text-xs ${
              isNearLimit ? "text-warning-1" : "text-muted"
            }`}
          >
            {charCount} / {maxLength} characters
          </div>
          <div className="flex gap-2">
            <Button
              type="button"
              variant="ghost"
              size="sm"
              onClick={() => fileInputRef.current?.click()}
              disabled={isLoading}
              title={imagePreviewUrl ? "Change photo" : "Add photo"}
            >
              <span className="inline-flex items-center gap-2">
                <span className="inline-grid place-items-center w-7 h-7 rounded-full bg-gray-100 dark:bg-white/5 border border-gray-200 dark:border-white/10">
                  📷
                </span>
                {imagePreviewUrl ? "Change" : "Photo"}
              </span>
            </Button>
            <Button
              variant="secondary"
              size="sm"
              onClick={() => {
                setContent("");
                handleRemoveImage();
              }}
              disabled={(!content.trim() && !imagePreviewUrl) || isLoading}
            >
              Clear
            </Button>
            <Button
              type="submit"
              size="sm"
              isLoading={isLoading}
              disabled={(!content.trim() && !imagePreviewUrl) || isLoading}
            >
              {isLoading ? "Posting..." : "Post"}
            </Button>
          </div>
        </div>

        {error && (
          <div className="bg-danger-1/20 border border-danger-1/50 rounded-lg p-3 text-sm text-danger-1">
            {error}
          </div>
        )}
      </form>
    </Card>
  );
}
