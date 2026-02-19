namespace XeniaTokenBackend.Dto
{
    public class TokenHistorySummaryDto
    {
        public int TotalTokensIssued { get; set; }

        public int AverageWaitTimeMinutes { get; set; }

        public PeakHourDto PeakHour { get; set; } = new();

        public List<CounterPerformanceDto> CounterPerformanceData { get; set; }
            = new();

        public int TokensOnHold { get; set; }

        public int TokensPending { get; set; }
    }

    public class PeakHourDto
    {
        public string? Hour { get; set; }  

        public int TokensIssued { get; set; }
    }

    public class CounterPerformanceDto
    {
        public string CounterName { get; set; } = string.Empty;

        public int TokensServed { get; set; }
    }
}
