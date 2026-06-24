namespace XeniaAkcaBackend.Dto
{
    public class UpdateDistrictRequest
    {
        public string? ContactPerson1 { get; set; }
        public string? ContactNumber1 { get; set; }
        public string? EmailAddress1 { get; set; }
        public string? ContactPerson2 { get; set; }
        public string? ContactNumber2 { get; set; }
        public bool Status { get; set; }
        public string? Password { get; set; }
    }

    public class DistrictResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
