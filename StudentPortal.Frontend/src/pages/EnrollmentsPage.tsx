import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Layout } from '../components/Layout';
import { coursesApi } from '../api/courses';
import { getCatalogUserId } from '../api/users';
import { useAuth } from '../auth/useAuth';
import type { StudentCourseDto } from '../types/course';

export function EnrollmentsPage() {
  const { email, name } = useAuth();
  const navigate = useNavigate();

  const [enrollments, setEnrollments] = useState<StudentCourseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!email) return;
    loadEnrollments();
  }, [email]);

  async function loadEnrollments() {
    try {
      setLoading(true);
      setError(null);
      const userId = await getCatalogUserId(email!, name);
      if (!userId) {
        setEnrollments([]);
        return;
      }
      setEnrollments(await coursesApi.getUserEnrollments(userId));
    } catch {
      setError('Failed to load your enrollments.');
    } finally {
      setLoading(false);
    }
  }

  async function handleUnenroll(enrollment: StudentCourseDto) {
    const userId = await getCatalogUserId(email!, name).catch(() => null);
    if (!userId) return;
    try {
      await coursesApi.unenroll(userId, enrollment.courseId);
      setEnrollments(prev => prev.filter(e => e.courseId !== enrollment.courseId));
    } catch {
      setError('Failed to unenroll. Please try again.');
    }
  }

  const enrolledAt = (iso: string) =>
    new Date(iso).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });

  return (
    <Layout>
      <div className="max-w-3xl">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-900">My Enrollments</h1>
          <p className="text-gray-500 mt-1">Courses you are currently enrolled in.</p>
        </div>

        {error && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{error}</div>
        )}

        {loading ? (
          <div className="space-y-3">
            {Array.from({ length: 3 }).map((_, i) => (
              <div key={i} className="bg-white rounded-xl border border-gray-100 shadow-sm p-5 animate-pulse flex gap-4">
                <div className="flex-1 space-y-2">
                  <div className="h-4 bg-gray-100 rounded w-1/4" />
                  <div className="h-5 bg-gray-200 rounded w-2/3" />
                  <div className="h-4 bg-gray-100 rounded w-1/3" />
                </div>
              </div>
            ))}
          </div>
        ) : enrollments.length === 0 ? (
          <div className="text-center py-16 text-gray-400">
            <p className="text-lg font-medium">No enrollments yet</p>
            <p className="text-sm mt-1">
              Browse{' '}
              <button
                onClick={() => navigate('/courses')}
                className="text-indigo-600 hover:underline"
              >
                available courses
              </button>{' '}
              to get started.
            </p>
          </div>
        ) : (
          <div className="space-y-3">
            {enrollments.map(enrollment => {
              const course = enrollment.course;
              return (
                <div
                  key={enrollment.courseId}
                  className="bg-white rounded-xl border border-gray-100 shadow-sm p-5 flex items-center gap-4"
                >
                  <div className="flex-1 min-w-0">
                    {course ? (
                      <>
                        <span className="text-xs font-mono text-indigo-500 bg-indigo-50 px-2 py-0.5 rounded">
                          {course.code}
                        </span>
                        <h3 className="font-semibold text-gray-900 mt-1">{course.title}</h3>
                        {course.instructorName && (
                          <p className="text-sm text-gray-400 mt-0.5">By {course.instructorName}</p>
                        )}
                      </>
                    ) : (
                      <p className="font-semibold text-gray-900">Course #{enrollment.courseId}</p>
                    )}
                    <p className="text-xs text-gray-400 mt-1.5">
                      Enrolled {enrolledAt(enrollment.enrolledAt)}
                    </p>
                  </div>

                  <div className="flex items-center gap-2 shrink-0">
                    <button
                      onClick={() => navigate(`/courses/${enrollment.courseId}`)}
                      className="px-4 py-2 bg-indigo-600 text-white text-sm rounded-lg hover:bg-indigo-700 transition-colors"
                    >
                      Continue
                    </button>
                    <button
                      onClick={() => handleUnenroll(enrollment)}
                      className="px-4 py-2 bg-gray-100 text-gray-600 text-sm rounded-lg hover:bg-red-50 hover:text-red-700 border border-gray-200 transition-colors"
                    >
                      Unenroll
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </Layout>
  );
}