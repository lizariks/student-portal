namespace StudentPortal.AggregatorService.HealthChecks;

using StudentPortal.EnrollmentService.Grpc;
using StudentPortal.ServiceDefaults.Health;
using Microsoft.Extensions.Logging;



    public class EnrollmentServiceHealthCheck : GrpcServiceHealthCheck<EnrollmentGrpcService.EnrollmentGrpcServiceClient>
    {
        protected override string ServiceName => "EnrollmentService";
        
        protected override TimeSpan Timeout => TimeSpan.FromSeconds(2); 

        public EnrollmentServiceHealthCheck(
            EnrollmentGrpcService.EnrollmentGrpcServiceClient client,
            ILogger<EnrollmentServiceHealthCheck> logger)
            : base(client, logger)
        {
        }

        protected override async Task<bool> PerformHealthCheckAsync(CancellationToken cancellationToken)
        {
            var request = new GetEnrollmentByIdRequest { 
                EnrollmentId = 0 
            }; 
            var response = await Client.GetEnrollmentByIdAsync(request, cancellationToken: cancellationToken);
            return response is not null;
        }
    }