"use client";

import Link from "next/link";
import { useMutation, useQuery } from "@apollo/client";
import { useCallback, useEffect, useRef, useState } from "react";
import {
  COMMENTS_QUERY,
  CREATE_COMMENT_MUTATION,
  POST_DETAIL_QUERY,
} from "@/lib/graphql/queries";
import { PostDetail as PostDetailType, Comment as CommentType } from "@/types";
import { Card, Button, Textarea, Avatar } from "@/components/ui/atoms";
import { formatFacebookDate } from "@/lib/formatDate";
import { useAuth } from "@/hooks/useAuth";

export function PostDetailPage({ postId }: { postId: string }) {
  const { isAuthenticated } = useAuth();
  const [commentText, setCommentText] = useState("");
  const [commentError, setCommentError] = useState<string | null>(null);

  // Comments pagination state
  const [comments, setComments] = useState<CommentType[]>([]);
  const [commentsPage, setCommentsPage] = useState(1);
  const [hasMoreComments, setHasMoreComments] = useState(true);
  const [isLoadingMoreComments, setIsLoadingMoreComments] = useState(false);
  const [totalCommentCount, setTotalCommentCount] = useState(0);
  const COMMENTS_PAGE_SIZE = 10;
  const commentsContainerRef = useRef<HTMLDivElement | null>(null);

  const {
    data,
    loading: postLoading,
    error: postError,
    refetch,
  } = useQuery(POST_DETAIL_QUERY, {
    variables: { id: postId },
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
    fetchPolicy: "cache-and-network",
    onCompleted: (data) => {
      const fetchedComments = data?.comments || [];
      setComments(fetchedComments);
      setHasMoreComments(fetchedComments.length >= COMMENTS_PAGE_SIZE);
      setCommentsPage(1);
    },
  });

  // Reset comments when postId changes
  useEffect(() => {
    setComments([]);
    setCommentsPage(1);
    setHasMoreComments(true);
    setIsLoadingMoreComments(false);
  }, [postId]);

  // Initialize total comment count from post data
  useEffect(() => {
    if (post?.commentCount !== undefined) {
      setTotalCommentCount(post.commentCount);
    }
  }, [post?.commentCount]);

  // Load more comments
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

  const [createComment, { loading: isCommentSubmitting }] = useMutation(
    CREATE_COMMENT_MUTATION,
    {
      onError: (error) => {
        console.error("Error creating comment:", error);
      },
    },
  );

  const handleSubmitComment = async (e: React.FormEvent) => {
    e.preventDefault();
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

  if (postLoading && !post) {
    return (
      <div className="max-w-2xl mx-auto">
        <Card>
          <div className="py-10 text-center text-muted">Loading post…</div>
        </Card>
      </div>
    );
  }

  if (postError) {
    return (
      <div className="max-w-2xl mx-auto">
        <Card className="text-center bg-danger-1/10 border border-danger-1/30">
          <div className="py-10">
            <p className="text-danger-1 font-semibold mb-2">
              Failed to load post
            </p>
            <p className="text-muted text-sm mb-6">{postError.message}</p>
            <Button size="sm" onClick={() => refetch()}>
              Try Again
            </Button>
          </div>
        </Card>
      </div>
    );
  }

  if (!post) {
    return (
      <div className="max-w-2xl mx-auto">
        <Card>
          <div className="py-10 text-center text-muted">Post not found.</div>
        </Card>
      </div>
    );
  }

  const { display: postTime, fullDate: postFullDate } = formatFacebookDate(
    post.createdDate,
  );

  return (
    <div className="max-w-2xl mx-auto space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-white">Post</h1>
        <Link href="/" className="text-sm text-primary-1 hover:underline">
          ← Back to feed
        </Link>
      </div>

      <Card>
        <div className="flex items-start gap-3">
          <Avatar initials={post.username} src={post.userProfile} />
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2">
              <div className="font-semibold text-white">{post.username}</div>
              <div className="text-xs text-muted" title={postFullDate}>
                {postTime}
              </div>
            </div>
            <p className="text-white/90 whitespace-pre-wrap mt-2">
              {post.text}
            </p>
          </div>
        </div>

        {post.imageUrl && (
          <img
            src={post.imageUrl}
            alt="Post content"
            className="w-full rounded-[10px] mt-4 max-h-96 object-cover"
          />
        )}
      </Card>

      <Card>
        <h2 className="text-lg font-semibold text-white mb-3">Add a comment</h2>

        {!isAuthenticated && (
          <div className="mb-4 text-sm text-muted">
            You need to{" "}
            <Link href="/login" className="text-primary-1 hover:underline">
              log in
            </Link>{" "}
            to comment.
          </div>
        )}

        {commentError && (
          <div className="mb-4 bg-danger-1/10 border border-danger-1/30 rounded-lg p-3 text-sm text-danger-1">
            {commentError}
          </div>
        )}

        <form onSubmit={handleSubmitComment} className="space-y-3">
          <Textarea
            value={commentText}
            onChange={(e) => setCommentText(e.target.value)}
            placeholder="Write a comment…"
            rows={3}
            maxLength={500}
            showCharCount
            disabled={isCommentSubmitting}
          />

          <div className="flex justify-end">
            <Button
              type="submit"
              size="sm"
              isLoading={isCommentSubmitting}
              disabled={!commentText.trim() || isCommentSubmitting}
            >
              Comment
            </Button>
          </div>
        </form>
      </Card>

      <Card>
        <h2 className="text-lg font-semibold text-white mb-4">
          Comments ({totalCommentCount})
        </h2>

        {commentsLoading && comments.length === 0 ? (
          <div className="py-4 text-center text-muted text-sm">
            Loading comments…
          </div>
        ) : comments.length === 0 ? (
          <div className="text-sm text-muted">No comments yet.</div>
        ) : (
          <div
            ref={commentsContainerRef}
            onScroll={handleCommentsScroll}
            className="space-y-4 max-h-96 overflow-y-auto"
          >
            {comments.map((c) => {
              const { display, fullDate } = formatFacebookDate(c.createdDate);
              return (
                <div
                  key={c.id}
                  id={`comment-${c.id}`}
                  className="flex items-start gap-3 border-t border-white/10 pt-4 first:border-t-0 first:pt-0"
                >
                  <Avatar initials={c.username} src={c.userProfile} size="sm" />
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <div className="text-sm font-semibold text-white">
                        {c.username}
                      </div>
                      <div className="text-xs text-muted" title={fullDate}>
                        {display}
                      </div>
                    </div>
                    <p className="text-sm text-white/90 whitespace-pre-wrap mt-1">
                      {c.text}
                    </p>
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
            {!hasMoreComments && comments.length >= COMMENTS_PAGE_SIZE && (
              <div className="py-3 text-center text-muted text-xs">
                No more comments
              </div>
            )}
          </div>
        )}
      </Card>
    </div>
  );
}
