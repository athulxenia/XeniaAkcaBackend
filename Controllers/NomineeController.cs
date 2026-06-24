using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Repositories;

namespace XeniaAkcaBackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/nominee")]
    public class NomineeController : ControllerBase
    {
        private readonly INomineeRepository _repo;

        public NomineeController(INomineeRepository repo)
        {
            _repo = repo;
        }

        // GET /api/nominee/{userId?}
        // if userId provided → get single nominee
        // if no userId      → get paginated list
        [HttpGet("{userId:int?}")]
        public async Task<IActionResult> GetNominee(
            int? userId,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string search = "",
            [FromQuery] int? unitid = null)
        {
            if (userId.HasValue)
            {
                var nominee = await _repo.GetNomineeAsync(userId.Value);
                return nominee != null ? Ok(nominee) : NotFound();
            }
            else
            {
                var result = await _repo.GetAllNomineesAsync(page, limit, search, unitid);
                return Ok(result);
            }
        }

        // POST /api/nominee/{userId}
        [HttpPost("{userId:int}")]
        public async Task<IActionResult> UpdateNominee(
            int userId, [FromBody] UpdateNomineeRequest request)
        {
            var result = await _repo.UpdateNomineeAsync(userId, request);
            return result.Status == "success" ? Ok(result) : NotFound(result);
        }

        // PUT /api/nominee/{userId}
        [HttpPut("{userId:int}")]
        public async Task<IActionResult> ApproveNominee(
            int userId, [FromBody] ApproveNomineeRequest request)
        {
            var result = await _repo.ApproveNomineeAsync(userId, request.MemberStatus);
            return result != null ? Ok(result) : NotFound(new { error = "Member not found." });
        }
    }
}