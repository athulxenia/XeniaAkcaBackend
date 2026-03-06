using XeniaTempleBackend.Dtos;
using XeniaTokenBackend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using XeniaQLaunchBackend.Service.Payment;
using XeniaQLaunchBackend.Dto;


namespace XeniaQLaunchBackend.Repositories.Subscription
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentService _paymentService;
        private readonly HttpClient _httpClient;

        public SubscriptionRepository(ApplicationDbContext context, IPaymentService paymentService, HttpClient httpClient)
        {
            _context = context;
            _paymentService = paymentService;
            _httpClient = httpClient;
        }


        public async Task<List<PlanDto>> GetMainPlansAsync()
        {
            var plans = await _context.SubscribePlan
                .Where(p => p.PlanActive && !p.PlanIsAddOn)
                .Include(p => p.PlanDurations)
                .ToListAsync();

            var result = plans.Select(plan => new PlanDto
            {
                PlanId = plan.PlanId,
                PlanName = plan.PlanName,
                PlanDescription = plan.PlanDescription,
                PlanDeps = plan.PlanDep,
                Durations = plan.PlanDurations
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.DurationDays)
                    .Select(d => new PlanDurationDto
                    {
                        PlanDurationId = d.PlanDurationId,
                        DurationDays = d.DurationDays,
                        Price = d.Price
                    })
                    .ToList()
            }).ToList();

            return result;
        }

        public async Task<List<AddonPlanDto>> GetAddonPlansAsync()
        {
            return await _context.SubscribePlan
                .Where(p => p.PlanActive && p.PlanIsAddOn)
                .Select(p => new AddonPlanDto
                {
                    PlanId = p.PlanId,
                    PlanName = p.PlanName,
                    PlanPrice = p.PlanDurations
                                .Where(d => d.IsActive && d.DurationDays == 0)
                                .Select(d => d.Price)
                                .FirstOrDefault(),

                    PlanDeps = p.PlanDep
                })
                .ToListAsync();
        }


        public async Task<RenewSubscriptionResponseDto?> RenewSubscriptionAsync(RenewSubscriptionDto dto)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            var now = SafeSqlDate(DateTime.Now);

            var existingTransaction = await _context.SubscriptionTransaction
                .Where(t =>
                    t.CompanyId == dto.CompanyId &&
                    (t.Status == "INITIATED" || t.Status == "PENDING"))
                .OrderByDescending(t => t.CreatedOn)
                .FirstOrDefaultAsync();

            if (existingTransaction != null)
            {
                var gatewayResponse =
                    await CheckTransactionStatusAsync(existingTransaction.ProviderTransactionId);

                if (gatewayResponse.ResponseMessage == "Transaction detail not found" ||
                    gatewayResponse.Status == "PENDING")
                {
                    return new RenewSubscriptionResponseDto
                    {
                        TransactionId = existingTransaction.ProviderTransactionId,
                        PaymentLink = existingTransaction.PaymentLink,
                        PaymentStatus = gatewayResponse.Status,
                        Message = "A payment is already in progress. Please complete the existing transaction."
                    };
                }

                if (gatewayResponse.Status == "FAILED")
                {
                    await ExpirePendingSubscriptions(
                        existingTransaction.CompanyId,
                        existingTransaction.TransactionRefId);

                    existingTransaction.Status = "FAILED";
                    await _context.SaveChangesAsync();
                }
            }

            var mainPlan = await _context.SubscribePlan
                .Include(p => p.PlanDurations)
                .FirstOrDefaultAsync(p =>
                    p.PlanId == dto.PlanId &&
                    p.PlanActive &&
                    !p.PlanIsAddOn);

            if (mainPlan == null)
                return null;

            var selectedDuration = mainPlan.PlanDurations
                .FirstOrDefault(d =>
                    d.PlanDurationId == dto.PlanDurationId &&
                    d.IsActive);

            if (selectedDuration == null)
                return null;

            decimal totalAmount = selectedDuration.Price;

            var merchantTxnId =
                $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid():N}".Substring(0, 30);

            var addons = new List<xtm_SubscribePlan>();

            if (dto.AddonPlanIds?.Any() == true)
            {
                addons = await _context.SubscribePlan
                    .Include(p => p.PlanDurations)
                    .Where(p => dto.AddonPlanIds.Contains(p.PlanId) && p.PlanIsAddOn)
                    .ToListAsync();

                totalAmount += addons.Sum(a =>
                    a.PlanDurations
                        .Where(d => d.IsActive && d.DurationDays == 0)
                        .Select(d => d.Price)
                        .FirstOrDefault());
            }

            var transaction = new xtm_SubscriptionTransaction
            {
                CompanyId = dto.CompanyId,
                Amount = totalAmount,
                PaymentProvider = "MSWIPE",
                TransactionRefId = merchantTxnId,
                Status = "INITIATED",
                CreatedOn = now
            };

            _context.SubscriptionTransaction.Add(transaction);
            await _context.SaveChangesAsync();

            var paymentLink = await _paymentService.CreatePaymentLink(
                transaction.TransactionRefId,
                totalAmount);

            transaction.PaymentLink = paymentLink;
            transaction.ProviderTransactionId = ExtractTransId(paymentLink);

            await _context.SaveChangesAsync();

            var startDate = now;
            var endDate = SafeSqlDate(startDate.AddDays(selectedDuration.DurationDays));

            var subscription = new xtm_CompanySubscription
            {
                CompanyId = dto.CompanyId,
                PlanId = mainPlan.PlanId,
                SubscriptionDate = now,
                SubscriptionStartDate = startDate,
                SubscriptionEndDate = endDate,
                SubscriptionAmount = selectedDuration.Price,
                SubscriptionDays = selectedDuration.DurationDays,
                SubscriptionDepCount = mainPlan.PlanDep,
                subscriptionTransRef = merchantTxnId,
                CreatedAt = now,
                Status = "PENDING"
            };

            _context.CompanySubscription.Add(subscription);
            await _context.SaveChangesAsync();

            foreach (var addon in addons)
            {
                var addonPrice = addon.PlanDurations
                    .Where(d => d.IsActive && d.DurationDays == 0)
                    .Select(d => d.Price)
                    .FirstOrDefault();

                _context.CompanySubscriptionAddon.Add(new xtm_CompanySubscriptionAddon
                {
                    CompanyId = dto.CompanyId,
                    MainPlanId = mainPlan.PlanId,
                    PlanId = addon.PlanId,
                    Amount = addonPrice,
                    DepCount = addon.PlanDep,
                    Status = "PENDING"
                });
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new RenewSubscriptionResponseDto
            {
                TransactionId = transaction.ProviderTransactionId,
                PaymentLink = paymentLink,
                Message = "Payment link generated successfully"
            };
        }


        private static DateTime SafeSqlDate(DateTime date)
        {
            var minSqlDate = new DateTime(1753, 1, 1);
            return date < minSqlDate ? minSqlDate : date;
        }

        public async Task<PaymentStatusResult> UpdatePaymentStatusAsync(string transactionRefId, string success)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            var transaction = await _context.SubscriptionTransaction
                .FirstOrDefaultAsync(x => x.TransactionRefId == transactionRefId);

            if (transaction == null)
            {
                return new PaymentStatusResult
                {
                    Success = "NOT_FOUND",
                    SubscriptionEndDate = null
                };
            }


            if (transaction.Status == "PENDING")
            {
                return new PaymentStatusResult
                {
                    Success = "PENDING",
                    SubscriptionEndDate = null
                };
            }

            success = success?.ToUpperInvariant();


            if (success == "FAILED")
            {
                transaction.Status = "FAILED";

                await ExpirePendingSubscriptions(
                    transaction.CompanyId,
                    transaction.TransactionRefId);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return new PaymentStatusResult
                {
                    Success = "FAILED",
                    SubscriptionEndDate = null
                };
            }

            if (success != "SUCCESS")
            {
                return new PaymentStatusResult
                {
                    Success = "INVALID_STATUS",
                    SubscriptionEndDate = null
                };
            }

            transaction.Status = "SUCCESS";

            var previousSub = await _context.CompanySubscription
                .FirstOrDefaultAsync(x =>
                    x.CompanyId == transaction.CompanyId &&
                    (x.Status == "ACTIVE" || x.Status == "TRIAL"));

            if (previousSub != null)
            {
                previousSub.Status = previousSub.Status == "TRIAL"
                    ? "TRIAL_EXPIRED"
                    : "EXPIRED";
            }

            var previousAddons = await _context.CompanySubscriptionAddon
                .Where(x =>
                    x.CompanyId == transaction.CompanyId &&
                    (x.Status == "ACTIVE" || x.Status == "TRIAL"))
                .ToListAsync();

            foreach (var addon in previousAddons)
            {
                addon.Status = addon.Status == "TRIAL"
                    ? "TRIAL_EXPIRED"
                    : "EXPIRED";
            }

            var subscription = await _context.CompanySubscription
                .FirstAsync(x =>
                    x.CompanyId == transaction.CompanyId &&
                    x.subscriptionTransRef == transaction.TransactionRefId);

            subscription.Status = "ACTIVE";

            var newAddons = await _context.CompanySubscriptionAddon
                .Where(x =>
                    x.CompanyId == transaction.CompanyId &&
                    x.Status == "PENDING")
                .ToListAsync();

            foreach (var addon in newAddons)
                addon.Status = "ACTIVE";

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new PaymentStatusResult
            {
                Success = "SUCCESS",
                SubscriptionEndDate = subscription.SubscriptionEndDate
            };
        }

        public async Task<MswipeTransactionStatusResponse> CheckTransactionStatusAsync(string transId)
        {
            var statusRequest = new
            {
                id = transId,
            };

            var statusResponse = await _httpClient.PostAsJsonAsync(
                "https://dcuat.mswipetech.co.in/ipg/api/getPBLTransactionDetails", statusRequest);

            var rawJson = await statusResponse.Content.ReadAsStringAsync();

            if (!statusResponse.IsSuccessStatusCode)
                throw new Exception($"Failed to check transaction status. Raw Response: {rawJson}");

            var result = System.Text.Json.JsonSerializer.Deserialize<MswipeTransactionStatusResponse>(
                rawJson,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result == null || !string.Equals(result.Status, "True", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Transaction status check failed: " + result?.ResponseMessage);

            return result;
        }

        private async Task ExpirePendingSubscriptions(int companyId, string transactionRefId)
        {

            var subscription = await _context.CompanySubscription
                .FirstOrDefaultAsync(x =>
                    x.CompanyId == companyId &&
                    x.subscriptionTransRef == transactionRefId);

            if (subscription == null)
                return;

            _context.CompanySubscription.Remove(subscription);


            var pendingAddons = await _context.CompanySubscriptionAddon
                .Where(x => x.CompanyId == companyId && x.Status == "PENDING")
                .ToListAsync();

            if (pendingAddons.Any())
                _context.CompanySubscriptionAddon.RemoveRange(pendingAddons);

            await _context.SaveChangesAsync();
        }

        private static string ExtractTransId(string paymentLink)
        {
            var uri = new Uri(paymentLink);
            var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

            return queryParams.TryGetValue("TransID", out var transId)
                ? transId.ToString()
                : "";
        }
    }
}
