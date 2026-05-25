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
};