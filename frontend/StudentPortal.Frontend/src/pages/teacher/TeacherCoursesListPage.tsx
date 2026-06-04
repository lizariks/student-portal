import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Layout } from '../../components/Layout';
import { coursesApi } from '../../api/courses';
import { getCatalogUserId } from '../../api/users';
import { useAuth } from '../../auth/useAuth';
import type { CourseDto } from '../../types/course';

export function TeacherCoursesListPage() {
  const { email, name } = useAuth();
  const navigate = useNavigate();

  const [courses, setCourses] = useState<CourseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);
  const [form, setForm] = useState({ code: '', title: '', description: '', isPublished: false });

  useEffect(() => {
    if (email) load();
  }, [email]);

  async function load() {
    try {
      setLoading(true);
      const userId = await getCatalogUserId(email!, name);
      if (!userId) return;
      const data = await coursesApi.getByInstructor(userId);
      setCourses(data);
    } catch {
      // silently fail
    } finally {
      setLoading(false);
    }
  }

  async function handleCreate() {
    if (!form.code.trim() || !form.title.trim()) return;
    setCreating(true);
    setCreateError(null);
    try {
      const userId = await getCatalogUserId(email!, name);
      const created = await coursesApi.createCourse({
        code: form.code.trim(),
        title: form.title.trim(),
        description: form.description.trim() || undefined,
        isPublished: form.isPublished,
        instructorId: userId ?? undefined,
      });
      setForm({ code: '', title: '', description: '', isPublished: false });
      setShowCreate(false);
      navigate(`/teacher/courses/${created.id}`);
    } catch {
      setCreateError('Failed to create course. Check that the code is unique.');
    } finally {
      setCreating(false);
    }
  }

  if (loading) {
    return (
      <Layout>
        <div className="max-w-4xl animate-pulse space-y-4">
          <div className="h-7 bg-gray-200 rounded w-48 mb-6" />
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {[1, 2, 3].map(i => (
              <div key={i} className="bg-white rounded-xl border p-5 h-28" />
            ))}
          </div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="max-w-4xl">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">My Courses</h1>
            <p className="text-gray-500 mt-1 text-sm">{courses.length} course{courses.length !== 1 ? 's' : ''}</p>
          </div>
          <button
            onClick={() => { setShowCreate(v => !v); setCreateError(null); }}
            className="px-4 py-2 rounded-lg bg-indigo-600 text-white text-sm font-medium hover:bg-indigo-700 transition-colors"
          >
            {showCreate ? 'Cancel' : '+ Create Course'}
          </button>
        </div>

        {showCreate && (
          <div className="bg-white rounded-xl border border-indigo-100 shadow-sm p-6 mb-6 space-y-4">
            <h2 className="text-base font-semibold text-gray-700">New Course</h2>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1">Code <span className="text-red-500">*</span></label>
                <input
                  type="text"
                  value={form.code}
                  onChange={e => setForm(p => ({ ...p, code: e.target.value }))}
                  maxLength={20}
                  placeholder="e.g. CS101"
                  className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300"
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1">Title <span className="text-red-500">*</span></label>
                <input
                  type="text"
                  value={form.title}
                  onChange={e => setForm(p => ({ ...p, title: e.target.value }))}
                  placeholder="Course title"
                  className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300"
                />
              </div>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">Description</label>
              <textarea
                value={form.description}
                onChange={e => setForm(p => ({ ...p, description: e.target.value }))}
                rows={2}
                placeholder="Optional description…"
                className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300 resize-none"
              />
            </div>
            <div className="flex items-center gap-3">
              <label className="flex items-center gap-2 cursor-pointer select-none">
                <input
                  type="checkbox"
                  checked={form.isPublished}
                  onChange={e => setForm(p => ({ ...p, isPublished: e.target.checked }))}
                  className="w-4 h-4 rounded accent-indigo-600"
                />
                <span className="text-sm text-gray-700">Publish immediately</span>
              </label>
            </div>
            {createError && <p className="text-xs text-red-600">{createError}</p>}
            <button
              onClick={handleCreate}
              disabled={creating || !form.code.trim() || !form.title.trim()}
              className="px-5 py-2 rounded-lg bg-indigo-600 text-white text-sm font-medium hover:bg-indigo-700 disabled:opacity-50 transition-colors"
            >
              {creating ? 'Creating…' : 'Create Course'}
            </button>
          </div>
        )}

        {courses.length === 0 ? (
          <div className="text-center py-16 text-gray-400 text-sm bg-white rounded-xl border border-gray-100">
            No courses yet. Create your first course above.
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {courses.map(course => (
              <div
                key={course.id}
                onClick={() => navigate(`/teacher/courses/${course.id}`)}
                className="bg-white rounded-xl border border-gray-100 shadow-sm p-5 cursor-pointer hover:border-indigo-200 hover:shadow-md transition-all"
              >
                <div className="flex items-start justify-between mb-2">
                  <span className="inline-block text-xs font-mono text-indigo-500 bg-indigo-50 px-2 py-0.5 rounded">
                    {course.code}
                  </span>
                  <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${course.isPublished ? 'bg-green-50 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                    {course.isPublished ? 'Published' : 'Draft'}
                  </span>
                </div>
                <h3 className="font-semibold text-gray-900 mb-1">{course.title}</h3>
                {course.description && (
                  <p className="text-xs text-gray-400 line-clamp-2">{course.description}</p>
                )}
                <p className="text-xs text-indigo-500 mt-3 font-medium">Manage course →</p>
              </div>
            ))}
          </div>
        )}
      </div>
    </Layout>
  );
}
