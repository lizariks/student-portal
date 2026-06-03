import { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Layout } from '../../components/Layout';
import { coursesApi } from '../../api/courses';
import { CourseDiscussions } from '../../components/CourseDiscussions';
import type { CourseDetailsDto, ModuleDto, LessonDetailDto, MaterialDto, StudentCourseDto } from '../../types/course';

type MaterialType = 'Video' | 'Image' | 'Link' | 'HtmlContent' | 'File' | 'Quiz';

export function TeacherCoursePage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const courseId = Number(id);

  const [course, setCourse] = useState<CourseDetailsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  // Course edit
  const [editingCourse, setEditingCourse] = useState(false);
  const [courseForm, setCourseForm] = useState({ title: '', description: '', imageUrl: '' });
  const imageInputRef = useRef<HTMLInputElement>(null) as React.RefObject<HTMLInputElement>;
  const [imageUploading, setImageUploading] = useState(false);

  // Module forms
  const [newModuleTitle, setNewModuleTitle] = useState('');
  const [newModuleDesc, setNewModuleDesc] = useState('');
  const [addingModule, setAddingModule] = useState(false);
  const [editingModule, setEditingModule] = useState<number | null>(null);
  const [moduleForm, setModuleForm] = useState({ title: '', description: '' });

  // Lesson forms
  const [expandedModule, setExpandedModule] = useState<number | null>(null);
  const [lessonDetails, setLessonDetails] = useState<Record<number, LessonDetailDto>>({});
  const [addingLesson, setAddingLesson] = useState<number | null>(null);
  const [lessonForm, setLessonForm] = useState({ title: '', content: '', estimatedDuration: '' });
  const [editingLesson, setEditingLesson] = useState<number | null>(null);

  // Students
  const [students, setStudents] = useState<StudentCourseDto[]>([]);
  const [studentsLoading, setStudentsLoading] = useState(false);
  const [studentsExpanded, setStudentsExpanded] = useState(false);

  // Material forms
  const [expandedLesson, setExpandedLesson] = useState<number | null>(null);
  const [addingMaterial, setAddingMaterial] = useState<number | null>(null);
  const [materialForm, setMaterialForm] = useState<{ title: string; type: MaterialType; url: string; content: string }>({ title: '', type: 'Link', url: '', content: '' });
  const [editingMaterial, setEditingMaterial] = useState<number | null>(null);
  const materialImageRef = useRef<HTMLInputElement>(null) as React.RefObject<HTMLInputElement>;

  function apiError(e: unknown): string {
    if (e instanceof Error) return e.message;
    try { return JSON.stringify(e); } catch { return String(e); }
  }

  useEffect(() => { load(); }, [courseId]);

  async function load() {
    try {
      const data = await coursesApi.getById(courseId);
      setCourse(data);
      setCourseForm({ title: data.title, description: data.description ?? '', imageUrl: data.imageUrl ?? '' });
      return data;
    } catch {
      return null;
    } finally {
      setLoading(false);
    }
  }

  async function loadStudents() {
    setStudentsLoading(true);
    try {
      const data = await coursesApi.getStudentsByCourse(courseId);
      setStudents(data);
    } catch {
      // non-critical; section stays collapsed
    } finally {
      setStudentsLoading(false);
    }
  }

  async function loadLesson(lessonId: number) {
    try {
      const data = await coursesApi.getLesson(lessonId);
      setLessonDetails(prev => ({ ...prev, [lessonId]: data }));
    } catch { /* ignore */ }
  }

  async function saveCourse() {
    if (!course) return;
    setSaving(true);
    setError(null);
    try {
      await coursesApi.updateCourse(courseId, {
        code: course.code,
        title: courseForm.title,
        description: courseForm.description || undefined,
        imageUrl: courseForm.imageUrl || undefined,
        isPublished: course.isPublished,
        instructorId: course.instructorId,
      });
      setEditingCourse(false);
      await load();
    } catch (e) {
      setError(apiError(e));
      console.error('saveCourse', e);
    } finally {
      setSaving(false);
    }
  }

  async function handleImageUpload(file: File) {
    setImageUploading(true);
    try {
      const { url } = await coursesApi.uploadImage(file);
      setCourseForm(f => ({ ...f, imageUrl: url }));
    } finally {
      setImageUploading(false);
    }
  }

  async function addModule() {
    if (!newModuleTitle.trim() || !course) return;
    setSaving(true);
    setError(null);
    try {
      const nextModuleOrder = Math.max(0, ...course.modules.map(m => m.order)) + 1;
      await coursesApi.createModule({ title: newModuleTitle.trim(), description: newModuleDesc || undefined, order: nextModuleOrder, courseId });
      setNewModuleTitle('');
      setNewModuleDesc('');
      setAddingModule(false);
      await load();
    } catch (e) {
      setError(apiError(e));
      console.error('addModule', e);
    } finally {
      setSaving(false);
    }
  }

  async function saveModule(mod: ModuleDto) {
    setSaving(true);
    setError(null);
    try {
      await coursesApi.updateModule(mod.id, { title: moduleForm.title, description: moduleForm.description || undefined, order: mod.order, courseId });
      setEditingModule(null);
      await load();
    } catch (e) {
      setError(apiError(e));
      console.error('saveModule', e);
    } finally {
      setSaving(false);
    }
  }

  async function deleteModule(modId: number) {
    if (!confirm('Delete this module and all its lessons?')) return;
    setError(null);
    try {
      await coursesApi.deleteModule(modId);
      await load();
    } catch (e) {
      setError(apiError(e));
      console.error('deleteModule', e);
    }
  }

  async function toggleModule(modId: number) {
    if (expandedModule === modId) { setExpandedModule(null); return; }
    setExpandedModule(modId);
    const mod = course?.modules.find(m => m.id === modId);
    if (mod) {
      await Promise.all(mod.lessons.map(l => loadLesson(l.id)));
    }
  }

  async function addLesson(moduleId: number) {
    if (!lessonForm.title.trim()) return;
    setSaving(true);
    setError(null);
    try {
      const freshCourse = await coursesApi.getById(courseId);
      const mod = freshCourse.modules.find(m => m.id === moduleId);
      const nextLessonOrder = Math.max(0, ...(mod?.lessons.map(l => l.order) ?? [])) + 1;
      const created = await coursesApi.createLesson({ moduleId, title: lessonForm.title.trim(), content: lessonForm.content || undefined, order: nextLessonOrder, estimatedDuration: lessonForm.estimatedDuration || undefined });
      setLessonForm({ title: '', content: '', estimatedDuration: '' });
      setAddingLesson(null);
      await loadLesson(created.id);
      await load();
    } catch (e) {
      setError(apiError(e));
      console.error('addLesson', e);
    } finally {
      setSaving(false);
    }
  }

  async function saveLesson(lessonId: number, moduleId: number, lessonOrder: number) {
    setSaving(true);
    setError(null);
    try {
      await coursesApi.updateLesson(lessonId, { moduleId, title: lessonForm.title, content: lessonForm.content || undefined, order: lessonOrder, estimatedDuration: lessonForm.estimatedDuration || undefined });
      setEditingLesson(null);
      await load();
      await loadLesson(lessonId);
    } catch (e) {
      setError(apiError(e));
      console.error('saveLesson', e);
    } finally {
      setSaving(false);
    }
  }

  async function deleteLesson(lessonId: number) {
    if (!confirm('Delete this lesson and all its materials?')) return;
    setError(null);
    try {
      await coursesApi.deleteLesson(lessonId);
      await load();
    } catch (e) {
      setError(apiError(e));
      console.error('deleteLesson', e);
    }
  }

  async function toggleLesson(lessonId: number) {
    if (expandedLesson === lessonId) { setExpandedLesson(null); return; }
    setExpandedLesson(lessonId);
    if (!lessonDetails[lessonId]) await loadLesson(lessonId);
  }

  async function saveMaterial(lessonId: number, existingId?: number) {
    if (!materialForm.title.trim()) return;
    const urlValue = materialForm.type === 'HtmlContent' || materialForm.type === 'Quiz'
      ? materialForm.content
      : materialForm.url;
    const lesson = lessonDetails[lessonId];
    setSaving(true);
    setError(null);
    try {
      if (existingId) {
        await coursesApi.updateMaterial(existingId, { lessonId, title: materialForm.title, url: urlValue || undefined, type: materialForm.type, order: lesson?.materials.find(m => m.id === existingId)?.order ?? 1 });
        setEditingMaterial(null);
      } else {
        await coursesApi.createMaterial({ lessonId, title: materialForm.title, url: urlValue || undefined, type: materialForm.type, order: (lesson?.materials.length ?? 0) + 1 });
        setAddingMaterial(null);
        setMaterialForm({ title: '', type: 'Link', url: '', content: '' });
      }
      await loadLesson(lessonId);
    } catch (e) {
      setError(apiError(e));
      console.error('saveMaterial', e);
    } finally {
      setSaving(false);
    }
  }

  async function deleteMaterial(materialId: number, lessonId: number) {
    if (!confirm('Delete this material?')) return;
    setError(null);
    try {
      await coursesApi.deleteMaterial(materialId);
      await loadLesson(lessonId);
    } catch (e) {
      setError(apiError(e));
      console.error('deleteMaterial', e);
    }
  }

  async function handleMaterialImageUpload(file: File) {
    setImageUploading(true);
    try {
      const { url } = await coursesApi.uploadImage(file);
      setMaterialForm(f => ({ ...f, url }));
    } finally {
      setImageUploading(false);
    }
  }

  function openEditMaterial(mat: MaterialDto) {
    setEditingMaterial(mat.id);
    setMaterialForm({
      title: mat.title,
      type: mat.type as MaterialType,
      url: (mat.type === 'HtmlContent' || mat.type === 'Quiz') ? '' : (mat.url ?? ''),
      content: (mat.type === 'HtmlContent' || mat.type === 'Quiz') ? (mat.url ?? '') : '',
    });
  }

  const typeIcon: Record<MaterialType, string> = {
    Video: '▶', Image: '🖼', Link: '🔗', HtmlContent: '📝', File: '📎', Quiz: '✏️',
  };

  if (loading) return <Layout><div className="animate-pulse h-10 bg-gray-100 rounded w-64" /></Layout>;
  if (!course) return <Layout><p className="text-red-500">Course not found.</p></Layout>;

  return (
    <Layout>
      <div className="max-w-3xl">
        {/* Back */}
        <button onClick={() => navigate(-1)} className="text-sm text-indigo-500 hover:text-indigo-700 mb-4 flex items-center gap-1">
          ← Back
        </button>

        {error && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700 flex items-start gap-2">
            <span className="shrink-0 font-bold">Error:</span>
            <span className="flex-1">{error}</span>
            <button onClick={() => setError(null)} className="shrink-0 text-red-400 hover:text-red-600">✕</button>
          </div>
        )}

        {/* Course header */}
        <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-6 mb-6">
          {editingCourse ? (
            <div className="space-y-3">
              <input value={courseForm.title} onChange={e => setCourseForm(f => ({ ...f, title: e.target.value }))}
                className="w-full text-lg font-semibold border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300" placeholder="Course title" />
              <textarea value={courseForm.description} onChange={e => setCourseForm(f => ({ ...f, description: e.target.value }))}
                className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300 resize-none" rows={3} placeholder="Description" />
              {/* Image upload */}
              <div className="flex items-center gap-3">
                {courseForm.imageUrl && <img src={courseForm.imageUrl} alt="preview" className="w-20 h-14 object-cover rounded-lg border" />}
                <div>
                  <input ref={imageInputRef} type="file" accept="image/*" className="hidden"
                    onChange={e => e.target.files?.[0] && handleImageUpload(e.target.files[0])} />
                  <button onClick={() => imageInputRef.current?.click()} disabled={imageUploading}
                    className="text-sm px-3 py-1.5 rounded-lg border border-gray-200 hover:bg-gray-50 disabled:opacity-50">
                    {imageUploading ? 'Uploading…' : courseForm.imageUrl ? 'Change image' : 'Upload image'}
                  </button>
                  {courseForm.imageUrl && (
                    <button onClick={() => setCourseForm(f => ({ ...f, imageUrl: '' }))} className="ml-2 text-xs text-red-400 hover:text-red-600">Remove</button>
                  )}
                </div>
              </div>
              <div className="flex gap-2 pt-1">
                <button onClick={saveCourse} disabled={saving} className="text-sm px-4 py-1.5 rounded-lg bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50">
                  {saving ? 'Saving…' : 'Save'}
                </button>
                <button onClick={() => setEditingCourse(false)} className="text-sm px-4 py-1.5 rounded-lg border border-gray-200 hover:bg-gray-50">Cancel</button>
              </div>
            </div>
          ) : (
            <div className="flex gap-4">
              {course.imageUrl && <img src={course.imageUrl} alt={course.title} className="w-28 h-20 object-cover rounded-lg border shrink-0" />}
              <div className="flex-1">
                <div className="flex items-start justify-between">
                  <div>
                    <span className="text-xs font-mono text-indigo-500 bg-indigo-50 px-2 py-0.5 rounded">{course.code}</span>
                    <h1 className="text-xl font-bold text-gray-900 mt-1">{course.title}</h1>
                    {course.description && <p className="text-sm text-gray-500 mt-1">{course.description}</p>}
                  </div>
                  <button onClick={() => { setEditingCourse(true); setCourseForm({ title: course.title, description: course.description ?? '', imageUrl: course.imageUrl ?? '' }); }}
                    className="text-sm text-indigo-500 hover:text-indigo-700 shrink-0 ml-4">Edit</button>
                </div>
                <span className={`mt-3 inline-block text-xs px-2 py-0.5 rounded-full font-medium ${course.isPublished ? 'bg-green-50 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                  {course.isPublished ? 'Published' : 'Draft'}
                </span>
              </div>
            </div>
          )}
        </div>

        {/* Modules */}
        <div className="flex items-center justify-between mb-3">
          <h2 className="text-base font-semibold text-gray-700">Modules</h2>
          <button onClick={() => setAddingModule(v => !v)} className="text-sm px-3 py-1.5 rounded-lg bg-indigo-600 text-white hover:bg-indigo-700">
            {addingModule ? 'Cancel' : '+ Module'}
          </button>
        </div>

        {addingModule && (
          <div className="bg-white rounded-xl border border-indigo-100 p-4 mb-3 space-y-2">
            <input value={newModuleTitle} onChange={e => setNewModuleTitle(e.target.value)} placeholder="Module title"
              className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300" />
            <input value={newModuleDesc} onChange={e => setNewModuleDesc(e.target.value)} placeholder="Description (optional)"
              className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300" />
            <button onClick={addModule} disabled={saving || !newModuleTitle.trim()}
              className="text-sm px-4 py-1.5 rounded-lg bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50">Add</button>
          </div>
        )}

        <div className="space-y-3">
          {course.modules.sort((a, b) => a.order - b.order).map(mod => (
            <div key={mod.id} className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
              {/* Module header */}
              <div className="flex items-center gap-2 px-4 py-3">
                <button onClick={() => toggleModule(mod.id)} className="flex-1 text-left">
                  {editingModule === mod.id ? (
                    <span className="text-sm font-medium text-gray-400">Editing…</span>
                  ) : (
                    <div>
                      <span className="font-medium text-gray-900">{mod.title}</span>
                      {mod.description && <span className="ml-2 text-xs text-gray-400">{mod.description}</span>}
                    </div>
                  )}
                </button>
                <button onClick={() => { setEditingModule(mod.id); setModuleForm({ title: mod.title, description: mod.description ?? '' }); }}
                  className="text-xs text-indigo-500 hover:text-indigo-700">Edit</button>
                <button onClick={() => deleteModule(mod.id)} className="text-xs text-red-400 hover:text-red-600">Delete</button>
                <span className="text-gray-300 text-sm">{expandedModule === mod.id ? '▲' : '▼'}</span>
              </div>

              {/* Module edit form */}
              {editingModule === mod.id && (
                <div className="px-4 pb-3 border-t border-gray-50 pt-3 space-y-2">
                  <input value={moduleForm.title} onChange={e => setModuleForm(f => ({ ...f, title: e.target.value }))} placeholder="Module title"
                    className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300" />
                  <input value={moduleForm.description} onChange={e => setModuleForm(f => ({ ...f, description: e.target.value }))} placeholder="Description (optional)"
                    className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300" />
                  <div className="flex gap-2">
                    <button onClick={() => saveModule(mod)} disabled={saving} className="text-sm px-3 py-1.5 rounded-lg bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50">Save</button>
                    <button onClick={() => setEditingModule(null)} className="text-sm px-3 py-1.5 rounded-lg border border-gray-200 hover:bg-gray-50">Cancel</button>
                  </div>
                </div>
              )}

              {/* Lessons */}
              {expandedModule === mod.id && (
                <div className="border-t border-gray-100 px-4 py-3 space-y-2">
                  {mod.lessons.sort((a, b) => a.order - b.order).map(ls => {
                    const detail = lessonDetails[ls.id];
                    return (
                      <div key={ls.id} className="border border-gray-100 rounded-lg overflow-hidden">
                        {/* Lesson header */}
                        <div className="flex items-center gap-2 px-3 py-2 bg-gray-50">
                          <button onClick={() => toggleLesson(ls.id)} className="flex-1 text-left text-sm font-medium text-gray-800">{ls.title}</button>
                          <button onClick={() => { setEditingLesson(ls.id); setLessonForm({ title: detail?.title ?? ls.title, content: detail?.content ?? '', estimatedDuration: '' }); }}
                            className="text-xs text-indigo-500 hover:text-indigo-700">Edit</button>
                          <button onClick={() => deleteLesson(ls.id)} className="text-xs text-red-400 hover:text-red-600">Delete</button>
                          <span className="text-gray-400 text-xs">{expandedLesson === ls.id ? '▲' : '▼'}</span>
                        </div>

                        {/* Lesson edit form */}
                        {editingLesson === ls.id && (
                          <div className="px-3 py-2 border-t border-gray-100 space-y-2 bg-white">
                            <input value={lessonForm.title} onChange={e => setLessonForm(f => ({ ...f, title: e.target.value }))} placeholder="Lesson title"
                              className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300" />
                            <textarea value={lessonForm.content} onChange={e => setLessonForm(f => ({ ...f, content: e.target.value }))} placeholder="Lesson content / description"
                              className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300 resize-none" rows={3} />
                            <input value={lessonForm.estimatedDuration} onChange={e => setLessonForm(f => ({ ...f, estimatedDuration: e.target.value }))} placeholder="Duration e.g. 00:30:00"
                              className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300" />
                            <div className="flex gap-2">
                              <button onClick={() => saveLesson(ls.id, mod.id, ls.order)} disabled={saving} className="text-sm px-3 py-1.5 rounded-lg bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50">Save</button>
                              <button onClick={() => setEditingLesson(null)} className="text-sm px-3 py-1.5 rounded-lg border border-gray-200 hover:bg-gray-50">Cancel</button>
                            </div>
                          </div>
                        )}

                        {/* Materials */}
                        {expandedLesson === ls.id && (
                          <div className="px-3 py-2 border-t border-gray-100 bg-white space-y-2">
                            {detail?.materials.sort((a, b) => a.order - b.order).map(mat => (
                              <div key={mat.id} className="flex items-center gap-2 text-sm">
                                <span className="text-base">{typeIcon[mat.type as MaterialType] ?? '📄'}</span>
                                <span className="flex-1 text-gray-700 truncate">{mat.title}</span>
                                <span className="text-xs text-gray-400">{mat.type}</span>
                                <button onClick={() => openEditMaterial(mat)} className="text-xs text-indigo-500 hover:text-indigo-700">Edit</button>
                                <button onClick={() => deleteMaterial(mat.id, ls.id)} className="text-xs text-red-400 hover:text-red-600">Delete</button>
                              </div>
                            ))}

                            {/* Edit material form */}
                            {editingMaterial !== null && detail?.materials.some(m => m.id === editingMaterial) && (
                              <MaterialForm
                                form={materialForm} setForm={setMaterialForm}
                                onSave={() => saveMaterial(ls.id, editingMaterial!)}
                                onCancel={() => setEditingMaterial(null)}
                                saving={saving} imageUploading={imageUploading}
                                imageRef={materialImageRef}
                                onImageUpload={handleMaterialImageUpload}
                              />
                            )}

                            {/* Add material */}
                            {addingMaterial === ls.id ? (
                              <MaterialForm
                                form={materialForm} setForm={setMaterialForm}
                                onSave={() => saveMaterial(ls.id)}
                                onCancel={() => { setAddingMaterial(null); setMaterialForm({ title: '', type: 'Link', url: '', content: '' }); }}
                                saving={saving} imageUploading={imageUploading}
                                imageRef={materialImageRef}
                                onImageUpload={handleMaterialImageUpload}
                              />
                            ) : (
                              <button onClick={() => { setAddingMaterial(ls.id); setEditingMaterial(null); setMaterialForm({ title: '', type: 'Link', url: '', content: '' }); }}
                                className="text-xs text-indigo-500 hover:text-indigo-700">+ Add material</button>
                            )}
                          </div>
                        )}
                      </div>
                    );
                  })}

                  {/* Add lesson */}
                  {addingLesson === mod.id ? (
                    <div className="border border-indigo-100 rounded-lg p-3 space-y-2">
                      <input value={lessonForm.title} onChange={e => setLessonForm(f => ({ ...f, title: e.target.value }))} placeholder="Lesson title"
                        className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300" />
                      <textarea value={lessonForm.content} onChange={e => setLessonForm(f => ({ ...f, content: e.target.value }))} placeholder="Content (optional)"
                        className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300 resize-none" rows={2} />
                      <div className="flex gap-2">
                        <button onClick={() => addLesson(mod.id)} disabled={saving || !lessonForm.title.trim()} className="text-sm px-3 py-1.5 rounded-lg bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50">Add</button>
                        <button onClick={() => { setAddingLesson(null); setLessonForm({ title: '', content: '', estimatedDuration: '' }); }} className="text-sm px-3 py-1.5 rounded-lg border border-gray-200 hover:bg-gray-50">Cancel</button>
                      </div>
                    </div>
                  ) : (
                    <button onClick={() => { setAddingLesson(mod.id); setLessonForm({ title: '', content: '', estimatedDuration: '' }); }}
                      className="text-xs text-indigo-500 hover:text-indigo-700">+ Add lesson</button>
                  )}
                </div>
              )}
            </div>
          ))}

          {course.modules.length === 0 && (
            <div className="text-center py-10 text-gray-400 text-sm bg-white rounded-xl border border-gray-100">
              No modules yet. Add one above.
            </div>
          )}
        </div>

        {/* Students */}
        <div className="mt-6">
          <button
            onClick={() => { setStudentsExpanded(v => !v); if (!studentsExpanded && students.length === 0) loadStudents(); }}
            className="w-full flex items-center justify-between px-4 py-3 bg-white rounded-xl border border-gray-100 shadow-sm hover:bg-gray-50 transition-colors"
          >
            <span className="text-base font-semibold text-gray-700">
              Students
              {students.length > 0 && <span className="ml-2 text-xs font-normal text-gray-400">{students.length} enrolled</span>}
            </span>
            <span className="text-gray-400 text-sm">{studentsExpanded ? '▲' : '▼'}</span>
          </button>

          {studentsExpanded && (
            <div className="mt-1 bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
              {studentsLoading ? (
                <div className="p-6 space-y-3">
                  {[1, 2, 3].map(i => <div key={i} className="h-10 bg-gray-100 rounded-lg animate-pulse" />)}
                </div>
              ) : students.length === 0 ? (
                <p className="p-6 text-sm text-gray-400 text-center">No students enrolled yet.</p>
              ) : (
                <div className="divide-y divide-gray-50">
                  {students.map(s => {
                    const fullName = s.user ? `${s.user.firstName} ${s.user.lastName}`.trim() : `User #${s.userId}`;
                    const initials = s.user ? (s.user.firstName[0] ?? '') + (s.user.lastName[0] ?? '') : '?';
                    const enrolledDate = new Date(s.enrolledAt).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
                    return (
                      <div key={s.userId} className="flex items-center gap-3 px-4 py-3">
                        <div className="w-8 h-8 rounded-full bg-indigo-100 text-indigo-600 text-xs font-semibold flex items-center justify-center shrink-0 uppercase">
                          {initials}
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-medium text-gray-900 truncate">{fullName}</p>
                          <p className="text-xs text-gray-400 truncate">
                            {s.user?.nickname && <span className="mr-2">@{s.user.nickname}</span>}
                            {s.user?.email}
                          </p>
                        </div>
                        <div className="text-right shrink-0">
                          <p className="text-xs text-gray-400">Enrolled</p>
                          <p className="text-xs font-medium text-gray-600">{enrolledDate}</p>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>
          )}
        </div>

        <CourseDiscussions courseId={courseId} />
      </div>
    </Layout>
  );
}

