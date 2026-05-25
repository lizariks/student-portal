import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Layout } from '../components/Layout';
import { coursesApi } from '../api/courses';
import { getCatalogUserId } from '../api/users';
import { useAuth } from '../auth/useAuth';
import type { CourseDetailsDto, LessonDetailDto, MaterialDto } from '../types/course';

export function CourseDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { email, name } = useAuth();
  const navigate = useNavigate();
  const courseId = parseInt(id ?? '0', 10);

  const [course, setCourse] = useState<CourseDetailsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [isEnrolled, setIsEnrolled] = useState(false);
  const [enrollLoading, setEnrollLoading] = useState(false);
  const [enrollError, setEnrollError] = useState<string | null>(null);

  const [expandedModule, setExpandedModule] = useState<number | null>(null);
  const [expandedLesson, setExpandedLesson] = useState<number | null>(null);
  const [lessonDetail, setLessonDetail] = useState<LessonDetailDto | null>(null);
  const [lessonLoading, setLessonLoading] = useState(false);

  useEffect(() => {
    if (!courseId) return;
    loadCourse();
  }, [courseId]);

  async function loadCourse() {
    try {
      setLoading(true);
      setError(null);
      const data = await coursesApi.getById(courseId);
      setCourse(data);

      if (email) {
        const userId = await getCatalogUserId(email, name);
        if (userId) {
          const enrolled = await coursesApi.checkEnrollment(userId, courseId);
          setIsEnrolled(enrolled);
        }
      }
    } catch {
      setError('Failed to load course.');
    } finally {
      setLoading(false);
    }
  }

  async function handleEnroll() {
    if (!email) return;
    setEnrollLoading(true);
    setEnrollError(null);
    try {
      const userId = await getCatalogUserId(email, name);
      if (!userId) {
        setEnrollError('Your account was not found in the course catalog. Please contact support.');
        return;
      }
      if (isEnrolled) {
        await coursesApi.unenroll(userId, courseId);
        setIsEnrolled(false);
      } else {
        await coursesApi.enroll(userId, courseId);
        setIsEnrolled(true);
      }
    } catch {
      setEnrollError('Action failed. Please try again.');
    } finally {
      setEnrollLoading(false);
    }
  }

  async function handleLessonClick(lessonId: number) {
    if (expandedLesson === lessonId) {
      setExpandedLesson(null);
      setLessonDetail(null);
      return;
    }
    setExpandedLesson(lessonId);
    setLessonDetail(null);
    setLessonLoading(true);
    try {
      setLessonDetail(await coursesApi.getLesson(lessonId));
    } finally {
      setLessonLoading(false);
    }
  }

  if (loading) {
    return (
      <Layout>
        <div className="max-w-3xl animate-pulse space-y-4">
          <div className="h-4 bg-gray-100 rounded w-24 mb-6" />
          <div className="bg-white rounded-xl border border-gray-100 p-6 space-y-3">
            <div className="h-4 bg-gray-100 rounded w-16" />
            <div className="h-7 bg-gray-200 rounded w-2/3" />
            <div className="h-4 bg-gray-100 rounded w-1/3" />
            <div className="h-16 bg-gray-100 rounded" />
          </div>
        </div>
      </Layout>
    );
  }

  if (error || !course) {
    return (
      <Layout>
        <div className="max-w-3xl">
          <p className="text-sm text-red-600 mb-4">{error ?? 'Course not found.'}</p>
          <button onClick={() => navigate('/courses')} className="text-sm text-indigo-600 hover:underline">
            ← Back to courses
          </button>
        </div>
      </Layout>
    );
  }

  const sortedModules = [...(course.modules ?? [])].sort((a, b) => a.order - b.order);

  return (
    <Layout>
      <div className="max-w-3xl">
        <button
          onClick={() => navigate('/courses')}
          className="text-sm text-indigo-600 hover:underline mb-5 block"
        >
          ← Back to courses
        </button>

        {/* Course header card */}
        <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-6 mb-6">
          <div className="flex items-start gap-4">
            <div className="flex-1 min-w-0">
              <span className="inline-block text-xs font-mono text-indigo-500 bg-indigo-50 px-2 py-0.5 rounded mb-2">
                {course.code}
              </span>
              <h1 className="text-2xl font-bold text-gray-900">{course.title}</h1>
              {course.instructorName && (
                <p className="text-sm text-gray-500 mt-1">Instructor: {course.instructorName}</p>
              )}
              {course.description && (
                <p className="text-gray-600 mt-3 text-sm leading-relaxed">{course.description}</p>
              )}
            </div>

            <button
              onClick={handleEnroll}
              disabled={enrollLoading}
              className={`shrink-0 px-5 py-2 rounded-lg text-sm font-medium transition-colors disabled:opacity-50 ${
                isEnrolled
                  ? 'bg-gray-100 text-gray-700 hover:bg-red-50 hover:text-red-700 border border-gray-200'
                  : 'bg-indigo-600 text-white hover:bg-indigo-700'
              }`}
            >
              {enrollLoading ? '...' : isEnrolled ? 'Unenroll' : 'Enroll'}
            </button>
          </div>

          {enrollError && (
            <p className="mt-3 text-sm text-red-600">{enrollError}</p>
          )}

          {isEnrolled && (
            <div className="mt-4 flex items-center gap-2 text-sm text-green-700 bg-green-50 px-3 py-2 rounded-lg">
              <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
              </svg>
              You are enrolled in this course
            </div>
          )}
        </div>

        {/* Modules & Lessons */}
        {sortedModules.length > 0 ? (
          <div className="space-y-3">
            <h2 className="text-base font-semibold text-gray-700">Course Content</h2>
            {sortedModules.map(mod => (
              <div key={mod.id} className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
                <button
                  onClick={() => setExpandedModule(expandedModule === mod.id ? null : mod.id)}
                  className="w-full flex items-center justify-between px-5 py-4 text-left hover:bg-gray-50 transition-colors"
                >
                  <div>
                    <span className="font-medium text-gray-900">{mod.title}</span>
                    {mod.description && (
                      <span className="text-sm text-gray-400 ml-2">— {mod.description}</span>
                    )}
                  </div>
                  <div className="flex items-center gap-3 shrink-0 text-gray-400 text-sm">
                    <span>{mod.lessons.length} lesson{mod.lessons.length !== 1 ? 's' : ''}</span>
                    <svg
                      className={`w-4 h-4 transition-transform ${expandedModule === mod.id ? 'rotate-180' : ''}`}
                      fill="none" viewBox="0 0 24 24" stroke="currentColor"
                    >
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                    </svg>
                  </div>
                </button>

                {expandedModule === mod.id && (
                  <div className="border-t border-gray-100 divide-y divide-gray-50">
                    {[...mod.lessons].sort((a, b) => a.order - b.order).map(lesson => (
                      <div key={lesson.id}>
                        <button
                          onClick={() => handleLessonClick(lesson.id)}
                          className="w-full flex items-center gap-3 px-5 py-3 text-left hover:bg-indigo-50 transition-colors"
                        >
                          <div className="w-6 h-6 rounded-full bg-indigo-100 text-indigo-600 text-xs flex items-center justify-center shrink-0 font-medium">
                            {lesson.order}
                          </div>
                          <span className="text-sm text-gray-700 flex-1">{lesson.title}</span>
                          <svg
                            className={`w-3.5 h-3.5 text-gray-400 transition-transform ${expandedLesson === lesson.id ? 'rotate-180' : ''}`}
                            fill="none" viewBox="0 0 24 24" stroke="currentColor"
                          >
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                          </svg>
                        </button>

                        {expandedLesson === lesson.id && (
                          <div className="px-14 py-4 bg-gray-50 border-t border-gray-100">
                            {lessonLoading ? (
                              <div className="animate-pulse space-y-2">
                                <div className="h-3 bg-gray-200 rounded w-3/4" />
                                <div className="h-3 bg-gray-200 rounded w-1/2" />
                              </div>
                            ) : lessonDetail ? (
                              <LessonContent detail={lessonDetail} />
                            ) : null}
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                )}
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-10 text-gray-400 text-sm">
            No modules available yet.
          </div>
        )}
      </div>
    </Layout>
  );
}

function LessonContent({ detail }: { detail: LessonDetailDto }) {
  return (
    <div className="space-y-3">
      {detail.content && (
        <p className="text-sm text-gray-700 leading-relaxed">{detail.content}</p>
      )}
      {detail.estimatedDuration && (
        <p className="text-xs text-gray-400">Duration: {detail.estimatedDuration}</p>
      )}
      {detail.materials.length > 0 && (
        <div>
          <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-2">Materials</p>
          <div className="space-y-1.5">
            {[...detail.materials].sort((a, b) => a.order - b.order).map(m => (
              <div key={m.id} className="flex items-center gap-2">
                <MaterialTypeIcon type={m.type} />
                {m.url ? (
                  <a
                    href={m.url}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-sm text-indigo-600 hover:underline"
                  >
                    {m.title}
                  </a>
                ) : (
                  <span className="text-sm text-gray-700">{m.title}</span>
                )}
              </div>
            ))}
          </div>
        </div>
      )}
      {!detail.content && detail.materials.length === 0 && (
        <p className="text-sm text-gray-400 italic">No content available for this lesson.</p>
      )}
    </div>
  );
}

function MaterialTypeIcon({ type }: { type: MaterialDto['type'] }) {
  const cls = 'w-4 h-4 shrink-0 text-gray-400';
  switch (type) {
    case 'Video':
      return (
        <svg className={cls} fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 10l4.553-2.276A1 1 0 0121 8.723v6.554a1 1 0 01-1.447.894L15 14M3 8a2 2 0 012-2h8a2 2 0 012 2v8a2 2 0 01-2 2H5a2 2 0 01-2-2V8z" />
        </svg>
      );
    case 'Link':
      return (
        <svg className={cls} fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1" />
        </svg>
      );
    case 'Quiz':
      return (
        <svg className={cls} fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
        </svg>
      );
    default:
      return (
        <svg className={cls} fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
      );
  }
}