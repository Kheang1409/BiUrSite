import React from "react";

interface AvatarProps {
  initials: string;
  size?: "sm" | "md" | "lg" | "xl";
  className?: string;
  src?: string;
  alt?: string;
}

const sizeStyles = {
  sm: "w-8 h-8 text-xs",
  md: "w-10 h-10 text-sm",
  lg: "w-12 h-12 text-base",
  xl: "w-20 h-20 text-2xl",
};

export function Avatar({
  initials,
  size = "md",
  className = "",
  src,
  alt,
}: AvatarProps) {
  if (src) {
    return (
      <img
        src={src}
        alt={alt || initials}
        className={[
          sizeStyles[size],
          "rounded-full object-cover bg-gray-200 dark:bg-white/10 border border-gray-200 dark:border-white/10 shadow-sm",
          className,
        ]
          .filter(Boolean)
          .join(" ")}
      />
    );
  }

  return (
    <div
      className={[
        sizeStyles[size],
        "rounded-full bg-gradient-to-br from-primary-1 to-primary-2 border border-gray-200 dark:border-white/10 shadow-sm flex items-center justify-center font-bold text-white",
        className,
      ]
        .filter(Boolean)
        .join(" ")}
    >
      {initials.toUpperCase().slice(0, 2)}
    </div>
  );
}
