import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Layout } from '../components/Layout';
import { coursesApi } from '../api/courses';
import { discussionsApi } from '../api/discussions';
import { getCatalogUserId } from '../api/users';
import { useAuth } from '../auth/useAuth';
import type { DiscussionThread } from '../types/discussion';

interface ThreadWithCourse {
  thread: DiscussionThread;
  courseId: number;
  courseTitle: string;
}

export function DiscussionsPage() {
  const { email, name } = useAuth();
  const navigate = useNavigate();
  const [items, setItems] = useState<ThreadWithCourse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!email) return;
    load();
  }, [email]);

  async function load() {
    try {
      setLoading(true);
      setError(null);
      const userId = await getCatalogUserId(email!, name);
      if (!userId) { setItems([]); return; }

      const enrollments = await coursesApi.getUserEnrollments(userId);
      const all: ThreadWithCourse[] = [];

      await Promise.all(
        enrollments.map(async enr => {
          const courseId = enr.courseId;
          const courseTitle = enr.course?.title ?? `Course #${courseId}`;
          try {
            const threads = await discussionsApi.getByTarget(String(courseId), 0);
            threads.forEach(thread => all.push({ thread, courseId, courseTitle }));
          } catch {
            // skip courses where discussions can't be fetched
          }
        })
      );

      all.sort((a, b) => new Date(b.thread.updatedAt).getTime() - new Date(a.thread.updatedAt).getTime());
      setItems(all);
    } catch {
      setError('Failed to load discussions.');
    } finally {
      setLoading(false);
    }
  }

  if (loading) {
    return (
      <Layout>
        <div className="max-w-3xl animate-pulse space-y-4">
          <div className="h-7 bg-gray-200 rounded w-48 mb-6" />
          {[1, 2, 3].map(i => (
            <div key={i} className="bg-white rounded-xl border border-gray-100 p-5 space-y-2">
              <div className="h-4 bg-gray-100 rounded w-1/3" />
              <div className="h-5 bg-gray-200 rounded w-2/3" />
              <div className="h-3 bg-gray-100 rounded w-1/4" />
            </div>
          ))}
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="max-w-3xl">
        <h1 className="text-2xl font-bold text-gray-900 mb-6">Discussions</h1>

        {error && <p className="text-sm text-red-600 mb-4">{error}</p>}

        {items.length === 0 ? (
          <div className="text-center py-16 text-gray-400">
            <p className="text-sm">No discussions found across your enrolled courses.</p>
            <button
              onClick={() => navigate('/courses')}
              className="mt-4 text-sm text-indigo-600 hover:underline"
            >
              Browse courses
            </button>
          </div>
        ) : (
          <div className="space-y-3">
            {items.map(({ thread, courseId, courseTitle }) => (
              <div
                key={thread.id}
                onClick={() => navigate(`/courses/${courseId}`)}
                className="bg-white rounded-xl border border-gray-100 shadow-sm p-5 cursor-pointer hover:border-indigo-200 hover:shadow-md transition-all"
              >
                <p className="text-xs text-indigo-500 font-medium mb-1">{courseTitle}</p>
                <p className="font-semibold text-gray-900">{thread.title}</p>
                <p className="text-xs text-gray-400 mt-1">
                  by {thread.createdBy.userName}
                  {' · '}
                  {thread.comments.length} comment{thread.comments.length !== 1 ? 's' : ''}
                  {thread.isClosed && <span className="ml-2 text-amber-500">· Closed</span>}
                </p>
              </div>
            ))}
          </div>
        )}
      </div>
    </Layout>
  );
}