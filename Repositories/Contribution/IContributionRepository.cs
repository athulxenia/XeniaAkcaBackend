using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Models;

namespace XeniaAkcaBackend.Repositories  
{
    public interface IContributionRepository
    {
        Task<ContributionResponse> CreateContributionAsync(CreateContributionRequest request);
        Task<List<object>> GetMembersByPartialNameAsync(string partialName);
        Task<ContributionResponse> UpdateContributionAsync(int contributionId, UpdateContributionRequest request);
        Task<object?> GetContributionAsync(int contributionId);
        Task<PaginatedResult<object>> GetDistrictPendingContributionAsync(int page, int limit, string? searchText, int? districtId);
        Task<PaginatedResult<object>> GetDistrictApproveContributionAsync(int page, int limit, string? searchText, int? districtId);
        Task<PaginatedResult<object>> GetUnitPendingContributionAsync(int page, int limit, string? searchText, int? unitId);
        Task<PaginatedResult<object>> GetUnitApproveContributionAsync(int page, int limit, string? searchText, int? unitId);
        Task<PaginatedResult<object>> GetStatePendingContributionAsync(int page, int limit, string? searchText);
        Task<PaginatedResult<object>> GetStateApproveContributionAsync(int page, int limit, string? searchText);
        Task<List<object>> ConPendingDetailsAsync(int userId);
        Task<List<object>> ConPayedDetailsAsync(int userId);
        Task<ContributionResponse> ApproveContributionAsync(int contributionId, bool activeStatus);
        Task<object?> GetContributionDetailsAsync(int contributionId);
        Task<object> ContributionAmountNotificationAsync(int memberId);
        Task<object?> DetailsOfContributionAsync(int contributionId);
        Task<object> ProcessAllContributionPaymentsAsync();
        Task<ContributionResponse> ContributionUpdationAsync(int contributionId, ContributionUpdationRequest request);
        Task<object> SendFirebaseNotificationAsync(int contributionMemberId);
    }
}
