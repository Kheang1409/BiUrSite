export function LogoIcon({ className = "w-8 h-8" }) {
  return (
    <svg
      viewBox="0 0 48 48"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className={className}
    >
      <circle
        cx="24"
        cy="24"
        r="23"
        fill="#0682A5"
        opacity="0.1"
        stroke="#0682A5"
        strokeWidth="0.5"
      />

      <defs>
        <linearGradient id="logoGradient" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stopColor="#0682A5" />
          <stop offset="100%" stopColor="#45B0D1" />
        </linearGradient>
      </defs>

      <circle cx="24" cy="10" r="3" fill="url(#logoGradient)" />

      <circle cx="14" cy="24" r="3" fill="url(#logoGradient)" />

      <circle cx="34" cy="24" r="3" fill="url(#logoGradient)" />

      <circle cx="17" cy="37" r="3" fill="url(#logoGradient)" />

      <circle cx="31" cy="37" r="3" fill="url(#logoGradient)" />

      <circle cx="24" cy="26" r="4" fill="url(#logoGradient)" />

      <line
        x1="24"
        y1="12"
        x2="24"
        y2="22"
        stroke="url(#logoGradient)"
        strokeWidth="1.5"
        opacity="0.6"
      />
      <line
        x1="22"
        y1="24"
        x2="14"
        y2="24"
        stroke="url(#logoGradient)"
        strokeWidth="1.5"
        opacity="0.6"
      />
      <line
        x1="26"
        y1="24"
        x2="34"
        y2="24"
        stroke="url(#logoGradient)"
        strokeWidth="1.5"
        opacity="0.6"
      />
      <line
        x1="24"
        y1="30"
        x2="19"
        y2="34"
        stroke="url(#logoGradient)"
        strokeWidth="1.5"
        opacity="0.6"
      />
      <line
        x1="24"
        y1="30"
        x2="29"
        y2="34"
        stroke="url(#logoGradient)"
        strokeWidth="1.5"
        opacity="0.6"
      />

      <path d="M 32 14 Q 36 12 38 16 L 36 18 Z" fill="#45B0D1" opacity="0.4" />
    </svg>
  );
}

export function LogoTextIcon({ className = "w-32 h-8" }) {
  return (
    <svg
      viewBox="0 0 200 60"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className={className}
    >
      <defs>
        <linearGradient id="textGradient" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stopColor="#0682A5" />
          <stop offset="100%" stopColor="#45B0D1" />
        </linearGradient>
      </defs>

      <text
        x="10"
        y="42"
        fontSize="36"
        fontWeight="700"
        fill="url(#textGradient)"
        fontFamily="system-ui, -apple-system, sans-serif"
        letterSpacing="-0.5"
      >
        BiUrSite
      </text>

      <text
        x="10"
        y="55"
        fontSize="10"
        fontWeight="500"
        fill="#45B0D1"
        opacity="0.8"
        fontFamily="system-ui, -apple-system, sans-serif"
        letterSpacing="1"
      >
        SOCIAL NETWORK
      </text>
    </svg>
  );
}
