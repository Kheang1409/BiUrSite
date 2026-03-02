"use client";

export function Logo({
  className = "w-40 h-8",
  title = "BiUrSite",
}: {
  className?: string;
  title?: string;
}) {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 48 48"
      role="img"
      aria-label={title}
      className={className}
    >
      <title>{title}</title>
      {/* Background - light in light mode, dark in dark mode */}
      <rect
        width="48"
        height="48"
        rx="8"
        className="fill-gray-100 dark:fill-[#0A1628]"
      />
      {/* Icon lines and circles */}
      <g
        transform="translate(4,4)"
        fill="none"
        className="stroke-primary-1"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      >
        <circle cx="20" cy="6" r="2.6" className="fill-primary-1" />
        <circle cx="6" cy="28" r="2.6" className="fill-primary-1" />
        <circle cx="34" cy="28" r="2.6" className="fill-primary-1" />
        <line x1="20" y1="8.6" x2="20" y2="20" />
        <line x1="9.7" y1="28" x2="15" y2="21.6" />
        <line x1="30.3" y1="28" x2="25" y2="21.6" />
      </g>
    </svg>
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
    <svg
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 48 48"
      role="img"
      aria-label={title}
      className={className}
    >
      <title>{title}</title>
      {/* Background - light in light mode, dark in dark mode */}
      <rect
        width="48"
        height="48"
        rx="8"
        className="fill-gray-100 dark:fill-[#0A1628]"
      />
      {/* Icon lines and circles */}
      <g
        transform="translate(4,4)"
        fill="none"
        className="stroke-primary-1"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      >
        <circle cx="20" cy="6" r="2.6" className="fill-primary-1" />
        <circle cx="6" cy="28" r="2.6" className="fill-primary-1" />
        <circle cx="34" cy="28" r="2.6" className="fill-primary-1" />
        <line x1="20" y1="8.6" x2="20" y2="20" />
        <line x1="9.7" y1="28" x2="15" y2="21.6" />
        <line x1="30.3" y1="28" x2="25" y2="21.6" />
      </g>
    </svg>
  );
}
