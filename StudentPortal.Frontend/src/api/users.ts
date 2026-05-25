import client from './client';

interface CatalogUser {
  id: number;
  email: string;
  nickname: string;
  firstName: string;
  lastName: string;
}

let _cachedUserId: number | null = null;

export async function getCatalogUserId(email: string, name?: string): Promise<number | null> {
  if (_cachedUserId !== null) return _cachedUserId;

  const users = await client.get<CatalogUser[]>('/users').then(r => r.data);
  const match = users.find(u => u.email === email);

  if (match) {
    _cachedUserId = match.id;
    return match.id;
  }

  // Not in catalog yet — auto-register using Keycloak identity info
  if (!name) return null;

  const parts = name.trim().split(' ');
  const firstName = parts[0];
  const lastName = parts.slice(1).join(' ') || firstName;

  try {
    const res = await client.post<CatalogUser>('/users', {
      email,
      password: crypto.randomUUID(),
      nickname: email.split('@')[0],
      firstName,
      lastName,
    });
    _cachedUserId = res.data.id;
    return _cachedUserId;
  } catch {
    return null;
  }
}