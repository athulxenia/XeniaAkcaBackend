// Dto/UnitDto.cs

namespace XeniaAkcaBackend.Dto
{
    // Create/Update Unit
    public class UnitRequest
    {
        public string? UnitName { get; set; }
        public string? UnitCode { get; set; }
        public int UnitDistrictId { get; set; }
        public string? UnitContactPerson { get; set; }
        public string? UnitContactPerson2 { get; set; }
        public string? UnitContactNumber { get; set; }
        public string? UnitContactNumber2 { get; set; }
        public string? UnitEmailAddress { get; set; }
        public string? Password { get; set; }
        public bool Status { get; set; } = true;
        public bool UserStatus { get; set; } = true;
    }

    // Unit Response
    public class UnitResponse
    {
        public string Status { get; set; } = "success";
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }

    // Paginated Unit Response
    public class PaginatedUnitResponse
    {
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int Limit { get; set; }
        public int TotalRecords { get; set; }
        public object? Units { get; set; }
    }

    // Unit with District Name
    public class UnitWithDistrictDto
    {
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? UnitCode { get; set; }
        public int UnitDistrictId { get; set; }
        public string? DistrictName { get; set; }
        public string? UnitContactPerson { get; set; }
        public string? UnitContactPerson2 { get; set; }
        public string? UnitContactNumber { get; set; }
        public string? UnitContactNumber2 { get; set; }
        public string? UnitEmailAddress { get; set; }
        public string? UnitMemNumberPrefix { get; set; }
        public string? LastMembershipNumber { get; set; }
        public bool Status { get; set; }
    }
}