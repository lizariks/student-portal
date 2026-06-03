import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Layout } from '../../components/Layout';
import { coursesApi } from '../../api/courses';
import { progressApi } from '../../api/progress';
import { getCatalogUserId } from '../../api/users';
import { useAuth } from '../../auth/useAuth';
import type { CourseDto, StudentCourseDto } from '../../types/course';

interface CourseStudents {
  course: CourseDto;
  students: StudentCourseDto[];
  totalLessons: number;
  completionByStudent: Record<number, number>; // studentId → completed count
}

export function TeacherStudentsPage() {
  const { email, name } = useAuth();
  const navigate = useNavigate();

  const [data, setData] = useState<CourseStudents[]>([]);
  const [loading, setLoading] = useState(true);
  const [expandedCourse, setExpandedCourse] = useState<number | null>(null);

  useEffect(() => {
    if (email) load();
  }, [email]);

  async function load() {
    try {
      setLoading(true);
      const userId = await getCatalogUserId(email!, name);
      if (!userId) return;

      const courses = await coursesApi.getByInstructor(userId);

      const withData = await Promise.all(
        courses.map(async course => {
          const [students, courseDetail, progressList] = await Promise.all([
            coursesApi.getStudentsByCourse(course.id).catch(() => [] as StudentCourseDto[]),
            coursesApi.getById(course.id).catch(() => null),
            progressApi.getByCourse(course.id).catch(() => []),
          ]);

          const totalLessons = courseDetail?.modules.reduce((s, m) => s + m.lessons.length, 0) ?? 0;

          const completionByStudent: Record<number, number> = {};
          for (const p of progressList) {
            completionByStudent[p.studentId] = (completionByStudent[p.studentId] ?? 0) + 1;
          }

          return { course, students, totalLessons, completionByStudent };
        })
      );

      setData(withData);
      if (withData.length > 0) setExpandedCourse(withData[0].course.id);
    } catch {
      // silently fail
    } finally {
      setLoading(false);
    }
  }

  const totalStudents = data.reduce((sum, d) => sum + d.students.length, 0);

  if (loading) {
    return (
      <Layout>
        <div className="max-w-3xl animate-pulse space-y-4">
          <div className="h-7 bg-gray-200 rounded w-48 mb-6" />
          {[1, 2].map(i => (
            <div key={i} className="bg-white rounded-xl border p-5 space-y-3">
              <div className="h-4 bg-gray-100 rounded w-1/3" />
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
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-900">Students</h1>
          <p className="text-gray-500 mt-1 text-sm">{totalStudents} enrolled across {data.length} course{data.length !== 1 ? 's' : ''}</p>
        </div>

        {data.length === 0 ? (
          <div className="text-center py-16 text-gray-400 text-sm bg-white rounded-xl border border-gray-100">
            No courses found.
          </div>
        ) : (
          <div className="space-y-4">
            {data.map(({ course, students, totalLessons, completionByStudent }) => (
              <div key={course.id} className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
                <button
                  onClick={() => setExpandedCourse(expandedCourse === course.id ? null : course.id)}
                  className="w-full flex items-center justify-between px-5 py-4 hover:bg-gray-50 transition-colors text-left"
                >
                  <div className="flex items-center gap-3">
                    <span className="text-xs font-mono text-indigo-500 bg-indigo-50 px-2 py-0.5 rounded">{course.code}</span>
                    <span className="font-semibold text-gray-900">{course.title}</span>
                  </div>
                  <div className="flex items-center gap-3">
                    <span className="text-sm text-gray-500">{students.length} student{students.length !== 1 ? 's' : ''}</span>
                    <svg
                      className={`w-4 h-4 text-gray-400 transition-transform ${expandedCourse === course.id ? 'rotate-180' : ''}`}
                      fill="none" viewBox="0 0 24 24" stroke="currentColor"
                    >
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                    </svg>
                  </div>
                </button>

                {expandedCourse === course.id && (
                  <div className="border-t border-gray-100">
                    {students.length === 0 ? (
                      <p className="text-sm text-gray-400 italic px-5 py-4">No students enrolled yet.</p>
                    ) : (
                      <div className="divide-y divide-gray-50">
                        {students.map(sc => {
                          const user = sc.user;
                          const displayName = user
                            ? `${user.firstName} ${user.lastName}`.trim() || user.nickname
                            : `User #${sc.userId}`;
                          const initials = displayName.split(' ').map(p => p[0]).join('').toUpperCase().slice(0, 2);
                          const enrolledDate = new Date(sc.enrolledAt).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
                          const completed = completionByStudent[sc.userId] ?? 0;
                          const pct = totalLessons > 0 ? Math.round((completed / totalLessons) * 100) : 0;

                          return (
                            <div key={sc.userId} className="flex items-center gap-4 px-5 py-3 hover:bg-gray-50 transition-colors">
                              <div className="w-9 h-9 rounded-full bg-indigo-100 text-indigo-600 text-sm flex items-center justify-center shrink-0 font-semibold">
                                {initials}
                              </div>
                              <div className="flex-1 min-w-0">
                                <p className="text-sm font-medium text-gray-900 truncate">{displayName}</p>
                                <p className="text-xs text-gray-400 truncate">
                                  {user?.email ?? ''}
                                  {user?.nickname ? <span className="ml-1 text-gray-300">@{user.nickname}</span> : null}
                                </p>
                                {totalLessons > 0 && (
                                  <div className="flex items-center gap-2 mt-1">
                                    <div className="flex-1 h-1.5 bg-gray-100 rounded-full overflow-hidden max-w-[120px]">
                                      <div
                                        className="h-1.5 bg-indigo-400 rounded-full transition-all"
                                        style={{ width: `${pct}%` }}
                                      />
                                    </div>
                                    <span className="text-xs text-gray-400">{completed}/{totalLessons} lessons</span>
                                  </div>
                                )}
                              </div>
                              <div className="text-right shrink-0">
                                <p className="text-xs text-gray-400">Enrolled</p>
                                <p className="text-xs text-gray-600 font-medium">{enrolledDate}</p>
                                {totalLessons > 0 && (
                                  <p className={`text-xs font-semibold mt-0.5 ${pct === 100 ? 'text-green-600' : 'text-indigo-500'}`}>{pct}%</p>
                                )}
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    )}
                    <div className="px-5 py-3 border-t border-gray-50">
                      <button
                        onClick={() => navigate(`/teacher/courses/${course.id}`)}
                        className="text-xs text-indigo-500 hover:text-indigo-700 font-medium"
                      >
                        Manage course →
                      </button>
                    </div>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </Layout>
  );
}
