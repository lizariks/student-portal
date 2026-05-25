import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { Layout } from '../components/Layout';
import { coursesApi } from '../api/courses';
import type { CourseDto } from '../types/course';

export function CoursesPage() {
  const [courses, setCourses] = useState<CourseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  const loadPublished = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      setCourses(await coursesApi.getPublished());
    } catch {
      setError('Failed to load courses.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadPublished(); }, [loadPublished]);

  async function handleSearch(e: React.FormEvent) {
    e.preventDefault();
    if (!search.trim()) { loadPublished(); return; }
    try {
      setLoading(true);
      setError(null);
      setCourses(await coursesApi.search(search.trim()));
    } catch {
      setError('Search failed.');
    } finally {
      setLoading(false);
    }
  }

  function handleClear() {
    setSearch('');
    loadPublished();
  }

  return (
    <Layout>
      <div className="max-w-5xl">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-900">Explore Courses</h1>
          <p className="text-gray-500 mt-1">Browse all available courses and enroll to start learning.</p>
        </div>

        <form onSubmit={handleSearch} className="mb-6 flex gap-2">
          <input
            type="text"
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder="Search by title or keyword..."
            className="flex-1 border border-gray-200 rounded-lg px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
          />
          <button
            type="submit"
            className="px-4 py-2 bg-indigo-600 text-white text-sm rounded-lg hover:bg-indigo-700 transition-colors"
          >
            Search
          </button>
          {search && (
            <button
              type="button"
              onClick={handleClear}
              className="px-4 py-2 bg-gray-100 text-gray-600 text-sm rounded-lg hover:bg-gray-200 transition-colors"
            >
              Clear
            </button>
          )}
        </form>

        {error && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{error}</div>
        )}

        {loading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="bg-white rounded-xl border border-gray-100 shadow-sm p-5 animate-pulse">
                <div className="h-4 bg-gray-100 rounded w-1/3 mb-3" />
                <div className="h-5 bg-gray-200 rounded w-3/4 mb-2" />
                <div className="h-4 bg-gray-100 rounded w-full mb-1" />
                <div className="h-4 bg-gray-100 rounded w-2/3" />
              </div>
            ))}
          </div>
        ) : courses.length === 0 ? (
          <div className="text-center py-16 text-gray-400">
            <p className="text-lg font-medium">No courses found</p>
            <p className="text-sm mt-1">Try a different keyword or check back later.</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {courses.map(course => (
              <CourseCard
                key={course.id}
                course={course}
                onClick={() => navigate(`/courses/${course.id}`)}
              />
            ))}
          </div>
        )}
      </div>
    </Layout>
  );
}

function CourseCard({ course, onClick }: { course: CourseDto; onClick: () => void }) {
  return (
    <div
      onClick={onClick}
      className="bg-white rounded-xl border border-gray-100 shadow-sm p-5 cursor-pointer hover:shadow-md hover:border-indigo-200 transition-all"
    >
      <span className="inline-block text-xs font-mono text-indigo-500 bg-indigo-50 px-2 py-0.5 rounded mb-2">
        {course.code}
      </span>
      <h3 className="font-semibold text-gray-900 mb-1 line-clamp-2">{course.title}</h3>
      {course.description && (
        <p className="text-sm text-gray-500 line-clamp-3 mb-3">{course.description}</p>
      )}
      {course.instructorName && (
        <p className="text-xs text-gray-400 mt-auto">By {course.instructorName}</p>
      )}
    </div>
  );
}