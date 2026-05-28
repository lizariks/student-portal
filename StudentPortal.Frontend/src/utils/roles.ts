export const isTeacher = (roles: string[]) =>
  roles.some(r => r.toLowerCase() === 'teacher');

export const isAdmin = (roles: string[]) =>
  roles.some(r => r.toLowerCase() === 'admin');