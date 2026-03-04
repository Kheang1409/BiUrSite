"use client";

import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  ReactNode,
} from "react";
import { getAuthTokenInfo } from "@/lib/jwt";

interface AuthContextValue {
  token: string | null;
  isAuthenticated: boolean;
  isHydrating: boolean;
  role: string | null;
  isAdmin: boolean;
  userId: string | null;
  username: string | null;
  login: (token: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const TOKEN_KEY = "authToken";

function readToken(): string | null {
  if (typeof window === "undefined") return null;
  const raw = window.localStorage.getItem(TOKEN_KEY);
  if (!raw) return null;
  try {
    const parsed = JSON.parse(raw);
    return typeof parsed === "string" ? parsed : null;
  } catch {
    return raw;
  }
}

function extractRole(token: string | null): string | null {
  if (!token) return null;
  const info = getAuthTokenInfo(token);
  if (info.isExpired || !info.raw) return null;
  // .NET JwtSecurityTokenHandler maps ClaimTypes.Role to "role" in the JWT payload
  const role =
    (info.raw as Record<string, unknown>)["role"] ??
    (info.raw as Record<string, unknown>)[
      "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    ];
  return typeof role === "string" ? role : null;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(null);
  const [role, setRole] = useState<string | null>(null);
  const [userId, setUserId] = useState<string | null>(null);
  const [username, setUsername] = useState<string | null>(null);
  const [isHydrating, setIsHydrating] = useState(true);

  // Hydrate from localStorage on mount
  useEffect(() => {
    const stored = readToken();
    if (stored) {
      const info = getAuthTokenInfo(stored);
      if (info.isExpired) {
        window.localStorage.removeItem(TOKEN_KEY);
      } else {
        setToken(stored);
        setRole(extractRole(stored));
        setUserId(info.userId ?? null);
        setUsername(info.username ?? null);
      }
    }
    setIsHydrating(false);
  }, []);

  const login = useCallback((newToken: string) => {
    window.localStorage.setItem(TOKEN_KEY, JSON.stringify(newToken));
    setToken(newToken);
    setRole(extractRole(newToken));
    const info = getAuthTokenInfo(newToken);
    setUserId(info.userId ?? null);
    setUsername(info.username ?? null);
  }, []);

  const logout = useCallback(() => {
    window.localStorage.removeItem(TOKEN_KEY);
    setToken(null);
    setRole(null);
    setUserId(null);
    setUsername(null);
  }, []);

  const isAuthenticated = !!token;
  const isAdmin = role === "Admin";

  return (
    <AuthContext.Provider
      value={{
        token,
        isAuthenticated,
        isHydrating,
        role,
        isAdmin,
        userId,
        username,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuthContext(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuthContext must be used within an AuthProvider");
  }
  return ctx;
}
