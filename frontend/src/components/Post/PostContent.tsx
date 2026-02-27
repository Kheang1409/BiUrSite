"use client";

import React, { memo } from "react";
import { Button } from "@/components/ui/atoms";

interface PostContentProps {
  text: string;
  isExpanded: boolean;
  onExpand: () => void;
  textLimit: number;
}

export const PostContent = memo(function PostContent({
  text,
  isExpanded,
  onExpand,
  textLimit,
}: PostContentProps) {
  const isTextLong = text.length > textLimit;
  const displayText =
    isExpanded || !isTextLong ? text : text.slice(0, textLimit).trim() + "...";

  return (
    <div className="py-3">
      {isTextLong && !isExpanded ? (
        <p className="text-white/90 leading-relaxed whitespace-pre-wrap text-sm">
          {displayText}{" "}
          <button
            onClick={onExpand}
            className="inline bg-none border-none p-0 m-0 text-primary-1 hover:text-primary-1 hover:underline font-semibold text-xs underline-offset-2 cursor-pointer"
            style={{ font: "inherit", appearance: "none" }}
          >
            See More
          </button>
        </p>
      ) : (
        <p className="text-white/90 leading-relaxed whitespace-pre-wrap text-sm">
          {displayText}
        </p>
      )}
    </div>
  );
});

interface PostImageProps {
  imageUrl: string;
  onClick: () => void;
}

export const PostImage = memo(function PostImage({
  imageUrl,
  onClick,
}: PostImageProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="w-full text-left"
      aria-label="View post details"
    >
      <img
        src={imageUrl}
        alt="Post content"
        className="w-full rounded-[10px] mb-3 max-h-96 object-cover hover:opacity-90 transition-opacity"
      />
    </button>
  );
});

interface PostActionsProps {
  commentCount?: number;
  onCommentClick: () => void;
}

export const PostActions = memo(function PostActions({
  commentCount = 0,
  onCommentClick,
}: PostActionsProps) {
  return (
    <>
      {commentCount > 0 && (
        <>
          <div className="border-t border-white/10 py-1.5 text-center">
            <button
              className="text-xs text-muted hover:text-primary-1 transition-colors"
              onClick={onCommentClick}
            >
              💬 {commentCount} {commentCount === 1 ? "comment" : "comments"}
            </button>
          </div>
          <div className="border-t border-white/10" />
        </>
      )}

      <div className="pt-0">
        <Button
          fullWidth
          variant="ghost"
          size="sm"
          onClick={onCommentClick}
          className="flex items-center justify-center gap-2 text-muted hover:text-primary-1 hover:bg-white/5 transition-colors text-sm py-1.5"
        >
          💬 Comment
        </Button>
      </div>
    </>
  );
});
