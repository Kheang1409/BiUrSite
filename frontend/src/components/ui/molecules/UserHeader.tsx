import React from "react";
import { Avatar } from "../atoms/Avatar";

interface UserHeaderProps {
  username: string;
  timestamp: string;
  timestampTitle?: string;
  avatarInitials?: string;
  avatarSrc?: string;
  avatarAlt?: string;
  subtext?: string;
  action?: React.ReactNode;
}

export function UserHeader({
  username,
  timestamp,
  timestampTitle,
  avatarInitials,
  avatarSrc,
  avatarAlt,
  subtext,
  action,
}: UserHeaderProps) {
  return (
    <div className="flex items-start justify-between gap-3">
      <div className="flex items-center gap-3 flex-1">
        {(avatarInitials || avatarSrc) && (
          <Avatar
            initials={(avatarInitials || username).slice(0, 2)}
            src={avatarSrc}
            alt={avatarAlt || username}
            size="md"
          />
        )}
        <div className="flex-1 min-w-0">
          <div className="flex flex-col gap-0.5">
            <p className="font-semibold text-white hover:text-primary-1 transition-colors cursor-pointer text-sm">
              {username}
            </p>
            {subtext && <p className="text-xs text-muted">{subtext}</p>}
            <span
              className="text-xs text-muted inline-block"
              title={timestampTitle || timestamp}
            >
              {timestamp}
            </span>
          </div>
        </div>
      </div>
      {action && (
        <div className="flex-shrink-0 flex items-center gap-1">{action}</div>
      )}
    </div>
  );
}
