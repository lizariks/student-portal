import { useState, useEffect } from 'react';
import { Layout } from '../../components/Layout';
import { coursesApi } from '../../api/courses';
import { discussionsApi } from '../../api/discussions';
import { getCatalogUserId } from '../../api/users';
import { useAuth } from '../../auth/useAuth';
import type { CourseDto, StudentCourseDto } from '../../types/course';

interface CourseStats {
  course: CourseDto;
  students: StudentCourseDto[];
  threadCount: number;
}

function monthKey(dateStr: string) {
  const d = new Date(dateStr);
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
}

function monthLabel(key: string) {
  const [y, m] = key.split('-');
  return new Date(Number(y), Number(m) - 1).toLocaleDateString('en-US', { month: 'short', year: '2-digit' });
}

export function TeacherAnalyticsPage() {
  const { email, name } = useAuth();
  const [stats, setStats] = useState<CourseStats[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => { if (email) load(); }, [email]);

  async function load() {
    try {
      const userId = await getCatalogUserId(email!, name);
      if (!userId) return;
      const courses = await coursesApi.getByInstructor(userId);
      const withStats = await Promise.all(
        courses.map(async course => {
          const [students, threads] = await Promise.all([
            coursesApi.getStudentsByCourse(course.id).catch(() => [] as StudentCourseDto[]),
            discussionsApi.getByTarget(String(course.id), 0).catch(() => []),
          ]);
          return { course, students, threadCount: threads.length };
        })
      );
      setStats(withStats);
    } catch {
      // silently fail
    } finally {
      setLoading(false);
    }
  }

  const allEnrollments = stats.flatMap(s => s.students);
  const totalStudents = allEnrollments.length;
  const totalThreads = stats.reduce((s, i) => s + i.threadCount, 0);
  const maxStudents = Math.max(...stats.map(s => s.students.length), 1);
  const maxThreads = Math.max(...stats.map(s => s.threadCount), 1);

  const now = new Date();
  const last6Months = Array.from({ length: 6 }, (_, i) => {
    const d = new Date(now.getFullYear(), now.getMonth() - (5 - i), 1);
    const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
    return { key, label: monthLabel(key), count: allEnrollments.filter(e => monthKey(e.enrolledAt) === key).length };
  });
  const maxMonthCount = Math.max(...last6Months.map(m => m.count), 1);

  const recentEnrollments = [...allEnrollments]
    .sort((a, b) => new Date(b.enrolledAt).getTime() - new Date(a.enrolledAt).getTime())
    .slice(0, 6)
    .map(e => ({
      ...e,
      courseName: stats.find(s => s.students.includes(e))?.course.title ?? '',
    }));

  if (loading) {
    return (
      <Layout>
        <div className="max-w-4xl animate-pulse space-y-6">
          <div className="h-7 bg-gray-200 rounded w-40 mb-2" />
          <div className="grid grid-cols-4 gap-4">
            {[1, 2, 3, 4].map(i => <div key={i} className="bg-white rounded-xl border h-24" />)}
          </div>
          <div className="grid grid-cols-2 gap-6">
            <div className="bg-white rounded-xl border h-48" />
            <div className="bg-white rounded-xl border h-48" />
          </div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="max-w-4xl">
        <div className="mb-8">
          <h1 className="text-2xl font-bold text-gray-900">Analytics</h1>
          <p className="text-gray-500 mt-1 text-sm">Overview of your teaching activity.</p>
        </div>

        {/* Summary cards */}
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-6">
          {[
            { label: 'Total Courses', value: stats.length, emoji: '📚', color: 'text-indigo-600' },
            { label: 'Total Students', value: totalStudents, emoji: '👥', color: 'text-emerald-600' },
            { label: 'Discussions', value: totalThreads, emoji: '💬', color: 'text-violet-600' },
            { label: 'Avg per Course', value: stats.length ? Math.round(totalStudents / stats.length) : 0, emoji: '📊', color: 'text-amber-600' },
          ].map(({ label, value, emoji, color }) => (
            <div key={label} className="bg-white rounded-xl border border-gray-100 shadow-sm p-5">
              <span className="text-xl">{emoji}</span>
              <p className={`text-3xl font-black mt-2 ${color}`}>{value}</p>
              <p className="text-xs text-gray-500 mt-0.5">{label}</p>
            </div>
          ))}
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
          {/* Students per course — horizontal bar chart */}
          <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5">
            <h2 className="text-sm font-semibold text-gray-700 mb-5">Students per Course</h2>
            {stats.length === 0 ? (
              <p className="text-sm text-gray-400 italic">No data yet.</p>
            ) : (
              <div className="space-y-3">
                {[...stats]
                  .sort((a, b) => b.students.length - a.students.length)
                  .map(({ course, students }) => (
                    <div key={course.id} className="flex items-center gap-3">
                      <span className="text-xs text-gray-500 w-28 truncate shrink-0 text-right">{course.title}</span>
                      <div className="flex-1 bg-gray-100 rounded-full h-3 overflow-hidden">
                        <div
                          className="bg-indigo-500 h-3 rounded-full transition-all duration-700"
                          style={{ width: `${Math.max((students.length / maxStudents) * 100, students.length > 0 ? 4 : 0)}%` }}
                        />
                      </div>
                      <span className="text-xs font-bold text-gray-700 w-5 text-right shrink-0">{students.length}</span>
                    </div>
                  ))}
              </div>
            )}
          </div>

          {/* Enrollment timeline — vertical bar chart */}
          <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5">
            <h2 className="text-sm font-semibold text-gray-700 mb-5">New Enrollments — Last 6 Months</h2>
            <div className="flex items-end gap-2 h-32">
              {last6Months.map(m => (
                <div key={m.key} className="flex flex-col items-center gap-1 flex-1 h-full justify-end">
                  {m.count > 0 && (
                    <span className="text-xs font-bold text-indigo-600">{m.count}</span>
                  )}
                  <div className="w-full flex items-end" style={{ height: '85%' }}>
                    <div
                      className="w-full bg-gradient-to-t from-indigo-600 to-indigo-400 rounded-t-md transition-all duration-700"
                      style={{ height: `${Math.max((m.count / maxMonthCount) * 100, m.count > 0 ? 6 : 2)}%`, opacity: m.count === 0 ? 0.2 : 1 }}
                    />
                  </div>
                  <span className="text-xs text-gray-400 whitespace-nowrap">{m.label}</span>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Discussion threads per course */}
        <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5 mb-6">
          <h2 className="text-sm font-semibold text-gray-700 mb-5">Discussion Activity per Course</h2>
          {stats.length === 0 ? (
            <p className="text-sm text-gray-400 italic">No data yet.</p>
          ) : (
            <div className="space-y-3">
              {[...stats]
                .sort((a, b) => b.threadCount - a.threadCount)
                .map(({ course, threadCount }) => (
                  <div key={course.id} className="flex items-center gap-3">
                    <span className="text-xs text-gray-500 w-28 truncate shrink-0 text-right">{course.title}</span>
                    <div className="flex-1 bg-gray-100 rounded-full h-3 overflow-hidden">
                      <div
                        className="bg-violet-500 h-3 rounded-full transition-all duration-700"
                        style={{ width: `${Math.max((threadCount / maxThreads) * 100, threadCount > 0 ? 4 : 0)}%` }}
                      />
                    </div>
                    <span className="text-xs font-bold text-gray-700 w-5 text-right shrink-0">{threadCount}</span>
                  </div>
                ))}
            </div>
          )}
        </div>

        {/* Recent enrollments */}
        <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5">
          <h2 className="text-sm font-semibold text-gray-700 mb-4">Recent Enrollments</h2>
          {recentEnrollments.length === 0 ? (
            <p className="text-sm text-gray-400 italic">No enrollments yet.</p>
          ) : (
            <div className="divide-y divide-gray-50">
              {recentEnrollments.map((e, idx) => {
                const user = e.user;
                const displayName = user
                  ? `${user.firstName} ${user.lastName}`.trim() || user.nickname
                  : `User #${e.userId}`;
                const initials = displayName.split(' ').map(p => p[0]).join('').toUpperCase().slice(0, 2);
                return (
                  <div key={idx} className="flex items-center gap-3 py-3">
                    <div className="w-8 h-8 rounded-full bg-indigo-100 text-indigo-600 text-xs flex items-center justify-center shrink-0 font-semibold">
                      {initials}
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-gray-900 truncate">{displayName}</p>
                      <p className="text-xs text-gray-400 truncate">{e.courseName}</p>
                    </div>
                    <p className="text-xs text-gray-400 shrink-0">
                      {new Date(e.enrolledAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}
                    </p>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </Layout>
  );
}
