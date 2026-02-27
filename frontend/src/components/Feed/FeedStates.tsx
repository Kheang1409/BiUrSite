"use client";

import React, { memo } from "react";
import { Card, LoadingSkeleton, Button } from "@/components/ui/atoms";

interface FeedLoadingSkeletonProps {
  count?: number;
  className?: string;
}

export const FeedLoadingSkeleton = memo(function FeedLoadingSkeleton({
  count = 3,
  className = "",
}: FeedLoadingSkeletonProps) {
  return (
    <div className={`max-w-lg mx-auto space-y-4 ${className}`}>
      {Array.from({ length: count }).map((_, i) => (
        <Card key={i}>
          <div className="space-y-4">
            <LoadingSkeleton variant="avatar" />
            <LoadingSkeleton lines={2} />
            <LoadingSkeleton variant="image" />
          </div>
        </Card>
      ))}
    </div>
  );
});

interface FeedErrorStateProps {
  message?: string;
  onRetry?: () => void;
  description?: string;
}

export const FeedErrorState = memo(function FeedErrorState({
  message = "Failed to load posts",
  description = "Something went wrong while fetching posts. Please try again.",
  onRetry,
}: FeedErrorStateProps) {
  return (
    <div className="max-w-2xl mx-auto">
      <Card className="text-center bg-danger-1/10 border border-danger-1/30">
        <div className="py-8">
          <p className="text-danger-1 font-semibold mb-4">{message}</p>
          <p className="text-muted text-sm mb-6">{description}</p>
          {onRetry && (
            <Button size="sm" onClick={onRetry}>
              Try Again
            </Button>
          )}
        </div>
      </Card>
    </div>
  );
});

interface InfiniteScrollTriggerProps {
  triggerRef: React.RefObject<HTMLDivElement | null>;
  hasMore: boolean;
  isLoadingMore: boolean;
}

export const InfiniteScrollTrigger = memo(function InfiniteScrollTrigger({
  triggerRef,
  hasMore,
  isLoadingMore,
}: InfiniteScrollTriggerProps) {
  if (!hasMore && !isLoadingMore) return null;

  return (
    <>
      <div ref={triggerRef} className="h-1" />
      {isLoadingMore && (
        <div className="flex justify-center py-4">
          <div className="w-6 h-6 border-2 border-primary-1/30 border-t-primary-1 rounded-full animate-spin" />
        </div>
      )}
    </>
  );
});
