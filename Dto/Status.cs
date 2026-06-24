namespace XeniaAkcaBackend.Dto
{
    public class DeactivateRequest
    {
        public string? MemberReviseRemarks { get; set; }
    }

    public class AccountInfoRequest
    {
        public int UserId { get; set; }
    }

    public class UpdateMemberFullDetailsRequest
    {
        public int MemberId { get; set; }
        public string? MemberName { get; set; }
        public string? MemberAddress { get; set; }
        public string? MemberEmail { get; set; }
        public string? MemberMobilenumber { get; set; }
        public DateTime? MemberDob { get; set; }
        public int? MemberAge { get; set; }
        public string? MemberIdProof { get; set; }
        public string? MemberIdProofNumber { get; set; }
        public string? MemberBankName { get; set; }
        public string? MemberBankAcName { get; set; }
        public string? MemberBankAcNumber { get; set; }
        public string? MemberBankBranch { get; set; }
        public string? MemberIfsc { get; set; }
        public string? MemberIdUrl1 { get; set; }
        public string? MemberIdUrl2 { get; set; }
        public string? MemberBusinessName { get; set; }
        public string? MemberBusinessAddress { get; set; }
        public string? MemberBusinessDetails { get; set; }
        public string? MemberBusinessFSSAIno { get; set; }
        public string? MemberBusinessCmpyType { get; set; }
        public bool MemberActiveStatus { get; set; }
        public string? UserImageUrl { get; set; }
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
        public int? NomineeStatus { get; set; }
    }
}