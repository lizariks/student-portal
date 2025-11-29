namespace StudentPortal.AggregatorService.HealthChecks;

using StudentPortal.Discussion.Grpc;
using StudentPortal.ServiceDefaults.Health;

    public class DiscussionServiceHealthCheck : GrpcServiceHealthCheck<Discussion.DiscussionClient>
    {
        protected override string ServiceName => "DiscussionService";
        protected override TimeSpan Timeout => TimeSpan.FromSeconds(2); 

        public DiscussionServiceHealthCheck(
            Discussion.DiscussionClient client,
            ILogger<DiscussionServiceHealthCheck> logger)
            : base(client, logger)
        {
        }

        protected override async Task<bool> PerformHealthCheckAsync(CancellationToken cancellationToken)
        {
            var request = new GetThreadByIdRequest 
            { 
                ThreadId = "health-check-0" 
            }; 
            var response = await Client.GetThreadByIdAsync(request, cancellationToken: cancellationToken);
            return response is not null;
        }
    }