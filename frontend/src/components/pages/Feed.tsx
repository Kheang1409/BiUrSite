"use client";

import { gql, useMutation, useQuery } from "@apollo/client";
import {
  DELETE_POST_MUTATION,
  EDIT_POST_MUTATION,
  POSTS_QUERY,
} from "@/lib/graphql/queries";
import { Post as PostType } from "@/types";
import { Post } from "@/components/Post";
import { CreatePostCard } from "@/components/CreatePostCard";
import { useAuth } from "@/hooks/useAuth";
import { useEffect, useMemo, useRef, useState } from "react";
import { PostDetailModal } from "@/components/modals/PostDetailModal";
import { ConfirmDialog } from "@/components/modals/ConfirmDialog";
import {
  EmptyState,
  LoadingSkeleton,
  Card,
  Button,
} from "@/components/ui/atoms";
import { ApiErrorHandler } from "@/utils/helpers";

export function Feed() {
  const { isAuthenticated } = useAuth();
  const PAGE_SIZE = 10;
  const [pageNumber, setPageNumber] = useState(1);
  const [searchKeyword, setSearchKeyword] = useState("");
  const [activePostId, setActivePostId] = useState<string | null>(null);
  const [deletePostId, setDeletePostId] = useState<string | null>(null);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  const [hasMore, setHasMore] = useState(true);
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const loadMoreRef = useRef<HTMLDivElement | null>(null);
  const prevServerCountRef = useRef(0);
  const loadingMoreRef = useRef(false);

  const { data, loading, error, refetch } = useQuery(POSTS_QUERY, {
    variables: {
      pageNumber,
      keywords: searchKeyword || null,
    },
    fetchPolicy: "cache-and-network",
    notifyOnNetworkStatusChange: true,
  });

  const posts: PostType[] = data?.posts || [];
  const [localNewPosts, setLocalNewPosts] = useState<PostType[]>([]);
  const localBlobUrlsRef = useRef<Set<string>>(new Set());
  const refetchTimersRef = useRef<number[]>([]);

  const displayedPosts = useMemo(() => {
    // Merge strategy:
    // - Always show locally-created posts at the top immediately.
    // - If server returns the same post but imageUrl is still null, keep local blob image.
    // - Once server has a real imageUrl, prefer server and allow cleanup.
    const serverById = new Map<string, PostType>();
    for (const p of posts) serverById.set(p.id, p);

    const mergedById = new Map<string, PostType>(serverById);

    for (const local of localNewPosts) {
      const server = serverById.get(local.id);
      if (!server) {
        mergedById.set(local.id, local);
        continue;
      }

      const localIsBlob =
        !!local.imageUrl && local.imageUrl.startsWith("blob:");
      const serverHasRealImage =
        !!server.imageUrl && !server.imageUrl.startsWith("blob:");

      if (localIsBlob && !serverHasRealImage) {
        mergedById.set(local.id, { ...server, imageUrl: local.imageUrl });
      }
    }

    const result: PostType[] = [];
    const seen = new Set<string>();

    for (const local of localNewPosts) {
      const p = mergedById.get(local.id);
      if (p && !seen.has(p.id)) {
        result.push(p);
        seen.add(p.id);
      }
    }

    for (const server of posts) {
      const p = mergedById.get(server.id);
      if (p && !seen.has(p.id)) {
        result.push(p);
        seen.add(p.id);
      }
    }

    return result;
  }, [posts, localNewPosts]);

  useEffect(() => {
    // Cleanup local blob previews once the server has a real imageUrl
    if (!localNewPosts.length) return;

    setLocalNewPosts((prev) => {
      const serverById = new Map<string, PostType>();
      for (const p of posts) serverById.set(p.id, p);

      return prev.filter((local) => {
        const server = serverById.get(local.id);
        if (!server) return true;
        const localIsBlob =
          !!local.imageUrl && local.imageUrl.startsWith("blob:");
        const serverHasRealImage =
          !!server.imageUrl && !server.imageUrl.startsWith("blob:");

        if (localIsBlob && serverHasRealImage) {
          try {
            URL.revokeObjectURL(local.imageUrl!);
          } catch {
            // ignore
          }
          localBlobUrlsRef.current.delete(local.imageUrl!);
          return false;
        }
        return true;
      });
    });
  }, [posts, localNewPosts.length]);

  useEffect(() => {
    return () => {
      // Clean up any outstanding blob URLs if user navigates away
      for (const url of localBlobUrlsRef.current) {
        try {
          URL.revokeObjectURL(url);
        } catch {
          // ignore
        }
      }
      localBlobUrlsRef.current.clear();

      // Clear timers
      for (const id of refetchTimersRef.current) window.clearTimeout(id);
      refetchTimersRef.current = [];
    };
  }, []);
  useEffect(() => {
    // After login, scroll to pending comment post if available
    const pendingPostId = localStorage.getItem("pendingCommentPostId");
    if (pendingPostId && isAuthenticated && displayedPosts.length > 0) {
      // Find the post to ensure it exists
      const post = displayedPosts.find((p) => p.id === pendingPostId);
      if (post) {
        // Clear the stored ID
        localStorage.removeItem("pendingCommentPostId");

        // Use requestAnimationFrame to ensure DOM is ready
        requestAnimationFrame(() => {
          setTimeout(() => {
            const postElement = document.getElementById(
              `post-${pendingPostId}`,
            );
            if (postElement) {
              postElement.scrollIntoView({
                behavior: "smooth",
                block: "center",
              });
              // Automatically open the comment modal for this post
              setActivePostId(pendingPostId);
            }
          }, 200);
        });
      }
    }
  }, [isAuthenticated, displayedPosts]);

  useEffect(() => {
    // Reset pagination when search changes
    setPageNumber(1);
    setHasMore(true);
    setIsLoadingMore(false);
    prevServerCountRef.current = 0;
  }, [searchKeyword]);

  useEffect(() => {
    // Initial page: infer whether there might be more
    if (pageNumber === 1 && !loading) {
      setHasMore(posts.length >= PAGE_SIZE);
    }
  }, [loading, pageNumber, posts.length]);

  useEffect(() => {
    // If we requested more and the query finished, decide if more pages exist.
    if (!isLoadingMore) return;
    if (loading) return;

    const delta = posts.length - prevServerCountRef.current;
    setHasMore(delta >= PAGE_SIZE);
    setIsLoadingMore(false);
    loadingMoreRef.current = false;
  }, [isLoadingMore, loading, posts.length]);

  const [editPost] = useMutation(EDIT_POST_MUTATION);
  const [deletePost] = useMutation(DELETE_POST_MUTATION);

  const handleSearch = (value: string) => {
    setSearchKeyword(value);
  };

  const triggerLoadMore = () => {
    if (loading) return;
    if (isLoadingMore || loadingMoreRef.current) return;
    if (!hasMore) return;

    prevServerCountRef.current = posts.length;
    loadingMoreRef.current = true;
    setIsLoadingMore(true);
    setPageNumber((p) => p + 1);
  };

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
  }, [hasMore, isLoadingMore, loading, posts.length]);

  // Loading skeleton
  const renderLoadingState = () => (
    <div className="max-w-lg mx-auto space-y-4">
      {Array.from({ length: 3 }).map((_, i) => (
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

  // Error state
  if (error) {
    return (
      <div className="max-w-2xl mx-auto">
        <Card className="text-center bg-danger-1/10 border border-danger-1/30">
          <div className="py-8">
            <p className="text-danger-1 font-semibold mb-4">
              Failed to load posts
            </p>
            <p className="text-muted text-sm mb-6">
              Something went wrong while fetching posts. Please try again.
            </p>
            <Button size="sm" onClick={() => refetch()}>
              Try Again
            </Button>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <div className="max-w-lg mx-auto space-y-4">
      <ConfirmDialog
        isOpen={isDeleteOpen}
        title="Delete post?"
        description="You can’t undo this action. This post will be deleted permanently."
        cancelText="Cancel"
        confirmText="Delete"
        confirmVariant="danger"
        isConfirmLoading={isDeleting}
        onCancel={() => {
          if (isDeleting) return;
          setIsDeleteOpen(false);
          setDeletePostId(null);
        }}
        onConfirm={async () => {
          if (!deletePostId) return;
          const id = deletePostId;
          setIsDeleting(true);
          try {
            await deletePost({
              variables: { id },
              optimisticResponse: { deletePost: true },
              update: (cache) => {
                // Remove from any cached posts lists (all argument variants)
                cache.modify({
                  id: "ROOT_QUERY",
                  fields: {
                    posts(existingRefs: readonly any[] = [], { readField }) {
                      return existingRefs.filter(
                        (ref) => readField("id", ref) !== id,
                      );
                    },
                    myPosts(existingRefs: readonly any[] = [], { readField }) {
                      return existingRefs.filter(
                        (ref) => readField("id", ref) !== id,
                      );
                    },
                  },
                });

                // Best-effort evict the entity itself
                const postDtoId = cache.identify({
                  __typename: "PostDto",
                  id,
                });
                if (postDtoId) cache.evict({ id: postDtoId });

                const postDetailDtoId = cache.identify({
                  __typename: "PostDetailDto",
                  id,
                });
                if (postDetailDtoId) cache.evict({ id: postDetailDtoId });
                cache.gc();
              },
            });

            // If the deleted post was locally inserted (e.g., had a blob preview),
            // remove it from local state too so it can't re-appear.
            const local = localNewPosts.find((p) => p.id === id);
            if (local?.imageUrl && local.imageUrl.startsWith("blob:")) {
              try {
                URL.revokeObjectURL(local.imageUrl);
              } catch {
                // ignore
              }
              localBlobUrlsRef.current.delete(local.imageUrl);
            }
            setLocalNewPosts((prev) => prev.filter((p) => p.id !== id));

            if (activePostId === id) setActivePostId(null);
            setIsDeleteOpen(false);
            setDeletePostId(null);
            // Keep as a safety net (pagination/search can change server list)
            await refetch();
          } catch (e: any) {
            alert(ApiErrorHandler.handleGraphQLError(e));
          } finally {
            setIsDeleting(false);
          }
        }}
      />
      <PostDetailModal
        postId={activePostId ?? ""}
        isOpen={!!activePostId}
        onClose={() => setActivePostId(null)}
        onDeleted={(id) => {
          const local = localNewPosts.find((p) => p.id === id);
          if (local?.imageUrl && local.imageUrl.startsWith("blob:")) {
            try {
              URL.revokeObjectURL(local.imageUrl);
            } catch {
              // ignore
            }
            localBlobUrlsRef.current.delete(local.imageUrl);
          }
          setLocalNewPosts((prev) => prev.filter((p) => p.id !== id));
        }}
      />
      {isAuthenticated && (
        <CreatePostCard
          onPostCreated={(created, localPreviewUrl) => {
            if (created) {
              setLocalNewPosts((prev) => {
                const next = [created, ...prev];
                // Deduplicate by id
                const seen = new Set<string>();
                return next.filter((p) => {
                  if (seen.has(p.id)) return false;
                  seen.add(p.id);
                  return true;
                });
              });
            }

            if (localPreviewUrl) {
              localBlobUrlsRef.current.add(localPreviewUrl);
            }

            // Fetch server version in background. If image upload is async,
            // do a couple delayed refetches so the real imageUrl arrives.
            refetch();
            if (localPreviewUrl) {
              refetchTimersRef.current.push(
                window.setTimeout(() => refetch(), 2000),
              );
              refetchTimersRef.current.push(
                window.setTimeout(() => refetch(), 5000),
              );
            }
          }}
        />
      )}

      {loading && !displayedPosts.length ? (
        renderLoadingState()
      ) : displayedPosts.length > 0 ? (
        // Posts List
        <div className="space-y-4">
          {displayedPosts.map((post) => (
            <div key={post.id} id={`post-${post.id}`}>
              <Post
                post={post}
                onComment={(postId) => setActivePostId(postId)}
                onDelete={async (postId) => {
                  setDeletePostId(postId);
                  setIsDeleteOpen(true);
                }}
                onEdit={async (postId, content, options) => {
                  try {
                    await editPost({
                      variables: {
                        id: postId,
                        text: content,
                        data: options?.data ?? null,
                        removeImage: options?.removeImage ?? false,
                      },
                      optimisticResponse: { editPost: true },
                      update: (cache) => {
                        // Update cached feed list immediately (text + remove-image)
                        cache.modify({
                          id: "ROOT_QUERY",
                          fields: {
                            posts(
                              existingRefs: readonly any[] = [],
                              { readField },
                            ) {
                              return existingRefs.map((ref) => {
                                const id = readField("id", ref) as
                                  | string
                                  | undefined;
                                if (id !== postId) return ref;
                                const cacheId = cache.identify(ref);
                                if (!cacheId) return ref;
                                cache.writeFragment({
                                  id: cacheId,
                                  fragment: gql`
                                    fragment _PostOptimistic on PostDto {
                                      text
                                      imageUrl
                                    }
                                  `,
                                  data: {
                                    __typename:
                                      readField("__typename", ref) ?? "PostDto",
                                    text: content,
                                    imageUrl:
                                      options?.removeImage === true
                                        ? null
                                        : options?.previewUrl
                                          ? options.previewUrl
                                          : readField("imageUrl", ref),
                                  },
                                });
                                return ref;
                              });
                            },
                          },
                        });
                      },
                    });
                  } catch (e: any) {
                    alert(ApiErrorHandler.handleGraphQLError(e));
                  }
                }}
              />
            </div>
          ))}

          <div ref={loadMoreRef} className="h-1" />

          {!hasMore && (
            <div className="text-center py-8 text-sm text-muted">
              You’re all caught up.
            </div>
          )}
        </div>
      ) : (
        // Empty State
        <EmptyState
          icon="📝"
          title="No posts found"
          description={
            searchKeyword
              ? "Try adjusting your search terms or clearing the filter"
              : "Be the first to share your thoughts with the community!"
          }
          action={
            searchKeyword
              ? {
                  label: "Clear Search",
                  onClick: () => handleSearch(""),
                }
              : isAuthenticated
                ? undefined
                : {
                    label: "Get Started",
                    onClick: () => {},
                  }
          }
        />
      )}

      {(isLoadingMore || (loading && posts.length > 0)) && (
        <div className="text-center py-4">
          <div className="inline-block">
            <div className="animate-spin w-5 h-5 border-2 border-primary-1 border-t-transparent rounded-full"></div>
          </div>
        </div>
      )}
    </div>
  );
}
