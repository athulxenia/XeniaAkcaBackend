namespace XeniaTokenBackend.Dto
{
    public class TokenUpdateDto
    {
        public int TokenId { get; set; }
        public int NewDepId { get; set; }
        public int ServiceId { get; set; }
        public int CounterId { get; set; }
    }
}
