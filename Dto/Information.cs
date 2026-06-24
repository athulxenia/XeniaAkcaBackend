namespace XeniaAkcaBackend.Dto
{
    public class CreateInformationRequest
    {
        public string? InformationName { get; set; }
        public int? DistrictId { get; set; }
        public string? InformationImgUrl { get; set; }
        public string? InformationCreatedUser { get; set; }
        public string? InformationContent { get; set; }
        public DateTime? InformationStartDate { get; set; }
        public DateTime? InformationEndDate { get; set; }
        public bool ActiveStatus { get; set; }
        public int? InformationApproveStatus { get; set; }
    }

    public class UpdateInformationRequest
    {
        public string? InformationName { get; set; }
        public int? DistrictId { get; set; }
        public string? InformationImgUrl { get; set; }
        public string? InformationModifiedUser { get; set; }
        public string? InformationContent { get; set; }
        public DateTime? InformationStartDate { get; set; }
        public DateTime? InformationEndDate { get; set; }
    }

    public class ApproveInformationRequest
    {
        public bool ActiveStatus { get; set; }
    }

    public class InformationResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class InformationListRequest
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
        public string? SearchText { get; set; }
        public int? DistrictId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? InformationId { get; set; }
    }
}
