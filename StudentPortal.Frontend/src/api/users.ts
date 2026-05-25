import client from './client';

export interface CatalogUser {
  id: number;
  email: string;
  nickname: string;
  firstName: string;
  lastName: string;
}

let _cachedUser: CatalogUser | null = null;

export async function getCatalogUser(email: string, name?: string): Promise<CatalogUser | null> {
  if (_cachedUser !== null) return _cachedUser;

  const users = await client.get<CatalogUser[]>('/users').then(r => r.data);
  const match = users.find(u => u.email === email);

  if (match) {
    _cachedUser = match;
    return match;
  }

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
    _cachedUser = res.data;
    return _cachedUser;
  } catch {
    return null;
  }
}

export async function getCatalogUserId(email: string, name?: string): Promise<number | null> {
  const user = await getCatalogUser(email, name);
  return user?.id ?? null;
}