namespace XeniaTokenBackend.Dto
{
    public class TokenValuesDto
    {
        public string TotalToken { get; set; }
        public string CallToken { get; set; }
        public string NextToken { get; set; }
        public IEnumerable<CounterCallTokenDto> LatestCounterCallToken { get; set; }
    }

    public class CounterCallTokenDto
    {
        public int CounterID { get; set; }
        public string? CounterName { get; set; }
        public int TokenID { get; set; }
        public int DepID { get; set; }
        public string? DepPrefix { get; set; }
        public required string DepName { get; set; }
        public int? ServiceID { get; set; }
        public int LastOnCallToken { get; set; }
    }

}
