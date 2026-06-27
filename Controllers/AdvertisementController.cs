using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Repositories.Advertisements;

namespace XeniaAkcaBackend.Controllers
{
    [ApiController]
    [Route("api/advertisement")]
    public class AdvertisementController : ControllerBase
    {
        private readonly IAdvertisementRepository _repo;

        public AdvertisementController(IAdvertisementRepository repo) => _repo = repo;

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateAdvertisementRequest request)
        {
            var result = await _repo.CreateAsync(request);
            return result.Status == "success" ? StatusCode(201, result) : BadRequest(result);
        }

        [Authorize]
        [HttpPut("{advertisementId:int}")]
        public async Task<IActionResult> Update(int advertisementId, [FromBody] UpdateAdvertisementRequest request)
        {
            var result = await _repo.UpdateAsync(advertisementId, request);
            return result.Status == "success" ? Ok(result) : NotFound(new { error = "Advertisement not found" });
        }

        [Authorize]
        [HttpGet("search/{partialName}")]
        public async Task<IActionResult> Search(string partialName)
            => Ok(await _repo.SearchAsync(partialName));

        [Authorize]
        [HttpGet("state")]
        public async Task<IActionResult> GetState(
            [FromQuery] int page = 1, [FromQuery] int limit = 10,
            [FromQuery] string? searchText = null, [FromQuery] int? districtid = null,
            [FromQuery] DateTime? fromdate = null, [FromQuery] DateTime? todate = null,
            [FromQuery] int? advertisementId = null)
            => Ok(await _repo.GetStateAsync(page, limit, searchText, districtid, fromdate, todate, advertisementId));

        [Authorize]
        [HttpPut("Approve/{advertisementId:int}")]
        public async Task<IActionResult> Approve(int advertisementId, [FromBody] ApproveAdvertisementRequest request)
        {
            if (request.ActiveStatus == null)
                return BadRequest(new { error = "Invalid activeStatus value. It must be a boolean." });

            var result = await _repo.ApproveAsync(advertisementId, request.ActiveStatus.Value);
            return result.Status == "success" ? Ok(result) : NotFound(new { error = "Advertisement not found" });
        }

        [HttpGet("advertisementList")]
        public async Task<IActionResult> GetList()
        {
            var districtIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserDistrictId");
            int districtId = districtIdClaim != null ? int.Parse(districtIdClaim.Value) : 0;

            var result = await _repo.GetListAsync(districtId);
            return Ok(new { status = "success", data = result });
        }
    }
}