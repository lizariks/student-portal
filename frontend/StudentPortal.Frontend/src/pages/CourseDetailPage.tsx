import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Layout } from '../components/Layout';
import { coursesApi } from '../api/courses';
import { getCatalogUserId } from '../api/users';
import { progressApi } from '../api/progress';
import { useAuth } from '../auth/useAuth';
import type { CourseDetailsDto, LessonDetailDto, MaterialDto } from '../types/course';
import { CourseDiscussions } from '../components/CourseDiscussions';
import { isTeacher } from '../utils/roles';
import { markLessonViewed } from '../utils/lessonProgress';

export function CourseDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { email, name, roles } = useAuth();
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

  const [completedLessonIds, setCompletedLessonIds] = useState<Set<number>>(new Set());
  const [togglingLesson, setTogglingLesson] = useState<number | null>(null);
  const [catalogUserId, setCatalogUserId] = useState<number | null>(null);

  const teacher = isTeacher(roles);

  // Teacher — add module
  const [showAddModule, setShowAddModule] = useState(false);
  const [newModuleTitle, setNewModuleTitle] = useState('');
  const [newModuleDesc, setNewModuleDesc] = useState('');
  const [moduleSubmitting, setModuleSubmitting] = useState(false);
  const [moduleError, setModuleError] = useState<string | null>(null);

  // Teacher — add lesson (keyed by moduleId)
  const [addLessonFor, setAddLessonFor] = useState<number | null>(null);
  const [newLessonTitle, setNewLessonTitle] = useState('');
  const [newLessonContent, setNewLessonContent] = useState('');
  const [lessonSubmitting, setLessonSubmitting] = useState(false);
  const [lessonError, setLessonError] = useState<string | null>(null);

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
          setCatalogUserId(userId);
          const [enrolled, progress] = await Promise.all([
            coursesApi.checkEnrollment(userId, courseId),
            progressApi.getByStudentAndCourse(userId, courseId).catch(() => []),
          ]);
          setIsEnrolled(enrolled);
          setCompletedLessonIds(new Set(progress.map(p => p.lessonId)));
        }
      }
    } catch {
      setError('Failed to load course.');
    } finally {
      setLoading(false);
    }
  }

  async function handleToggleComplete(lessonId: number) {
    if (!catalogUserId) return;
    setTogglingLesson(lessonId);
    try {
      if (completedLessonIds.has(lessonId)) {
        await progressApi.markIncomplete(catalogUserId, lessonId);
        setCompletedLessonIds(prev => { const s = new Set(prev); s.delete(lessonId); return s; });
      } else {
        await progressApi.markComplete(catalogUserId, lessonId, courseId);
        setCompletedLessonIds(prev => new Set([...prev, lessonId]));
      }
    } catch {
      // silently ignore — UI stays in sync via optimistic update reversal if needed
    } finally {
      setTogglingLesson(null);
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

  async function handleCreateModule() {
    if (!newModuleTitle.trim()) return;
    setModuleSubmitting(true);
    setModuleError(null);
    try {
      const nextOrder = (course?.modules.length ?? 0) + 1;
      const created = await coursesApi.createModule({
        title: newModuleTitle.trim(),
        description: newModuleDesc.trim() || undefined,
        order: nextOrder,
        courseId,
      });
      setCourse(prev => prev ? {
        ...prev,
        modules: [...prev.modules, { ...created, lessons: [] }],
      } : prev);
      setNewModuleTitle('');
      setNewModuleDesc('');
      setShowAddModule(false);
    } catch {
      setModuleError('Failed to create module. Please try again.');
    } finally {
      setModuleSubmitting(false);
    }
  }

  async function handleCreateLesson(moduleId: number) {
    if (!newLessonTitle.trim()) return;
    setLessonSubmitting(true);
    setLessonError(null);
    try {
      const mod = course?.modules.find(m => m.id === moduleId);
      const nextOrder = (mod?.lessons.length ?? 0) + 1;
      const created = await coursesApi.createLesson({
        moduleId,
        title: newLessonTitle.trim(),
        content: newLessonContent.trim() || undefined,
        order: nextOrder,
      });
      setCourse(prev => prev ? {
        ...prev,
        modules: prev.modules.map(m =>
          m.id === moduleId
            ? { ...m, lessons: [...m.lessons, { id: created.id, title: created.title, order: created.order }] }
            : m
        ),
      } : prev);
      setNewLessonTitle('');
      setNewLessonContent('');
      setAddLessonFor(null);
    } catch {
      setLessonError('Failed to create lesson. Please try again.');
    } finally {
      setLessonSubmitting(false);
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
    markLessonViewed(courseId, lessonId);
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

            {!isTeacher(roles) && (
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
            )}
          </div>

          {enrollError && (
            <p className="mt-3 text-sm text-red-600">{enrollError}</p>
          )}

          {teacher && (
            <div className="mt-4 px-1 py-1 text-xs text-indigo-500 font-medium">
              Teaching this course
            </div>
          )}

        {isEnrolled && (() => {
          const totalLessons = course.modules.reduce((s, m) => s + m.lessons.length, 0);
          const completed = completedLessonIds.size;
          const pct = totalLessons > 0 ? Math.round((completed / totalLessons) * 100) : 0;
          return (
            <div className="mt-4 space-y-2">
              <div className="flex items-center gap-2 text-sm text-green-700 bg-green-50 px-3 py-2 rounded-lg">
                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                </svg>
                You are enrolled in this course
              </div>
              {totalLessons > 0 && (
                <div className="px-1">
                  <div className="flex items-center justify-between text-xs text-gray-500 mb-1">
                    <span>Progress</span>
                    <span className="font-semibold text-indigo-600">{completed}/{totalLessons} lessons · {pct}%</span>
                  </div>
                  <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
                    <div
                      className="h-2 bg-indigo-500 rounded-full transition-all duration-500"
                      style={{ width: `${pct}%` }}
                    />
                  </div>
                </div>
              )}
            </div>
          );
        })()}
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
                    {[...mod.lessons].sort((a, b) => a.order - b.order).map(lesson => {
                      const isDone = completedLessonIds.has(lesson.id);
                      return (
                      <div key={lesson.id}>
                        <div className="flex items-center">
                          <button
                            onClick={() => handleLessonClick(lesson.id)}
                            className="flex-1 flex items-center gap-3 px-5 py-3 text-left hover:bg-indigo-50 transition-colors"
                          >
                            <div className={`w-6 h-6 rounded-full text-xs flex items-center justify-center shrink-0 font-medium transition-colors ${isDone ? 'bg-green-100 text-green-600' : 'bg-indigo-100 text-indigo-600'}`}>
                              {isDone
                                ? <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M5 13l4 4L19 7" /></svg>
                                : lesson.order}
                            </div>
                            <span className={`text-sm flex-1 ${isDone ? 'text-gray-400 line-through' : 'text-gray-700'}`}>{lesson.title}</span>
                            <svg
                              className={`w-3.5 h-3.5 text-gray-400 transition-transform ${expandedLesson === lesson.id ? 'rotate-180' : ''}`}
                              fill="none" viewBox="0 0 24 24" stroke="currentColor"
                            >
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                            </svg>
                          </button>
                          {isEnrolled && !teacher && (
                            <button
                              onClick={() => handleToggleComplete(lesson.id)}
                              disabled={togglingLesson === lesson.id}
                              title={isDone ? 'Mark incomplete' : 'Mark complete'}
                              className={`mr-3 w-5 h-5 rounded border-2 flex items-center justify-center shrink-0 transition-colors disabled:opacity-40 ${isDone ? 'bg-green-500 border-green-500 text-white' : 'border-gray-300 hover:border-indigo-400'}`}
                            >
                              {isDone && <svg className="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" /></svg>}
                            </button>
                          )}
                        </div>

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
                      );
                    })}

                    {/* Teacher — add lesson */}
                    {teacher && (
                      <div className="px-5 py-3">
                        {addLessonFor === mod.id ? (
                          <div className="space-y-2">
                            <input
                              type="text"
                              placeholder="Lesson title…"
                              value={newLessonTitle}
                              onChange={e => setNewLessonTitle(e.target.value)}
                              onKeyDown={e => e.key === 'Enter' && handleCreateLesson(mod.id)}
                              autoFocus
                              className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300"
                            />
                            <textarea
                              placeholder="Content (optional)…"
                              value={newLessonContent}
                              onChange={e => setNewLessonContent(e.target.value)}
                              rows={2}
                              className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300 resize-none"
                            />
                            {lessonError && <p className="text-xs text-red-600">{lessonError}</p>}
                            <div className="flex gap-2">
                              <button
                                onClick={() => handleCreateLesson(mod.id)}
                                disabled={lessonSubmitting || !newLessonTitle.trim()}
                                className="text-xs px-3 py-1.5 rounded-lg bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50 transition-colors"
                              >
                                {lessonSubmitting ? 'Adding…' : 'Add lesson'}
                              </button>
                              <button
                                onClick={() => { setAddLessonFor(null); setNewLessonTitle(''); setNewLessonContent(''); setLessonError(null); }}
                                className="text-xs px-3 py-1.5 rounded-lg text-gray-500 hover:text-gray-700"
                              >
                                Cancel
                              </button>
                            </div>
                          </div>
                        ) : (
                          <button
                            onClick={() => { setAddLessonFor(mod.id); setExpandedModule(mod.id); }}
                            className="text-xs text-indigo-500 hover:text-indigo-700 font-medium flex items-center gap-1"
                          >
                            <span className="text-base leading-none">+</span> Add lesson
                          </button>
                        )}
                      </div>
                    )}
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

        {/* Teacher — add module */}
        {teacher && (
          <div className="mt-4">
            {showAddModule ? (
              <div className="bg-white rounded-xl border border-indigo-100 shadow-sm p-4 space-y-3">
                <input
                  type="text"
                  placeholder="Module title…"
                  value={newModuleTitle}
                  onChange={e => setNewModuleTitle(e.target.value)}
                  onKeyDown={e => e.key === 'Enter' && handleCreateModule()}
                  autoFocus
                  className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300"
                />
                <input
                  type="text"
                  placeholder="Description (optional)…"
                  value={newModuleDesc}
                  onChange={e => setNewModuleDesc(e.target.value)}
                  className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300"
                />
                {moduleError && <p className="text-xs text-red-600">{moduleError}</p>}
                <div className="flex gap-2">
                  <button
                    onClick={handleCreateModule}
                    disabled={moduleSubmitting || !newModuleTitle.trim()}
                    className="text-xs px-3 py-1.5 rounded-lg bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50 transition-colors"
                  >
                    {moduleSubmitting ? 'Adding…' : 'Add module'}
                  </button>
                  <button
                    onClick={() => { setShowAddModule(false); setNewModuleTitle(''); setNewModuleDesc(''); setModuleError(null); }}
                    className="text-xs px-3 py-1.5 rounded-lg text-gray-500 hover:text-gray-700"
                  >
                    Cancel
                  </button>
                </div>
              </div>
            ) : (
              <button
                onClick={() => setShowAddModule(true)}
                className="w-full py-3 rounded-xl border-2 border-dashed border-indigo-200 text-indigo-500 hover:border-indigo-400 hover:text-indigo-700 text-sm font-medium transition-colors flex items-center justify-center gap-1"
              >
                <span className="text-base leading-none">+</span> Add module
              </button>
            )}
          </div>
        )}

        {(isEnrolled || isTeacher(roles)) && <CourseDiscussions courseId={courseId} />}
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