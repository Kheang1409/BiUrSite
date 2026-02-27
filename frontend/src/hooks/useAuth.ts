"use client";

import { useEffect, useMemo } from "react";
import { useLocalStorage } from "./useLocalStorage";
import { getAuthTokenInfo } from "@/lib/jwt";

export function useAuth() {
  const [token, setToken] = useLocalStorage<string | null>("authToken", null);

  const tokenInfo = useMemo(() => getAuthTokenInfo(token), [token]);

  const login = (newToken: string) => {
    setToken(newToken);
  };

  const logout = () => {
    setToken(null);
  };

  const isAuthenticated = !!token && !tokenInfo.isExpired;

  // If token is expired, clear it.
  useEffect(() => {
    if (!token) return;
    if (tokenInfo.isExpired) {
      setToken(null);
    }
  }, [token, tokenInfo.isExpired, setToken]);

  // Schedule an automatic logout at expiry time.
  useEffect(() => {
    if (!token) return;
    if (!tokenInfo.expiresAt) return;
    if (tokenInfo.isExpired) return;

    const msUntilExpiry = tokenInfo.expiresAt.getTime() - Date.now();
    if (msUntilExpiry <= 0) {
      setToken(null);
      return;
    }

    const timeoutId = window.setTimeout(() => {
      setToken(null);
    }, msUntilExpiry);

    return () => window.clearTimeout(timeoutId);
  }, [token, tokenInfo.expiresAt, tokenInfo.isExpired, setToken]);

  const getAuthHeader = () => {
    if (!token || tokenInfo.isExpired) return {};
    return {
      Authorization: `Bearer ${token}`,
    };
  };

  return {
    token,
    isAuthenticated,
    userId: tokenInfo.userId,
    username: tokenInfo.username,
    expiresAt: tokenInfo.expiresAt,
    isExpired: tokenInfo.isExpired,
    tokenPayload: tokenInfo.raw,
    login,
    logout,
    getAuthHeader,
  };
}
