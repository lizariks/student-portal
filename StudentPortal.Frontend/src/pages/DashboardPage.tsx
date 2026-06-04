import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/useAuth';
import { Layout } from '../components/Layout';
import { getCatalogUserId } from '../api/users';
import { coursesApi } from '../api/courses';
import { discussionsApi } from '../api/discussions';
import { isTeacher } from '../utils/roles';
import { progressApi } from '../api/progress';
import { TeacherDashboardPage } from './teacher/TeacherDashboardPage';
import type { CourseDetailsDto } from '../types/course';
import type { DiscussionThread } from '../types/discussion';

function timeAgo(dateStr: string) {
  const diff = Date.now() - new Date(dateStr).getTime();
  const m = Math.floor(diff / 60000);
  if (m < 60) return `${m}m ago`;
  const h = Math.floor(m / 60);
  if (h < 24) return `${h}h ago`;
  return `${Math.floor(h / 24)}d ago`;
}

function totalLessonsIn(course: CourseDetailsDto) {
  return course.modules.reduce((s, m) => s + m.lessons.length, 0);
}

export function DashboardPage() {
  const { roles } = useAuth();
  if (isTeacher(roles)) return <TeacherDashboardPage />;
  return <StudentDashboardPage />;
}

function StudentDashboardPage() {
  const { name, email } = useAuth();
  const navigate = useNavigate();
  const firstName = name?.split(' ')[0] ?? 'Student';

  const [loading, setLoading] = useState(true);
  const [courses, setCourses] = useState<CourseDetailsDto[]>([]);
  const [completedByCourse, setCompletedByCourse] = useState<Record<number, number>>({});
  const [threads, setThreads] = useState<{ thread: DiscussionThread; courseTitle: string; courseId: number }[]>([]);

  const statLessons = courses.reduce((s, c) => s + totalLessonsIn(c), 0);
  const statDiscussions = threads.length;

  useEffect(() => {
    if (!email) return;
    loadData();
  }, [email]);

  async function loadData() {
    try {
      setLoading(true);
      const userId = await getCatalogUserId(email!, name);
      if (!userId) return;

      const enrollments = await coursesApi.getUserEnrollments(userId);
      if (enrollments.length === 0) { setLoading(false); return; }

      const [courseDetails, allThreads, allProgress] = await Promise.all([
        Promise.all(enrollments.map(e => coursesApi.getById(e.courseId))),
        Promise.all(enrollments.map(e =>
          discussionsApi.getByTarget(String(e.courseId), 1).catch(() => [] as DiscussionThread[])
        )),
        Promise.all(enrollments.map(e =>
          progressApi.getByStudentAndCourse(userId, e.courseId).catch(() => [])
        )),
      ]);

      setCourses(courseDetails);

      const completedMap: Record<number, number> = {};
      enrollments.forEach((e, i) => { completedMap[e.courseId] = allProgress[i].length; });
      setCompletedByCourse(completedMap);

      const flat = allThreads
        .flatMap((list, i) => list.map(t => ({
          thread: t,
          courseTitle: courseDetails[i].title,
          courseId: courseDetails[i].id,
        })))
        .sort((a, b) => new Date(b.thread.createdAt).getTime() - new Date(a.thread.createdAt).getTime())
        .slice(0, 6);
      setThreads(flat);
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
    }
  }

  return (
    <Layout>
      <div className="max-w-5xl">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-2xl font-bold text-gray-900">Welcome back, {firstName}!</h1>
          <p className="text-gray-500 mt-1">Here's your learning activity.</p>
        </div>

        {/* Stats */}
        <div className="grid grid-cols-3 gap-4 mb-8">
          <StatCard
            label="Enrolled Courses"
            value={loading ? '…' : String(courses.length)}
            color="indigo"
            icon={
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.746 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
              </svg>
            }
          />
          <StatCard
            label="Total Lessons"
            value={loading ? '…' : String(statLessons)}
            color="violet"
            icon={
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
              </svg>
            }
          />
          <StatCard
            label="Active Discussions"
            value={loading ? '…' : String(statDiscussions)}
            color="sky"
            icon={
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
              </svg>
            }
          />
        </div>

        {/* Two-column body */}
        <div className="grid grid-cols-3 gap-6">
          {/* My Courses — 2/3 */}
          <div className="col-span-2 space-y-4">
            <h2 className="text-sm font-semibold text-gray-500 uppercase tracking-wide">My Courses</h2>

            {loading ? (
              <div className="space-y-3">
                {[1, 2].map(i => (
                  <div key={i} className="bg-white rounded-xl border border-gray-100 p-5 animate-pulse space-y-3">
                    <div className="h-4 bg-gray-100 rounded w-2/3" />
                    <div className="h-3 bg-gray-100 rounded w-1/3" />
                    <div className="h-2 bg-gray-100 rounded" />
                  </div>
                ))}
              </div>
            ) : courses.length === 0 ? (
              <div className="bg-white rounded-xl border border-gray-100 p-8 text-center">
                <p className="text-gray-400 text-sm mb-3">You're not enrolled in any courses yet.</p>
                <button
                  onClick={() => navigate('/courses')}
                  className="text-sm text-indigo-600 font-medium hover:underline"
                >
                  Browse courses →
                </button>
              </div>
            ) : (
              courses.map(course => {
                const total = totalLessonsIn(course);
                const viewed = completedByCourse[course.id] ?? 0;
                const pct = total > 0 ? Math.round((viewed / total) * 100) : 0;
                return (
                  <button
                    key={course.id}
                    onClick={() => navigate(`/courses/${course.id}`)}
                    className="w-full text-left bg-white rounded-xl border border-gray-100 shadow-sm p-5 hover:border-indigo-200 hover:shadow-md transition-all group"
                  >
                    <div className="flex items-start justify-between gap-3 mb-3">
                      <div className="min-w-0">
                        <span className="text-xs font-mono text-indigo-400 bg-indigo-50 px-2 py-0.5 rounded">
                          {course.code}
                        </span>
                        <h3 className="font-semibold text-gray-900 mt-1 group-hover:text-indigo-700 transition-colors">
                          {course.title}
                        </h3>
                        {course.instructorName && (
                          <p className="text-xs text-gray-400 mt-0.5">{course.instructorName}</p>
                        )}
                      </div>
                      <svg className="w-4 h-4 text-gray-300 group-hover:text-indigo-400 transition-colors shrink-0 mt-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                      </svg>
                    </div>

                    {/* Progress bar */}
                    <div className="mb-2">
                      <div className="flex justify-between text-xs text-gray-400 mb-1">
                        <span>Progress</span>
                        <span>{viewed}/{total} lessons</span>
                      </div>
                      <div className="h-1.5 bg-gray-100 rounded-full overflow-hidden">
                        <div
                          className="h-full bg-indigo-500 rounded-full transition-all"
                          style={{ width: `${pct}%` }}
                        />
                      </div>
                    </div>

                    <div className="flex gap-3 text-xs text-gray-400 mt-2">
                      <span>{course.modules.length} module{course.modules.length !== 1 ? 's' : ''}</span>
                      <span>·</span>
                      <span>{total} lesson{total !== 1 ? 's' : ''}</span>
                      {pct === 100 && (
                        <>
                          <span>·</span>
                          <span className="text-green-600 font-medium">Completed</span>
                        </>
                      )}
                    </div>
                  </button>
                );
              })
            )}
          </div>

          {/* Recent Discussions — 1/3 */}
          <div className="space-y-4">
            <h2 className="text-sm font-semibold text-gray-500 uppercase tracking-wide">Recent Discussions</h2>

            {loading ? (
              <div className="space-y-3">
                {[1, 2, 3].map(i => (
                  <div key={i} className="bg-white rounded-xl border border-gray-100 p-4 animate-pulse space-y-2">
                    <div className="h-3 bg-gray-100 rounded w-3/4" />
                    <div className="h-2 bg-gray-100 rounded w-1/2" />
                  </div>
                ))}
              </div>
            ) : threads.length === 0 ? (
              <div className="bg-white rounded-xl border border-gray-100 p-6 text-center">
                <p className="text-gray-400 text-xs">No discussions yet in your courses.</p>
              </div>
            ) : (
              threads.map(({ thread, courseTitle, courseId }) => (
                <button
                  key={thread.id}
                  onClick={() => navigate(`/courses/${courseId}`)}
                  className="w-full text-left bg-white rounded-xl border border-gray-100 p-4 hover:border-indigo-200 hover:shadow-sm transition-all group"
                >
                  <p className="text-sm font-medium text-gray-900 group-hover:text-indigo-700 transition-colors line-clamp-2 mb-2">
                    {thread.title}
                  </p>
                  <div className="flex items-center justify-between text-xs text-gray-400">
                    <span className="truncate max-w-[60%]">{courseTitle}</span>
                    <div className="flex items-center gap-2 shrink-0">
                      <span>{thread.comments.length} {thread.comments.length === 1 ? 'reply' : 'replies'}</span>
                      <span>·</span>
                      <span>{timeAgo(thread.createdAt)}</span>
                    </div>
                  </div>
                </button>
              ))
            )}

            {threads.length > 0 && (
              <button
                onClick={() => navigate('/discussions')}
                className="w-full text-xs text-indigo-500 hover:text-indigo-700 py-2 text-center"
              >
                View all discussions →
              </button>
            )}
          </div>
        </div>
      </div>
    </Layout>
  );
}

function StatCard({
  label, value, color, icon,
}: {
  label: string;
  value: string;
  color: 'indigo' | 'violet' | 'sky';
  icon: React.ReactNode;
}) {
  const bg = { indigo: 'bg-indigo-50 text-indigo-600', violet: 'bg-violet-50 text-violet-600', sky: 'bg-sky-50 text-sky-600' }[color];
  return (
    <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5 flex items-center gap-4">
      <div className={`w-10 h-10 rounded-xl flex items-center justify-center shrink-0 ${bg}`}>
        {icon}
      </div>
      <div>
        <p className="text-xs text-gray-500">{label}</p>
        <p className="text-2xl font-bold text-gray-900 leading-tight">{value}</p>
      </div>
    </div>
  );
}