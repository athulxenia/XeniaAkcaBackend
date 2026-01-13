

using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Models;

namespace XeniaTokenBackend.Repositories.Token
{
    public interface ITokenRepository
    {
        Task<string> GetAndUpdateCustomToken(TokenRequestDto tokenData);
        Task<IEnumerable<TokenOnStatusDto>> GetTokensOnHold(int companyId, int userId);
        Task<IEnumerable<TokenOnStatusDto>> GetTokensOnPending(int companyId, int userId);
        Task<IEnumerable<TokenOnStatusDto>> GetTokensByStatus(int companyId, int userId, string tokenStatus);
        Task<TokenValuesDto> GetTokenValuesAsync(int companyId, int depId);
        Task<TokenUpdateResponse> GetAndUpdateCounterTokenAsync(TokenUpdateRequest request);
        Task<(bool Success, string Message)> UpdateTokenStatusAsync(int companyId, int depId, string depPrefix, int tokenValue,int userId, int serviceId, int? customerId, int? counterId);
        Task<int> GetPendingTokenAsync(int companyId, int userId);
        Task<IEnumerable<xtm_TokenRegister>> CheckTokenValueAsync(int companyId, int depId, int tokenValue);

      //  Task<PagedResponse<TokenHistoryReportDto>> GetTokenHistoryReportAsync(int companyId,DateTime startDate,DateTime endDate,int pageNumber,int pageSize,string searchParam);
        Task<IEnumerable<TokenTimelineDto>> GetTokenTimelineAsync(int companyId, int depId, string depPrefix, int tokenValue);
        Task<bool> ResetTokenAsync(int companyId, int depId);
        Task<bool> UpdateIsAnnouncedAsync(int companyId, int depId, int tokenValue);
        Task<(bool Success, string Message)> UpdateDepartmentAsync(int companyId,int oldDepId, string depPrefix, int tokenValue, TokenUpdateDto tokenData);
        Task<IEnumerable<object>> GetTokensOnHoldAsync(int companyId, int userId);
        Task<string> RecallTokenAsync(TokenRecallDto tokenData);
        Task<object> UpsertTokenAsync(TokenUpsertDto tokenData);
        Task<Stream> GetTokenAudioAsync(string tokenNumber, string counterName);
    }
}
