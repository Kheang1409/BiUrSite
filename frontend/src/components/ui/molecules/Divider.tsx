import React from "react";

interface DividerProps {
  className?: string;
}

export function Divider({ className = "" }: DividerProps) {
  return (
    <div
      className={["border-t border-gray-200 dark:border-white/10", className]
        .filter(Boolean)
        .join(" ")}
    />
  );
}
