"use client";

import Image from "next/image";

export function Logo({
  className = "w-40 h-8",
  title = "BiUrSite",
}: {
  className?: string;
  title?: string;
}) {
  return (
    <Image
      src="/favicon.svg"
      alt={title}
      width={48}
      height={48}
      className={className}
      role="img"
      aria-label={title}
    />
  );
}

export function LogoIcon({
  className = "w-8 h-8",
  title = "BiUrSite",
}: {
  className?: string;
  title?: string;
}) {
  return (
    <Image
      src="/favicon.svg"
      alt={title}
      width={48}
      height={48}
      className={className}
      role="img"
      aria-label={title}
    />
  );
}
