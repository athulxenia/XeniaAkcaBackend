using XeniaAkcaBackend.Dto;

namespace XeniaAkcaBackend.Repositories
{
    public interface IStatusRepository
    {
        Task<object?> CheckServerStatusAsync();
        Task<object?> GetAccStatusAndPaymentHistoryAsync(int userId);
        Task<string?> GetTermsAndConditionsAsync(int statusId);
        Task<string?> GetPrivacyPolicyAsync(int statusId);
        Task<object?> GetFamilyMemberAsync(int userId);
        Task<object?> MemberDeactivationAsync(int userId, string? memberReviseRemarks);
        Task<object?> GetMemberDetailsAsync(int userId);
        Task<List<object>> GetReceiptDetailsAsync(int userId, string? tranId);
        Task<object?> CompanyWhatsappMobAsync();
        Task<List<object>?> MemberAccountDetailsAsync(int userId);
        Task<object?> UpdateMemberFullDetailsAsync(UpdateMemberFullDetailsRequest data);
    }
}