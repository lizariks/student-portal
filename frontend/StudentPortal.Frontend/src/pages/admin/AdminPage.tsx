import { useState, useEffect } from 'react';
import { Layout } from '../../components/Layout';
import { adminApi } from '../../api/admin';
import type { AdminUserDto } from '../../api/admin';

const ROLE_OPTIONS = ['Student', 'Teacher'];
const BADGE_COLORS: Record<string, string> = {
  Admin:   'bg-red-100 text-red-700',
  Teacher: 'bg-indigo-100 text-indigo-700',
  Student: 'bg-green-100 text-green-700',
  User:    'bg-gray-100 text-gray-500',
};

type RoleFilter = 'All' | 'Student' | 'Teacher';

export function AdminPage() {
  const [users, setUsers] = useState<AdminUserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<RoleFilter>('All');

  const [form, setForm] = useState({ userName: '', email: '', password: '', role: 'Student' });
  const [formError, setFormError] = useState<string | null>(null);
  const [formSubmitting, setFormSubmitting] = useState(false);
  const [formSuccess, setFormSuccess] = useState<string | null>(null);

  const [roleAction, setRoleAction] = useState<{ userId: string; role: string } | null>(null);

  useEffect(() => { loadUsers(); }, []);

  async function loadUsers() {
    try {
      setLoading(true);
      setError(null);
      setUsers(await adminApi.getUsers());
    } catch (err: unknown) {
      const detail = (err as { response?: { data?: { error?: string; detail?: string } } })?.response?.data;
      setError(detail?.detail ?? detail?.error ?? 'Failed to load users. Check that the backend is running.');
    } finally {
      setLoading(false);
    }
  }

  async function handleRegister(e: React.FormEvent) {
    e.preventDefault();
    if (!form.userName || !form.email || !form.password) return;
    setFormSubmitting(true);
    setFormError(null);
    setFormSuccess(null);
    try {
      await adminApi.registerUser(form);
      setFormSuccess(`User ${form.email} registered as ${form.role}.`);
      setForm({ userName: '', email: '', password: '', role: 'Student' });
      await loadUsers();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { errors?: string[] } } })?.response?.data?.errors?.[0];
      setFormError(msg ?? 'Registration failed.');
    } finally {
      setFormSubmitting(false);
    }
  }

  async function handleAddRole(userId: string, role: string) {
    try {
      await adminApi.addRole(userId, role);
      setUsers(prev => prev.map(u =>
        u.id === userId ? { ...u, roles: [...new Set([...u.roles, role])] } : u
      ));
    } catch {
      alert('Failed to add role.');
    }
    setRoleAction(null);
  }

  function matchesFilter(roles: string[], f: RoleFilter): boolean {
    if (f === 'All') return true;
    const lower = roles.map(r => r.toLowerCase());
    if (f === 'Student') return lower.some(r => r === 'student' || r === 'user');
    if (f === 'Teacher') return lower.some(r => r === 'teacher' || r === 'admin');
    return false;
  }

  const visibleUsers = users.filter(u => matchesFilter(u.roles, filter));

  async function handleRemoveRole(userId: string, role: string) {
    try {
      await adminApi.removeRole(userId, role);
      setUsers(prev => prev.map(u =>
        u.id === userId ? { ...u, roles: u.roles.filter(r => r !== role) } : u
      ));
    } catch {
      alert('Failed to remove role.');
    }
  }

  return (
    <Layout>
      <div className="max-w-4xl space-y-8">
        <h1 className="text-2xl font-bold text-gray-900">Admin Panel</h1>

        {/* Register new user */}
        <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-6">
          <h2 className="text-base font-semibold text-gray-800 mb-4">Register New User</h2>
          <form onSubmit={handleRegister} className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1">Username</label>
              <input
                type="text"
                value={form.userName}
                onChange={e => setForm(f => ({ ...f, userName: e.target.value }))}
                placeholder="johndoe"
                required
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-300"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1">Email</label>
              <input
                type="email"
                value={form.email}
                onChange={e => setForm(f => ({ ...f, email: e.target.value }))}
                placeholder="john@portal.com"
                required
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-300"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1">Password</label>
              <input
                type="password"
                value={form.password}
                onChange={e => setForm(f => ({ ...f, password: e.target.value }))}
                placeholder="Min. 8 characters"
                required
                minLength={8}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-300"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1">Role</label>
              <select
                value={form.role}
                onChange={e => setForm(f => ({ ...f, role: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-300 bg-white"
              >
                {ROLE_OPTIONS.map(r => (
                  <option key={r} value={r}>{r}</option>
                ))}
              </select>
            </div>
            <div className="sm:col-span-2 flex items-center gap-4">
              <button
                type="submit"
                disabled={formSubmitting}
                className="px-5 py-2 rounded-lg bg-indigo-600 text-white text-sm font-medium hover:bg-indigo-700 disabled:opacity-50 transition-colors"
              >
                {formSubmitting ? 'Registering…' : 'Register User'}
              </button>
              {formError && <p className="text-sm text-red-600">{formError}</p>}
              {formSuccess && <p className="text-sm text-green-600">{formSuccess}</p>}
            </div>
          </form>
        </div>

        {/* User table */}
        <div className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-100 flex items-center justify-between gap-4 flex-wrap">
            <div className="flex items-center gap-1 bg-gray-100 rounded-lg p-1">
              {(['All', 'Student', 'Teacher'] as RoleFilter[]).map(tab => (
                <button
                  key={tab}
                  onClick={() => setFilter(tab)}
                  className={`px-3 py-1 rounded-md text-xs font-medium transition-colors ${
                    filter === tab
                      ? 'bg-white text-gray-900 shadow-sm'
                      : 'text-gray-500 hover:text-gray-700'
                  }`}
                >
                  {tab === 'All' ? `All (${users.length})` : `${tab}s (${users.filter(u => matchesFilter(u.roles, tab)).length})`}
                </button>
              ))}
            </div>
            <button onClick={loadUsers} className="text-xs text-indigo-500 hover:text-indigo-700">Refresh</button>
          </div>

          {loading ? (
            <div className="p-6 space-y-3 animate-pulse">
              {[1,2,3].map(i => <div key={i} className="h-10 bg-gray-100 rounded" />)}
            </div>
          ) : error ? (
            <p className="p-6 text-sm text-red-600">{error}</p>
          ) : visibleUsers.length === 0 ? (
            <p className="p-6 text-sm text-gray-400">
              {filter === 'All' ? 'No users found.' : `No ${filter.toLowerCase()}s found.`}
            </p>
          ) : (
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-xs text-gray-500 uppercase tracking-wide">
                <tr>
                  <th className="px-6 py-3 text-left">User</th>
                  <th className="px-6 py-3 text-left">Roles</th>
                  <th className="px-6 py-3 text-left">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {visibleUsers.map(user => (
                  <tr key={user.id} className="hover:bg-gray-50/50">
                    <td className="px-6 py-3">
                      <p className="font-medium text-gray-900">{user.userName}</p>
                      <p className="text-gray-400 text-xs">{user.email}</p>
                    </td>
                    <td className="px-6 py-3">
                      <div className="flex flex-wrap gap-1">
                        {user.roles.map(role => (
                          <span
                            key={role}
                            className={`inline-flex items-center gap-1 text-xs px-2 py-0.5 rounded-full font-medium ${BADGE_COLORS[role] ?? 'bg-gray-100 text-gray-600'}`}
                          >
                            {role}
                            {role !== 'User' && role !== 'Admin' && (
                              <button
                                onClick={() => handleRemoveRole(user.id, role)}
                                className="hover:text-red-500 leading-none"
                                title={`Remove ${role}`}
                              >
                                ×
                              </button>
                            )}
                          </span>
                        ))}
                      </div>
                    </td>
                    <td className="px-6 py-3">
                      {roleAction?.userId === user.id ? (
                        <div className="flex items-center gap-2">
                          <select
                            value={roleAction.role}
                            onChange={e => setRoleAction({ userId: user.id, role: e.target.value })}
                            className="border border-gray-200 rounded px-2 py-1 text-xs bg-white"
                          >
                            {ROLE_OPTIONS.filter(r => !user.roles.includes(r)).map(r => (
                              <option key={r} value={r}>{r}</option>
                            ))}
                          </select>
                          <button
                            onClick={() => handleAddRole(user.id, roleAction.role)}
                            className="text-xs px-2 py-1 bg-indigo-600 text-white rounded hover:bg-indigo-700"
                          >
                            Add
                          </button>
                          <button
                            onClick={() => setRoleAction(null)}
                            className="text-xs text-gray-400 hover:text-gray-600"
                          >
                            Cancel
                          </button>
                        </div>
                      ) : (
                        <button
                          onClick={() => {
                            const available = ROLE_OPTIONS.filter(r => !user.roles.includes(r));
                            if (available.length > 0)
                              setRoleAction({ userId: user.id, role: available[0] });
                          }}
                          disabled={ROLE_OPTIONS.every(r => user.roles.includes(r))}
                          className="text-xs text-indigo-500 hover:text-indigo-700 disabled:opacity-40 disabled:cursor-default"
                        >
                          + Add role
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </Layout>
  );
}