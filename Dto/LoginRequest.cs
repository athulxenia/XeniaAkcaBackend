namespace XeniaAkcaBackend.Models
{
    // existing
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirebaseToken { get; set; }
    }

    public class LoginResponse
    {
        public string Status { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? Message { get; set; }
    }

    // ── new ──────────────────────────────────────────────────────
    public class RegisterRequest
    {
        public int RegType { get; set; }
        public int FMYuserGroupId { get; set; }
        public int FMYuserDistrictId { get; set; }
        public int FMYuserUnitId { get; set; }
        public string? FMYuserName { get; set; }
        public string? FMYpassword { get; set; }
        public string? FMYuserImageUrl { get; set; }
        public string? FMYfirebaseToken { get; set; }
        public string? FMYCreatedOn { get; set; }
        public string? FYMCreatedOn { get; set; }
        public int? FMYmemberParentId { get; set; }
        public int? FYMmemberId { get; set; }
        public string? FMYmemberReviseRemarks { get; set; }
        public string? FMYmemberName { get; set; }
        public string? FMYmemberAddress { get; set; }
        public string? FMYmemberEmail { get; set; }
        public string? FMYmemberMobilenumber { get; set; }
        public string? FMYmemberDob { get; set; }
        public string? FMYmemberIdProofNumber { get; set; }
        public string? FMYmemberBankName { get; set; }
        public string? FMYmemberBankAcName { get; set; }
        public string? FMYmemberBankAcNumber { get; set; }
        public string? FMYmemberBankBranch { get; set; }
        public string? FMYmemberIfsc { get; set; }
        public string? FMYmemberIdUrl1 { get; set; }
        public string? FMYmemberIdUrl2 { get; set; }
        public string? FMYmemberBusinessName { get; set; }
        public string? FMYmemberBusinessAddress { get; set; }
        public int? FMYmemberAge { get; set; }
        public string? FMYmemberBusinessDetails { get; set; }
        public string? FMYmemberBusinessFSSAIno { get; set; }
        public string? FMYmemberBusinessCmpyType { get; set; }
        public string? FMYmemberGstCertificateUrl { get; set; }
        public string? FMYmemberPartnershipDeedUrl { get; set; }
        public string? FMYmemberIdProof { get; set; }
        public string? FMYnomineeName { get; set; }
        public string? FMYnomineeAddress { get; set; }
        public string? FMYnomineeEmail { get; set; }
        public string? FMYnomineeMobilenumber { get; set; }
        public string? FMYnomineeIdProof { get; set; }
        public string? FMYnomineeIdProofNumber { get; set; }
        public string? FMYnomineeBankName { get; set; }
        public string? FMYnomineeBankAcName { get; set; }
        public string? FMYnomineeBankAcNumber { get; set; }
        public string? FMYnomineeBankBranch { get; set; }
        public string? FMYnomineeIfsc { get; set; }
        public string? FMYnomineeIdUrl1 { get; set; }
        public string? FMYnomineeIdUrl2 { get; set; }
        public int FMYnomineeStatus { get; set; }
        public string? FMYnomineeRelation { get; set; }
    }

    public class RegisterResponse
    {
        public int StatusCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string? Token { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string Password { get; set; } = string.Empty;
    }

    public class CheckSmsRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}