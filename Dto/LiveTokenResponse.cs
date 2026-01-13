namespace XeniaTokenBackend.Dto
{
    public class LiveTokenResponse
    {
        public int CounterCount { get; set; }
        public List<TokenLiveDto> Result { get; set; } = new();
    }

    public class TokenLiveDto
    {
        public int? CounterID { get; set; }
        public int TokenValue { get; set; }
        public string CounterName { get; set; }
        public bool IsAnnounced { get; set; }
        public string TokenStatus { get; set; }
        public int DepID { get; set; }
        public string DepPrefix { get; set; }
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
    }

}
