namespace XeniaAkcaBackend.Dto
{
    public class UpdateMemberStatusRequest
    {
        public string MemberStatus { get; set; } = string.Empty;
        public string? MemberReviseRemarks { get; set; }
    }

    public class ChildApproveRequest
    {
        public string MemberStatus { get; set; } = string.Empty;
        public bool MemberAction { get; set; }
    }
}
