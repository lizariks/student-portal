namespace StudentPortal.ServiceDefaults.Metrics;
public static class MetricConstants
{
    public static class Keys
    {
        public const string Operation = "operation";
        public const string Status = "status";
        public const string ErrorType = "error.type";
        public const string Found = "found";
        
    }

    public static class Values
    {
        public const string Success = "success";
        public const string Failure = "failure";
        public const string Create = "create";
        public const string Update = "update";
        public const string Delete = "delete";
        public const string GetById = "get_by_id";
        public const string List = "list";
        public const string Publish = "publish"; 
        public const string Unpublish = "unpublish";
    }

    public static class Tags
    {
        public static readonly KeyValuePair<string, object?> OperationList = new(Keys.Operation, Values.List);
        public static readonly KeyValuePair<string, object?> OperationGetById = new(Keys.Operation, Values.GetById);
        public static readonly KeyValuePair<string, object?> OperationCreate = new(Keys.Operation, Values.Create);
        public static readonly KeyValuePair<string, object?> OperationUpdate = new(Keys.Operation, Values.Update);
        public static readonly KeyValuePair<string, object?> OperationDelete = new(Keys.Operation, Values.Delete);
        public static readonly KeyValuePair<string, object?> OperationPublish = new(Keys.Operation, Values.Publish); 
        public static readonly KeyValuePair<string, object?> OperationUnpublish = new(Keys.Operation, Values.Unpublish);
    }
}