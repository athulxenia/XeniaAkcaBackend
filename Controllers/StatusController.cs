using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Repositories;

namespace XeniaAkcaBackend.Controllers
{
    [ApiController]
    [Route("api/status")]
    public class StatusController : ControllerBase
    {
        private readonly IStatusRepository _repo;

        public StatusController(IStatusRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("server")]
        public async Task<IActionResult> CheckServerStatus()
        {
            var result = await _repo.CheckServerStatusAsync();
            return Ok(result);
        }

        [HttpGet("check/{userid:int}")]
        public async Task<IActionResult> GetAccStatus(int userid)
        {
            var result = await _repo.GetAccStatusAndPaymentHistoryAsync(userid);
            return Ok(result);
        }

        [HttpGet("TermsAndConditions/{statusId:int}")]
        public async Task<IActionResult> GetTermsAndConditions(int statusId)
        {
            var html = await _repo.GetTermsAndConditionsAsync(statusId);
            if (html == null) return NotFound();
            return Content(html, "text/html");
        }

        [HttpGet("PrivacyPolicy/{statusId:int}")]
        public async Task<IActionResult> GetPrivacyPolicy(int statusId)
        {
            var html = await _repo.GetPrivacyPolicyAsync(statusId);
            if (html == null) return NotFound();
            return Content(html, "text/html");
        }

        [Authorize]
        [HttpGet("familymember/{userId:int}")]
        public async Task<IActionResult> GetFamilyMember(int userId)
        {
            var result = await _repo.GetFamilyMemberAsync(userId);
            return result != null ? Ok(result) : NotFound(new { message = "No family member found" });
        }

        [Authorize]
        [HttpPut("deactivate/{userId:int}")]
        public async Task<IActionResult> MemberDeactivation(int userId, [FromBody] DeactivateRequest request)
        {
            var result = await _repo.MemberDeactivationAsync(userId, request.MemberReviseRemarks);
            return result != null ? Ok(result) : NotFound(new { error = "Member not found" });
        }

        [HttpGet("details/{userId:int}")]
        public async Task<IActionResult> GetMemberDetails(int userId)
        {
            var result = await _repo.GetMemberDetailsAsync(userId);
            return result != null ? Ok(result) : NotFound(new { error = "Details not found" });
        }

        [HttpGet("receipt/{userId:int}/{tranId?}")]
        public async Task<IActionResult> GetReceiptDetails(int userId, string? tranId)
        {
            var result = await _repo.GetReceiptDetailsAsync(userId, tranId);
            return result.Count > 0 ? Ok(result) : NotFound(new { error = "Details not found" });
        }

        [HttpGet("watsappMob")]
        public async Task<IActionResult> CompanyWhatsappMob()
        {
            var result = await _repo.CompanyWhatsappMobAsync();
            return Ok(result);
        }

        [HttpPost("accountInfo")]
        public async Task<IActionResult> MemberAccountDetails([FromBody] AccountInfoRequest request)
        {
            var result = await _repo.MemberAccountDetailsAsync(request.UserId);
            return result != null ? Ok(result) : NotFound(new { error = "Details not found" });
        }

        [HttpPut("account/update")]
        public async Task<IActionResult> UpdateMemberAccountDetails([FromBody] UpdateMemberFullDetailsRequest request)
        {
            var result = await _repo.UpdateMemberFullDetailsAsync(request);
            return result != null ? Ok(result) : NotFound(new { error = "Member not found or no changes made" });
        }
    }
}