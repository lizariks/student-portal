namespace StudentPortal.CourseCatalogService.BLL.Metrics;

using System.Diagnostics.Metrics;
using System.Diagnostics;

    public static class CourseMetrics
    {
        private static readonly Meter Meter = new("StudentPortal.CourseCatalogService.Courses", "1.0.0");

        public static readonly Counter<long> CoursesCreated =
            Meter.CreateCounter<long>("courses.created_total", "{courses}", "Total number of courses created");

        public static readonly Counter<long> CoursesUpdated =
            Meter.CreateCounter<long>("courses.updated_total", "{courses}", "Total number of courses updated");

        public static readonly Counter<long> CoursesDeleted =
            Meter.CreateCounter<long>("courses.deleted_total", "{courses}", "Total number of courses deleted");

        public static readonly Counter<long> CoursesFetched =
            Meter.CreateCounter<long>("courses.fetched_total", "{requests}", "Total number of course retrievals (cache or DB)");

        public static readonly Histogram<double> OperationLatency =
            Meter.CreateHistogram<double>("courses.operation_duration_seconds", "s", "Duration of course operations in seconds");

        public static void RecordFetch(string operation, bool found)
        {
            CoursesFetched.Add(1, new TagList
            {
                new("operation", operation),
                new("found", found.ToString().ToLower())
            });
        }
    }
