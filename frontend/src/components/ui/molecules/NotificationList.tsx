"use client";

import React, { useState, useRef, useCallback, useEffect } from "react";
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";

dayjs.extend(relativeTime);

import { Notification } from "@/types";

const INITIAL_COUNT = 5;
const LOAD_MORE_COUNT = 5;

interface Props {
  notifications: Notification[];
  onMarkAllRead?: () => Promise<void> | void;
  onView?: (n: Notification) => void;
  onLoadMore?: () => Promise<void> | void;
  hasMore?: boolean;
  isLoadingMore?: boolean;
}

export default function NotificationList({
  notifications,
  onMarkAllRead,
  onView,
  onLoadMore,
  hasMore = false,
  isLoadingMore = false,
}: Props) {
  const [visibleCount, setVisibleCount] = useState(INITIAL_COUNT);
  const scrollContainerRef = useRef<HTMLDivElement>(null);
  const loadMoreTriggerRef = useRef<HTMLDivElement>(null);

  const visibleNotifications = notifications.slice(0, visibleCount);
  const hasLocalMore = visibleCount < notifications.length;
  const showSeeMore = hasLocalMore && visibleCount === INITIAL_COUNT;

  const handleSeeMore = () => {
    setVisibleCount((prev) => prev + LOAD_MORE_COUNT);
  };

  // Infinite scroll on reaching bottom
  const handleScroll = useCallback(() => {
    const container = scrollContainerRef.current;
    if (!container) return;

    const { scrollTop, scrollHeight, clientHeight } = container;
    const isNearBottom = scrollHeight - scrollTop - clientHeight < 40;

    if (isNearBottom && !isLoadingMore) {
      // First expand local visible count
      if (visibleCount < notifications.length) {
        setVisibleCount((prev) =>
          Math.min(prev + LOAD_MORE_COUNT, notifications.length),
        );
      } else if (hasMore && onLoadMore) {
        // Then fetch more from server
        void onLoadMore();
      }
    }
  }, [visibleCount, notifications.length, hasMore, onLoadMore, isLoadingMore]);

  useEffect(() => {
    const container = scrollContainerRef.current;
    if (!container) return;
    container.addEventListener("scroll", handleScroll);
    return () => container.removeEventListener("scroll", handleScroll);
  }, [handleScroll]);

  return (
    <div className="absolute right-0 mt-2 w-80 bg-primary-2 rounded-lg border border-white/10 shadow-xl z-50 overflow-hidden">
      <div className="flex items-center justify-between p-3 border-b border-white/5">
        <div className="text-sm font-semibold text-white">Notifications</div>
        <div className="flex items-center gap-2">
          {onMarkAllRead && (
            <button
              onClick={() => void onMarkAllRead()}
              className="text-xs text-white/70 hover:text-white transition-colors"
            >
              Mark all read
            </button>
          )}
        </div>
      </div>

      <div
        ref={scrollContainerRef}
        className="max-h-[400px] overflow-y-auto bg-gradient-to-b from-primary-2/95 to-primary-2/90 scrollbar-thin scrollbar-thumb-white/10 scrollbar-track-transparent"
      >
        {visibleNotifications.length > 0 ? (
          <>
            {visibleNotifications.map((n) => (
              <button
                key={n.id}
                onClick={() => onView?.(n)}
                className="w-full text-left p-3 hover:bg-white/5 transition-colors flex gap-3 items-start border-b border-white/5"
              >
                <div className="flex-shrink-0">
                  <div className="w-9 h-9 rounded-full bg-white/5 overflow-hidden flex items-center justify-center">
                    {n.userProfile ? (
                      // eslint-disable-next-line @next/next/no-img-element
                      <img
                        src={n.userProfile}
                        alt={n.username}
                        className="w-full h-full object-cover"
                      />
                    ) : (
                      <span className="text-xs text-white/60">
                        {n.username?.[0] || "U"}
                      </span>
                    )}
                  </div>
                </div>

                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between gap-2">
                    <div className="text-sm font-medium text-white truncate">
                      <span className="font-semibold mr-1">{n.username}</span>
                      <span className="text-white/75">{n.title}</span>
                    </div>
                    <div className="text-xs text-white/50 whitespace-nowrap">
                      {dayjs(n.createdDate).fromNow()}
                    </div>
                  </div>
                  <div className="text-sm text-white/70 mt-1 line-clamp-2">
                    {n.message}
                  </div>
                </div>
              </button>
            ))}

            {/* See previous notifications button */}
            {showSeeMore && (
              <button
                onClick={handleSeeMore}
                className="w-full py-3 text-center text-sm text-primary-1 hover:bg-white/5 transition-colors font-medium"
              >
                See previous notifications
              </button>
            )}

            {/* Loading indicator for infinite scroll */}
            {(isLoadingMore || (hasLocalMore && !showSeeMore)) && (
              <div
                ref={loadMoreTriggerRef}
                className="py-3 text-center text-xs text-white/50"
              >
                {isLoadingMore ? (
                  <span className="inline-flex items-center gap-2">
                    <svg
                      className="animate-spin h-4 w-4"
                      viewBox="0 0 24 24"
                      fill="none"
                    >
                      <circle
                        className="opacity-25"
                        cx="12"
                        cy="12"
                        r="10"
                        stroke="currentColor"
                        strokeWidth="4"
                      />
                      <path
                        className="opacity-75"
                        fill="currentColor"
                        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                      />
                    </svg>
                    Loading...
                  </span>
                ) : hasMore ? (
                  "Scroll for more"
                ) : null}
              </div>
            )}

            {/* End of notifications */}
            {!hasMore && !hasLocalMore && visibleCount > INITIAL_COUNT && (
              <div className="py-2 text-center text-xs text-white/40">
                You&apos;re all caught up
              </div>
            )}
          </>
        ) : (
          <div className="p-4 text-sm text-white/60">No notifications</div>
        )}
      </div>
    </div>
  );
}
