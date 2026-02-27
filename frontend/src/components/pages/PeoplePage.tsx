"use client";

import { useQuery } from "@apollo/client";
import { USERS_QUERY } from "@/lib/graphql/queries";
import { User } from "@/types";
import { UserCard } from "@/components/UserCard";
import { useAuth } from "@/hooks/useAuth";
import { useState, useMemo } from "react";

export function PeoplePage() {
  const { userId: currentUserId } = useAuth();
  const [pageNumber, setPageNumber] = useState(1);

  const { data, loading, error } = useQuery(USERS_QUERY, {
    variables: { pageNumber },
    fetchPolicy: "cache-and-network",
  });

  const users: User[] = data?.users || [];

  // Filter out current user
  const filteredUsers = useMemo(() => {
    if (!currentUserId) return users;
    return users.filter(
      (user) =>
        user.id.trim().toLowerCase() !== currentUserId.trim().toLowerCase(),
    );
  }, [users, currentUserId]);

  if (error) {
    return (
      <div className="card-bg p-4 text-center text-danger-1">
        <p>Failed to load people. Please try again.</p>
      </div>
    );
  }

  return (
    <div>
      <h1 className="text-3xl font-bold text-white mb-8">Discover People</h1>

      {loading && !users.length ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {[...Array(6)].map((_, i) => (
            <div key={i} className="card-bg p-4 animate-pulse">
              <div className="w-20 h-20 bg-white/10 rounded-full mx-auto mb-4"></div>
              <div className="h-4 bg-white/10 rounded w-3/4 mx-auto mb-2"></div>
              <div className="h-3 bg-white/10 rounded w-1/2 mx-auto"></div>
            </div>
          ))}
        </div>
      ) : users.length > 0 ? (
        <div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
            {filteredUsers.map((user) => (
              <UserCard key={user.id} user={user} />
            ))}
          </div>

          <div className="flex gap-4 justify-center">
            <button
              onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
              disabled={pageNumber === 1}
              className="btn-secondary disabled:opacity-50"
            >
              Previous
            </button>
            <span className="px-4 py-2 text-white">Page {pageNumber}</span>
            <button
              onClick={() => setPageNumber((p) => p + 1)}
              disabled={filteredUsers.length < 10}
              className="btn-secondary disabled:opacity-50"
            >
              Next
            </button>
          </div>
        </div>
      ) : (
        <div className="card-bg p-8 text-center text-muted">
          <p>No people found.</p>
        </div>
      )}
    </div>
  );
}
