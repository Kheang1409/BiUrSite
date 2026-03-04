export type JwtPayload = Record<string, unknown> & {
  exp?: number;
  sub?: string;
};

function base64UrlDecode(input: string): string {
  // base64url -> base64
  const base64 = input.replace(/-/g, "+").replace(/_/g, "/");
  const padded = base64.padEnd(
    base64.length + ((4 - (base64.length % 4)) % 4),
    "=",
  );

  // atob is available in browsers; fall back for SSR/node environments.
  if (typeof atob === "function") {
    return atob(padded);
  }

  // eslint-disable-next-line no-restricted-globals
  return Buffer.from(padded, "base64").toString("utf-8");
}

export function decodeJwtPayload(token: string): JwtPayload | null {
  try {
    const parts = token.split(".");
    if (parts.length < 2) return null;

    const json = base64UrlDecode(parts[1]);
    const payload = JSON.parse(json) as JwtPayload;
    return payload;
  } catch {
    return null;
  }
}

export type AuthTokenInfo = {
  userId?: string;
  username?: string;
  role?: string;
  expiresAt?: Date;
  isExpired: boolean;
  raw: JwtPayload | null;
};

function looksLikeGuid(value: string): boolean {
  return /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/.test(
    value,
  );
}

export function getAuthTokenInfo(token: string | null): AuthTokenInfo {
  const raw = token ? decodeJwtPayload(token) : null;

  const exp = typeof raw?.exp === "number" ? raw.exp : undefined;
  const expiresAt = exp ? new Date(exp * 1000) : undefined;
  const isExpired = expiresAt ? expiresAt.getTime() <= Date.now() : false;

  const sub = typeof raw?.sub === "string" ? raw.sub : undefined;
  const id =
    typeof (raw as any)?.id === "string"
      ? ((raw as any).id as string)
      : undefined;

  // Also handle common claim names.
  const nameId =
    typeof (raw as any)?.nameid === "string"
      ? ((raw as any).nameid as string)
      : undefined;
  const uniqueName =
    typeof (raw as any)?.unique_name === "string"
      ? ((raw as any).unique_name as string)
      : undefined;

  const userId = id || nameId || (sub && looksLikeGuid(sub) ? sub : undefined);
  const username = (sub && !looksLikeGuid(sub) ? sub : undefined) || uniqueName;

  // Extract role claim (.NET maps ClaimTypes.Role to "role" in JWT)
  const roleClaim =
    typeof (raw as any)?.role === "string"
      ? ((raw as any).role as string)
      : typeof (raw as any)?.[
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
          ] === "string"
        ? ((raw as any)[
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
          ] as string)
        : undefined;

  return {
    userId,
    username,
    role: roleClaim,
    expiresAt,
    isExpired,
    raw,
  };
}
