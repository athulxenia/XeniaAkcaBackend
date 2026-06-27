

using XeniaAkcaBackend.Dto;

namespace XeniaAkcaBackend.Repositories
{
    public interface IPaymentRepository
    {
     
        Task<List<PaymentSettingDto>> GetAllPaymentSettingsAsync();
        Task<PaymentResponse> UpdatePaymentSettingsAsync(List<PaymentSettingDto> settings);
        Task<PaymentResponse> RegistrationPaymentAsync(int userId, RegistrationPaymentRequest request);
        Task<PaymentResponse> ContributionPaymentAsync(int userId, ContributionPaymentRequest request);
        Task<object> InitiatePaymentAsync(int userId, InitiatePaymentRequest request);
        Task<object> CheckPaymentStatusAsync(string txnid);
        Task<object> RecheckRecentTransactionsAsync();
        Task<WalletBalanceDto> GetMemberWalletBalanceAsync(int userId);
    }
}