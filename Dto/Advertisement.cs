namespace XeniaAkcaBackend.Dto
{
    public class CreateAdvertisementRequest
    {
        public string? AdvertisementName { get; set; }
        public int DistrictId { get; set; }
        public string? FileUrl { get; set; }
        public string? AdvertisementCreatedUser { get; set; }
        public string? AdvertisementContent { get; set; }
        public DateTime? AdvertisementStartDate { get; set; }
        public DateTime? AdvertisementEndDate { get; set; }
        public bool ActiveStatus { get; set; }
        public int? AdvertisementApproveStatus { get; set; }
    }

    public class UpdateAdvertisementRequest
    {
        public string? AdvertisementName { get; set; }
        public int DistrictId { get; set; }
        public string? FileUrl { get; set; }
        public string? AdvertisementModifiedUser { get; set; }
        public string? AdvertisementContent { get; set; }
        public DateTime? AdvertisementStartDate { get; set; }
        public DateTime? AdvertisementEndDate { get; set; }
    }

    public class ApproveAdvertisementRequest
    {
        public bool? ActiveStatus { get; set; }
    }

    public class AdvertisementResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}