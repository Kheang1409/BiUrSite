"use client";

import { User } from "@/types";
import { useRouter } from "next/navigation";
import { Card, Avatar } from "@/components/ui/atoms";

interface UserCardProps {
  user: User;
}

export function UserCard({ user }: UserCardProps) {
  const router = useRouter();

  return (
    <button
      onClick={() => router.push(`/profile/user/${user.id}`)}
      className="w-full text-left"
    >
      <Card
        hoverable
        className="text-center animate-scaleIn h-full hover:border-primary-1/50 transition-colors cursor-pointer"
      >
        <div className="flex justify-center mb-4">
          {user.profile ? (
            <img
              src={user.profile}
              alt={user.username}
              className="w-20 h-20 rounded-full object-cover border-2 border-gray-200 dark:border-white/20"
            />
          ) : (
            <Avatar initials={user.username} size="xl" />
          )}
        </div>

        <h3 className="font-semibold text-gray-900 dark:text-white text-lg">
          {user.username}
        </h3>
        <p className="text-xs text-muted mt-1 truncate">{user.email}</p>

        {user.bio && (
          <p className="text-sm text-gray-600 dark:text-white/70 mt-3 leading-relaxed line-clamp-3">
            {user.bio}
          </p>
        )}
      </Card>
    </button>
  );
}
