using XeniaAkcaBackend.Dto;

namespace XeniaAkcaBackend.Repositories
{
    public interface INomineeRepository
    {
        Task<object?> GetNomineeAsync(int userId);
        Task<object> GetAllNomineesAsync(int page, int limit, string search, int? unitId);
        Task<NomineeResponse> UpdateNomineeAsync(int userId, UpdateNomineeRequest request);
        Task<NomineeResponse> ApproveNomineeAsync(int memberUserId, bool memberStatus);
    }
}