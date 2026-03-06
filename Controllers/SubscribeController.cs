using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaQLaunchBackend.Dto;
using XeniaQLaunchBackend.Repositories.Subscription;

namespace XeniaQLaunchBackend.Controllers
{
    [ApiController]
    [Route("api/subscription")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionRepository _subscriptionRepository;

        public SubscriptionController(ISubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }


        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans()
        {
            var plans = await _subscriptionRepository.GetMainPlansAsync();
            return Ok(plans);
        }


        [HttpGet("addonPlans")]
        public async Task<IActionResult> GetAddonPlans()
        {
            var plans = await _subscriptionRepository.GetAddonPlansAsync();
            return Ok(plans);
        }


        [HttpPost("renew")]
        public async Task<IActionResult> RenewSubscription(
          [FromBody] RenewSubscriptionDto dto)
        {
            if (dto == null || dto.CompanyId <= 0 || dto.PlanId <= 0)
                return BadRequest("Invalid request");

            var result = await _subscriptionRepository.RenewSubscriptionAsync(dto);

            if (result == null)
                return BadRequest("Plan not found or inactive");

            return Ok(new
            {
                success = true,
                transactionId = result.TransactionId,
                paymentLink = result.PaymentLink,
                paymentStatus = result.PaymentStatus,
                message = result.Message
            });
        }


        [HttpPost("renew/update")]
        public async Task<IActionResult> UpdatePaymentStatus([FromQuery] string transactionRefId, [FromQuery] string success)
        {
            if (string.IsNullOrWhiteSpace(transactionRefId))
                return BadRequest("Invalid parameters");

            var result = await _subscriptionRepository
                .UpdatePaymentStatusAsync(transactionRefId, success);


            if (result.Success == "NOT_FOUND")
            {
                return NotFound(new
                {
                    success = false,
                    status = "NOT_FOUND",
                    message = "Transaction not found"
                });
            }


            if (result.Success == "PENDING")
            {
                return Ok(new
                {
                    success = false,
                    status = "PENDING",
                    message = "Payment is still pending. Please wait."
                });
            }


            if (result.Success == "FAILED")
            {
                return Ok(new
                {
                    success = false,
                    status = "FAILED",
                    message = "Payment failed. Subscription not activated."
                });
            }

            return Ok(new
            {
                success = true,
                status = "SUCCESS",
                message = "Payment successful. Subscription activated.",
                subscriptionEndDate = result.SubscriptionEndDate?.ToString("yyyy-MM-dd") ?? ""
            });
        }





        [AllowAnonymous]
        [HttpPost("mswipe/checkStatus")]
        public async Task<IActionResult> CheckTransactionStatus(string transId)
        {
            if (string.IsNullOrWhiteSpace(transId))
                return BadRequest("TransId is required.");

            try
            {
                var result = await _subscriptionRepository.CheckTransactionStatusAsync(transId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }


    }
}