interface MaterialFormProps {
  form: { title: string; type: MaterialType; url: string; content: string };
  setForm: React.Dispatch<React.SetStateAction<{ title: string; type: MaterialType; url: string; content: string }>>;
  onSave: () => void;
  onCancel: () => void;
  saving: boolean;
  imageUploading: boolean;
  imageRef: React.RefObject<HTMLInputElement>;
  onImageUpload: (f: File) => void;
}

function MaterialForm({ form, setForm, onSave, onCancel, saving, imageUploading, imageRef, onImageUpload }: MaterialFormProps) {
  return (
    <div className="border border-indigo-100 rounded-lg p-3 space-y-2 bg-indigo-50/30">
      <input value={form.title} onChange={e => setForm(f => ({ ...f, title: e.target.value }))} placeholder="Material title"
        className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300 bg-white" />
      <select value={form.type} onChange={e => setForm(f => ({ ...f, type: e.target.value as MaterialType, url: '', content: '' }))}
        className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300 bg-white">
        <option value="Video">Video (URL)</option>
        <option value="Image">Image (upload)</option>
        <option value="Link">Link</option>
        <option value="HtmlContent">Text / Exercise</option>
        <option value="File">File (URL)</option>
        <option value="Quiz">Quiz description</option>
      </select>

      {form.type === 'Image' ? (
        <div className="flex items-center gap-3">
          {form.url && <img src={form.url} alt="preview" className="w-16 h-12 object-cover rounded border" />}
          <div>
            <input ref={imageRef} type="file" accept="image/*" className="hidden"
              onChange={e => e.target.files?.[0] && onImageUpload(e.target.files[0])} />
            <button type="button" onClick={() => imageRef.current?.click()} disabled={imageUploading}
              className="text-sm px-3 py-1.5 rounded-lg border border-gray-200 bg-white hover:bg-gray-50 disabled:opacity-50">
              {imageUploading ? 'Uploading…' : form.url ? 'Change' : 'Upload image'}
            </button>
          </div>
        </div>
      ) : form.type === 'HtmlContent' || form.type === 'Quiz' ? (
        <textarea value={form.content} onChange={e => setForm(f => ({ ...f, content: e.target.value }))}
          placeholder={form.type === 'Quiz' ? 'Quiz description / instructions' : 'Content (supports plain text)'}
          className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300 bg-white resize-none" rows={4} />
      ) : (
        <input value={form.url} onChange={e => setForm(f => ({ ...f, url: e.target.value }))}
          placeholder={form.type === 'Video' ? 'YouTube or video URL' : 'URL'}
          className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300 bg-white" />
      )}

      <div className="flex gap-2">
        <button onClick={onSave} disabled={saving || !form.title.trim()} className="text-sm px-3 py-1.5 rounded-lg bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50">
          {saving ? 'Saving…' : 'Save'}
        </button>
        <button onClick={onCancel} className="text-sm px-3 py-1.5 rounded-lg border border-gray-200 bg-white hover:bg-gray-50">Cancel</button>
      </div>
    </div>
  );
}
