using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Repositories.Informations;

namespace XeniaAkcaBackend.Controllers
{
    [ApiController]
    [Route("api/information")]
    public class InformationController : ControllerBase
    {
        private readonly IInformationRepository _repo;

        public InformationController(IInformationRepository repo)
        {
            _repo = repo;
        }

        // POST /api/information/create
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateInformation([FromBody] CreateInformationRequest request)
        {
            var result = await _repo.CreateInformationAsync(request);
            return result.Status == "success" ? StatusCode(201, result) : BadRequest(result);
        }

        // PUT /api/information/{informationId}
        [Authorize]
        [HttpPut("{informationId:int}")]
        public async Task<IActionResult> UpdateInformation(
            int informationId, [FromBody] UpdateInformationRequest request)
        {
            var result = await _repo.UpdateInformationAsync(informationId, request);
            return result.Status == "success" ? Ok(result) : NotFound(result);
        }

        // GET /api/information/search/{partialName}
        [Authorize]
        [HttpGet("search/{partialName}")]
        public async Task<IActionResult> GetInformation(string partialName)
        {
            var result = await _repo.GetInformationByPartialNameAsync(partialName);
            return Ok(result);
        }

        // GET /api/information/state
        [Authorize]
        [HttpGet("state")]
        public async Task<IActionResult> GetStateInformation(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? searchText = null,
            [FromQuery] int? districtid = null,
            [FromQuery] DateTime? fromdate = null,
            [FromQuery] DateTime? todate = null,
            [FromQuery] int? informationId = null)
        {
            var request = new InformationListRequest
            {
                Page = page,
                Limit = limit,
                SearchText = searchText,
                DistrictId = districtid,
                FromDate = fromdate,
                ToDate = todate,
                InformationId = informationId
            };

            var (records, total) = await _repo.GetStateInformationAsync(request);
            var totalPages = (int)Math.Ceiling((double)total / limit);

            // Mirror Node.js: if single record by id → return object, else array
            object responseData = informationId.HasValue && records.Count == 1
                ? records[0]
                : records;

            return Ok(new
            {
                Status = "success",
                Data = responseData,
                TotalPages = totalPages,
                CurrentPage = page,
                Limit = limit,
                TotalRecords = total
            });
        }

        // PUT /api/information/Approve/{informationId}
        [Authorize]
        [HttpPut("Approve/{informationId:int}")]
        public async Task<IActionResult> ApproveInformation(
            int informationId, [FromBody] ApproveInformationRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Invalid activeStatus value. It must be a boolean." });

            var result = await _repo.ApproveInformationAsync(informationId, request.ActiveStatus);

            return result != null ? Ok(result) : NotFound(new { error = "Information not found." });
        }

        // GET /api/information/details/{districtId}
        [HttpGet("details/{districtId:int}")]
        public async Task<IActionResult> GetInformationDetails(int districtId)
        {
            var result = await _repo.GetInformationDetailsAsync(districtId);

            return result.Count > 0
                ? Ok(result)
                : NotFound(new { error = "Details not found." });
        }
    }
}