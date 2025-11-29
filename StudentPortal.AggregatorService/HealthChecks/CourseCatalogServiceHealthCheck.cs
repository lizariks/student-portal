namespace StudentPortal.AggregatorService.HealthChecks;

using StudentPortal.CourseCatalog.Grpc;
using StudentPortal.ServiceDefaults.Health;


    public class CourseCatalogServiceHealthCheck : GrpcServiceHealthCheck<CourseCatalog.CourseCatalogClient>
    {
        protected override string ServiceName => "CourseCatalogService";
        protected override TimeSpan Timeout => TimeSpan.FromSeconds(2); 

        public CourseCatalogServiceHealthCheck(
            CourseCatalog.CourseCatalogClient client,
            ILogger<CourseCatalogServiceHealthCheck> logger)
            : base(client, logger)
        {
        }

        protected override async Task<bool> PerformHealthCheckAsync(CancellationToken cancellationToken)
        {
            var request = new GetCourseByIdRequest { Id = 0 }; 

            var response = await Client.GetCourseByIdAsync(request, cancellationToken: cancellationToken);
            return response is not null; 
        }
    }