import client from './client';
import type { CourseDto, CourseDetailsDto, LessonDetailDto, StudentCourseDto } from '../types/course';

export const coursesApi = {
  getPublished: () =>
    client.get<CourseDto[]>('/catalog/published').then(r => r.data),

  search: (keyword: string) =>
    client.get<CourseDto[]>('/catalog/search', { params: { keyword } }).then(r => r.data),

  getById: (id: number) =>
    client.get<CourseDetailsDto>(`/catalog/${id}`).then(r => r.data),

  getLesson: (lessonId: number) =>
    client.get<LessonDetailDto>(`/lessons/${lessonId}`).then(r => r.data),

  checkEnrollment: (userId: number, courseId: number) =>
    client.get<boolean>('/studentcourses/check', { params: { userId, courseId } }).then(r => r.data),

  enroll: (userId: number, courseId: number) =>
    client.post('/studentcourses/enroll', { userId, courseId }),

  unenroll: (userId: number, courseId: number) =>
    client.delete('/studentcourses/unenroll', { params: { userId, courseId } }),

  getUserEnrollments: (userId: number) =>
    client.get<StudentCourseDto[]>(`/studentcourses/user/${userId}`).then(r => r.data),

  getByInstructor: (instructorId: number) =>
    client.get<CourseDto[]>(`/catalog/instructor/${instructorId}`).then(r => r.data),

  getEnrollmentCount: (courseId: number) =>
    client.get<number>(`/studentcourses/course/${courseId}/count`).then(r => r.data),

  updateCourse: (id: number, dto: { code: string; title: string; description?: string; imageUrl?: string; isPublished: boolean; instructorId?: number }) =>
    client.put<CourseDto>(`/catalog/${id}`, dto).then(r => r.data),

  uploadImage: (file: File) => {
    const form = new FormData();
    form.append('file', file);
    return client.post<{ url: string }>('/upload/image', form, { headers: { 'Content-Type': 'multipart/form-data' } }).then(r => r.data);
  },

  createModule: (dto: { title: string; description?: string; order: number; courseId: number }) =>
    client.post<{ id: number; title: string; description?: string; order: number; courseId: number; lessons: [] }>('/modules', dto).then(r => r.data),

  updateModule: (id: number, dto: { title: string; description?: string; order: number; courseId: number }) =>
    client.put(`/modules/${id}`, { ...dto, id }),

  deleteModule: (id: number) =>
    client.delete(`/modules/${id}`),

  createLesson: (dto: { moduleId: number; title: string; content?: string; order: number; estimatedDuration?: string }) =>
    client.post<{ id: number; title: string; order: number; moduleId: number }>('/lessons', dto).then(r => r.data),

  updateLesson: (id: number, dto: { moduleId: number; title: string; content?: string; order: number; estimatedDuration?: string }) =>
    client.put(`/lessons/${id}`, dto),

  deleteLesson: (id: number) =>
    client.delete(`/lessons/${id}`),

  getMaterialsByLesson: (lessonId: number) =>
    client.get<import('../types/course').MaterialDto[]>(`/materials/lesson/${lessonId}`).then(r => r.data),

  createMaterial: (dto: { lessonId: number; title: string; url?: string; type: string; order: number }) =>
    client.post<import('../types/course').MaterialDto>('/materials', dto).then(r => r.data),

  updateMaterial: (id: number, dto: { lessonId: number; title: string; url?: string; type: string; order: number }) =>
    client.put(`/materials/${id}`, { ...dto, id }),

  deleteMaterial: (id: number) =>
    client.delete(`/materials/${id}`),
};