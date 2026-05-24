import { useState, useEffect } from 'react';
import { useAuth } from '../auth/useAuth';
import { Layout } from '../components/Layout';
import { getCatalogUserId } from '../api/users';
import { coursesApi } from '../api/courses';

function StatCard({ label, value, icon }: { label: string; value: string; icon: React.ReactNode }) {
  return (
    <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-6 flex items-center gap-4">
      <div className="w-12 h-12 bg-indigo-50 rounded-xl flex items-center justify-center text-indigo-600 shrink-0">
        {icon}
      </div>
      <div>
        <p className="text-sm text-gray-500">{label}</p>
        <p className="text-2xl font-bold text-gray-900">{value}</p>
      </div>
    </div>
  );
}

export function DashboardPage() {
  const { name, email, roles } = useAuth();
  const firstName = name?.split(' ')[0] ?? 'Student';
  const [enrolledCount, setEnrolledCount] = useState<number | null>(null);

  useEffect(() => {
    if (!email) return;
    getCatalogUserId(email, name).then(userId => {
      if (!userId) return;
      coursesApi.getUserEnrollments(userId).then(list => setEnrolledCount(list.length));
    }).catch(() => {});
  }, [email, name]);

  return (
    <Layout>
      <div className="max-w-4xl">
        <div className="mb-8">
          <h1 className="text-2xl font-bold text-gray-900">Welcome back, {firstName}!</h1>
          <p className="text-gray-500 mt-1">Here's an overview of your portal activity.</p>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
          <StatCard
            label="Enrolled Courses"
            value={enrolledCount === null ? '…' : String(enrolledCount)}
            icon={
              <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.746 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
              </svg>
            }
          />
          <StatCard
            label="Discussions"
            value="0"
            icon={
              <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
              </svg>
            }
          />
          <StatCard
            label="Role"
            value={roles[0] ?? 'user'}
            icon={
              <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
            }
          />
        </div>

        <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-6">
          <h2 className="text-base font-semibold text-gray-900 mb-4">Account details</h2>
          <dl className="divide-y divide-gray-50">
            <div className="flex py-3 gap-4">
              <dt className="w-28 text-sm text-gray-500 shrink-0">Full name</dt>
              <dd className="text-sm font-medium text-gray-900">{name}</dd>
            </div>
            <div className="flex py-3 gap-4">
              <dt className="w-28 text-sm text-gray-500 shrink-0">Email</dt>
              <dd className="text-sm font-medium text-gray-900">{email}</dd>
            </div>
            <div className="flex py-3 gap-4">
              <dt className="w-28 text-sm text-gray-500 shrink-0">Roles</dt>
              <dd className="flex flex-wrap gap-1.5">
                {roles.map((r) => (
                  <span key={r} className="text-xs font-medium bg-indigo-50 text-indigo-700 px-2.5 py-1 rounded-full">
                    {r}
                  </span>
                ))}
              </dd>
            </div>
          </dl>
        </div>
      </div>
    </Layout>
  );
}