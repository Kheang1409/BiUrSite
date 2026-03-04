"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { createPortal } from "react-dom";
import Link from "next/link";
import { useMutation, useQuery } from "@apollo/client";
import {
  COMMENTS_QUERY,
  CREATE_COMMENT_MUTATION,
  DELETE_COMMENT_MUTATION,
  DELETE_POST_MUTATION,
  EDIT_COMMENT_MUTATION,
  EDIT_POST_MUTATION,
  ME_QUERY,
  POST_DETAIL_QUERY,
  USER_QUERY,
} from "@/lib/graphql/queries";
import {
  Comment as CommentType,
  PostDetail as PostDetailType,
  User,
} from "@/types";
import { Card, Button, Textarea, Avatar } from "@/components/ui/atoms";
import { ConfirmDialog } from "@/components/modals/ConfirmDialog";
import { formatAgeShort, formatFacebookDate } from "@/lib/formatDate";
import { useAuth } from "@/hooks/useAuth";

export function PostDetailModal({
  postId,
  isOpen,
  onClose,
  onDeleted,
}: {
  postId: string;
  isOpen: boolean;
  onClose: () => void;
  onDeleted?: (postId: string) => void;
}) {
  const { isAuthenticated, userId, username } = useAuth();
  const [commentText, setCommentText] = useState("");
  const [commentError, setCommentError] = useState<string | null>(null);
  const [postActionError, setPostActionError] = useState<string | null>(null);
  const [isPostExpanded, setIsPostExpanded] = useState(false);
  const [isEmojiOpen, setIsEmojiOpen] = useState(false);
  const [isPostMenuOpen, setIsPostMenuOpen] = useState(false);
  const [isEditingPost, setIsEditingPost] = useState(false);
  const [editPostText, setEditPostText] = useState("");
  const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false);
  const [editingCommentId, setEditingCommentId] = useState<string | null>(null);
  const [editCommentText, setEditCommentText] = useState("");
  const [commentMenuOpenId, setCommentMenuOpenId] = useState<string | null>(
    null,
  );
  const [deleteCommentId, setDeleteCommentId] = useState<string | null>(null);
  const [commentActionError, setCommentActionError] = useState<string | null>(
    null,
  );
  const textareaRef = useRef<HTMLTextAreaElement | null>(null);
  const emojiPopoverRef = useRef<HTMLDivElement | null>(null);
  const postMenuPopoverRef = useRef<HTMLDivElement | null>(null);
  const commentMenuPopoverRef = useRef<HTMLDivElement | null>(null);
  const commentsContainerRef = useRef<HTMLDivElement | null>(null);
  const [mounted, setMounted] = useState(false);

  // Handle SSR - only render portal after mount
  useEffect(() => {
    setMounted(true);
  }, []);

  // Comments pagination state
  const [comments, setComments] = useState<CommentType[]>([]);
  const [commentsPage, setCommentsPage] = useState(1);
  const [hasMoreComments, setHasMoreComments] = useState(true);
  const [isLoadingMoreComments, setIsLoadingMoreComments] = useState(false);
  const [totalCommentCount, setTotalCommentCount] = useState(0);
  const COMMENTS_PAGE_SIZE = 10;

  const {
    data,
    loading: postLoading,
    error: postError,
    refetch,
  } = useQuery(POST_DETAIL_QUERY, {
    variables: { id: postId },
    skip: !isOpen,
    fetchPolicy: "cache-and-network",
  });

  const post: PostDetailType | undefined = data?.post;

  // Separate query for paginated comments
  const {
    data: commentsData,
    loading: commentsLoading,
    refetch: refetchComments,
  } = useQuery(COMMENTS_QUERY, {
    variables: { postId, pageNumber: 1 },
    skip: !isOpen,
    fetchPolicy: "cache-and-network",
  });

  useEffect(() => {
    const fetchedComments = commentsData?.comments || [];
    if (fetchedComments) {
      setComments(fetchedComments);
      setHasMoreComments(fetchedComments.length >= COMMENTS_PAGE_SIZE);
      setCommentsPage(1);
    }
  }, [commentsData]);

  const { data: meData } = useQuery(ME_QUERY, {
    skip: !isOpen || !isAuthenticated,
    fetchPolicy: "cache-and-network",
  });

  const { data: meByIdData } = useQuery(USER_QUERY, {
    variables: { id: userId },
    skip: !isOpen || !userId,
    fetchPolicy: "cache-and-network",
  });

  const me: User | undefined = meData?.me;
  const meFallback: User | undefined = meByIdData?.user;
  const isPostOwner = (() => {
    if (!post?.userId) return false;
    const normalize = (value: string) => value.trim().toLowerCase();
    const postOwnerId = normalize(post.userId);

    if (userId && normalize(userId) === postOwnerId) return true;
    if (me?.id && normalize(me.id) === postOwnerId) return true;
    return false;
  })();

  const [createComment, { loading: isCommentSubmitting }] = useMutation(
    CREATE_COMMENT_MUTATION,
    {
      onError: (error) => {
        console.error("Error creating comment:", error);
      },
    },
  );

  const [editPost, { loading: isPostSaving }] = useMutation(
    EDIT_POST_MUTATION,
    {
      onError: (error) => {
        console.error("Error editing post:", error);
      },
    },
  );

  const [deletePost, { loading: isPostDeleting }] = useMutation(
    DELETE_POST_MUTATION,
    {
      onError: (error) => {
        console.error("Error deleting post:", error);
      },
    },
  );

  const [editComment, { loading: isCommentSaving }] = useMutation(
    EDIT_COMMENT_MUTATION,
    {
      onError: (error) => {
        console.error("Error editing comment:", error);
      },
    },
  );

  const [deleteComment, { loading: isCommentDeleting }] = useMutation(
    DELETE_COMMENT_MUTATION,
    {
      onError: (error) => {
        console.error("Error deleting comment:", error);
      },
    },
  );

  const isCommentOwner = (commentUserId: string) => {
    if (!commentUserId) return false;
    const normalize = (value: string) => value.trim().toLowerCase();
    const ownerId = normalize(commentUserId);
    if (userId && normalize(userId) === ownerId) return true;
    if (me?.id && normalize(me.id) === ownerId) return true;
    return false;
  };

  const canDeleteComment = (commentUserId: string) => {
    return isPostOwner || isCommentOwner(commentUserId);
  };

  const canEditComment = (commentUserId: string) => {
    return isCommentOwner(commentUserId);
  };

  // Load more comments when scrolling to bottom
  const loadMoreComments = useCallback(async () => {
    if (isLoadingMoreComments || !hasMoreComments) return;

    setIsLoadingMoreComments(true);
    const nextPage = commentsPage + 1;

    try {
      const result = await refetchComments({ postId, pageNumber: nextPage });
      const newComments = result.data?.comments || [];

      if (newComments.length < COMMENTS_PAGE_SIZE) {
        setHasMoreComments(false);
      }

      if (newComments.length > 0) {
        setComments((prev) => {
          // Avoid duplicates
          const existingIds = new Set(prev.map((c) => c.id));
          const unique = newComments.filter(
            (c: CommentType) => !existingIds.has(c.id),
          );
          return [...prev, ...unique];
        });
        setCommentsPage(nextPage);
      }
    } catch (error) {
      console.error("Error loading more comments:", error);
    } finally {
      setIsLoadingMoreComments(false);
    }
  }, [
    isLoadingMoreComments,
    hasMoreComments,
    commentsPage,
    postId,
    refetchComments,
    COMMENTS_PAGE_SIZE,
  ]);

  // Handle scroll to detect near-bottom
  const handleCommentsScroll = useCallback(() => {
    const container = commentsContainerRef.current;
    if (!container) return;

    const { scrollTop, scrollHeight, clientHeight } = container;
    const nearBottom = scrollHeight - scrollTop - clientHeight < 100;

    if (nearBottom && hasMoreComments && !isLoadingMoreComments) {
      loadMoreComments();
    }
  }, [hasMoreComments, isLoadingMoreComments, loadMoreComments]);

  const submitEditPost = async () => {
    setPostActionError(null);

    if (!isAuthenticated) {
      setPostActionError("Please log in.");
      return;
    }

    if (!post) return;

    const nextText = editPostText.trim();
    if (!nextText) {
      setPostActionError("Post cannot be empty.");
      return;
    }

    try {
      await editPost({
        variables: { id: post.id, text: nextText },
        refetchQueries: ["Post", "Posts", "MyPosts"],
      });
      setIsEditingPost(false);
      setIsPostExpanded(false);
      await refetch();
    } catch (error: any) {
      setPostActionError(error?.message || "Failed to update post.");
    }
  };

  const submitDeletePost = async () => {
    setPostActionError(null);
    if (!post) return;

    setIsPostMenuOpen(false);
    setIsDeleteConfirmOpen(true);
  };

  const confirmDeletePost = async () => {
    setPostActionError(null);
    if (!post) return;

    const id = post.id;

    try {
      await deletePost({
        variables: { id },
        optimisticResponse: { deletePost: true },
        refetchQueries: ["Posts", "MyPosts"],
        update: (cache) => {
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

          const postDtoId = cache.identify({ __typename: "PostDto", id });
          if (postDtoId) cache.evict({ id: postDtoId });

          const postDetailDtoId = cache.identify({
            __typename: "PostDetailDto",
            id,
          });
          if (postDetailDtoId) cache.evict({ id: postDetailDtoId });

          cache.gc();
        },
      });
      setIsDeleteConfirmOpen(false);
      onDeleted?.(id);
      onClose();
    } catch (error: any) {
      setPostActionError(error?.message || "Failed to delete post.");
    }
  };

  const submitComment = async () => {
    setCommentError(null);

    if (!isAuthenticated) {
      setCommentError("Please log in to comment.");
      return;
    }

    if (!commentText.trim()) {
      setCommentError("Comment cannot be empty.");
      return;
    }

    try {
      const result = await createComment({
        variables: {
          postId,
          text: commentText.trim(),
        },
      });

      // Add new comment to the beginning (newest first)
      const newComment = result.data?.createComment;
      if (newComment) {
        setComments((prev) => [newComment, ...prev]);
        setTotalCommentCount((prev) => prev + 1);
      }

      setCommentText("");
    } catch (error: any) {
      setCommentError(error?.message || "Failed to comment. Please try again.");
    }
  };

  const handleSubmitComment = async (e: React.FormEvent) => {
    e.preventDefault();
    await submitComment();
  };

  const submitEditComment = async () => {
    setCommentActionError(null);

    if (!isAuthenticated) {
      setCommentActionError("Please log in.");
      return;
    }

    if (!editingCommentId) return;

    const nextText = editCommentText.trim();
    if (!nextText) {
      setCommentActionError("Comment cannot be empty.");
      return;
    }

    try {
      await editComment({
        variables: { postId, id: editingCommentId, text: nextText },
      });

      // Update comment in local state
      setComments((prev) =>
        prev.map((c) =>
          c.id === editingCommentId ? { ...c, text: nextText } : c,
        ),
      );

      setEditingCommentId(null);
      setEditCommentText("");
    } catch (error: any) {
      setCommentActionError(error?.message || "Failed to update comment.");
    }
  };

  const confirmDeleteComment = async () => {
    setCommentActionError(null);
    if (!deleteCommentId) return;

    console.log("Deleting comment:", { postId, id: deleteCommentId });

    try {
      const result = await deleteComment({
        variables: { postId, id: deleteCommentId },
      });
      console.log("Delete result:", result);

      // Remove comment from local state
      setComments((prev) => prev.filter((c) => c.id !== deleteCommentId));
      setTotalCommentCount((prev) => Math.max(0, prev - 1));

      setDeleteCommentId(null);
    } catch (error: any) {
      console.error("Delete error:", error);
      setDeleteCommentId(null); // Close dialog first so error is visible
      setCommentActionError(error?.message || "Failed to delete comment.");
    }
  };

  // ESC to close + lock body scroll
  useEffect(() => {
    if (!isOpen) return;

    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        if (isEmojiOpen) {
          setIsEmojiOpen(false);
          return;
        }
        if (isPostMenuOpen) {
          setIsPostMenuOpen(false);
          return;
        }
        if (commentMenuOpenId) {
          setCommentMenuOpenId(null);
          return;
        }
        if (editingCommentId) {
          setEditingCommentId(null);
          setEditCommentText("");
          return;
        }
        onClose();
      }
    };

    document.addEventListener("keydown", onKeyDown);
    const prevOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";

    return () => {
      document.removeEventListener("keydown", onKeyDown);
      document.body.style.overflow = prevOverflow;
    };
  }, [
    isOpen,
    isEmojiOpen,
    isPostMenuOpen,
    commentMenuOpenId,
    editingCommentId,
    onClose,
  ]);

  // Reset composer when opening a different post
  useEffect(() => {
    if (!isOpen) return;
    setCommentText("");
    setCommentError(null);
    setPostActionError(null);
    setIsPostExpanded(false);
    setIsEmojiOpen(false);
    setIsPostMenuOpen(false);
    setIsEditingPost(false);
    setEditPostText("");
    setIsDeleteConfirmOpen(false);
    setEditingCommentId(null);
    setEditCommentText("");
    setCommentMenuOpenId(null);
    setDeleteCommentId(null);
    setCommentActionError(null);
    // Reset comments pagination
    setComments([]);
    setCommentsPage(1);
    setHasMoreComments(true);
    setIsLoadingMoreComments(false);
  }, [isOpen, postId]);

  useEffect(() => {
    if (!isOpen) return;
    if (!post) return;
    setEditPostText(post.text ?? "");
    setTotalCommentCount(post.commentCount ?? 0);
  }, [isOpen, post]);

  // Close emoji picker on outside click
  useEffect(() => {
    if (!isEmojiOpen) return;

    const onMouseDown = (e: MouseEvent) => {
      const popover = emojiPopoverRef.current;
      if (!popover) return;
      if (e.target instanceof Node && popover.contains(e.target)) return;
      setIsEmojiOpen(false);
    };

    document.addEventListener("mousedown", onMouseDown);
    return () => document.removeEventListener("mousedown", onMouseDown);
  }, [isEmojiOpen]);

  // Close post menu on outside click
  useEffect(() => {
    if (!isPostMenuOpen) return;

    const onMouseDown = (e: MouseEvent) => {
      const popover = postMenuPopoverRef.current;
      if (!popover) return;
      if (e.target instanceof Node && popover.contains(e.target)) return;
      setIsPostMenuOpen(false);
    };

    document.addEventListener("mousedown", onMouseDown);
    return () => document.removeEventListener("mousedown", onMouseDown);
  }, [isPostMenuOpen]);

  // Close comment menu on outside click
  useEffect(() => {
    if (!commentMenuOpenId) return;

    const onMouseDown = (e: MouseEvent) => {
      const popover = commentMenuPopoverRef.current;
      if (!popover) return;
      if (e.target instanceof Node && popover.contains(e.target)) return;
      setCommentMenuOpenId(null);
    };

    document.addEventListener("mousedown", onMouseDown);
    return () => document.removeEventListener("mousedown", onMouseDown);
  }, [commentMenuOpenId]);

  const postDate = useMemo(() => {
    if (!post) return null;
    return formatFacebookDate(post.createdDate);
  }, [post]);

  if (!isOpen) return null;

  const postTextLimit = post?.imageUrl ? 220 : 480;
  const isPostTextLong = (post?.text?.length ?? 0) > postTextLimit;
  const postDisplayText =
    !post || isPostExpanded || !isPostTextLong
      ? (post?.text ?? "")
      : (post.text ?? "").slice(0, postTextLimit).trim() + "...";

  const emojiList = [
    "😀",
    "😁",
    "😂",
    "🤣",
    "😊",
    "😍",
    "😘",
    "😎",
    "😢",
    "😭",
    "😡",
    "👍",
    "👎",
    "🙏",
    "👏",
    "🔥",
    "❤️",
    "🎉",
  ];

  const insertEmoji = (emoji: string) => {
    setCommentText((current) => {
      const el = textareaRef.current;
      if (!el) return current + emoji;

      const start = el.selectionStart ?? current.length;
      const end = el.selectionEnd ?? current.length;
      const next = current.slice(0, start) + emoji + current.slice(end);

      requestAnimationFrame(() => {
        try {
          el.focus();
          const cursor = start + emoji.length;
          el.selectionStart = cursor;
          el.selectionEnd = cursor;
        } catch {
          // ignore
        }
      });

      return next;
    });
  };

  // Don't render on server or before mount
  if (!mounted) return null;

  const modalContent = (
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center p-3 sm:p-6"
      role="dialog"
      aria-modal="true"
      onMouseDown={(e) => {
        // close on backdrop click
        if (e.target === e.currentTarget) onClose();
      }}
    >
      <ConfirmDialog
        isOpen={isDeleteConfirmOpen}
        title="Delete post?"
        description="You can’t undo this action. This post will be deleted permanently."
        cancelText="Cancel"
        confirmText="Delete"
        confirmVariant="danger"
        isConfirmLoading={isPostDeleting}
        onCancel={() => {
          if (isPostDeleting) return;
          setIsDeleteConfirmOpen(false);
        }}
        onConfirm={confirmDeletePost}
      />
      <div className="absolute inset-0 bg-black/70 backdrop-blur-[2px]" />

      <div className="relative w-full max-w-2xl max-h-[85vh] overflow-hidden">
        <Card className="bg-white/95 dark:bg-primary-3/95 border border-gray-200 dark:border-white/10 shadow-2xl">
          <div className="flex items-center justify-between gap-3 pb-3 border-b border-gray-200 dark:border-white/10">
            <div className="text-lg font-semibold text-gray-900 dark:text-white">
              Post
            </div>
            <Button variant="ghost" size="sm" onClick={onClose}>
              ✕
            </Button>
          </div>

          <div className="flex flex-col max-h-[75vh]">
            <div
              ref={commentsContainerRef}
              onScroll={handleCommentsScroll}
              className="flex-1 overflow-y-auto pr-1 pb-3"
            >
              <div className="pt-4">
                {postLoading && !post ? (
                  <div className="py-10 text-center text-muted">
                    Loading post…
                  </div>
                ) : postError ? (
                  <div className="py-6">
                    <div className="bg-danger-1/10 border border-danger-1/30 rounded-lg p-4">
                      <p className="text-danger-1 font-semibold mb-1">
                        Failed to load post
                      </p>
                      <p className="text-muted text-sm mb-4">
                        {postError.message}
                      </p>
                      <Button size="sm" onClick={() => refetch()}>
                        Try Again
                      </Button>
                    </div>
                  </div>
                ) : !post ? (
                  <div className="py-10 text-center text-muted">
                    Post not found.
                  </div>
                ) : (
                  <>
                    <div className="flex items-start gap-3">
                      <Link
                        href={`/profile/user/${post.userId}`}
                        className="shrink-0"
                      >
                        <Avatar
                          initials={post.username}
                          src={post.userProfile}
                        />
                      </Link>
                      <div className="flex-1 min-w-0 flex items-start justify-between gap-2">
                        <div className="min-w-0">
                          <Link
                            href={`/profile/user/${post.userId}`}
                            className="font-semibold text-gray-900 dark:text-white leading-snug hover:text-primary-1 transition-colors"
                          >
                            {post.username}
                          </Link>
                          {postDate && (
                            <div
                              className="text-xs text-muted mt-0.5"
                              title={postDate.fullDate}
                            >
                              {postDate.display}
                            </div>
                          )}
                        </div>

                        {isPostOwner && (
                          <div
                            className="relative flex-shrink-0"
                            ref={postMenuPopoverRef}
                          >
                            <button
                              type="button"
                              onClick={() => setIsPostMenuOpen((open) => !open)}
                              className="w-9 h-9 rounded-full grid place-items-center bg-transparent hover:bg-gray-100 dark:hover:bg-white/10 transition-colors text-muted hover:text-gray-900 dark:hover:text-white focus:outline-none focus-visible:ring-2 focus-visible:ring-gray-200 dark:focus-visible:ring-white/20"
                              aria-label="Post options"
                              title="Options"
                              disabled={isPostSaving || isPostDeleting}
                            >
                              <svg
                                width="18"
                                height="18"
                                viewBox="0 0 24 24"
                                fill="currentColor"
                                aria-hidden="true"
                              >
                                <circle cx="5" cy="12" r="2" />
                                <circle cx="12" cy="12" r="2" />
                                <circle cx="19" cy="12" r="2" />
                              </svg>
                            </button>

                            {isPostMenuOpen && (
                              <div className="absolute right-0 mt-2 w-48 rounded-xl border border-gray-200 dark:border-white/10 bg-white dark:bg-primary-3 shadow-2xl overflow-hidden">
                                <button
                                  type="button"
                                  className="w-full text-left px-3 py-2.5 text-sm text-gray-900 dark:text-white hover:bg-gray-100 dark:hover:bg-white/10 transition-colors"
                                  onClick={() => {
                                    setIsPostMenuOpen(false);
                                    setPostActionError(null);
                                    setIsEditingPost(true);
                                    setIsPostExpanded(false);
                                  }}
                                >
                                  Edit post
                                </button>
                                <div className="h-px bg-gray-200 dark:bg-white/10" />
                                <button
                                  type="button"
                                  className="w-full text-left px-3 py-2.5 text-sm text-danger-1 hover:bg-gray-100 dark:hover:bg-white/10 transition-colors"
                                  onClick={() => {
                                    setIsPostMenuOpen(false);
                                    void submitDeletePost();
                                  }}
                                >
                                  Delete post
                                </button>
                              </div>
                            )}
                          </div>
                        )}
                      </div>
                    </div>

                    {postActionError && (
                      <div className="mt-3 bg-danger-1/10 border border-danger-1/30 rounded-lg p-3 text-sm text-danger-1">
                        {postActionError}
                      </div>
                    )}

                    <div className="pt-3">
                      {isEditingPost ? (
                        <div className="space-y-2">
                          <Textarea
                            value={editPostText}
                            onChange={(e) => setEditPostText(e.target.value)}
                            rows={3}
                            maxLength={500}
                            disabled={isPostSaving || isPostDeleting}
                            className="rounded-xl"
                          />
                          <div className="flex items-center justify-end gap-2">
                            <Button
                              type="button"
                              variant="secondary"
                              size="sm"
                              onClick={() => {
                                setIsEditingPost(false);
                                setPostActionError(null);
                                setEditPostText(post.text ?? "");
                              }}
                              disabled={isPostSaving || isPostDeleting}
                            >
                              Cancel
                            </Button>
                            <Button
                              type="button"
                              size="sm"
                              isLoading={isPostSaving}
                              onClick={() => void submitEditPost()}
                              disabled={isPostSaving || isPostDeleting}
                            >
                              Save
                            </Button>
                          </div>
                        </div>
                      ) : isPostTextLong && !isPostExpanded ? (
                        <p className="text-gray-700 dark:text-white/90 leading-relaxed whitespace-pre-wrap text-sm">
                          {postDisplayText}{" "}
                          <button
                            type="button"
                            onClick={() => setIsPostExpanded(true)}
                            className="inline bg-none border-none p-0 m-0 text-primary-1 hover:text-primary-1 hover:underline font-semibold text-xs underline-offset-2 cursor-pointer"
                            style={{ font: "inherit", appearance: "none" }}
                          >
                            See More
                          </button>
                        </p>
                      ) : (
                        <p className="text-gray-700 dark:text-white/90 leading-relaxed whitespace-pre-wrap text-sm">
                          {postDisplayText}
                          {isPostTextLong && isPostExpanded && (
                            <>
                              {" "}
                              <button
                                type="button"
                                onClick={() => setIsPostExpanded(false)}
                                className="inline bg-none border-none p-0 m-0 text-primary-1 hover:text-primary-1 hover:underline font-semibold text-xs underline-offset-2 cursor-pointer"
                                style={{
                                  font: "inherit",
                                  appearance: "none",
                                }}
                              >
                                Show Less
                              </button>
                            </>
                          )}
                        </p>
                      )}
                    </div>

                    {post.imageUrl && (
                      <img
                        src={post.imageUrl}
                        alt="Post content"
                        className="w-full rounded-[10px] mt-4 max-h-96 object-cover"
                      />
                    )}
                  </>
                )}
              </div>

              <div className="pt-6 pb-2">
                <div className="text-sm font-semibold text-gray-900 dark:text-white mb-3">
                  Comments ({totalCommentCount})
                </div>

                {commentActionError && (
                  <div className="mb-3 bg-danger-1/10 border border-danger-1/30 rounded-lg p-3 text-sm text-danger-1">
                    {commentActionError}
                  </div>
                )}

                {deleteCommentId && (
                  <ConfirmDialog
                    isOpen={!!deleteCommentId}
                    title="Delete comment?"
                    description="You can't undo this action. This comment will be deleted permanently."
                    cancelText="Cancel"
                    confirmText="Delete"
                    confirmVariant="danger"
                    isConfirmLoading={isCommentDeleting}
                    onCancel={() => {
                      if (isCommentDeleting) return;
                      setDeleteCommentId(null);
                    }}
                    onConfirm={confirmDeleteComment}
                  />
                )}

                {commentsLoading && comments.length === 0 ? (
                  <div className="py-4 text-center text-muted text-sm">
                    Loading comments…
                  </div>
                ) : comments.length === 0 ? (
                  <div className="text-sm text-muted">No comments yet.</div>
                ) : (
                  <div className="space-y-3">
                    {comments.map((c) => {
                      const { display, fullDate } = formatAgeShort(
                        c.createdDate,
                      );
                      const isEditing = editingCommentId === c.id;
                      const showMenu =
                        canEditComment(c.userId) || canDeleteComment(c.userId);

                      return (
                        <div
                          key={c.id}
                          id={`comment-${c.id}`}
                          className="flex items-start gap-2 group"
                        >
                          <Link
                            href={`/profile/user/${c.userId}`}
                            className="shrink-0"
                          >
                            <Avatar
                              initials={c.username}
                              src={c.userProfile}
                              size="sm"
                              className="mt-0.5"
                            />
                          </Link>

                          <div className="flex-1 min-w-0">
                            {isEditing ? (
                              <div className="space-y-2">
                                <Textarea
                                  value={editCommentText}
                                  onChange={(e) =>
                                    setEditCommentText(e.target.value)
                                  }
                                  rows={2}
                                  maxLength={500}
                                  disabled={isCommentSaving}
                                  className="rounded-xl"
                                  autoFocus
                                />
                                <div className="flex items-center justify-end gap-2">
                                  <Button
                                    type="button"
                                    variant="secondary"
                                    size="sm"
                                    onClick={() => {
                                      setEditingCommentId(null);
                                      setEditCommentText("");
                                      setCommentActionError(null);
                                    }}
                                    disabled={isCommentSaving}
                                  >
                                    Cancel
                                  </Button>
                                  <Button
                                    type="button"
                                    size="sm"
                                    isLoading={isCommentSaving}
                                    onClick={() => void submitEditComment()}
                                    disabled={isCommentSaving}
                                  >
                                    Save
                                  </Button>
                                </div>
                              </div>
                            ) : (
                              <>
                                <div className="flex items-start gap-1">
                                  <div className="inline-block max-w-full rounded-2xl bg-gray-100 dark:bg-white/10 border border-gray-200 dark:border-white/10 px-3 py-2">
                                    <Link
                                      href={`/profile/user/${c.userId}`}
                                      className="text-sm font-semibold text-gray-900 dark:text-white truncate leading-snug hover:text-primary-1 transition-colors"
                                    >
                                      {c.username}
                                    </Link>
                                    <p className="text-sm text-gray-700 dark:text-white/90 whitespace-pre-wrap mt-1 leading-snug">
                                      {c.text}
                                    </p>
                                  </div>

                                  {showMenu && (
                                    <div
                                      className="relative flex-shrink-0 opacity-0 group-hover:opacity-100 transition-opacity"
                                      ref={
                                        commentMenuOpenId === c.id
                                          ? commentMenuPopoverRef
                                          : undefined
                                      }
                                    >
                                      <button
                                        type="button"
                                        onClick={() =>
                                          setCommentMenuOpenId(
                                            commentMenuOpenId === c.id
                                              ? null
                                              : c.id,
                                          )
                                        }
                                        className="w-7 h-7 rounded-full grid place-items-center bg-transparent hover:bg-gray-100 dark:hover:bg-white/10 transition-colors text-muted hover:text-gray-900 dark:hover:text-white focus:outline-none focus-visible:ring-2 focus-visible:ring-gray-200 dark:focus-visible:ring-white/20"
                                        aria-label="Comment options"
                                        title="Options"
                                        disabled={
                                          isCommentSaving || isCommentDeleting
                                        }
                                      >
                                        <svg
                                          width="14"
                                          height="14"
                                          viewBox="0 0 24 24"
                                          fill="currentColor"
                                          aria-hidden="true"
                                        >
                                          <circle cx="5" cy="12" r="2" />
                                          <circle cx="12" cy="12" r="2" />
                                          <circle cx="19" cy="12" r="2" />
                                        </svg>
                                      </button>

                                      {commentMenuOpenId === c.id && (
                                        <div className="absolute left-0 mt-1 w-36 rounded-xl border border-gray-200 dark:border-white/10 bg-white dark:bg-primary-3 shadow-2xl overflow-hidden z-10">
                                          {canEditComment(c.userId) && (
                                            <button
                                              type="button"
                                              className="w-full text-left px-3 py-2 text-sm text-gray-900 dark:text-white hover:bg-gray-100 dark:hover:bg-white/10 transition-colors"
                                              onClick={() => {
                                                setCommentMenuOpenId(null);
                                                setCommentActionError(null);
                                                setEditingCommentId(c.id);
                                                setEditCommentText(c.text);
                                              }}
                                            >
                                              Edit
                                            </button>
                                          )}
                                          {canEditComment(c.userId) &&
                                            canDeleteComment(c.userId) && (
                                              <div className="h-px bg-gray-200 dark:bg-white/10" />
                                            )}
                                          {canDeleteComment(c.userId) && (
                                            <button
                                              type="button"
                                              className="w-full text-left px-3 py-2 text-sm text-danger-1 hover:bg-gray-100 dark:hover:bg-white/10 transition-colors"
                                              onClick={() => {
                                                setCommentMenuOpenId(null);
                                                setDeleteCommentId(c.id);
                                              }}
                                            >
                                              Delete
                                            </button>
                                          )}
                                        </div>
                                      )}
                                    </div>
                                  )}
                                </div>

                                <div
                                  className="text-xs text-muted mt-1 ml-3"
                                  title={fullDate}
                                >
                                  {display}
                                </div>
                              </>
                            )}
                          </div>
                        </div>
                      );
                    })}

                    {/* Loading indicator for infinite scroll */}
                    {isLoadingMoreComments && (
                      <div className="py-3 text-center text-muted text-sm">
                        Loading more comments…
                      </div>
                    )}

                    {/* End of comments indicator */}
                    {!hasMoreComments &&
                      comments.length >= COMMENTS_PAGE_SIZE && (
                        <div className="py-3 text-center text-muted text-xs">
                          No more comments
                        </div>
                      )}
                  </div>
                )}
              </div>
            </div>

            <div className="sticky bottom-0 border-t border-gray-200 dark:border-white/10 bg-white/95 dark:bg-primary-3/95 backdrop-blur supports-[backdrop-filter]:bg-white/80 dark:supports-[backdrop-filter]:bg-primary-3/80">
              {commentError && (
                <div className="mb-3 bg-danger-1/10 border border-danger-1/30 rounded-lg p-3 text-sm text-danger-1">
                  {commentError}
                </div>
              )}

              <form onSubmit={handleSubmitComment} className="px-1 py-3">
                <div className="flex items-start gap-2">
                  <Avatar
                    initials={
                      isAuthenticated ? me?.username || username || "You" : "?"
                    }
                    src={
                      isAuthenticated
                        ? me?.profile || meFallback?.profile || undefined
                        : undefined
                    }
                    size="sm"
                    className="mt-0.5"
                  />

                  <div className="relative flex-1">
                    <Textarea
                      ref={textareaRef}
                      value={commentText}
                      onChange={(e) => setCommentText(e.target.value)}
                      onKeyDown={async (e) => {
                        if (e.key === "Enter" && !e.shiftKey) {
                          e.preventDefault();
                          await submitComment();
                        }
                      }}
                      placeholder={
                        isAuthenticated
                          ? `Comment as ${
                              me?.username ||
                              meFallback?.username ||
                              username ||
                              "You"
                            }…`
                          : "Log in to comment…"
                      }
                      rows={1}
                      maxLength={500}
                      disabled={!isAuthenticated || isCommentSubmitting}
                      className="rounded-full resize-none pr-[92px] py-2"
                    />

                    <div className="absolute right-2 top-1/2 -translate-y-1/2 flex items-center gap-1">
                      <button
                        type="button"
                        onClick={() => setIsEmojiOpen((open) => !open)}
                        className="w-8 h-8 rounded-full hover:bg-gray-100 dark:hover:bg-white/10 transition-colors text-muted hover:text-gray-900 dark:hover:text-white"
                        aria-label="Add emoji"
                        title="Emoji"
                        disabled={!isAuthenticated || isCommentSubmitting}
                      >
                        😊
                      </button>

                      <button
                        type="submit"
                        className="w-8 h-8 rounded-full hover:bg-gray-100 dark:hover:bg-white/10 transition-colors text-muted hover:text-gray-900 dark:hover:text-white disabled:opacity-50"
                        aria-label="Send comment"
                        title="Send"
                        disabled={
                          !isAuthenticated ||
                          !commentText.trim() ||
                          isCommentSubmitting
                        }
                      >
                        ➤
                      </button>

                      {isEmojiOpen && (
                        <div
                          ref={emojiPopoverRef}
                          className="absolute bottom-10 right-0 w-60 p-2 rounded-xl border border-gray-200 dark:border-white/10 bg-white dark:bg-primary-3 shadow-2xl"
                        >
                          <div className="grid grid-cols-6 gap-1">
                            {emojiList.map((emoji) => (
                              <button
                                key={emoji}
                                type="button"
                                className="w-8 h-8 rounded-lg hover:bg-gray-100 dark:hover:bg-white/10 transition-colors"
                                onClick={() => {
                                  insertEmoji(emoji);
                                  setIsEmojiOpen(false);
                                }}
                              >
                                {emoji}
                              </button>
                            ))}
                          </div>
                        </div>
                      )}
                    </div>
                  </div>
                </div>

                {!isAuthenticated && (
                  <div className="text-xs text-muted mt-2 pl-[44px]">
                    Log in to add a comment.
                  </div>
                )}
              </form>
            </div>
          </div>
        </Card>
      </div>
    </div>
  );

  // Render modal in a portal to document.body for proper centering
  return createPortal(modalContent, document.body);
}
