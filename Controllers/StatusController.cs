
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

        // ─── Helper — get userId from JWT token ───────────────────
        private int GetUserIdFromToken()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
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
        [HttpGet("familymember")]                            // ← removed /{userId} from route
        public async Task<IActionResult> GetFamilyMember()
        {
            var userId = GetUserIdFromToken();               // ← from token
            if (userId == 0) return Unauthorized();

            var result = await _repo.GetFamilyMemberAsync(userId);
            return result != null ? Ok(result) : NotFound(new { message = "No family member found" });
        }

        [Authorize]
        [HttpPut("deactivate")]                              // ← removed /{userId} from route
        public async Task<IActionResult> MemberDeactivation([FromBody] DeactivateRequest request)
        {
            var userId = GetUserIdFromToken();               // ← from token
            if (userId == 0) return Unauthorized();

            var result = await _repo.MemberDeactivationAsync(userId, request.MemberReviseRemarks);
            return result != null ? Ok(result) : NotFound(new { error = "Member not found" });
        }

        [Authorize]
        [HttpGet("details")]                                 // ← removed /{userId} from route
        public async Task<IActionResult> GetMemberDetails()
        {
            var userId = GetUserIdFromToken();               // ← from token
            if (userId == 0) return Unauthorized();

            var result = await _repo.GetMemberDetailsAsync(userId);
            return result != null ? Ok(result) : NotFound(new { error = "Details not found" });
        }

        [Authorize]
        [HttpGet("receipt/{tranId?}")]                       // ← removed /{userId} from route
        public async Task<IActionResult> GetReceiptDetails(string? tranId)
        {
            var userId = GetUserIdFromToken();               // ← from token
            if (userId == 0) return Unauthorized();

            var result = await _repo.GetReceiptDetailsAsync(userId, tranId);
            return result.Count > 0 ? Ok(result) : NotFound(new { error = "Details not found" });
        }

        [HttpGet("watsappMob")]
        public async Task<IActionResult> CompanyWhatsappMob()
        {
            var result = await _repo.CompanyWhatsappMobAsync();
            return Ok(result);
        }


    }
}