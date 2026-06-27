using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaAkcaBackend.Repositories;

namespace XeniaAkcaBackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _repo;

        public DashboardController(IDashboardRepository repo)
        {
            _repo = repo;
        }

        private int GetUserIdFromToken()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

        private int? GetDistrictIdFromToken()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "UserDistrictId");
            if (claim != null && !string.IsNullOrEmpty(claim.Value) && int.TryParse(claim.Value, out var id) && id > 0)
                return id;
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard(
            [FromQuery] int? districtid,
            [FromQuery] int? unitid,
            [FromQuery] string? dateid,
            [FromQuery] DateTime? fromdate,
            [FromQuery] DateTime? todate)
        {
            int userId = GetUserIdFromToken();
            if (userId == 0) return Unauthorized();

           
            int? finalDistrictId = districtid ?? GetDistrictIdFromToken();

            var result = await _repo.GetDashboardAsync(userId, finalDistrictId, unitid, dateid, fromdate, todate);
            return Ok(result);
        }
    }
}