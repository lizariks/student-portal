export interface CourseDto {
  id: number;
  code: string;
  title: string;
  description?: string;
  isPublished: boolean;
  publishedAt?: string;
  instructorId?: number;
  instructorName?: string;
}

export interface LessonSummaryDto {
  id: number;
  title: string;
  order: number;
}

export interface ModuleDto {
  id: number;
  title: string;
  description?: string;
  order: number;
  courseId: number;
  lessons: LessonSummaryDto[];
}

export interface MaterialDto {
  id: number;
  title: string;
  url?: string;
  type: 'File' | 'Link' | 'Video' | 'Quiz' | 'HtmlContent';
  order: number;
  lessonId: number;
}

export interface LessonDetailDto {
  id: number;
  moduleId: number;
  title: string;
  content?: string;
  order: number;
  estimatedDuration?: string;
  materials: MaterialDto[];
}

export interface CourseDetailsDto extends CourseDto {
  createdAt: string;
  updatedAt: string;
  modules: ModuleDto[];
}

export interface StudentCourseDto {
  userId: number;
  courseId: number;
  enrolledAt: string;
  course?: CourseDto;
}