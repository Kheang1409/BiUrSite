"use client";

import React, { memo } from "react";
import { Comment as CommentType } from "@/types";
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";
import { Avatar, Button } from "@/components/ui/atoms";

dayjs.extend(relativeTime);

interface CommentProps {
  comment: CommentType;
  onEdit?: (commentId: string, content: string) => void;
  onDelete?: (commentId: string) => void;
  isOwner?: boolean;
}

export const Comment = memo(function Comment({
  comment,
  onEdit,
  onDelete,
  isOwner = false,
}: CommentProps) {
  return (
    <div className="flex gap-3 pb-4 border-b border-white/5 last:border-b-0 transition-colors hover:bg-white/5 rounded-lg p-2 -mx-2">
      <Avatar initials={comment.username} size="sm" />

      <div className="flex-1 min-w-0">
        <div className="flex items-start justify-between gap-2">
          <div className="flex-1">
            <div className="flex items-center gap-2">
              <p className="text-sm font-semibold text-white">
                {comment.username}
              </p>
              <time className="text-xs text-muted">
                {dayjs(comment.createdDate).fromNow()}
              </time>
            </div>
            <p className="text-sm text-white/80 mt-1 whitespace-pre-wrap">
              {comment.text}
            </p>
          </div>

          {isOwner && (
            <div className="flex gap-1 flex-shrink-0">
              <button
                onClick={() => onEdit?.(comment.id, comment.text)}
                className="p-1.5 hover:bg-primary-1/20 rounded transition-colors text-muted hover:text-primary-1"
                aria-label="Edit comment"
                title="Edit"
              >
                ✎
              </button>
              <button
                onClick={() => onDelete?.(comment.id)}
                className="p-1.5 hover:bg-danger-1/20 rounded transition-colors text-muted hover:text-danger-1"
                aria-label="Delete comment"
                title="Delete"
              >
                ✕
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
});
