namespace StudentPortal.DiscussionService.Infrastructure;

    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = default!;
        public string DatabaseName { get; set; } = default!;
        public int MaxConnectionPoolSize { get; set; } = 100;
        public int MinConnectionPoolSize { get; set; } = 5;
        public int ConnectTimeoutSeconds { get; set; } = 10;
        public int SocketTimeoutSeconds { get; set; } = 10;
    }