using XeniaQLaunchBackend.Dto;
using XeniaTempleBackend.Dtos;

namespace XeniaQLaunchBackend.Repositories.Subscription
{
    public interface ISubscriptionRepository
    {
        Task<List<PlanDto>> GetMainPlansAsync();
        Task<List<AddonPlanDto>> GetAddonPlansAsync();
        Task<RenewSubscriptionResponseDto?> RenewSubscriptionAsync(RenewSubscriptionDto dto);
        Task<MswipeTransactionStatusResponse> CheckTransactionStatusAsync(string transId);
        Task<PaymentStatusResult> UpdatePaymentStatusAsync(string transactionRefId, string success);
    }
}
