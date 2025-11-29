using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace StudentPortal.ServiceDefaults.Metrics;

public static class MetricRecorder
    {
        public static async Task RecordOperationAsync(
            Histogram<double> histogram,
            string operation,
            Func<Task> func)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await func();
                sw.Stop();

                histogram.Record(sw.Elapsed.TotalSeconds, new TagList
                {
                    new(MetricConstants.Keys.Operation, operation),
                    new(MetricConstants.Keys.Status, MetricConstants.Values.Success)
                });
            }
            catch (Exception ex)
            {
                sw.Stop();

                histogram.Record(sw.Elapsed.TotalSeconds, new TagList
                {
                    new(MetricConstants.Keys.Operation, operation),
                    new(MetricConstants.Keys.Status, MetricConstants.Values.Failure),
                    new(MetricConstants.Keys.ErrorType, ex.GetType().Name)
                });

                throw;
            }
        }

        public static async Task<T> RecordOperationAsync<T>(
            Histogram<double> histogram,
            string operation,
            Func<Task<T>> func)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                T result = await func();
                sw.Stop();

                histogram.Record(sw.Elapsed.TotalSeconds, new TagList
                {
                    new(MetricConstants.Keys.Operation, operation),
                    new(MetricConstants.Keys.Status, MetricConstants.Values.Success)
                });

                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();

                histogram.Record(sw.Elapsed.TotalSeconds, new TagList
                {
                    new(MetricConstants.Keys.Operation, operation),
                    new(MetricConstants.Keys.Status, MetricConstants.Values.Failure),
                    new(MetricConstants.Keys.ErrorType, ex.GetType().Name)
                });

                throw;
            }
        }
    }