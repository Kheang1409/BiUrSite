"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuth } from "@/hooks/useAuth";
import { useGraphQLAuth } from "@/hooks/useGraphQLAuth";
import { useState, useEffect, useRef, useCallback } from "react";
import { Button, Input } from "@/components/ui/atoms";
import { LogoIcon } from "@/components/Logo";
import { useQuery, useMutation, useLazyQuery } from "@apollo/client";
import {
  NOTIFICATIONS_QUERY,
  MARK_NOTIFICATIONS_AS_READ,
} from "@/lib/graphql/queries";
import { useNotificationHub } from "@/hooks/useNotificationHub";
import NotificationList from "@/components/ui/molecules/NotificationList";

export function Header() {
  const router = useRouter();
  const { isAuthenticated, logout } = useAuth();
  const { currentUser, refetchCurrentUser } = useGraphQLAuth();
  const [hasUnread, setHasUnread] = useState<boolean>(false);
  const [isMounted, setIsMounted] = useState(false);
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [isMobile, setIsMobile] = useState(false);
  const [isTablet, setIsTablet] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const [isSearchFocused, setIsSearchFocused] = useState(false);
  const [searchResults, setSearchResults] = useState<any[]>([]);
  const searchRef = useRef<HTMLDivElement>(null);
  const [notifications, setNotifications] = useState<any[]>([]);
  const [showNotifications, setShowNotifications] = useState(false);
  const [notificationsPage, setNotificationsPage] = useState(1);
  const [hasMoreNotifications, setHasMoreNotifications] = useState(true);
  const [isLoadingMoreNotifications, setIsLoadingMoreNotifications] =
    useState(false);
  const notifRef = useRef<HTMLDivElement>(null);

  const { data: notifData, refetch: refetchNotifications } = useQuery(
    NOTIFICATIONS_QUERY,
    {
      variables: { pageNumber: 1 },
      skip: !isAuthenticated,
      fetchPolicy: "network-only",
      onCompleted: (data) => {
        if (data?.notifications) {
          setNotifications(data.notifications);
          setHasMoreNotifications(data.notifications.length >= 10);
          setNotificationsPage(1);
        }
      },
    },
  );

  const [fetchMoreNotifications] = useLazyQuery(NOTIFICATIONS_QUERY, {
    fetchPolicy: "network-only",
    onCompleted: (data) => {
      if (data?.notifications) {
        setNotifications((prev) => {
          const existingIds = new Set(prev.map((n) => n.id));
          const newNotifications = data.notifications.filter(
            (n: any) => !existingIds.has(n.id),
          );
          return [...prev, ...newNotifications];
        });
        setHasMoreNotifications(data.notifications.length >= 10);
      } else {
        setHasMoreNotifications(false);
      }
      setIsLoadingMoreNotifications(false);
    },
  });

  const loadMoreNotifications = useCallback(async () => {
    if (isLoadingMoreNotifications || !hasMoreNotifications) return;
    setIsLoadingMoreNotifications(true);
    const nextPage = notificationsPage + 1;
    setNotificationsPage(nextPage);
    await fetchMoreNotifications({ variables: { pageNumber: nextPage } });
  }, [
    isLoadingMoreNotifications,
    hasMoreNotifications,
    notificationsPage,
    fetchMoreNotifications,
  ]);

  const [markNotificationsAsRead] = useMutation(MARK_NOTIFICATIONS_AS_READ);

  // Start SignalR and handle incoming notifications
  useNotificationHub((payload) => {
    setNotifications((prev) => [payload, ...(prev || [])]);
    // mark local red-dot unread state when a real-time notification arrives
    setHasUnread(true);
  });

  useEffect(() => {
    const handleClickOutsideNotif = (event: MouseEvent) => {
      if (
        notifRef.current &&
        !notifRef.current.contains(event.target as Node)
      ) {
        setShowNotifications(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutsideNotif);
    return () =>
      document.removeEventListener("mousedown", handleClickOutsideNotif);
  }, []);

  useEffect(() => {
    setIsMounted(true);
  }, []);

  useEffect(() => {
    const handleResize = () => {
      const width = window.innerWidth;
      setIsMobile(width < 768);
      setIsTablet(width >= 768 && width < 1024);
    };

    handleResize();
    window.addEventListener("resize", handleResize);
    return () => window.removeEventListener("resize", handleResize);
  }, []);

  // Handle search results
  useEffect(() => {
    if (searchQuery.trim().length > 0) {
      // Simulate search results - in real app, this would query your API
      const mockResults = [
        {
          id: 1,
          type: "post",
          title: `Post containing "${searchQuery}"`,
          snippet: "This is a sample post...",
        },
        {
          id: 2,
          type: "user",
          title: `User: ${searchQuery}`,
          snippet: "User profile...",
        },
        {
          id: 3,
          type: "post",
          title: `Another post about "${searchQuery}"`,
          snippet: "Related content...",
        },
      ];
      setSearchResults(mockResults.slice(0, 5));
    } else {
      setSearchResults([]);
    }
  }, [searchQuery]);

  // Close search results when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        searchRef.current &&
        !searchRef.current.contains(event.target as Node)
      ) {
        setIsSearchFocused(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleLogout = () => {
    logout();
    router.push("/login");
    setIsMenuOpen(false);
  };

  const handleNavClick = () => {
    if (isMobile) {
      setIsMenuOpen(false);
      setIsSearchFocused(false);
    }
  };

  const navLinkStyles =
    "px-4 py-2 text-white hover:text-primary-1 hover:bg-white/5 rounded-lg transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-primary-1/50";
  const mobileNavLinkStyles =
    "block w-full text-left px-3 py-2 text-white hover:bg-white/10 rounded-lg transition-colors";

  const showAuthenticatedNav = isMounted && isAuthenticated;

  return (
    <header className="sticky top-0 z-50 bg-primary-3/95 backdrop-blur-md border-b border-white/10 shadow-lg">
      <div className={`app-container ${isMobile ? "py-2" : "py-3"}`}>
        <div className="flex items-center justify-between gap-2 md:gap-4">
          <Link
            href="/"
            className="flex items-center gap-1 sm:gap-2 hover:opacity-80 transition-opacity focus:outline-none focus:ring-2 focus:ring-primary-1/50 rounded-lg px-1 sm:px-2 py-1 flex-shrink-0 group"
          >
            <div className="p-1 sm:p-1.5 rounded-lg bg-gradient-to-br from-primary-1 to-primary-2 group-hover:shadow-lg group-hover:shadow-primary-1/30 transition-shadow">
              <LogoIcon className="w-5 h-5 sm:w-6 sm:h-6 md:w-7 md:h-7 text-white" />
            </div>
            <span className="hidden xs:block sm:block text-base sm:text-lg md:text-xl font-bold bg-gradient-to-r from-primary-1 to-primary-2 bg-clip-text text-transparent">
              BiUrSite
            </span>
          </Link>

          {!isMobile && (
            <div
              ref={searchRef}
              className={`relative hidden sm:block ${
                isTablet ? "max-w-xs" : "flex-1 max-w-lg"
              }`}
            >
              <div className="relative">
                <Input
                  type="text"
                  placeholder="Search..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  onFocus={() => setIsSearchFocused(true)}
                  className="w-full pl-10 py-2 text-sm md:text-base"
                />
                <svg
                  className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-white/40 pointer-events-none"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                  />
                </svg>
              </div>

              {isSearchFocused && searchResults.length > 0 && (
                <div className="absolute top-full left-0 right-0 mt-2 bg-primary-2 rounded-lg border border-white/10 shadow-lg max-h-80 overflow-y-auto z-10">
                  {searchResults.map((result, idx) => (
                    <button
                      key={idx}
                      onClick={() => {
                        setSearchQuery("");
                        setIsSearchFocused(false);
                      }}
                      className="w-full text-left px-4 py-3 hover:bg-white/10 border-b border-white/5 last:border-b-0 transition-colors text-sm"
                    >
                      <div className="flex items-start gap-2">
                        <span className="text-xs font-semibold text-primary-1 mt-0.5 px-2 py-0.5 bg-primary-1/10 rounded whitespace-nowrap">
                          {result.type}
                        </span>
                        <div className="flex-1 min-w-0">
                          <div className="text-sm font-medium text-white">
                            {result.title}
                          </div>
                          <div className="text-xs text-white/60 truncate">
                            {result.snippet}
                          </div>
                        </div>
                      </div>
                    </button>
                  ))}
                </div>
              )}
            </div>
          )}

          <nav className="hidden md:flex items-center gap-1">
            {showAuthenticatedNav ? (
              <>
                <Link href="/" className={navLinkStyles}>
                  <span className="flex items-center gap-1">
                    <svg
                      className="w-4 h-4"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M3 12l2-3m0 0l7-4 7 4M5 9v10a1 1 0 001 1h12a1 1 0 001-1V9m-9 11l4-4m0 0l4 4m-4-4V5"
                      />
                    </svg>
                    Feed
                  </span>
                </Link>
                <Link href="/people" className={navLinkStyles}>
                  <span className="flex items-center gap-1">
                    <svg
                      className="w-4 h-4"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M12 4.354a4 4 0 110 8.646 4 4 0 010-8.646M12 12H8m4 0h4m-8 4h8a2 2 0 012 2v2H10v-2a2 2 0 012-2z"
                      />
                    </svg>
                    People
                  </span>
                </Link>
                <Link href="/profile" className={navLinkStyles}>
                  <span className="flex items-center gap-1">
                    <svg
                      className="w-4 h-4"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
                      />
                    </svg>
                    Profile
                  </span>
                </Link>
                <div className="flex items-center gap-2">
                  <div className="relative" ref={notifRef}>
                    <button
                      onClick={async () => {
                        // Use functional update to avoid stale closure over `showNotifications`.
                        let willOpen = false;
                        setShowNotifications((prev) => {
                          willOpen = !prev;
                          return willOpen;
                        });

                        if (willOpen) {
                          // opening -> mark as read then refresh list
                          try {
                            await markNotificationsAsRead();
                            // clear local unread indicator and refresh current user
                            setHasUnread(false);
                            try {
                              await refetchCurrentUser?.();
                            } catch {}
                          } catch (e: any) {
                            // log GraphQL errors for debugging the 400
                            // eslint-disable-next-line no-console
                            console.error(
                              "markNotificationsAsRead error:",
                              e?.graphQLErrors || e?.networkError || e,
                            );

                            // If server indicates an auth issue, force logout to recover
                            const graphQLErrors = e?.graphQLErrors as
                              | any[]
                              | undefined;
                            const networkError = e?.networkError as
                              | any
                              | undefined;
                            const isUnauthorized =
                              graphQLErrors?.some?.((ge) =>
                                (ge?.extensions?.code || ge?.message || "")
                                  .toString()
                                  .toLowerCase()
                                  .includes("unauthorized"),
                              ) ||
                              String(
                                networkError?.result?.errors?.[0]?.message ||
                                  networkError?.statusCode ||
                                  "",
                              )
                                .toLowerCase()
                                .includes("invalid user id") ||
                              String(e?.message || "")
                                .toLowerCase()
                                .includes("invalid user id");

                            if (isUnauthorized) {
                              try {
                                logout();
                                router.push("/login");
                              } catch {}
                              return;
                            }
                          }

                          try {
                            await refetchNotifications();
                            // also refresh current user to sync hasNewNotification
                            try {
                              await refetchCurrentUser?.();
                            } catch {}
                          } catch (re: any) {
                            // eslint-disable-next-line no-console
                            console.error(
                              "refetchNotifications error:",
                              re?.graphQLErrors || re?.networkError || re,
                            );

                            const graphQLErrors = re?.graphQLErrors as
                              | any[]
                              | undefined;
                            const networkError = re?.networkError as
                              | any
                              | undefined;
                            const isUnauthorized =
                              graphQLErrors?.some?.((ge) =>
                                (ge?.extensions?.code || ge?.message || "")
                                  .toString()
                                  .toLowerCase()
                                  .includes("unauthorized"),
                              ) ||
                              String(
                                networkError?.result?.errors?.[0]?.message ||
                                  networkError?.statusCode ||
                                  "",
                              )
                                .toLowerCase()
                                .includes("invalid user id") ||
                              String(re?.message || "")
                                .toLowerCase()
                                .includes("invalid user id");

                            if (isUnauthorized) {
                              try {
                                logout();
                                router.push("/login");
                              } catch {}
                            }
                          }
                        }
                      }}
                      className="p-2 hover:bg-white/5 rounded-lg transition-colors focus:outline-none"
                      aria-label="Notifications"
                    >
                      <svg
                        className="w-5 h-5 text-white"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          strokeWidth={2}
                          d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6 6 0 10-12 0v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
                        />
                      </svg>
                      {(hasUnread || currentUser?.hasNewNotification) && (
                        <span className="absolute top-0 right-0 block w-2 h-2 bg-danger-1 rounded-full translate-x-1 -translate-y-1" />
                      )}
                    </button>

                    {showNotifications && (
                      <>
                        {/* Improved notification list UI */}
                        <NotificationList
                          notifications={notifications}
                          onLoadMore={loadMoreNotifications}
                          hasMore={hasMoreNotifications}
                          isLoadingMore={isLoadingMoreNotifications}
                          onMarkAllRead={async () => {
                            try {
                              await markNotificationsAsRead();
                              setHasUnread(false);
                              try {
                                await refetchCurrentUser?.();
                              } catch {}
                              try {
                                await refetchNotifications();
                              } catch {}
                            } catch (e) {
                              // swallow here, header already handles auth errors elsewhere
                            }
                          }}
                          onView={(n) => {
                            setShowNotifications(false);
                            if (!n?.postId) return;
                            const commentId =
                              n.relatedEntityId ?? n.relatedEntityId ?? n.id;
                            const target = `/post/${n.postId}${commentId ? `#comment-${commentId}` : ""}`;
                            try {
                              router.push(target);
                            } catch {}
                          }}
                        />
                      </>
                    )}
                  </div>
                  <Button
                    variant="danger"
                    size="sm"
                    onClick={handleLogout}
                    className="ml-2"
                  >
                    Logout
                  </Button>
                </div>
              </>
            ) : (
              <>
                <Link href="/login" className={navLinkStyles}>
                  Login
                </Link>
                <Link href="/register" className={navLinkStyles}>
                  <span className="px-6 py-2 bg-primary-1 hover:bg-primary-2 text-white rounded-[10px] font-semibold transition-colors inline-block">
                    Sign Up
                  </span>
                </Link>
              </>
            )}
          </nav>

          <button
            className="md:hidden p-1.5 hover:bg-white/10 rounded-lg transition-colors text-white focus:outline-none focus:ring-2 focus:ring-primary-1/50"
            onClick={() => setIsMenuOpen(!isMenuOpen)}
            aria-label="Toggle menu"
            aria-expanded={isMenuOpen}
          >
            <svg
              className={`w-5 h-5 sm:w-6 sm:h-6 transition-transform duration-200 ${
                isMenuOpen ? "rotate-90" : ""
              }`}
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d={
                  isMenuOpen
                    ? "M6 18L18 6M6 6l12 12"
                    : "M4 6h16M4 12h16M4 18h16"
                }
              />
            </svg>
          </button>
        </div>

        {isMobile && isMenuOpen && (
          <div ref={searchRef} className="mt-2 relative">
            <div className="relative">
              <Input
                type="text"
                placeholder="Search posts..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onFocus={() => setIsSearchFocused(true)}
                className="w-full pl-9 py-2 text-sm"
              />
              <svg
                className="absolute left-2.5 top-1/2 -translate-y-1/2 w-4 h-4 text-white/40 pointer-events-none"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                />
              </svg>
            </div>

            {isSearchFocused && searchResults.length > 0 && (
              <div className="absolute top-full left-0 right-0 mt-1 bg-primary-2 rounded-lg border border-white/10 shadow-lg max-h-56 overflow-y-auto z-10">
                {searchResults.map((result, idx) => (
                  <button
                    key={idx}
                    onClick={() => {
                      setSearchQuery("");
                      setIsSearchFocused(false);
                    }}
                    className="w-full text-left px-3 py-2 hover:bg-white/10 border-b border-white/5 last:border-b-0 transition-colors text-sm"
                  >
                    <div className="text-sm font-medium text-white">
                      {result.title}
                    </div>
                    <div className="text-xs text-white/60 truncate">
                      {result.snippet}
                    </div>
                  </button>
                ))}
              </div>
            )}
          </div>
        )}

        {isMenuOpen && (
          <nav className="md:hidden mt-2 pb-2 border-t border-white/10 pt-2 space-y-0.5 animate-slideIn">
            {showAuthenticatedNav ? (
              <>
                <Link
                  href="/"
                  className={`${mobileNavLinkStyles} text-sm`}
                  onClick={handleNavClick}
                >
                  <span className="flex items-center gap-2">
                    <svg
                      className="w-4 h-4"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M3 12l2-3m0 0l7-4 7 4M5 9v10a1 1 0 001 1h12a1 1 0 001-1V9m-9 11l4-4m0 0l4 4m-4-4V5"
                      />
                    </svg>
                    Feed
                  </span>
                </Link>
                <Link
                  href="/people"
                  className={`${mobileNavLinkStyles} text-sm`}
                  onClick={handleNavClick}
                >
                  <span className="flex items-center gap-2">
                    <svg
                      className="w-4 h-4"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M12 4.354a4 4 0 110 8.646 4 4 0 010-8.646M12 12H8m4 0h4m-8 4h8a2 2 0 012 2v2H10v-2a2 2 0 012-2z"
                      />
                    </svg>
                    People
                  </span>
                </Link>
                <Link
                  href="/profile"
                  className={`${mobileNavLinkStyles} text-sm`}
                  onClick={handleNavClick}
                >
                  <span className="flex items-center gap-2">
                    <svg
                      className="w-4 h-4"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
                      />
                    </svg>
                    Profile
                  </span>
                </Link>
                <button
                  onClick={handleLogout}
                  className={`${mobileNavLinkStyles} bg-danger-1/20 hover:bg-danger-1/30 text-danger-1 text-sm flex items-center gap-2`}
                >
                  <svg
                    className="w-4 h-4"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"
                    />
                  </svg>
                  Logout
                </button>
              </>
            ) : (
              <>
                <Link
                  href="/login"
                  className={`${mobileNavLinkStyles} text-sm`}
                  onClick={handleNavClick}
                >
                  Login
                </Link>
                <Link
                  href="/register"
                  className={`${mobileNavLinkStyles} bg-primary-1/20 hover:bg-primary-1/30 text-primary-1 font-semibold text-sm`}
                  onClick={handleNavClick}
                >
                  Sign Up
                </Link>
              </>
            )}
          </nav>
        )}
      </div>
    </header>
  );
}
