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

        // POST /api/contribution/create
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateContribution([FromBody] CreateContributionRequest request)
        {
            var result = await _repo.CreateContributionAsync(request);
            return result.Status == "success" ? Ok(result) : BadRequest(result);
        }

        // GET /api/contribution/searchMember/{partialName}
        [Authorize]
        [HttpGet("searchMember/{partialName}")]
        public async Task<IActionResult> GetMember(string partialName)
        {
            var result = await _repo.GetMembersByPartialNameAsync(partialName);
            return Ok(result);
        }

        // PUT /api/contribution/{contributionId}
        [Authorize]
        [HttpPut("{contributionId:int}")]
        public async Task<IActionResult> UpdateContribution(int contributionId,
            [FromBody] UpdateContributionRequest request)
        {
            var result = await _repo.UpdateContributionAsync(contributionId, request);
            return result.Status == "success" ? Ok(result) : BadRequest(result);
        }

        // GET /api/contribution/state/{status}/{contributionId?}
        [Authorize]
        [HttpGet("state/{status}/{contributionId:int?}")]
        public async Task<IActionResult> GetStateContribution(
            string status, int? contributionId,
            [FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? searchText = null)
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

        // GET /api/contribution/district/{status}/{districtId?}/{contributionId?}
        [Authorize]
        [HttpGet("district/{status}/{districtId:int?}/{contributionId:int?}")]
        public async Task<IActionResult> GetDistrictContribution(
            string status, int? districtId, int? contributionId,
            [FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? searchText = null)
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

        // GET /api/contribution/unit/{status}/{unitId?}/{contributionId?}
        [Authorize]
        [HttpGet("unit/{status}/{unitId:int?}/{contributionId:int?}")]
        public async Task<IActionResult> GetUnitContribution(
            string status, int? unitId, int? contributionId,
            [FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? searchText = null)
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

        // GET /api/contribution/pending/memberDtls/{userId}
        [Authorize]
        [HttpGet("pending/memberDtls/{userId:int}")]
        public async Task<IActionResult> ConPendingDetails(int userId)
        {
            var result = await _repo.ConPendingDetailsAsync(userId);
            return Ok(result);
        }

        // GET /api/contribution/payed/memberDtls/{userId}
        [Authorize]
        [HttpGet("payed/memberDtls/{userId:int}")]
        public async Task<IActionResult> ConPayedDetails(int userId)
        {
            var result = await _repo.ConPayedDetailsAsync(userId);
            return Ok(result);
        }

        // PUT /api/contribution/Approve/{contributionId}
        [Authorize]
        [HttpPut("Approve/{contributionId:int}")]
        public async Task<IActionResult> ApproveContribution(int contributionId,
            [FromBody] ApproveContributionRequest request)
        {
            var result = await _repo.ApproveContributionAsync(contributionId, request.ActiveStatus);
            return result.Status == "success" ? Ok(result) : BadRequest(result);
        }

        // GET /api/contribution/details/{contributionId?}
        [HttpGet("details/{contributionId:int?}")]
        public async Task<IActionResult> GetContributionDetails(int? contributionId)
        {
            if (!contributionId.HasValue) return BadRequest("ContributionId required.");
            var result = await _repo.GetContributionDetailsAsync(contributionId.Value);
            return result != null ? Ok(result) : NotFound();
        }

        // POST /api/contribution/notification/{memberId}
        [HttpPost("notification/{memberId:int}")]
        public async Task<IActionResult> ContributionAmountNotification(int memberId)
        {
            var result = await _repo.ContributionAmountNotificationAsync(memberId);
            return Ok(result);
        }

        // GET /api/contribution/contributionView/{contributionId}
        [HttpGet("contributionView/{contributionId:int}")]
        public async Task<IActionResult> DetailsOfContribution(int contributionId)
        {
            var result = await _repo.DetailsOfContributionAsync(contributionId);
            return result != null ? Ok(result) : NotFound();
        }

        // POST /api/contribution/all/notification
        [HttpPost("all/notification")]
        public async Task<IActionResult> ProcessAllContributionPayments()
        {
            var result = await _repo.ProcessAllContributionPaymentsAsync();
            return Ok(result);
        }

        // POST /api/contribution/notificationPush
        [HttpPost("notificationPush")]
        public async Task<IActionResult> FirebaseNotification([FromBody] int contributionMemberId)
        {
            var result = await _repo.SendFirebaseNotificationAsync(contributionMemberId);
            return Ok(result);
        }

        // PUT /api/contribution/updated/{contributionId}
        [HttpPut("updated/{contributionId:int}")]
        public async Task<IActionResult> ContributionUpdation(int contributionId,
            [FromBody] ContributionUpdationRequest request)
        {
            var result = await _repo.ContributionUpdationAsync(contributionId, request);
            return result.Status == "success" ? Ok(result) : BadRequest(result);
        }
    }
}