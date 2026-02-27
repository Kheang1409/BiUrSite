import React from "react";

interface DividerProps {
  className?: string;
}

export function Divider({ className = "" }: DividerProps) {
  return (
    <div
      className={["border-t border-white/10", className]
        .filter(Boolean)
        .join(" ")}
    />
  );
}
