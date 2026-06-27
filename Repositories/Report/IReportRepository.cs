using XeniaAkcaBackend.Dto;

namespace XeniaAkcaBackend.Repositories.Report
{
    public interface IReportRepository
    {
        Task<ReportResponse> GetContributionReportAsync(string level, int status, ContributionRequest request);

        Task<ReportResponse> GetPaymentReportAsync(PaymentRequest request);


        Task<object> GetEventsAsync(string? searchText);
    }
}
