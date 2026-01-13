namespace XeniaTokenBackend.Dto
{
    public class UpdateTokenStatusRequest
    {
        public int UserId { get; set; }
        public int ServiceId { get; set; }
        public int? CustomerId { get; set; }
        public int? CounterId { get; set; }
    }
}
