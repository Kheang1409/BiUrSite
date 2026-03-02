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
import { PostDetailModal } from "@/components/modals/PostDetailModal";
import { ThemeToggle } from "@/components/ThemeToggle";

export function Header() {
  const router = useRouter();
  const { isAuthenticated, logout } = useAuth();
  const { currentUser, refetchCurrentUser } = useGraphQLAuth();
  const [hasUnread, setHasUnread] = useState<boolean>(false);
  const [isMounted, setIsMounted] = useState(false);
  const [isMenuOpen, setIsMenuOpen] = useState(false);
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
  const [selectedPostId, setSelectedPostId] = useState<string | null>(null);

  const { data: notifData, refetch: refetchNotifications } = useQuery(
    NOTIFICATIONS_QUERY,
    {
      variables: { pageNumber: 1 },
      skip: !isAuthenticated,
      fetchPolicy: "network-only",
    },
  );

  useEffect(() => {
    if (notifData?.notifications) {
      setNotifications(notifData.notifications);
      setHasMoreNotifications(notifData.notifications.length >= 10);
      setNotificationsPage(1);
    }
  }, [notifData]);

  const [fetchMoreNotifications] = useLazyQuery(NOTIFICATIONS_QUERY, {
    fetchPolicy: "network-only",
  });

  const loadMoreNotifications = useCallback(async () => {
    if (isLoadingMoreNotifications || !hasMoreNotifications) return;
    setIsLoadingMoreNotifications(true);
    const nextPage = notificationsPage + 1;
    setNotificationsPage(nextPage);
    try {
      const res: any = await fetchMoreNotifications({
        variables: { pageNumber: nextPage },
      });
      const data = res?.data;
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
    } finally {
      setIsLoadingMoreNotifications(false);
    }
  }, [
    isLoadingMoreNotifications,
    hasMoreNotifications,
    notificationsPage,
    fetchMoreNotifications,
  ]);

  const [markNotificationsAsRead] = useMutation(MARK_NOTIFICATIONS_AS_READ);

  useNotificationHub((payload) => {
    setNotifications((prev) => [payload, ...(prev || [])]);
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
    if (searchQuery.trim().length > 0) {
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
    if (typeof window !== "undefined" && window.innerWidth < 768) {
      setIsMenuOpen(false);
      setIsSearchFocused(false);
    }
  };

  const navLinkStyles =
    "px-3 py-2 text-gray-700 dark:text-white hover:text-primary-1 hover:bg-gray-100 dark:hover:bg-white/5 rounded-lg transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-primary-1/50";
  const mobileNavLinkStyles =
    "block w-full text-left px-3 py-2 text-gray-700 dark:text-white hover:bg-gray-100 dark:hover:bg-white/10 rounded-lg transition-colors";

  const showAuthenticatedNav = isMounted && isAuthenticated;

  return (
    <header className="sticky top-0 z-50 bg-white/95 dark:bg-primary-3/95 backdrop-blur-md border-b border-gray-200 dark:border-white/10 shadow-sm dark:shadow-lg transition-colors duration-200">
      <div className={`app-container py-2 sm:py-3`}>
        <div className="flex items-center justify-between gap-2 md:gap-4">
          <Link
            href="/"
            className="flex items-center gap-1 sm:gap-2 hover:opacity-80 transition-opacity focus:outline-none focus:ring-2 focus:ring-primary-1/50 rounded-lg px-1 sm:px-2 py-1 flex-shrink-0 group"
          >
            <div className="p-1 sm:p-1.5 rounded-lg transition-shadow">
              <LogoIcon className="w-5 h-5 sm:w-6 sm:h-6 md:w-7 md:h-7 text-primary-1" />
            </div>
            <span className="hidden xs:block sm:block text-base sm:text-lg md:text-xl font-bold bg-gradient-to-r from-primary-1 to-primary-2 bg-clip-text text-transparent">
              BiUrSite
            </span>
          </Link>

          <div
            ref={searchRef}
            className="relative hidden sm:block sm:max-w-xs md:flex-1 md:max-w-lg"
          >
            <div className="relative">
              <Input
                type="text"
                placeholder="Search..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onFocus={() => setIsSearchFocused(true)}
                className="w-full pl-10 py-2 text-sm md:text-base border border-gray-300 dark:border-white/20"
              />
              <svg
                className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 dark:text-white/40 pointer-events-none"
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
              <div className="absolute top-full left-0 right-0 mt-2 bg-white dark:bg-primary-2 rounded-lg border border-gray-200 dark:border-white/10 shadow-lg max-h-80 overflow-y-auto z-10">
                {searchResults.map((result, idx) => (
                  <button
                    key={idx}
                    onClick={() => {
                      setSearchQuery("");
                      setIsSearchFocused(false);
                    }}
                    className="w-full text-left px-4 py-3 hover:bg-gray-100 dark:hover:bg-white/10 border-b border-gray-100 dark:border-white/5 last:border-b-0 transition-colors text-sm"
                  >
                    <div className="flex items-start gap-2">
                      <span className="text-xs font-semibold text-primary-1 mt-0.5 px-2 py-0.5 bg-primary-1/10 rounded whitespace-nowrap">
                        {result.type}
                      </span>
                      <div className="flex-1 min-w-0">
                        <div className="text-sm font-medium text-gray-900 dark:text-white">
                          {result.title}
                        </div>
                        <div className="text-xs text-gray-500 dark:text-white/60 truncate">
                          {result.snippet}
                        </div>
                      </div>
                    </div>
                  </button>
                ))}
              </div>
            )}
          </div>

          <nav className="hidden md:flex items-center gap-1">
            {showAuthenticatedNav ? (
              <>
                <Link href="/" className={navLinkStyles} title="Feed">
                  <span className="flex items-center gap-1.5">
                    <svg
                      className="w-5 h-5"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                      strokeWidth={1.75}
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"
                      />
                    </svg>
                  </span>
                </Link>
                <Link href="/people" className={navLinkStyles} title="People">
                  <span className="flex items-center gap-1.5">
                    <svg
                      className="w-5 h-5"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                      strokeWidth={1.75}
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
                      />
                    </svg>
                  </span>
                </Link>
                <Link href="/profile" className={navLinkStyles} title="Profile">
                  <span className="flex items-center gap-1.5">
                    <svg
                      className="w-5 h-5"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                      strokeWidth={1.75}
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M5.121 17.804A13.937 13.937 0 0112 16c2.5 0 4.847.655 6.879 1.804M15 10a3 3 0 11-6 0 3 3 0 016 0zm6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                      />
                    </svg>
                  </span>
                </Link>
                <div className="flex items-center gap-2">
                  <div className="relative" ref={notifRef}>
                    <button
                      onClick={async () => {
                        let willOpen = false;
                        setShowNotifications((prev) => {
                          willOpen = !prev;
                          return willOpen;
                        });

                        if (willOpen) {
                          try {
                            await markNotificationsAsRead();
                            setHasUnread(false);
                            try {
                              await refetchCurrentUser?.();
                            } catch {}
                          } catch (e: any) {
                            console.error(
                              "markNotificationsAsRead error:",
                              e?.graphQLErrors || e?.networkError || e,
                            );

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
                            try {
                              await refetchCurrentUser?.();
                            } catch {}
                          } catch (re: any) {
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
                      className="p-2 hover:bg-gray-100 dark:hover:bg-white/5 rounded-lg transition-colors focus:outline-none relative"
                      aria-label="Notifications"
                      title="Notifications"
                    >
                      <svg
                        className="w-5 h-5 text-gray-700 dark:text-white"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth={1.75}
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          d="M14.857 17.082a23.848 23.848 0 005.454-1.31A8.967 8.967 0 0118 9.75v-.7V9A6 6 0 006 9v.75a8.967 8.967 0 01-2.312 6.022c1.733.64 3.56 1.085 5.455 1.31m5.714 0a24.255 24.255 0 01-5.714 0m5.714 0a3 3 0 11-5.714 0"
                        />
                      </svg>
                      {(hasUnread || currentUser?.hasNewNotification) && (
                        <span className="absolute top-0 right-0 block w-2 h-2 bg-danger-1 rounded-full translate-x-1 -translate-y-1" />
                      )}
                    </button>

                    {showNotifications && (
                      <>
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
                            } catch (e) {}
                          }}
                          onView={(n) => {
                            setShowNotifications(false);
                            if (!n?.postId) return;
                            setSelectedPostId(n.postId);
                          }}
                        />
                      </>
                    )}
                  </div>
                  <ThemeToggle />
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
                <ThemeToggle />
                <Link
                  href="/login"
                  className="px-4 py-2 text-gray-700 dark:text-white hover:text-primary-1 dark:hover:text-primary-1 font-medium rounded-lg border border-gray-300 dark:border-white/20 hover:border-primary-1 dark:hover:border-primary-1 transition-all duration-200"
                >
                  Login
                </Link>
                <Link
                  href="/register"
                  className="px-5 py-2 bg-primary-1 hover:bg-primary-2 text-white rounded-lg font-semibold transition-all duration-200"
                >
                  Sign Up
                </Link>
              </>
            )}
          </nav>

          <button
            className="md:hidden p-1.5 hover:bg-gray-100 dark:hover:bg-white/10 rounded-lg transition-colors text-gray-700 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-1/50"
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

        {isMenuOpen && (
          <div className="md:hidden mt-2 relative">
            <div className="relative">
              <Input
                type="text"
                placeholder="Search posts..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onFocus={() => setIsSearchFocused(true)}
                className="w-full pl-9 py-2 text-sm border border-gray-300 dark:border-white/20"
              />
              <svg
                className="absolute left-2.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 dark:text-white/40 pointer-events-none"
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
              <div className="absolute top-full left-0 right-0 mt-1 bg-white dark:bg-primary-2 rounded-lg border border-gray-200 dark:border-white/10 shadow-lg max-h-56 overflow-y-auto z-10">
                {searchResults.map((result, idx) => (
                  <button
                    key={idx}
                    onClick={() => {
                      setSearchQuery("");
                      setIsSearchFocused(false);
                    }}
                    className="w-full text-left px-3 py-2 hover:bg-gray-100 dark:hover:bg-white/10 border-b border-gray-100 dark:border-white/5 last:border-b-0 transition-colors text-sm"
                  >
                    <div className="text-sm font-medium text-gray-900 dark:text-white">
                      {result.title}
                    </div>
                    <div className="text-xs text-gray-500 dark:text-white/60 truncate">
                      {result.snippet}
                    </div>
                  </button>
                ))}
              </div>
            )}
          </div>
        )}

        {isMenuOpen && (
          <nav className="md:hidden mt-2 pb-2 border-t border-gray-200 dark:border-white/10 pt-2 space-y-0.5 animate-slideIn">
            {showAuthenticatedNav ? (
              <>
                <Link
                  href="/"
                  className={`${mobileNavLinkStyles} text-sm`}
                  onClick={handleNavClick}
                >
                  <span className="flex items-center gap-2">
                    <svg
                      className="w-5 h-5"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                      strokeWidth={1.75}
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"
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
                      className="w-5 h-5"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                      strokeWidth={1.75}
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
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
                      className="w-5 h-5"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                      strokeWidth={1.75}
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M5.121 17.804A13.937 13.937 0 0112 16c2.5 0 4.847.655 6.879 1.804M15 10a3 3 0 11-6 0 3 3 0 016 0zm6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                      />
                    </svg>
                    Profile
                  </span>
                </Link>
                <div className="flex items-center justify-between px-3 py-2 bg-gray-100 dark:bg-white/5 rounded-lg">
                  <span className="text-sm text-gray-600 dark:text-white/60">
                    Theme
                  </span>
                  <ThemeToggle />
                </div>
                <button
                  onClick={handleLogout}
                  className={`${mobileNavLinkStyles} bg-danger-1/10 hover:bg-danger-1/20 text-danger-1 text-sm flex items-center gap-2`}
                >
                  <svg
                    className="w-5 h-5"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                    strokeWidth={1.75}
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
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
                  className={`${mobileNavLinkStyles} text-sm border border-gray-300 dark:border-white/20`}
                  onClick={handleNavClick}
                >
                  Login
                </Link>
                <Link
                  href="/register"
                  className="block w-full text-center px-3 py-2.5 bg-primary-1 hover:bg-primary-2 text-white font-semibold text-sm rounded-lg transition-all"
                  onClick={handleNavClick}
                >
                  Sign Up
                </Link>
                <div className="flex items-center justify-between px-3 py-2 mt-1 bg-gray-100 dark:bg-white/5 rounded-lg">
                  <span className="text-sm text-gray-600 dark:text-white/60">
                    Theme
                  </span>
                  <ThemeToggle />
                </div>
              </>
            )}
          </nav>
        )}
      </div>

      {selectedPostId && (
        <PostDetailModal
          postId={selectedPostId}
          isOpen={!!selectedPostId}
          onClose={() => setSelectedPostId(null)}
        />
      )}
    </header>
  );
}
