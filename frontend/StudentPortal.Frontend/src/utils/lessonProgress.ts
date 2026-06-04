const KEY = 'sp_viewed_lessons';

function load(): Record<number, number[]> {
  try { return JSON.parse(localStorage.getItem(KEY) ?? '{}'); }
  catch { return {}; }
}

export function markLessonViewed(courseId: number, lessonId: number) {
  const data = load();
  if (!data[courseId]) data[courseId] = [];
  if (!data[courseId].includes(lessonId)) {
    data[courseId].push(lessonId);
    localStorage.setItem(KEY, JSON.stringify(data));
  }
}

export function getViewedCount(courseId: number): number {
  return load()[courseId]?.length ?? 0;
}