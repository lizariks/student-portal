import client from './client';

export interface LessonProgressDto {
  progressId: number;
  studentId: number;
  lessonId: number;
  courseId: number;
  completedAt: string;
}

export const progressApi = {
  getByStudentAndCourse: (studentId: number, courseId: number) =>
    client.get<LessonProgressDto[]>(`/lessonprogress/student/${studentId}/course/${courseId}`).then(r => r.data),

  getByCourse: (courseId: number) =>
    client.get<LessonProgressDto[]>(`/lessonprogress/course/${courseId}`).then(r => r.data),

  markComplete: (studentId: number, lessonId: number, courseId: number) =>
    client.post('/lessonprogress', { studentId, lessonId, courseId }),

  markIncomplete: (studentId: number, lessonId: number) =>
    client.delete('/lessonprogress', { params: { studentId, lessonId } }),
};
