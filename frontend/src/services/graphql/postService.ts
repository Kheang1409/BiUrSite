"use client";

import { useMutation, useQuery, ApolloCache, Reference } from "@apollo/client";
import {
  POSTS_QUERY,
  MY_POSTS_QUERY,
  POST_DETAIL_QUERY,
  CREATE_POST_MUTATION,
  EDIT_POST_MUTATION,
  DELETE_POST_MUTATION,
  CREATE_COMMENT_MUTATION,
  EDIT_COMMENT_MUTATION,
  DELETE_COMMENT_MUTATION,
} from "@/lib/graphql/queries";
import { Post, PostDetail, Comment, CreatePostInput } from "@/types";

export interface UsePostsOptions {
  keywords?: string | null;
  pageNumber: number;
  skip?: boolean;
}

export interface UsePostsResult {
  posts: Post[];
  loading: boolean;
  error: Error | undefined;
  refetch: () => Promise<unknown>;
}

export interface UsePostDetailResult {
  post: PostDetail | undefined;
  comments: Comment[];
  loading: boolean;
  error: Error | undefined;
  refetch: () => Promise<unknown>;
}

export interface EditPostInput {
  id: string;
  text: string;
  data?: number[] | null;
  removeImage?: boolean;
}

export function usePosts({
  keywords,
  pageNumber,
  skip = false,
}: UsePostsOptions): UsePostsResult {
  const { data, loading, error, refetch } = useQuery(POSTS_QUERY, {
    variables: {
      pageNumber,
      keywords: keywords || null,
    },
    skip,
    fetchPolicy: "cache-and-network",
    notifyOnNetworkStatusChange: true,
  });

  return {
    posts: (data?.posts as Post[]) || [],
    loading,
    error,
    refetch,
  };
}

export function useMyPosts(pageNumber: number, skip = false): UsePostsResult {
  const { data, loading, error, refetch } = useQuery(MY_POSTS_QUERY, {
    variables: { pageNumber },
    skip,
    fetchPolicy: "cache-and-network",
    notifyOnNetworkStatusChange: true,
  });

  return {
    posts: (data?.myPosts as Post[]) || [],
    loading,
    error,
    refetch,
  };
}

export function usePostDetail(
  postId: string,
  skip = false,
): UsePostDetailResult {
  const { data, loading, error, refetch } = useQuery(POST_DETAIL_QUERY, {
    variables: { id: postId },
    skip,
    fetchPolicy: "cache-and-network",
  });

  const post = data?.post as PostDetail | undefined;

  return {
    post,
    comments: post?.comments || [],
    loading,
    error,
    refetch,
  };
}

export function useCreatePost() {
  const [createPost, { loading, error }] = useMutation(CREATE_POST_MUTATION);

  const create = async (input: CreatePostInput): Promise<Post | undefined> => {
    const result = await createPost({
      variables: {
        text: input.text,
        data: input.data,
      },
    });
    return result.data?.createPost as Post | undefined;
  };

  return { createPost: create, loading, error };
}

export function useEditPost() {
  const [editPost, { loading, error }] = useMutation(EDIT_POST_MUTATION);

  const edit = async (input: EditPostInput): Promise<boolean> => {
    const result = await editPost({
      variables: {
        id: input.id,
        text: input.text,
        data: input.data,
        removeImage: input.removeImage ?? false,
      },
      refetchQueries: ["Post", "Posts", "MyPosts"],
    });
    return result.data?.editPost ?? false;
  };

  return { editPost: edit, loading, error };
}

export function useDeletePost() {
  const [deletePost, { loading, error }] = useMutation(DELETE_POST_MUTATION);

  const remove = async (postId: string): Promise<boolean> => {
    const result = await deletePost({
      variables: { id: postId },
      optimisticResponse: { deletePost: true },
      update: (cache: ApolloCache<unknown>) => {
        updateCacheAfterPostDelete(cache, postId);
      },
    });
    return result.data?.deletePost ?? false;
  };

  return { deletePost: remove, loading, error };
}

export function updateCacheAfterPostDelete(
  cache: ApolloCache<unknown>,
  postId: string,
): void {
  cache.modify({
    id: "ROOT_QUERY",
    fields: {
      myPosts(existingRefs, { readField }) {
        if (!Array.isArray(existingRefs)) return existingRefs;
        return existingRefs.filter(
          (ref) => readField("id", ref as Reference) !== postId,
        );
      },
      posts(existingRefs, { readField }) {
        if (!Array.isArray(existingRefs)) return existingRefs;
        return existingRefs.filter(
          (ref) => readField("id", ref as Reference) !== postId,
        );
      },
    },
  });

  // Evict post from cache
  const postDtoId = cache.identify({ __typename: "PostDto", id: postId });
  if (postDtoId) cache.evict({ id: postDtoId });

  const postDetailDtoId = cache.identify({
    __typename: "PostDetailDto",
    id: postId,
  });
  if (postDetailDtoId) cache.evict({ id: postDetailDtoId });

  cache.gc();
}

export function useCreateComment() {
  const [createComment, { loading, error }] = useMutation(
    CREATE_COMMENT_MUTATION,
  );

  const create = async (
    postId: string,
    text: string,
  ): Promise<Comment | undefined> => {
    const result = await createComment({
      variables: { postId, text },
    });
    return result.data?.createComment as Comment | undefined;
  };

  return { createComment: create, loading, error };
}

export function useEditComment() {
  const [editComment, { loading, error }] = useMutation(EDIT_COMMENT_MUTATION);

  const edit = async (id: string, text: string): Promise<boolean> => {
    const result = await editComment({
      variables: { id, text },
    });
    return result.data?.editComment ?? false;
  };

  return { editComment: edit, loading, error };
}

export function useDeleteComment() {
  const [deleteComment, { loading, error }] = useMutation(
    DELETE_COMMENT_MUTATION,
  );

  const remove = async (id: string): Promise<boolean> => {
    const result = await deleteComment({
      variables: { id },
    });
    return result.data?.deleteComment ?? false;
  };

  return { deleteComment: remove, loading, error };
}
