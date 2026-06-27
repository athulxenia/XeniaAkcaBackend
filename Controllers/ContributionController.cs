using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Models;
using XeniaAkcaBackend.Repositories;

namespace XeniaAkcaBackend.Controllers
{
    [ApiController]
    [Route("api/contribution")]
    public class ContributionController : ControllerBase
    {
        private readonly IContributionRepository _repo;

        public ContributionController(IContributionRepository repo)
        {
            _repo = repo;
        }

    
        private int GetUserIdFromToken()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateContribution([FromBody] CreateContributionRequest request)
        {
            var result = await _repo.CreateContributionAsync(request);
            return result.Status == "success" ? Ok(result) : BadRequest(result);
        }

        [Authorize]
        [HttpGet("searchMember/{partialName}")]
        public async Task<IActionResult> GetMember(string partialName)
        {
            var result = await _repo.GetMembersByPartialNameAsync(partialName);
            return Ok(result);
        }

        [HttpPut("{contributionId:int}")]
        public async Task<IActionResult> UpdateContribution(int contributionId,
            [FromBody] UpdateContributionRequest request)
        {
            var result = await _repo.UpdateContributionAsync(contributionId, request);
            return result.Status == "success" ? Ok(result) : BadRequest(result);
        }

        [Authorize]
        [HttpGet("state/{status}/{contributionId:int?}")]
        public async Task<IActionResult> GetStateContribution(
            string status, int? contributionId,
            [FromQuery] int page = 1, [FromQuery] int limit = 10,
            [FromQuery] string? searchText = null)
        {
            if (contributionId.HasValue)
            {
                var detail = await _repo.GetContributionDetailsAsync(contributionId.Value);
                return detail != null ? Ok(detail) : NotFound();
            }

            var result = status == "pending"
                ? await _repo.GetStatePendingContributionAsync(page, limit, searchText)
                : await _repo.GetStateApproveContributionAsync(page, limit, searchText);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("district/{status}/{districtId:int?}/{contributionId:int?}")]
        public async Task<IActionResult> GetDistrictContribution(
            string status, int? districtId, int? contributionId,
            [FromQuery] int page = 1, [FromQuery] int limit = 10,
            [FromQuery] string? searchText = null)
        {
            if (contributionId.HasValue)
            {
                var detail = await _repo.GetContributionAsync(contributionId.Value);
                return detail != null ? Ok(detail) : NotFound();
            }

            var result = status == "pending"
                ? await _repo.GetDistrictPendingContributionAsync(page, limit, searchText, districtId)
                : await _repo.GetDistrictApproveContributionAsync(page, limit, searchText, districtId);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("unit/{status}/{unitId:int?}/{contributionId:int?}")]
        public async Task<IActionResult> GetUnitContribution(
            string status, int? unitId, int? contributionId,
            [FromQuery] int page = 1, [FromQuery] int limit = 10,
            [FromQuery] string? searchText = null)
        {
            if (contributionId.HasValue)
            {
                var detail = await _repo.GetContributionAsync(contributionId.Value);
                return detail != null ? Ok(detail) : NotFound();
            }

            var result = status == "pending"
                ? await _repo.GetUnitPendingContributionAsync(page, limit, searchText, unitId)
                : await _repo.GetUnitApproveContributionAsync(page, limit, searchText, unitId);

            return Ok(result);
        }

    
        [Authorize]
        [HttpGet("pending/memberDtls")]           
        public async Task<IActionResult> ConPendingDetails()
        {
            var userId = GetUserIdFromToken();
            if (userId == 0) return Unauthorized();

            var result = await _repo.ConPendingDetailsAsync(userId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("payed/memberDtls")]          
        public async Task<IActionResult> ConPayedDetails()
        {
            var userId = GetUserIdFromToken();
            if (userId == 0) return Unauthorized();

            var result = await _repo.ConPayedDetailsAsync(userId);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("Approve/{contributionId:int}")]
        public async Task<IActionResult> ApproveContribution(int contributionId,
            [FromBody] ApproveContributionRequest request)
        {
            var result = await _repo.ApproveContributionAsync(contributionId, request.ActiveStatus);
            return result.Status == "success" ? Ok(result) : BadRequest(result);
        }

        [HttpGet("details/{contributionId:int?}")]
        public async Task<IActionResult> GetContributionDetails(int? contributionId)
        {
            if (!contributionId.HasValue) return BadRequest("ContributionId required.");
            var result = await _repo.GetContributionDetailsAsync(contributionId.Value);
            return result != null ? Ok(result) : NotFound();
        }

        [Authorize]
        [HttpPost("notification")]               
        public async Task<IActionResult> ContributionAmountNotification()
        {
            var userId = GetUserIdFromToken();
            if (userId == 0) return Unauthorized();

            var result = await _repo.ContributionAmountNotificationAsync(userId);
            return Ok(result);
        }

        [HttpGet("contributionView/{contributionId:int}")]
        public async Task<IActionResult> DetailsOfContribution(int contributionId)
        {
            var result = await _repo.DetailsOfContributionAsync(contributionId);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost("all/notification")]
        public async Task<IActionResult> ProcessAllContributionPayments()
        {
            var result = await _repo.ProcessAllContributionPaymentsAsync();
            return Ok(result);
        }

        [HttpPost("notificationPush")]
        public async Task<IActionResult> FirebaseNotification([FromBody] int contributionMemberId)
        {
            var result = await _repo.SendFirebaseNotificationAsync(contributionMemberId);
            return Ok(result);
        }

        [HttpPut("updated/{contributionId:int}")]
        public async Task<IActionResult> ContributionUpdation(int contributionId,
            [FromBody] ContributionUpdationRequest request)
        {
            var result = await _repo.ContributionUpdationAsync(contributionId, request);
            return result.Status == "success" ? Ok(result) : BadRequest(result);
        }
    }
}