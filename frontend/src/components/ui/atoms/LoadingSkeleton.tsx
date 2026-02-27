import React from "react";

interface LoadingSkeletonProps {
  lines?: number;
  variant?: "text" | "card" | "avatar" | "image";
  className?: string;
}

export function LoadingSkeleton({
  lines = 3,
  variant = "text",
  className = "",
}: LoadingSkeletonProps) {
  if (variant === "avatar") {
    return <div className="w-10 h-10 rounded-full bg-white/10 animate-pulse" />;
  }

  if (variant === "image") {
    return (
      <div className="w-full h-64 rounded-[10px] bg-white/10 animate-pulse" />
    );
  }

  if (variant === "card") {
    return (
      <div className={["space-y-4", className].filter(Boolean).join(" ")}>
        <div className="h-4 bg-white/10 rounded w-3/4 animate-pulse" />
        <div className="h-4 bg-white/10 rounded w-full animate-pulse" />
        <div className="h-4 bg-white/10 rounded w-2/3 animate-pulse" />
      </div>
    );
  }

  return (
    <div className={["space-y-3", className].filter(Boolean).join(" ")}>
      {Array.from({ length: lines }).map((_, i) => (
        <div
          key={i}
          className={`h-4 bg-white/10 rounded animate-pulse ${
            i === lines - 1 ? "w-2/3" : "w-full"
          }`}
        />
      ))}
    </div>
  );
}
