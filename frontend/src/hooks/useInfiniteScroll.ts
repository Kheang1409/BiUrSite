"use client";

import { useState, useRef, useEffect, useCallback } from "react";

export interface UsePaginationOptions {
  pageSize?: number;
  initialPage?: number;
}

export interface UsePaginationResult {
  pageNumber: number;
  hasMore: boolean;
  isLoadingMore: boolean;
  setHasMore: (value: boolean) => void;
  nextPage: () => void;
  resetPagination: () => void;
  handleLoadMore: (currentCount: number) => void;
  loadMoreRef: React.RefObject<HTMLDivElement | null>;
}

export function useInfiniteScroll({
  pageSize = 10,
  initialPage = 1,
}: UsePaginationOptions = {}): UsePaginationResult {
  const [pageNumber, setPageNumber] = useState(initialPage);
  const [hasMore, setHasMore] = useState(true);
  const [isLoadingMore, setIsLoadingMore] = useState(false);

  const loadMoreRef = useRef<HTMLDivElement | null>(null);
  const prevServerCountRef = useRef(0);
  const loadingMoreRef = useRef(false);

  const triggerLoadMore = useCallback(() => {
    if (loadingMoreRef.current) return;
    if (!hasMore) return;

    loadingMoreRef.current = true;
    setIsLoadingMore(true);
    setPageNumber((p) => p + 1);
  }, [hasMore]);

  const handleLoadMore = useCallback(
    (currentCount: number) => {
      if (!isLoadingMore) return;

      const delta = currentCount - prevServerCountRef.current;
      setHasMore(delta >= pageSize);
      setIsLoadingMore(false);
      loadingMoreRef.current = false;
    },
    [isLoadingMore, pageSize],
  );

  const resetPagination = useCallback(() => {
    setPageNumber(initialPage);
    setHasMore(true);
    setIsLoadingMore(false);
    prevServerCountRef.current = 0;
    loadingMoreRef.current = false;
  }, [initialPage]);

  const nextPage = useCallback(() => {
    prevServerCountRef.current = pageNumber;
    triggerLoadMore();
  }, [pageNumber, triggerLoadMore]);

  // Set up intersection observer for infinite scroll
  useEffect(() => {
    const el = loadMoreRef.current;
    if (!el) return;

    const observer = new IntersectionObserver(
      (entries) => {
        const first = entries[0];
        if (!first?.isIntersecting) return;
        triggerLoadMore();
      },
      {
        root: null,
        rootMargin: "600px",
        threshold: 0,
      },
    );

    observer.observe(el);
    return () => observer.disconnect();
  }, [triggerLoadMore]);

  return {
    pageNumber,
    hasMore,
    isLoadingMore,
    setHasMore,
    nextPage,
    resetPagination,
    handleLoadMore,
    loadMoreRef,
  };
}
