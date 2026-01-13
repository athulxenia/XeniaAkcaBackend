namespace XeniaTokenBackend.Dto
{
    public class TokenTimelineDto
    {
        public string TokenStatus { get; set; } = string.Empty;
        public string TokenTime { get; set; } = string.Empty;
        public string? DepFromName { get; set; }
        public string? DepToName { get; set; }
        public string? CounterName { get; set; }
    }
}
