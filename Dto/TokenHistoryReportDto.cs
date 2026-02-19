namespace XeniaTokenBackend.Dto
{
    public class TokenHistoryReportDto
    {
        public int TokenHistoryID { get; set; }
        public int TokenID { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string StatusCreatedTime { get; set; }
        public int StatusCreatedUser { get; set; }
        public int TokenValue { get; set; }
        public string DepFromPrefix { get; set; }
        public string DepFromName { get; set; }
        public string DepToName { get; set; }
        public string CounterName { get; set; }
        public string ServiceName { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobileNumber { get; set; }
        public string OnCallTime { get; set; }
        public string CurrentTokenStatus { get; set; }
    }

    public class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
