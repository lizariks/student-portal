const TOKEN_URL = 'http://localhost:8080/realms/StudentPortal/protocol/openid-connect/token';
const CLIENT_ID = 'studentportal-frontend';
const STORAGE_KEY = 'sp_rt';

interface ParsedToken {
  name?: string;
  email?: string;
  role?: string[];
  roles?: string[];
  realm_access?: { roles?: string[] };
  resource_access?: Record<string, { roles?: string[] }>;
  exp?: number;
}

let _accessToken: string | null = null;
let _refreshToken: string | null = null;
let _parsedToken: ParsedToken | null = null;

function parseJwt(token: string): ParsedToken {
  try {
    const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    return JSON.parse(atob(base64));
  } catch {
    return {};
  }
}

function applyTokens(data: { access_token: string; refresh_token: string }) {
  _accessToken = data.access_token;
  _refreshToken = data.refresh_token;
  _parsedToken = parseJwt(data.access_token);
  sessionStorage.setItem(STORAGE_KEY, data.refresh_token);
}

export async function login(username: string, password: string): Promise<void> {
  const res = await fetch(TOKEN_URL, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: new URLSearchParams({
      grant_type: 'password',
      client_id: CLIENT_ID,
      username,
      password,
      scope: 'openid profile email',
    }),
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.error_description ?? 'Invalid credentials');
  }

  applyTokens(await res.json());
}

export async function refreshTokens(): Promise<boolean> {
  const rt = _refreshToken ?? sessionStorage.getItem(STORAGE_KEY);
  if (!rt) return false;

  const res = await fetch(TOKEN_URL, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: new URLSearchParams({
      grant_type: 'refresh_token',
      client_id: CLIENT_ID,
      refresh_token: rt,
    }),
  });

  if (!res.ok) {
    logout();
    return false;
  }

  applyTokens(await res.json());
  return true;
}

export async function initFromStorage(): Promise<boolean> {
  return refreshTokens();
}

export function logout(): void {
  _accessToken = null;
  _refreshToken = null;
  _parsedToken = null;
  sessionStorage.removeItem(STORAGE_KEY);
}

export function getAccessToken(): string | null { return _accessToken; }

export function isTokenExpiringSoon(): boolean {
  if (!_parsedToken?.exp) return true;
  return Date.now() / 1000 > _parsedToken.exp - 30;
}

export function getUserInfo() {
  const t = _parsedToken;
  const roles = [
    ...(t?.role ?? []),
    ...(t?.roles ?? []),
    ...(t?.realm_access?.roles ?? []),
    ...Object.values(t?.resource_access ?? {}).flatMap(c => c.roles ?? []),
  ];
  return {
    name: t?.name,
    email: t?.email,
    roles: [...new Set(roles)],
  };
}
