"use client";

import { useMutation, useQuery, ApolloCache, Reference } from "@apollo/client";
import {
  ADMIN_USERS_QUERY,
  ADMIN_POSTS_QUERY,
  BAN_USER_MUTATION,
  UNBAN_USER_MUTATION,
  ADMIN_DELETE_POST_MUTATION,
  ADMIN_DELETE_COMMENT_MUTATION,
} from "@/lib/graphql/queries";
import { User, Post } from "@/types";

// ─── Admin Users ────────────────────────────────────────────

export function useAdminUsers(pageNumber: number, skip = false) {
  const { data, loading, error, refetch } = useQuery(ADMIN_USERS_QUERY, {
    variables: { pageNumber },
    skip,
    // avoid writing paginated results into the shared cache for admin lists
    fetchPolicy: "no-cache",
  });

  return {
    users: (data?.adminUsers as User[]) || [],
    loading,
    error,
    refetch,
  };
}

// ─── Admin Posts (uses normal posts query) ──────────────────

export function useAdminPosts(pageNumber: number, skip = false) {
  const { data, loading, error, refetch } = useQuery(ADMIN_POSTS_QUERY, {
    variables: { pageNumber, keywords: null },
    skip,
    // fetch fresh page data and avoid cache merge artifacts
    fetchPolicy: "no-cache",
  });

  return {
    posts: (data?.posts as Post[]) || [],
    loading,
    error,
    refetch,
  };
}

// ─── Ban User ───────────────────────────────────────────────

export function useBanUser() {
  const [banUserMutation, { loading, error }] = useMutation(BAN_USER_MUTATION);

  const banUser = async (
    userId: string,
    reason?: string,
    durationMinutes?: number,
  ): Promise<boolean> => {
    const result = await banUserMutation({
      variables: {
        userId,
        reason: reason || null,
        durationMinutes: durationMinutes || null,
      },
      refetchQueries: ["AdminUsers"],
    });
    return result.data?.banUser ?? false;
  };

  return { banUser, loading, error };
}

// ─── Unban User ─────────────────────────────────────────────

export function useUnbanUser() {
  const [unbanUserMutation, { loading, error }] =
    useMutation(UNBAN_USER_MUTATION);

  const unbanUser = async (userId: string): Promise<boolean> => {
    const result = await unbanUserMutation({
      variables: { userId },
      refetchQueries: ["AdminUsers"],
    });
    return result.data?.unbanUser ?? false;
  };

  return { unbanUser, loading, error };
}

// ─── Admin Delete Post ──────────────────────────────────────

export function useAdminDeletePost() {
  const [adminDeletePostMutation, { loading, error }] = useMutation(
    ADMIN_DELETE_POST_MUTATION,
  );

  const adminDeletePost = async (
    postId: string,
    reason?: string,
  ): Promise<boolean> => {
    const result = await adminDeletePostMutation({
      variables: { postId, reason: reason || null },
      update: (cache: ApolloCache<unknown>) => {
        cache.modify({
          id: "ROOT_QUERY",
          fields: {
            posts(existingRefs, { readField }) {
              if (!Array.isArray(existingRefs)) return existingRefs;
              return existingRefs.filter(
                (ref) => readField("id", ref as Reference) !== postId,
              );
            },
          },
        });
        cache.gc();
      },
    });
    return result.data?.adminDeletePost ?? false;
  };

  return { adminDeletePost, loading, error };
}

// ─── Admin Delete Comment ───────────────────────────────────

export function useAdminDeleteComment() {
  const [adminDeleteCommentMutation, { loading, error }] = useMutation(
    ADMIN_DELETE_COMMENT_MUTATION,
  );

  const adminDeleteComment = async (
    postId: string,
    commentId: string,
    reason?: string,
  ): Promise<boolean> => {
    const result = await adminDeleteCommentMutation({
      variables: { postId, commentId, reason: reason || null },
      refetchQueries: ["Post", "Comments"],
    });
    return result.data?.adminDeleteComment ?? false;
  };

  return { adminDeleteComment, loading, error };
}
