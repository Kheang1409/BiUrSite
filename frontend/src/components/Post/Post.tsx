"use client";

import { Post as PostType } from "@/types";
import { useEffect, useMemo, useRef, useState, useCallback } from "react";
import { useRouter } from "next/navigation";
import { formatFacebookDate } from "@/lib/formatDate";
import { Card, LoadingSkeleton } from "@/components/ui/atoms";
import { UserHeader } from "@/components/ui/molecules";
import { useAuth } from "@/hooks/useAuth";
import { useDropdownMenu } from "@/hooks/useDropdownMenu";
import { useImageUpload } from "@/hooks/useImageUpload";
import { PostMenu } from "./PostMenu";
import { PostEditForm } from "./PostEditForm";
import { PostContent, PostImage, PostActions } from "./PostContent";

// ============================================================================
// Types
// ============================================================================

interface PostProps {
  post: PostType;
  onEdit?: (
    postId: string,
    content: string,
    options?: {
      data?: number[] | null;
      removeImage?: boolean;
      previewUrl?: string | null;
    },
  ) => void | Promise<void>;
  onDelete?: (postId: string) => void | Promise<void>;
  onComment?: (postId: string) => void;
  isOwner?: boolean;
  isLoading?: boolean;
}

export function Post({
  post,
  onEdit,
  onDelete,
  onComment,
  isOwner: isOwnerProp,
  isLoading = false,
}: PostProps) {
  const router = useRouter();
  const { isAuthenticated, userId } = useAuth();

  // State
  const [isEditing, setIsEditing] = useState(false);
  const [editContent, setEditContent] = useState(post.text);
  const [isSaving, setIsSaving] = useState(false);
  const [isExpanded, setIsExpanded] = useState(false);

  // Optimistic UI state
  const [optimisticText, setOptimisticText] = useState<string | null>(null);
  const [optimisticImageUrl, setOptimisticImageUrl] = useState<string | null>(
    null,
  );

  // Hooks
  const menu = useDropdownMenu();
  const imageUpload = useImageUpload();
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  // Computed values
  const isOwner = useMemo(() => {
    if (typeof isOwnerProp === "boolean") return isOwnerProp;
    if (!isAuthenticated) return false;
    if (!userId) return false;

    const normalize = (value: string) => value.trim().toLowerCase();
    return normalize(userId) === normalize(post.userId);
  }, [isOwnerProp, isAuthenticated, userId, post.userId]);

  const { display: timeDisplay, fullDate: timeFullDate } = formatFacebookDate(
    post.createdDate,
  );

  const effectiveText = optimisticText ?? post.text;
  const effectiveImageUrl = optimisticImageUrl ?? post.imageUrl ?? null;
  const textLimit = effectiveImageUrl ? 150 : 300;

  // ============================================================================
  // Effects
  // ============================================================================

  // Sync optimistic state with server data
  useEffect(() => {
    if (optimisticText !== null && optimisticText === post.text) {
      setOptimisticText(null);
    }
    if (optimisticImageUrl !== null && !post.imageUrl) {
      setOptimisticImageUrl(null);
    }
  }, [post.text, post.imageUrl, optimisticText, optimisticImageUrl]);

  // ============================================================================
  // Handlers
  // ============================================================================

  const handleDelete = useCallback(async () => {
    if (!onDelete) return;
    setIsSaving(true);
    try {
      await onDelete(post.id);
    } finally {
      setIsSaving(false);
    }
  }, [onDelete, post.id]);

  const handleCommentClick = useCallback(() => {
    if (!isAuthenticated) {
      localStorage.setItem("pendingCommentPostId", post.id);
      router.push("/login");
      return;
    }
    onComment?.(post.id);
  }, [isAuthenticated, onComment, post.id, router]);

  const handleStartEdit = useCallback(() => {
    setIsEditing(true);
    setEditContent(post.text);
    imageUpload.reset();
  }, [post.text, imageUpload]);

  const handleCancelEdit = useCallback(() => {
    setIsEditing(false);
    setEditContent(post.text);
    imageUpload.reset();
  }, [post.text, imageUpload]);

  const handleSaveEdit = useCallback(async () => {
    const hasText = !!editContent.trim();
    const hasTextChange = editContent !== post.text;
    const hasImageChange =
      imageUpload.shouldRemove || imageUpload.bytes !== null;

    if (!hasText) return;
    if (!hasTextChange && !hasImageChange) {
      setIsEditing(false);
      return;
    }
    if (!onEdit) return;

    // Optimistic UI
    const previousText = optimisticText ?? post.text;
    const previousImageUrl = optimisticImageUrl ?? post.imageUrl ?? null;

    setOptimisticText(editContent);
    if (imageUpload.shouldRemove) {
      setOptimisticImageUrl(null);
    } else if (imageUpload.previewUrl) {
      setOptimisticImageUrl(imageUpload.previewUrl);
    }

    setIsEditing(false);
    setIsSaving(true);

    try {
      await onEdit(post.id, editContent, {
        data: imageUpload.bytes,
        removeImage: imageUpload.shouldRemove,
        previewUrl: imageUpload.shouldRemove ? null : imageUpload.previewUrl,
      });
    } catch (e) {
      setOptimisticText(previousText);
      setOptimisticImageUrl(previousImageUrl);
      throw e;
    } finally {
      setIsSaving(false);
    }
  }, [
    editContent,
    post.text,
    post.id,
    post.imageUrl,
    onEdit,
    imageUpload,
    optimisticText,
    optimisticImageUrl,
  ]);

  const handleFileChange = useCallback(
    async (e: React.ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0];
      if (file) {
        await imageUpload.handleFileSelect(file);
      }
    },
    [imageUpload],
  );

  // ============================================================================
  // Render
  // ============================================================================

  if (isLoading) {
    return (
      <Card className="mb-6">
        <div className="space-y-4">
          <LoadingSkeleton variant="avatar" />
          <LoadingSkeleton lines={2} />
          <LoadingSkeleton variant="image" />
        </div>
      </Card>
    );
  }

  const currentImageUrl =
    imageUpload.previewUrl ??
    (imageUpload.shouldRemove ? null : (post.imageUrl ?? null));

  return (
    <article className="mb-4 animate-fadeIn">
      <Card
        hoverable
        className="bg-white/5 backdrop-blur-sm border border-white/10"
      >
        <div className="pb-2 border-b border-white/10">
          <UserHeader
            username={post.username}
            userId={post.userId}
            timestamp={timeDisplay}
            timestampTitle={timeFullDate}
            avatarInitials={post.username}
            avatarSrc={post.userProfile}
            action={
              isOwner && (
                <PostMenu
                  menu={menu}
                  isSaving={isSaving}
                  onEditClick={handleStartEdit}
                  onDeleteClick={handleDelete}
                />
              )
            }
          />
        </div>

        {isEditing ? (
          <>
            <input
              ref={fileInputRef}
              type="file"
              accept="image/*"
              className="hidden"
              onChange={handleFileChange}
            />
            <PostEditForm
              content={editContent}
              onContentChange={setEditContent}
              removeImage={imageUpload.shouldRemove}
              imageUrl={currentImageUrl}
              isSaving={isSaving}
              onCancel={handleCancelEdit}
              onSave={handleSaveEdit}
              onRemoveImage={imageUpload.removeImage}
              onUndoRemove={imageUpload.undoRemove}
              onAddPhotoClick={() => fileInputRef.current?.click()}
              hasExistingImage={!!post.imageUrl}
            />
          </>
        ) : (
          <>
            <PostContent
              text={effectiveText}
              isExpanded={isExpanded}
              onExpand={() => setIsExpanded(true)}
              textLimit={textLimit}
            />

            {effectiveImageUrl && (
              <PostImage
                imageUrl={effectiveImageUrl}
                onClick={handleCommentClick}
              />
            )}

            <PostActions
              commentCount={post.commentCount}
              onCommentClick={handleCommentClick}
            />
          </>
        )}
      </Card>
    </article>
  );
}
