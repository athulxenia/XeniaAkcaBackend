// Repositories/PaymentRepository.cs

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Models;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;

namespace XeniaAkcaBackend.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        // Easebuzz credentials
        private const string EASEBUZZ_KEY = "XIB2Z2FXW7";
        private const string EASEBUZZ_SALT = "GYIW2ZVE70";
        private const string EASEBUZZ_INITIATE_URL = "https://pay.easebuzz.in/payment/initiateLink";
        private const string EASEBUZZ_STATUS_URL = "https://dashboard.easebuzz.in/transaction/v2.1/retrieve";

        public PaymentRepository(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // ==================== SETTINGS ====================
        public async Task<List<PaymentSettingDto>> GetAllPaymentSettingsAsync()
        {
            return await _context.Settings
                .Select(s => new PaymentSettingDto
                {
                    SettingId = s.SettingId,
                    SettingName = s.SettingName,
                    SettingValue = s.SettingValue ?? 0,
                    PaymentGateway = s.PaymentGateway
                })
                .ToListAsync();
        }

        public async Task<PaymentResponse> UpdatePaymentSettingsAsync(List<PaymentSettingDto> settings)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var setting in settings)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        @"UPDATE AKCA_Settings 
                          SET settingValue = @settingValue 
                          WHERE settingId = @settingId",
                        new SqlParameter("@settingValue", setting.SettingValue),
                        new SqlParameter("@settingId", setting.SettingId)
                    );
                }

                await transaction.CommitAsync();
                return new PaymentResponse { Status = "success", Message = "Amount updated successfully" };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ==================== REGISTRATION PAYMENT ====================
        public async Task<PaymentResponse> RegistrationPaymentAsync(int userId, RegistrationPaymentRequest request)
        {
            if (string.IsNullOrEmpty(request.PaymentPaymentId) ||
                string.IsNullOrEmpty(request.PaymentOrderId) ||
                string.IsNullOrEmpty(request.PaymentSignature))
            {
                return new PaymentResponse { Status = "fail", Message = "PaymentPaymentId, PaymentOrderId, and PaymentSignature are required" };
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check existing payment
                var existingPayment = await _context.MemberPayments
                    .FirstOrDefaultAsync(p => p.PaymentOrderId == request.PaymentOrderId);

                if (existingPayment != null)
                {
                    // Update existing payment
                    await _context.Database.ExecuteSqlRawAsync(
                        @"UPDATE AKCA_MemberPayment
                          SET paymentStatus = @paymentStatus,
                              PaymentPaymentId = @PaymentPaymentId,
                              PaymentSignature = @PaymentSignature
                          WHERE PaymentOrderId = @PaymentOrderId",
                        new SqlParameter("@paymentStatus", request.PaymentStatus),
                        new SqlParameter("@PaymentPaymentId", request.PaymentPaymentId),
                        new SqlParameter("@PaymentSignature", request.PaymentSignature),
                        new SqlParameter("@PaymentOrderId", request.PaymentOrderId)
                    );

                    // Update member status
                    if (request.PaymentStatus == "success" || request.PaymentStatus == "pending")
                    {
                        int? memberStatus = request.PaymentTypeId switch
                        {
                            1 => 3,
                            2 => 4,
                            4 => 11,
                            _ => null
                        };

                        if (memberStatus.HasValue)
                        {
                            await _context.Database.ExecuteSqlRawAsync(
                                @"UPDATE AKCA_KaruthalMembers 
                                  SET memberStatus = @memberStatus 
                                  WHERE memberUserId = @userId",
                                new SqlParameter("@memberStatus", memberStatus.Value),
                                new SqlParameter("@userId", userId)
                            );
                        }
                    }

                    await transaction.CommitAsync();
                    return new PaymentResponse { Status = "success", Message = "Payment update successful" };
                }
                else
                {
                    await transaction.RollbackAsync();
                    return new PaymentResponse { Status = "fail", Message = "PaymentOrderId not found" };
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    
        public async Task<PaymentResponse> ContributionPaymentAsync(int userId, ContributionPaymentRequest request)
        {
            if (string.IsNullOrEmpty(request.ContributionPaymentId) ||
                string.IsNullOrEmpty(request.ContributionOrderId) ||
                string.IsNullOrEmpty(request.ContributionSignature))
            {
                return new PaymentResponse { Status = "fail", Message = "contributionPaymentId, contributionOrderId, and contributionSignature are required" };
            }

            var existingPayment = await _context.MemberContributions
                .FirstOrDefaultAsync(mc => mc.ContributionOrderId == request.ContributionOrderId);

            if (existingPayment != null)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    @"UPDATE AKCA_MemberContributions
                      SET paymentStatus = @paymentStatus,
                          contributionPaymentId = @contributionPaymentId,
                          contributionSignature = @contributionSignature
                      WHERE contributionOrderId = @contributionOrderId",
                    new SqlParameter("@paymentStatus", request.PaymentStatus),
                    new SqlParameter("@contributionPaymentId", request.ContributionPaymentId),
                    new SqlParameter("@contributionSignature", request.ContributionSignature),
                    new SqlParameter("@contributionOrderId", request.ContributionOrderId)
                );

                if (request.PaymentStatus == "success" || request.PaymentStatus == "pending")
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        @"UPDATE AKCA_KaruthalMembers 
                          SET memberStatus = 7
                          WHERE memberUserId = @userId",
                        new SqlParameter("@userId", userId)
                    );
                }

                return new PaymentResponse { Status = "success", Message = "Payment update successful" };
            }

            return new PaymentResponse { Status = "fail", Message = "contributionPaymentId not found" };
        }

        public async Task<object> InitiatePaymentAsync(int userId, InitiatePaymentRequest request)
        {
            // Generate hash
            string hashString = $"{EASEBUZZ_KEY}|{request.Txnid}|{request.Amount}|wallet|{request.Firstname}|{request.Email}|||||||||||{EASEBUZZ_SALT}";
            string hash = ComputeSha512Hash(hashString);

            var formData = new Dictionary<string, string>
            {
                { "key", EASEBUZZ_KEY },
                { "txnid", request.Txnid },
                { "amount", request.Amount.ToString() },
                { "productinfo", "wallet" },
                { "firstname", request.Firstname },
                { "phone", request.Phone },
                { "email", request.Email },
                { "surl", "https://akca.xeniapos.com/success" },
                { "furl", "https://akca.xeniapos.com/failure" },
                { "hash", hash },
                { "udf1", "" }, { "udf2", "" }, { "udf3", "" }, { "udf4", "" }, { "udf5", "" },
                { "udf6", "" }, { "udf7", "" }, { "udf8", "" }, { "udf9", "" }, { "udf10", "" },
                { "address1", request.Address1 ?? "" },
                { "address2", request.Address2 ?? "" },
                { "city", request.City ?? "" },
                { "state", request.State ?? "" },
                { "country", request.Country ?? "" },
                { "zipcode", request.Zipcode ?? "" }
            };

            var httpClient = _httpClientFactory.CreateClient();
            var content = new FormUrlEncodedContent(formData);
            var response = await httpClient.PostAsync(EASEBUZZ_INITIATE_URL, content);
            var responseData = await response.Content.ReadAsStringAsync();
            var easebuzzResponse = JsonConvert.DeserializeObject<dynamic>(responseData);

            if (easebuzzResponse?.data != null)
            {

                var member = await _context.KaruthalMembers
                    .Where(m => m.MemberUserId == userId)
                    .Select(m => new { m.MemberId, m.MemberDistrictId, m.MemberUnitId })
                    .FirstOrDefaultAsync();

                if (member == null)
                    return new { status = "fail", message = "User not found" };

                string paidDate = GetIstDateTimeString();

                
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO AKCA_MemberPayment
                      (memberId, paidAmount, paymentTypeId, paidDate, paidBy, paidDistrict, paidUnit, payMode, paymentStatus, isCallbackStatus, PaymentTxnRefNo)
                      VALUES (@memberId, @paidAmount, @paymentTypeId, @paidDate, @paidBy, @paidDistrict, @paidUnit, @payMode, 'initiate', 0, @PaymentTxnRefNo)",
                    new SqlParameter("@memberId", member.MemberId),
                    new SqlParameter("@paidAmount", request.Amount),
                    new SqlParameter("@paymentTypeId", request.PaymentTypeId),
                    new SqlParameter("@paidDate", paidDate),
                    new SqlParameter("@paidBy", userId),
                    new SqlParameter("@paidDistrict", member.MemberDistrictId),
                    new SqlParameter("@paidUnit", member.MemberUnitId ?? 0),
                    new SqlParameter("@payMode", request.PayOpt ?? "online"),
                    new SqlParameter("@PaymentTxnRefNo", request.Txnid)
                );

                
                if (request.PaymentTypeId == 5 && request.ContributionId.HasValue)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO AKCA_MemberContributions
                          (contributionId, memberId, contributionAmount, paidDate, paidBy, paidDistrict, paidUnit, payMode, paymentStatus, isCallbackStatus, PaymentTxnRefNo)
                          VALUES (@contributionId, @memberId, @paidAmount, @paidDate, @paidBy, @paidDistrict, @paidUnit, @payMode, 'initiate', 0, @PaymentTxnRefNo)",
                        new SqlParameter("@contributionId", request.ContributionId),
                        new SqlParameter("@memberId", member.MemberId),
                        new SqlParameter("@paidAmount", request.Amount),
                        new SqlParameter("@paidDate", paidDate),
                        new SqlParameter("@paidBy", userId),
                        new SqlParameter("@paidDistrict", member.MemberDistrictId),
                        new SqlParameter("@paidUnit", member.MemberUnitId ?? 0),
                        new SqlParameter("@payMode", request.PayOpt ?? "online"),
                        new SqlParameter("@PaymentTxnRefNo", request.Txnid)
                    );
                }

                return easebuzzResponse;
            }

            return new { status = "fail", message = "Easebuzz response invalid", detail = easebuzzResponse?.ToString() };
        }

        
        public async Task<object> CheckPaymentStatusAsync(string txnid)
        {
            string hashString = $"{EASEBUZZ_KEY}|{txnid}|{EASEBUZZ_SALT}";
            string hash = ComputeSha512Hash(hashString);

            var formData = new Dictionary<string, string>
            {
                { "txnid", txnid },
                { "key", EASEBUZZ_KEY },
                { "hash", hash }
            };

            var httpClient = _httpClientFactory.CreateClient();
            var content = new FormUrlEncodedContent(formData);
            var response = await httpClient.PostAsync(EASEBUZZ_STATUS_URL, content);
            var responseData = await response.Content.ReadAsStringAsync();
            var easebuzzResponse = JsonConvert.DeserializeObject<dynamic>(responseData);

            string paymentStatus = "unknown";
            if (easebuzzResponse?.msg != null)
            {
                var msgArray = easebuzzResponse.msg;
                var lastTxn = msgArray[msgArray.Count - 1];
                paymentStatus = lastTxn?.status?.ToString() ?? "unknown";
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
               
                await _context.Database.ExecuteSqlRawAsync(
                    @"UPDATE AKCA_MemberPayment
                      SET paymentStatus = @paymentStatus
                      WHERE PaymentTxnRefNo = @txnid AND paymentStatus <> 'success'",
                    new SqlParameter("@paymentStatus", paymentStatus),
                    new SqlParameter("@txnid", txnid)
                );

       
                await _context.Database.ExecuteSqlRawAsync(
                    @"UPDATE AKCA_MemberContributions
                      SET paymentStatus = @paymentStatus, isCallbackStatus = 1
                      WHERE PaymentTxnRefNo = @txnid",
                    new SqlParameter("@paymentStatus", paymentStatus),
                    new SqlParameter("@txnid", txnid)
                );

             
                if (paymentStatus == "success")
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        @"UPDATE KM
                          SET KM.memberKaruthalWallet = ISNULL(KM.memberKaruthalWallet, 0) + MP.paidAmount
                          FROM AKCA_KaruthalMembers KM
                          INNER JOIN AKCA_MemberPayment MP ON MP.memberId = KM.memberId
                          WHERE MP.PaymentTxnRefNo = @txnid
                            AND MP.paymentTypeId = 9
                            AND MP.paymentStatus = 'success'
                            AND MP.isCallbackStatus = 0",
                        new SqlParameter("@txnid", txnid)
                    );

                    await _context.Database.ExecuteSqlRawAsync(
                        @"UPDATE AKCA_MemberPayment
                          SET isCallbackStatus = 1
                          WHERE PaymentTxnRefNo = @txnid
                            AND paymentStatus = 'success'
                            AND isCallbackStatus = 0",
                        new SqlParameter("@txnid", txnid)
                    );
                }

                await transaction.CommitAsync();
                return easebuzzResponse;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

     
        public async Task<object> RecheckRecentTransactionsAsync()
        {
            var threeDaysAgo = DateTime.Now.AddDays(-3).Date;

            var txnList = await _context.MemberPayments
                .Where(p => p.PaymentStatus != "success" && p.PaidDate >= threeDaysAgo)
                .Select(p => p.PaymentTxnRefNo)
                .ToListAsync();

            if (txnList.Count == 0)
                return new { status = "ok", message = "No pending/failed transactions in last 3 days" };

            var updatedResults = new List<object>();
            foreach (var txnid in txnList)
            {
                try
                {
                    var result = await CheckPaymentStatusAsync(txnid!);
                    updatedResults.Add(new { txnid, result });
                }
                catch (Exception ex)
                {
                    updatedResults.Add(new { txnid, newStatus = "error", error = ex.Message });
                }
            }

            return new { status = "ok", updatedResults };
        }

     
        public async Task<WalletBalanceDto> GetMemberWalletBalanceAsync(int userId)
        {
            var member = await _context.KaruthalMembers
                .Where(m => m.MemberUserId == userId)
                .Select(m => new
                {
                    m.MemberUnitWallet,
                    m.MemberDistrictWallet,
                    m.MemberStateWallet,
                    m.MemberKaruthalWallet
                })
                .FirstOrDefaultAsync();

            if (member != null)
            {
                return new WalletBalanceDto
                {
                    Unit = member.MemberUnitWallet ?? 0,
                    District = member.MemberDistrictWallet ?? 0,
                    State = member.MemberStateWallet ?? 0,
                    Karuthal = member.MemberKaruthalWallet ?? 0
                };
            }

            return new WalletBalanceDto();
        }

     
        private string ComputeSha512Hash(string input)
        {
            using var sha512 = SHA512.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha512.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        private string GetIstDateTimeString()
        {
            var istTime = DateTime.UtcNow.AddMinutes(330);
            return istTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
}