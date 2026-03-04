import React, { ReactNode } from "react";

interface CardProps {
  children: ReactNode;
  className?: string;
  variant?: "default" | "elevated" | "flat";
  hoverable?: boolean;
}

const variantStyles = {
  default:
    "bg-white dark:bg-card backdrop-blur-sm border border-gray-200 dark:border-white/10 shadow-sm dark:shadow-none",
  elevated:
    "bg-white dark:bg-white/5 backdrop-blur-md border border-gray-200 dark:border-white/20 shadow-lg",
  flat: "bg-gray-50 dark:bg-white/5 backdrop-blur-sm",
};

export function Card({
  children,
  className = "",
  variant = "default",
  hoverable = false,
}: CardProps) {
  return (
    <div
      className={[
        "rounded-[10px] p-3 transition-all duration-200",
        variantStyles[variant],
        hoverable &&
          "hover:shadow-lg hover:border-primary-1/30 cursor-pointer transform hover:-translate-y-0.5",
        className,
      ]
        .filter(Boolean)
        .join(" ")}
    >
      {children}
    </div>
  );
}
