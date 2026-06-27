using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Repositories;

namespace XeniaAkcaBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentController(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        // ✅ Token-ൽ നിന്ന് userId extract ചെയ്യുന്ന helper
        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("UserId not found in token");

            return int.Parse(userIdClaim);
        }

        // ==================== SETTINGS (No userId needed) ====================
        [HttpGet]
        public async Task<IActionResult> GetAllPayment()
        {
            var result = await _paymentRepository.GetAllPaymentSettingsAsync();
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdatePayment([FromBody] UpdatePaymentSettingsRequest request)
        {
            var result = await _paymentRepository.UpdatePaymentSettingsAsync(request.Settings);
            return Ok(result);
        }

     
        [HttpPost("registration")]
        public async Task<IActionResult> RegistrationPayment([FromBody] RegistrationPaymentRequest request)
        {
            int userId = GetUserIdFromToken(); 
            var result = await _paymentRepository.RegistrationPaymentAsync(userId, request);
            return Ok(result);
        }

        [HttpPost("contribution")]
        public async Task<IActionResult> ContributionPayment([FromBody] ContributionPaymentRequest request)
        {
            int userId = GetUserIdFromToken(); 
            var result = await _paymentRepository.ContributionPaymentAsync(userId, request);
            return Ok(result);
        }

        [HttpGet("walletBalance")]
        public async Task<IActionResult> MemberWalletBalance()
        {
            int userId = GetUserIdFromToken(); 
            var result = await _paymentRepository.GetMemberWalletBalanceAsync(userId);
            return Ok(result);
        }

        [HttpPost("easebuzz/initiate")]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest request)
        {
            int userId = GetUserIdFromToken(); 
            var result = await _paymentRepository.InitiatePaymentAsync(userId, request);
            return Ok(result);
        }

       
        [HttpPost("easebuzz/status/{txnid}")]
        public async Task<IActionResult> CheckPaymentStatus(string txnid)
        {
            var result = await _paymentRepository.CheckPaymentStatusAsync(txnid);
            return Ok(result);
        }

        [HttpGet("recheckRecentTransactions")]
        public async Task<IActionResult> RecheckRecentTransactions()
        {
            var result = await _paymentRepository.RecheckRecentTransactionsAsync();
            return Ok(result);
        }
    }
}