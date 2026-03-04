"use client";

import { useState, useCallback } from "react";
import { AdminGuard } from "@/components/AdminGuard";
import { Card } from "@/components/ui/atoms/Card";
import { Button } from "@/components/ui/atoms/Button";
import { Badge } from "@/components/ui/atoms/Badge";
import { Avatar } from "@/components/ui/atoms/Avatar";
import { Input } from "@/components/ui/atoms/Input";
import {
  useAdminUsers,
  useAdminPosts,
  useBanUser,
  useUnbanUser,
  useAdminDeletePost,
} from "@/services/graphql/adminService";
import { User, Post } from "@/types";
import { Header } from "@/components/Header";

type AdminTab = "users" | "posts";

function BanModal({
  user,
  onClose,
  onBan,
  loading,
}: {
  user: User;
  onClose: () => void;
  onBan: (reason: string, durationMinutes?: number) => Promise<void>;
  loading: boolean;
}) {
  const [reason, setReason] = useState("");
  const [duration, setDuration] = useState("");

  const handleSubmit = async () => {
    const mins = duration ? parseInt(duration, 10) : undefined;
    await onBan(reason, mins && mins > 0 ? mins : undefined);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
      <Card className="w-full max-w-md mx-4 p-6">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Ban User: {user.username}
        </h3>
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-white/70 mb-1">
              Reason (optional)
            </label>
            <textarea
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder="Reason for banning..."
              rows={3}
              maxLength={500}
              className="w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-white/20 bg-white dark:bg-white/5 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-white/40 focus:outline-none focus:ring-2 focus:ring-primary-1/50 text-sm resize-none"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-white/70 mb-1">
              Duration in minutes (leave empty for permanent)
            </label>
            <Input
              type="number"
              min="1"
              value={duration}
              onChange={(e) => setDuration(e.target.value)}
              placeholder="e.g. 1440 for 24 hours"
              className="w-full"
            />
          </div>
        </div>
        <div className="flex justify-end gap-3 mt-6">
          <Button
            variant="ghost"
            size="sm"
            onClick={onClose}
            disabled={loading}
          >
            Cancel
          </Button>
          <Button
            variant="danger"
            size="sm"
            onClick={handleSubmit}
            isLoading={loading}
          >
            Ban User
          </Button>
        </div>
      </Card>
    </div>
  );
}

function DeletePostModal({
  post,
  onClose,
  onDelete,
  loading,
}: {
  post: Post;
  onClose: () => void;
  onDelete: (reason: string) => Promise<void>;
  loading: boolean;
}) {
  const [reason, setReason] = useState("");

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
      <Card className="w-full max-w-md mx-4 p-6">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
          Delete Post
        </h3>
        <p className="text-sm text-gray-500 dark:text-white/60 mb-4 line-clamp-2">
          &ldquo;{post.text}&rdquo; by {post.username}
        </p>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-white/70 mb-1">
            Reason (optional)
          </label>
          <textarea
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder="Reason for deletion..."
            rows={3}
            maxLength={500}
            className="w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-white/20 bg-white dark:bg-white/5 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-white/40 focus:outline-none focus:ring-2 focus:ring-primary-1/50 text-sm resize-none"
          />
        </div>
        <div className="flex justify-end gap-3 mt-6">
          <Button
            variant="ghost"
            size="sm"
            onClick={onClose}
            disabled={loading}
          >
            Cancel
          </Button>
          <Button
            variant="danger"
            size="sm"
            onClick={() => onDelete(reason)}
            isLoading={loading}
          >
            Delete Post
          </Button>
        </div>
      </Card>
    </div>
  );
}

function statusBadgeVariant(
  status?: string,
): "success" | "danger" | "muted" | "primary" {
  switch (status) {
    case "Active":
      return "success";
    case "Banned":
      return "danger";
    case "Deleted":
      return "muted";
    default:
      return "primary";
  }
}

