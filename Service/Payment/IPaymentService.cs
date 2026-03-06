using XeniaQLaunchBackend.Dto;
using XeniaTempleBackend.Dtos;

namespace XeniaQLaunchBackend.Service.Payment
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentLink(string orderId, decimal? netAmount);
        Task<MswipeTransactionStatusResponse> CheckTransactionStatusAsync(string transId);
    }
}
