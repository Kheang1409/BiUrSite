import React from "react";

interface BadgeProps {
  label: string;
  variant?: "primary" | "secondary" | "danger" | "success" | "muted";
  size?: "sm" | "md";
  className?: string;
}

const variantStyles = {
  primary: "bg-primary-1/20 text-primary-1 border border-primary-1/30",
  secondary: "bg-secondary-1/20 text-secondary-1 border border-secondary-1/30",
  danger: "bg-danger-1/20 text-danger-1 border border-danger-1/30",
  success: "bg-success-1/20 text-success-1 border border-success-1/30",
  muted: "bg-muted/20 text-muted border border-muted/30",
};

const sizeStyles = {
  sm: "px-2 py-1 text-xs",
  md: "px-3 py-1.5 text-sm",
};

export function Badge({
  label,
  variant = "primary",
  size = "sm",
  className = "",
}: BadgeProps) {
  return (
    <span
      className={[
        "inline-flex items-center gap-1 rounded-full font-medium whitespace-nowrap",
        variantStyles[variant],
        sizeStyles[size],
        className,
      ]
        .filter(Boolean)
        .join(" ")}
    >
      {label}
    </span>
  );
}
