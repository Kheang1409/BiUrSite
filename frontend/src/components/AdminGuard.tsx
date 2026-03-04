"use client";

import { ReactNode, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/hooks/useAuth";
import { useGraphQLAuth } from "@/hooks/useGraphQLAuth";

interface AdminGuardProps {
  children: ReactNode;
}

export function AdminGuard({ children }: AdminGuardProps) {
  const router = useRouter();
  const { isAuthenticated, isAdmin, isHydrating } = useAuth();
  const { currentUser, isLoading } = useGraphQLAuth();

  const loading = isHydrating || isLoading;

  useEffect(() => {
    if (!loading && !isAuthenticated) {
      router.replace("/login");
    }
  }, [isAuthenticated, loading, router]);

  useEffect(() => {
    if (
      !loading &&
      isAuthenticated &&
      currentUser &&
      currentUser.role !== "Admin" &&
      !isAdmin
    ) {
      router.replace("/");
    }
  }, [isAuthenticated, loading, currentUser, isAdmin, router]);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="inline-block w-8 h-8 border-4 border-primary-1 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  if (!isAuthenticated || (!isAdmin && currentUser?.role !== "Admin")) {
    return null;
  }

  return <>{children}</>;
}
