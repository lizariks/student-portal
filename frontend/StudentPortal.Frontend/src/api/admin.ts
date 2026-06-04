import client from './client';

export interface AdminUserDto {
  id: string;
  userName: string;
  email: string;
  roles: string[];
  createdAt: string;
}

export interface AdminRegisterDto {
  userName: string;
  email: string;
  password: string;
  role: string;
}

export const adminApi = {
  getUsers: () =>
    client.get<AdminUserDto[]>('/admin/users').then(r => r.data),

  registerUser: (dto: AdminRegisterDto) =>
    client.post('/admin/users/register', dto),

  addRole: (userId: string, roleName: string) =>
    client.post('/admin/users/roles/add', { userId, roleName }),

  removeRole: (userId: string, roleName: string) =>
    client.post('/admin/users/roles/remove', { userId, roleName }),
};