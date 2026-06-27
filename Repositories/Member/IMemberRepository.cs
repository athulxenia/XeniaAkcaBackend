using XeniaAkcaBackend.Dto;

namespace XeniaAkcaBackend.Repositories.Member
{
    public interface IMemberRepository
    {
        Task<object?> GetMemberAsync(string membershipNumberPrefix, string membershipNumber);
        Task<PaginatedResult<object>> GetAllStateWiseMembersAsync(int active, string? pending, int page, int limit, string? searchText, int? districtId, int? unitId);
        Task<PaginatedResult<object>> GetAllDistrictWiseMembersAsync(int active, int districtId, int page, int limit, string? searchText);
        Task<PaginatedResult<object>> GetAllUnitWiseMembersAsync(int active, int unitId, int page, int limit, string? searchText);
        Task<object?> GetMemberDetailsAsync(int memberId);
        Task<int> GetMemberIdByUserIdAsync(int userId);  // ✅ NEW
        Task<object> UpdateMemberStatusAsync(int userId, string memberStatus, string? memberReviseRemarks);
        Task<object?> GetMemberOutstandingAsync(int userId);
        Task<object?> GetPendingApproveDetailsAsync(int userId);
        Task<object> ChildMemberApproveAsync(int userId, string memberStatus, bool memberAction);
        Task<PaginatedResult<object>> GetMemberStatusDetailsAsync(int status, int page, int limit, string? searchText);
        Task<object?> GetOwnerDetailsAsync(string? prefix, string? number, string? suffix);
        Task<PaginatedResult<object>> GetAllStateKaruthalMemberAsync(int active, string? pending, int page, int limit, string? searchText, int? districtId, int? unitId);
        Task<PaginatedResult<object>> GetAllDistrictKaruthalMemberAsync(int active, int districtId, int page, int limit, string? searchText);
        Task<List<object>?> MemberAccountDetailsAsync(int userId);
        Task<object?> UpdateMemberFullDetailsAsync(UpdateMemberFullDetailsRequest data);
    }
}