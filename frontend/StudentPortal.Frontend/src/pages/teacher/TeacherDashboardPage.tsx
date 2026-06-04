import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Layout } from '../../components/Layout';
import { coursesApi } from '../../api/courses';
import { discussionsApi } from '../../api/discussions';
import { getCatalogUserId } from '../../api/users';
import { useAuth } from '../../auth/useAuth';
import type { CourseDto } from '../../types/course';

interface CourseWithStats {
  course: CourseDto;
  studentCount: number;
  threadCount: number;
}

export function TeacherDashboardPage() {
  const { email, name } = useAuth();
  const navigate = useNavigate();
  const firstName = name?.split(' ')[0] ?? 'Teacher';

  const [items, setItems] = useState<CourseWithStats[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!email) return;
    load();
  }, [email]);

  async function load() {
    try {
      const userId = await getCatalogUserId(email!, name);
      if (!userId) return;

      const courses = await coursesApi.getByInstructor(userId);

      const withStats = await Promise.all(
        courses.map(async course => {
          const [studentCount, threads] = await Promise.all([
            coursesApi.getEnrollmentCount(course.id).catch(() => 0),
            discussionsApi.getByTarget(String(course.id), 0).catch(() => []),
          ]);
          return { course, studentCount, threadCount: threads.length };
        })
      );

      setItems(withStats);
    } catch {
      // silently fail
    } finally {
      setLoading(false);
    }
  }

  const totalStudents = items.reduce((s, i) => s + i.studentCount, 0);
  const totalThreads  = items.reduce((s, i) => s + i.threadCount, 0);

  if (loading) {
    return (
      <Layout>
        <div className="max-w-4xl animate-pulse space-y-4">
          <div className="h-7 bg-gray-200 rounded w-48 mb-6" />
          <div className="grid grid-cols-3 gap-4 mb-6">
            {[1, 2, 3].map(i => <div key={i} className="bg-white rounded-xl border p-5 h-20" />)}
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {[1, 2].map(i => (
              <div key={i} className="bg-white rounded-xl border p-5 space-y-2">
                <div className="h-4 bg-gray-100 rounded w-1/4" />
                <div className="h-5 bg-gray-200 rounded w-3/4" />
                <div className="h-3 bg-gray-100 rounded w-1/3" />
              </div>
            ))}
          </div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="max-w-4xl">
        <div className="mb-8">
          <h1 className="text-2xl font-bold text-gray-900">Welcome, {firstName}!</h1>
          <p className="text-gray-500 mt-1">Here's an overview of your teaching activity.</p>
        </div>

        {/* Summary stats */}
        <div className="grid grid-cols-3 gap-4 mb-8">
          {[
            { label: 'Courses taught', value: items.length },
            { label: 'Total students', value: totalStudents },
            { label: 'Open discussions', value: totalThreads },
          ].map(({ label, value }) => (
            <div key={label} className="bg-white rounded-xl border border-gray-100 shadow-sm p-5">
              <p className="text-sm text-gray-500">{label}</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{value}</p>
            </div>
          ))}
        </div>

        {/* Course cards */}
        <h2 className="text-base font-semibold text-gray-700 mb-3">My Courses</h2>
        {items.length === 0 ? (
          <div className="text-center py-16 text-gray-400 text-sm bg-white rounded-xl border border-gray-100">
            No courses assigned yet.
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {items.map(({ course, studentCount, threadCount }) => (
              <div
                key={course.id}
                onClick={() => navigate(`/teacher/courses/${course.id}`)}
                className="bg-white rounded-xl border border-gray-100 shadow-sm p-5 cursor-pointer hover:border-indigo-200 hover:shadow-md transition-all"
              >
                <span className="inline-block text-xs font-mono text-indigo-500 bg-indigo-50 px-2 py-0.5 rounded mb-2">
                  {course.code}
                </span>
                <h3 className="font-semibold text-gray-900 mb-1">{course.title}</h3>
                {course.description && (
                  <p className="text-xs text-gray-400 line-clamp-2 mb-3">{course.description}</p>
                )}
                <div className="flex items-center gap-4 text-xs text-gray-500 pt-2 border-t border-gray-50">
                  <span className="flex items-center gap-1">
                    <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
                    </svg>
                    {studentCount} student{studentCount !== 1 ? 's' : ''}
                  </span>
                  <span className="flex items-center gap-1">
                    <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                    </svg>
                    {threadCount} discussion{threadCount !== 1 ? 's' : ''}
                  </span>
                  <span className={`ml-auto px-2 py-0.5 rounded-full text-xs font-medium ${course.isPublished ? 'bg-green-50 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                    {course.isPublished ? 'Published' : 'Draft'}
                  </span>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </Layout>
  );
}