function UsersPanel() {
  const [page, setPage] = useState(1);
  const { users, loading, error, refetch } = useAdminUsers(page);
  const { banUser, loading: banLoading } = useBanUser();
  const { unbanUser, loading: unbanLoading } = useUnbanUser();
  const [banTarget, setBanTarget] = useState<User | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  const showSuccess = useCallback((msg: string) => {
    setSuccessMsg(msg);
    setTimeout(() => setSuccessMsg(null), 3000);
  }, []);

  const handleBan = async (reason: string, durationMinutes?: number) => {
    if (!banTarget) return;
    try {
      await banUser(banTarget.id, reason, durationMinutes);
      setBanTarget(null);
      showSuccess(`${banTarget.username} has been banned.`);
      await refetch();
    } catch (err: any) {
      alert(err?.message || "Failed to ban user");
    }
  };

  const handleUnban = async (user: User) => {
    try {
      await unbanUser(user.id);
      showSuccess(`${user.username} has been unbanned.`);
      await refetch();
    } catch (err: any) {
      alert(err?.message || "Failed to unban user");
    }
  };

  if (error) {
    return (
      <Card className="p-6 text-center">
        <p className="text-danger-1">Failed to load users: {error.message}</p>
        <Button
          variant="ghost"
          size="sm"
          onClick={() => refetch()}
          className="mt-3"
        >
          Retry
        </Button>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
      {successMsg && (
        <div className="px-4 py-3 rounded-lg bg-success-1/10 border border-success-1/30 text-success-1 text-sm font-medium">
          {successMsg}
        </div>
      )}

      <Card className="overflow-hidden p-0">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5">
                <th className="text-left px-4 py-3 font-semibold text-gray-700 dark:text-white/70">
                  User
                </th>
                <th className="text-left px-4 py-3 font-semibold text-gray-700 dark:text-white/70">
                  Email
                </th>
                <th className="text-left px-4 py-3 font-semibold text-gray-700 dark:text-white/70">
                  Role
                </th>
                <th className="text-left px-4 py-3 font-semibold text-gray-700 dark:text-white/70">
                  Status
                </th>
                <th className="text-left px-4 py-3 font-semibold text-gray-700 dark:text-white/70">
                  Ban Info
                </th>
                <th className="text-right px-4 py-3 font-semibold text-gray-700 dark:text-white/70">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {loading && users.length === 0 ? (
                <tr>
                  <td
                    colSpan={6}
                    className="px-4 py-12 text-center text-gray-400 dark:text-white/40"
                  >
                    <div className="inline-block w-6 h-6 border-2 border-primary-1 border-t-transparent rounded-full animate-spin" />
                  </td>
                </tr>
              ) : users.length === 0 ? (
                <tr>
                  <td
                    colSpan={6}
                    className="px-4 py-12 text-center text-gray-400 dark:text-white/40"
                  >
                    No users found.
                  </td>
                </tr>
              ) : (
                users.map((user) => (
                  <tr
                    key={user.id}
                    className="border-b border-gray-100 dark:border-white/5 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors"
                  >
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-3">
                        <Avatar
                          initials={user.username?.slice(0, 2) || "??"}
                          size="sm"
                          src={user.profile || undefined}
                          alt={user.username}
                        />
                        <span className="font-medium text-gray-900 dark:text-white">
                          {user.username}
                        </span>
                      </div>
                    </td>
                    <td className="px-4 py-3 text-gray-600 dark:text-white/60">
                      {user.email}
                    </td>
                    <td className="px-4 py-3">
                      <Badge
                        label={user.role || "User"}
                        variant={user.role === "Admin" ? "primary" : "muted"}
                      />
                    </td>
                    <td className="px-4 py-3">
                      <Badge
                        label={user.status || "Active"}
                        variant={statusBadgeVariant(user.status)}
                      />
                    </td>
                    <td className="px-4 py-3 text-xs text-gray-500 dark:text-white/50">
                      {user.status === "Banned" ? (
                        <div>
                          {user.banReason && (
                            <p
                              className="truncate max-w-[200px]"
                              title={user.banReason}
                            >
                              {user.banReason}
                            </p>
                          )}
                          {user.banEndDate && (
                            <p className="text-gray-400 dark:text-white/40">
                              Until:{" "}
                              {new Date(user.banEndDate).toLocaleString()}
                            </p>
                          )}
                          {!user.banEndDate && user.status === "Banned" && (
                            <p className="text-danger-1">Permanent</p>
                          )}
                        </div>
                      ) : (
                        <span className="text-gray-300 dark:text-white/20">
                          &mdash;
                        </span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-right">
                      {user.role !== "Admin" && (
                        <div className="flex justify-end gap-2">
                          {user.status === "Banned" ? (
                            <Button
                              variant="secondary"
                              size="sm"
                              onClick={() => handleUnban(user)}
                              isLoading={unbanLoading}
                            >
                              Unban
                            </Button>
                          ) : user.status === "Active" ? (
                            <Button
                              variant="danger"
                              size="sm"
                              onClick={() => setBanTarget(user)}
                            >
                              Ban
                            </Button>
                          ) : null}
                        </div>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </Card>

      <div className="flex justify-center gap-3">
        <Button
          variant="ghost"
          size="sm"
          disabled={page <= 1 || loading}
          onClick={() => setPage((p) => Math.max(1, p - 1))}
        >
          Previous
        </Button>
        <span className="flex items-center text-sm text-gray-600 dark:text-white/60">
          Page {page}
        </span>
        <Button
          variant="ghost"
          size="sm"
          disabled={users.length < 10 || loading}
          onClick={() => setPage((p) => p + 1)}
        >
          Next
        </Button>
      </div>

      {banTarget && (
        <BanModal
          user={banTarget}
          onClose={() => setBanTarget(null)}
          onBan={handleBan}
          loading={banLoading}
        />
      )}
    </div>
  );
}

function PostsPanel() {
  const [page, setPage] = useState(1);
  const { posts, loading, error, refetch } = useAdminPosts(page);
  const { adminDeletePost, loading: deleteLoading } = useAdminDeletePost();
  const [deleteTarget, setDeleteTarget] = useState<Post | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  const showSuccess = useCallback((msg: string) => {
    setSuccessMsg(msg);
    setTimeout(() => setSuccessMsg(null), 3000);
  }, []);

  const handleDelete = async (reason: string) => {
    if (!deleteTarget) return;
    try {
      await adminDeletePost(deleteTarget.id, reason);
      setDeleteTarget(null);
      showSuccess("Post deleted successfully.");
      await refetch();
    } catch (err: any) {
      alert(err?.message || "Failed to delete post");
    }
  };

  if (error) {
    return (
      <Card className="p-6 text-center">
        <p className="text-danger-1">Failed to load posts: {error.message}</p>
        <Button
          variant="ghost"
          size="sm"
          onClick={() => refetch()}
          className="mt-3"
        >
          Retry
        </Button>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
      {successMsg && (
        <div className="px-4 py-3 rounded-lg bg-success-1/10 border border-success-1/30 text-success-1 text-sm font-medium">
          {successMsg}
        </div>
      )}

      {loading && posts.length === 0 ? (
        <Card className="p-12 text-center">
          <div className="inline-block w-6 h-6 border-2 border-primary-1 border-t-transparent rounded-full animate-spin" />
        </Card>
      ) : posts.length === 0 ? (
        <Card className="p-12 text-center text-gray-400 dark:text-white/40">
          No posts found.
        </Card>
      ) : (
        <div className="space-y-3">
          {posts.map((post) => (
            <Card key={post.id} className="p-4">
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 mb-1">
                    <Avatar
                      initials={post.username?.slice(0, 2) || "??"}
                      size="sm"
                      src={post.userProfile || undefined}
                      alt={post.username}
                    />
                    <span className="font-medium text-sm text-gray-900 dark:text-white">
                      {post.username}
                    </span>
                    <span className="text-xs text-gray-400 dark:text-white/40">
                      {new Date(post.createdDate).toLocaleDateString()}
                    </span>
                  </div>
                  <p className="text-sm text-gray-700 dark:text-white/80 line-clamp-3">
                    {post.text}
                  </p>
                  {post.imageUrl && (
                    <div className="mt-2">
                      <img
                        src={post.imageUrl}
                        alt="Post attachment"
                        className="w-24 h-24 rounded-lg object-cover"
                      />
                    </div>
                  )}
                </div>
                <Button
                  variant="danger"
                  size="sm"
                  onClick={() => setDeleteTarget(post)}
                >
                  Delete
                </Button>
              </div>
            </Card>
          ))}
        </div>
      )}

      <div className="flex justify-center gap-3">
        <Button
          variant="ghost"
          size="sm"
          disabled={page <= 1 || loading}
          onClick={() => setPage((p) => Math.max(1, p - 1))}
        >
          Previous
        </Button>
        <span className="flex items-center text-sm text-gray-600 dark:text-white/60">
          Page {page}
        </span>
        <Button
          variant="ghost"
          size="sm"
          disabled={posts.length < 10 || loading}
          onClick={() => setPage((p) => p + 1)}
        >
          Next
        </Button>
      </div>

      {deleteTarget && (
        <DeletePostModal
          post={deleteTarget}
          onClose={() => setDeleteTarget(null)}
          onDelete={handleDelete}
          loading={deleteLoading}
        />
      )}
    </div>
  );
}

export default function AdminPage() {
  const [activeTab, setActiveTab] = useState<AdminTab>("users");

  return (
    <>
      <Header />
      <AdminGuard>
        <main className="app-container py-6 sm:py-8">
          <div className="mb-6">
            <h1 className="text-2xl sm:text-3xl font-bold text-gray-900 dark:text-white">
              Admin Dashboard
            </h1>
            <p className="text-sm text-gray-500 dark:text-white/60 mt-1">
              Manage users and moderate content
            </p>
          </div>

          {/* Tab Navigation */}
          <div className="flex gap-1 mb-6 bg-gray-100 dark:bg-white/5 rounded-lg p-1 w-fit">
            <button
              onClick={() => setActiveTab("users")}
              className={[
                "px-4 py-2 rounded-md text-sm font-medium transition-all duration-200",
                activeTab === "users"
                  ? "bg-white dark:bg-primary-1 text-gray-900 dark:text-white shadow-sm"
                  : "text-gray-600 dark:text-white/60 hover:text-gray-900 dark:hover:text-white",
              ].join(" ")}
            >
              <span className="flex items-center gap-2">
                <svg
                  className="w-4 h-4"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                  strokeWidth={1.75}
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d="M15 19.128a9.38 9.38 0 002.625.372 9.337 9.337 0 004.121-.952 4.125 4.125 0 00-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128H9m6 0a5.97 5.97 0 00-.786-3.07M9 19.128v-.003c0-1.113.285-2.16.786-3.07m0 0A5.97 5.97 0 0112 13.5a5.97 5.97 0 012.214 2.558M9 19.128H3.375a4.125 4.125 0 017.533-2.493M15 7.5a3 3 0 11-6 0 3 3 0 016 0z"
                  />
                </svg>
                Users
              </span>
            </button>
            <button
              onClick={() => setActiveTab("posts")}
              className={[
                "px-4 py-2 rounded-md text-sm font-medium transition-all duration-200",
                activeTab === "posts"
                  ? "bg-white dark:bg-primary-1 text-gray-900 dark:text-white shadow-sm"
                  : "text-gray-600 dark:text-white/60 hover:text-gray-900 dark:hover:text-white",
              ].join(" ")}
            >
              <span className="flex items-center gap-2">
                <svg
                  className="w-4 h-4"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                  strokeWidth={1.75}
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d="M19.5 14.25v-2.625a3.375 3.375 0 00-3.375-3.375h-1.5A1.125 1.125 0 0113.5 7.125v-1.5a3.375 3.375 0 00-3.375-3.375H8.25m0 12.75h7.5m-7.5 3H12M10.5 2.25H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 00-9-9z"
                  />
                </svg>
                Posts
              </span>
            </button>
          </div>

          {/* Tab Content */}
          {activeTab === "users" && <UsersPanel />}
          {activeTab === "posts" && <PostsPanel />}
        </main>
      </AdminGuard>
    </>
  );
}
