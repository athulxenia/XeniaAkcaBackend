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

        private int GetUserIdFromToken()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }


        [HttpGet]
        public async Task<IActionResult> GetNominee()
        {
            var userId = GetUserIdFromToken();
            if (userId == 0) return Unauthorized();

            var nominee = await _repo.GetNomineeAsync(userId);
            return nominee != null ? Ok(nominee) : NotFound(new { error = "Nominee not found" });
        }


        [HttpPost("update")]
        public async Task<IActionResult> UpdateNominee([FromBody] UpdateNomineeRequest request)
        {
            var userId = GetUserIdFromToken();
            if (userId == 0) return Unauthorized();

            var result = await _repo.UpdateNomineeAsync(userId, request);
            return result.Status == "success" ? Ok(result) : NotFound(result);
        }

     
        [HttpPut("approve")]
        public async Task<IActionResult> ApproveNominee([FromBody] ApproveNomineeRequest request)
        {
            var userId = GetUserIdFromToken();
            if (userId == 0) return Unauthorized();

            var result = await _repo.ApproveNomineeAsync(userId, request.MemberStatus);
            return result != null ? Ok(result) : NotFound(new { error = "Member not found." });
        }
    }
}