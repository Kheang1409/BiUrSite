"use client";

import { gql, useMutation, useQuery } from "@apollo/client";
import {
  DELETE_POST_MUTATION,
  EDIT_POST_MUTATION,
  ME_QUERY,
  MY_POSTS_QUERY,
  UPDATE_ME_MUTATION,
} from "@/lib/graphql/queries";
import { User, Post as PostType } from "@/types";
import { Post } from "@/components/Post";
import { PostDetailModal } from "@/components/modals/PostDetailModal";
import { ConfirmDialog } from "@/components/modals/ConfirmDialog";
import { useAuth } from "@/hooks/useAuth";
import { useEffect, useRef, useState } from "react";
import { ApiErrorHandler, FileUtils } from "@/utils/helpers";

export function ProfilePage() {
  const { isAuthenticated } = useAuth();
  const PAGE_SIZE = 10;
  const [pageNumber, setPageNumber] = useState(1);
  const [activePostId, setActivePostId] = useState<string | null>(null);
  const [isEditingProfile, setIsEditingProfile] = useState(false);
  const [deletePostId, setDeletePostId] = useState<string | null>(null);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  const [hasMore, setHasMore] = useState(true);
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const loadMoreRef = useRef<HTMLDivElement | null>(null);
  const prevServerCountRef = useRef(0);
  const loadingMoreRef = useRef(false);

  const [username, setUsername] = useState("");
  const [bio, setBio] = useState("");
  const [phone, setPhone] = useState("");
  const [profileBytes, setProfileBytes] = useState<number[] | null>(null);
  const [profilePreviewUrl, setProfilePreviewUrl] = useState<string | null>(
    null,
  );
  const [removeProfileImage, setRemoveProfileImage] = useState(false);
  const [isSavingProfile, setIsSavingProfile] = useState(false);
  const profileFileInputRef = useRef<HTMLInputElement | null>(null);

  const {
    data: userData,
    loading: userLoading,
    refetch: refetchMe,
  } = useQuery(ME_QUERY, {
    skip: !isAuthenticated,
  });

  const {
    data: postsData,
    loading: postsLoading,
    refetch: refetchMyPosts,
  } = useQuery(MY_POSTS_QUERY, {
    variables: { pageNumber },
    skip: !isAuthenticated,
    fetchPolicy: "cache-and-network",
    notifyOnNetworkStatusChange: true,
  });

  const [editPost] = useMutation(EDIT_POST_MUTATION);
  const [deletePost] = useMutation(DELETE_POST_MUTATION);
  const [updateMe] = useMutation(UPDATE_ME_MUTATION);

  const user: User | undefined = userData?.me;
  const posts: PostType[] = postsData?.myPosts || [];

  useEffect(() => {
    // Initial page: infer whether there might be more
    if (pageNumber === 1 && !postsLoading) {
      setHasMore(posts.length >= PAGE_SIZE);
    }
  }, [PAGE_SIZE, pageNumber, posts.length, postsLoading]);

  useEffect(() => {
    // If we requested more and the query finished, decide if more pages exist.
    if (!isLoadingMore) return;
    if (postsLoading) return;

    const delta = posts.length - prevServerCountRef.current;
    setHasMore(delta >= PAGE_SIZE);
    setIsLoadingMore(false);
    loadingMoreRef.current = false;
  }, [PAGE_SIZE, isLoadingMore, posts.length, postsLoading]);

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

  useEffect(() => {
    if (!user) return;
    setUsername(user.username ?? "");
    setBio(user.bio ?? "");
    setPhone(user.phone ?? "");
  }, [user]);

  useEffect(() => {
    return () => {
      if (profilePreviewUrl) URL.revokeObjectURL(profilePreviewUrl);
    };
  }, [profilePreviewUrl]);

  if (!isAuthenticated) {
    return (
      <div className="card-bg p-8 text-center">
        <p className="text-muted">Please log in to view your profile.</p>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto">
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
                cache.modify({
                  id: "ROOT_QUERY",
                  fields: {
                    myPosts(existingRefs: readonly any[] = [], { readField }) {
                      return existingRefs.filter(
                        (ref) => readField("id", ref) !== id,
                      );
                    },
                    posts(existingRefs: readonly any[] = [], { readField }) {
                      return existingRefs.filter(
                        (ref) => readField("id", ref) !== id,
                      );
                    },
                  },
                });

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
            if (activePostId === id) setActivePostId(null);
            setIsDeleteOpen(false);
            setDeletePostId(null);
            await refetchMyPosts();
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
        onDeleted={() => {
          // Keep as a safety net in case pagination/window cache hides the change
          void refetchMyPosts();
        }}
      />
      {user && (
        <div className="card-bg p-8 mb-6">
          <div className="flex items-start gap-6">
            <div className="w-24 h-24 rounded-full overflow-hidden bg-gray-200 dark:bg-white/10 flex items-center justify-center flex-shrink-0 group relative">
              {profilePreviewUrl || user.profile ? (
                <img
                  src={profilePreviewUrl ?? user.profile}
                  alt={`${user.username} profile`}
                  className="w-full h-full object-cover"
                />
              ) : (
                <span className="text-4xl font-bold text-primary-1">
                  {user.username[0].toUpperCase()}
                </span>
              )}

              {isEditingProfile && (
                <>
                  <input
                    ref={profileFileInputRef}
                    type="file"
                    accept="image/*"
                    className="hidden"
                    onChange={async (e) => {
                      const file = e.target.files?.[0];
                      if (!file) return;
                      const bytes = await FileUtils.fileToByteArray(file);
                      if (profilePreviewUrl)
                        URL.revokeObjectURL(profilePreviewUrl);
                      setProfilePreviewUrl(URL.createObjectURL(file));
                      setProfileBytes(bytes);
                      setRemoveProfileImage(false);
                    }}
                  />

                  <div className="absolute inset-0 bg-black/20 opacity-0 group-hover:opacity-100 transition-opacity flex items-end justify-center pb-2">
                    <div className="flex items-center gap-3">
                      <button
                        type="button"
                        onClick={() => profileFileInputRef.current?.click()}
                        className="w-9 h-9 rounded-full bg-black/30 hover:bg-black/50 dark:bg-white/10 dark:hover:bg-white/20 flex items-center justify-center text-white transition-colors"
                        aria-label="Change photo"
                        title="Change photo"
                      >
                        <svg
                          xmlns="http://www.w3.org/2000/svg"
                          className="w-4 h-4"
                          viewBox="0 0 24 24"
                          fill="none"
                          stroke="currentColor"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M3 7h4l2-3h6l2 3h4v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V7z"
                          />
                          <circle cx="12" cy="13" r="3" />
                        </svg>
                      </button>

                      <button
                        type="button"
                        className="w-7 h-7 rounded-full bg-danger-1/20 hover:bg-danger-1/30 flex items-center justify-center text-danger-1 transition-colors"
                        aria-label="Remove photo"
                        title="Remove photo"
                        onClick={() => {
                          if (profilePreviewUrl)
                            URL.revokeObjectURL(profilePreviewUrl);
                          setProfilePreviewUrl(null);
                          setProfileBytes(null);
                          setRemoveProfileImage(true);
                        }}
                      >
                        <svg
                          xmlns="http://www.w3.org/2000/svg"
                          className="w-3.5 h-3.5"
                          viewBox="0 0 24 24"
                          fill="none"
                          stroke="currentColor"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                          />
                        </svg>
                      </button>
                    </div>
                  </div>
                </>
              )}
            </div>

            <div className="flex-1">
              <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
                {user.username}
              </h1>
              <p className="text-muted mt-2">{user.email}</p>
              {user.bio && (
                <p className="text-gray-600 dark:text-white/70 mt-3">
                  {user.bio}
                </p>
              )}
              {user.phone ? (
                <p className="text-gray-600 dark:text-white/70 mt-1">
                  {user.phone}
                </p>
              ) : (
                <p className="text-sm text-muted mt-1">No phone number yet</p>
              )}
              <p className="text-sm text-muted mt-4">{posts.length} posts</p>

              <div className="mt-4 flex gap-2">
                <button
                  className="btn-secondary"
                  onClick={() => setIsEditingProfile((v) => !v)}
                >
                  {isEditingProfile ? "Close" : "Edit profile"}
                </button>
              </div>
            </div>
          </div>

          {isEditingProfile && (
            <div className="mt-6 space-y-4">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <label className="text-sm text-muted">
                  Username
                  <input
                    className="mt-1 w-full rounded-lg bg-gray-100 dark:bg-white/5 border border-gray-200 dark:border-white/10 px-3 py-2 text-gray-900 dark:text-white"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                  />
                </label>

                <label className="text-sm text-muted">
                  Phone {(!user.phone || user.phone.trim() === "") && "(add)"}
                  <input
                    className="mt-1 w-full rounded-lg bg-gray-100 dark:bg-white/5 border border-gray-200 dark:border-white/10 px-3 py-2 text-gray-900 dark:text-white"
                    value={phone}
                    onChange={(e) => setPhone(e.target.value)}
                    placeholder="e.g. +1 555 123 4567"
                  />
                </label>
              </div>

              <label className="text-sm text-muted">
                Bio
                <textarea
                  className="mt-1 w-full rounded-lg bg-gray-100 dark:bg-white/5 border border-gray-200 dark:border-white/10 px-3 py-2 text-gray-900 dark:text-white"
                  value={bio}
                  onChange={(e) => setBio(e.target.value)}
                  rows={3}
                />
              </label>

              <div className="space-y-2">
                {removeProfileImage && (
                  <div className="flex items-center justify-between rounded-lg border border-gray-200 dark:border-white/10 bg-gray-100 dark:bg-white/5 px-3 py-2">
                    <p className="text-xs text-gray-600 dark:text-white/80">
                      Photo will be reset to default.
                    </p>
                    <button
                      type="button"
                      className="text-xs text-primary-1 hover:underline"
                      onClick={() => setRemoveProfileImage(false)}
                    >
                      Undo
                    </button>
                  </div>
                )}
              </div>

              <div className="flex justify-end gap-2">
                <button
                  className="btn-secondary"
                  onClick={() => {
                    setIsEditingProfile(false);
                    setUsername(user.username ?? "");
                    setBio(user.bio ?? "");
                    setPhone(user.phone ?? "");
                    if (profilePreviewUrl)
                      URL.revokeObjectURL(profilePreviewUrl);
                    setProfilePreviewUrl(null);
                    setProfileBytes(null);
                    setRemoveProfileImage(false);
                  }}
                  disabled={isSavingProfile}
                >
                  Cancel
                </button>
                <button
                  className="btn-primary disabled:opacity-60"
                  disabled={isSavingProfile || !username.trim()}
                  onClick={async () => {
                    try {
                      setIsSavingProfile(true);
                      await updateMe({
                        variables: {
                          username: username.trim(),
                          bio,
                          phone: phone.trim() ? phone.trim() : null,
                          data: profileBytes ?? null,
                          removeImage: removeProfileImage,
                        },
                      });
                      await refetchMe();
                      setIsEditingProfile(false);
                      if (profilePreviewUrl)
                        URL.revokeObjectURL(profilePreviewUrl);
                      setProfilePreviewUrl(null);
                      setProfileBytes(null);
                      setRemoveProfileImage(false);
                    } catch (e: any) {
                      alert(ApiErrorHandler.handleGraphQLError(e));
                    } finally {
                      setIsSavingProfile(false);
                    }
                  }}
                >
                  {isSavingProfile ? "Saving..." : "Save"}
                </button>
              </div>
            </div>
          )}
        </div>
      )}

      <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">
        My Posts
      </h2>

      {postsLoading ? (
        <div className="card-bg p-8 text-center">
          <div className="animate-pulse">
            <div className="h-4 bg-gray-200 dark:bg-white/10 rounded w-3/4 mx-auto mb-4"></div>
          </div>
        </div>
      ) : posts.length > 0 ? (
        <div>
          {posts.map((post) => (
            <Post
              key={post.id}
              post={post}
              isOwner={true}
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
                      cache.modify({
                        id: "ROOT_QUERY",
                        fields: {
                          myPosts(
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
                                  fragment _MyPostOptimistic on PostDto {
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
              onDelete={async (postId) => {
                setDeletePostId(postId);
                setIsDeleteOpen(true);
              }}
              onComment={(id) => setActivePostId(id)}
            />
          ))}

          <div ref={loadMoreRef} className="h-1" />

          {(isLoadingMore || (postsLoading && posts.length > 0)) && (
            <div className="text-center py-6">
              <div className="inline-block">
                <div className="animate-spin w-5 h-5 border-2 border-primary-1 border-t-transparent rounded-full"></div>
              </div>
            </div>
          )}

          {!hasMore && posts.length > 0 && (
            <div className="text-center py-8 text-sm text-muted">
              You’re all caught up.
            </div>
          )}
        </div>
      ) : (
        <div className="card-bg p-8 text-center text-muted">
          <p>No posts yet. Create one to get started!</p>
        </div>
      )}
    </div>
  );
}
