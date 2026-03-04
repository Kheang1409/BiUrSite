"use client";

import { useQuery } from "@apollo/client";
import { USER_QUERY, POSTS_QUERY } from "@/lib/graphql/queries";
import { User, Post as PostType } from "@/types";
import { Post } from "@/components/Post";
import { PostDetailModal } from "@/components/modals/PostDetailModal";
import { useAuth } from "@/hooks/useAuth";
import { useRouter } from "next/navigation";
import { useEffect, useRef, useState, useMemo } from "react";
import { Card, LoadingSkeleton } from "@/components/ui/atoms";

interface UserProfilePageProps {
  userId: string;
}

export function UserProfilePage({ userId }: UserProfilePageProps) {
  const router = useRouter();
  const { userId: currentUserId } = useAuth();
  const [pageNumber, setPageNumber] = useState(1);
  const [hasMore, setHasMore] = useState(true);
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const [activePostId, setActivePostId] = useState<string | null>(null);
  const loadMoreRef = useRef<HTMLDivElement | null>(null);
  const prevServerCountRef = useRef(0);
  const loadingMoreRef = useRef(false);
  const PAGE_SIZE = 10;

  const {
    data: userData,
    loading: userLoading,
    error: userError,
  } = useQuery(USER_QUERY, {
    variables: { id: userId },
  });

  const {
    data: postsData,
    loading: postsLoading,
    refetch: refetchUserPosts,
  } = useQuery(POSTS_QUERY, {
    variables: { pageNumber, keywords: null },
    fetchPolicy: "cache-and-network",
    notifyOnNetworkStatusChange: true,
  });

  const user: User | undefined = userData?.user;
  const allPosts: PostType[] = postsData?.posts || [];

  // Filter posts to only show this user's posts
  const posts = useMemo(
    () => allPosts.filter((p) => p.userId === userId),
    [allPosts, userId],
  );

  // Redirect to own profile if viewing own profile
  useEffect(() => {
    if (
      currentUserId &&
      userId &&
      currentUserId.trim().toLowerCase() === userId.trim().toLowerCase()
    ) {
      router.replace("/profile");
    }
  }, [userId, currentUserId, router]);

  // Check if user not found
  useEffect(() => {
    if (!userLoading && !user && userError) {
      router.replace("/404");
    }
  }, [user, userLoading, userError, router]);

  useEffect(() => {
    if (pageNumber === 1 && !postsLoading) {
      setHasMore(posts.length >= PAGE_SIZE);
    }
  }, [pageNumber, posts.length, postsLoading]);

  useEffect(() => {
    if (!isLoadingMore) return;
    if (postsLoading) return;

    const delta = posts.length - prevServerCountRef.current;
    setHasMore(delta >= PAGE_SIZE);
    setIsLoadingMore(false);
    loadingMoreRef.current = false;
  }, [isLoadingMore, posts.length, postsLoading]);

  const triggerLoadMore = () => {
    if (postsLoading) return;
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
  }, [hasMore, isLoadingMore, postsLoading, posts.length]);

  if (userLoading) {
    return (
      <div className="max-w-2xl mx-auto">
        <LoadingSkeleton variant="avatar" />
        <div className="mt-8 space-y-4">
          {[...Array(3)].map((_, i) => (
            <LoadingSkeleton key={i} lines={3} />
          ))}
        </div>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="max-w-2xl mx-auto card-bg p-8 text-center rounded-lg">
        <p className="text-muted">User not found.</p>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto">
      <Card className="mb-8">
        <div className="text-center">
          <div className="flex justify-center mb-6">
            {user.profile ? (
              <img
                src={user.profile}
                alt={user.username}
                className="w-24 h-24 rounded-full object-cover border-4 border-gray-200 dark:border-white/20"
              />
            ) : (
              <div className="w-24 h-24 rounded-full bg-primary-1 flex items-center justify-center text-white text-3xl font-bold border-4 border-gray-200 dark:border-white/20">
                {user.username?.charAt(0).toUpperCase()}
              </div>
            )}
          </div>

          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
            {user.username}
          </h1>
          <p className="text-muted mt-2">{user.email}</p>

          {user.phone && (
            <p className="text-sm text-gray-600 dark:text-white/70 mt-2">
              {user.phone}
            </p>
          )}

          {user.bio && (
            <p className="text-gray-700 dark:text-white/80 mt-4 leading-relaxed max-w-lg mx-auto">
              {user.bio}
            </p>
          )}
        </div>
      </Card>

      <div>
        <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">
          Posts
        </h2>

        {postsLoading && !posts.length ? (
          <div className="space-y-4">
            {[...Array(3)].map((_, i) => (
              <LoadingSkeleton key={i} lines={3} />
            ))}
          </div>
        ) : posts.length > 0 ? (
          <div className="space-y-4 mb-8">
            {posts.map((post) => (
              <Post
                key={post.id}
                post={post}
                onComment={(postId) => setActivePostId(postId)}
              />
            ))}

            <div ref={loadMoreRef} className="h-1" />
          </div>
        ) : (
          <Card className="text-center py-8">
            <p className="text-muted">No posts yet.</p>
          </Card>
        )}
      </div>

      <PostDetailModal
        postId={activePostId ?? ""}
        isOpen={!!activePostId}
        onClose={() => setActivePostId(null)}
      />
    </div>
  );
}
