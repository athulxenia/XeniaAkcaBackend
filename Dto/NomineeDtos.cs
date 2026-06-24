namespace XeniaAkcaBackend.Dto
{
    public class UpdateNomineeRequest
    {
        public string? NomineeName { get; set; }
        public string? NomineeAddress { get; set; }
        public string? NomineeEmail { get; set; }
        public string? NomineeMobilenumber { get; set; }
        public string? NomineeIdProof { get; set; }
        public string? NomineeIdProofNumber { get; set; }
        public string? NomineeBankName { get; set; }
        public string? NomineeBankAcName { get; set; }
        public string? NomineeBankAcNumber { get; set; }
        public string? NomineeBankBranch { get; set; }
        public string? NomineeIfsc { get; set; }
        public string? NomineeIdUrl1 { get; set; }
        public string? NomineeIdUrl2 { get; set; }
        public string? NomineeRelation { get; set; }
    }

    public class ApproveNomineeRequest
    {
        public bool MemberStatus { get; set; }
    }

    public class NomineeResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}