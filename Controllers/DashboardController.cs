using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaAkcaBackend.Repositories.Dashboard;

namespace XeniaAkcaBackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _repo;

        public DashboardController(IDashboardRepository repo) => _repo = repo;

        /// <summary>
        /// Get state level dashboard with membership stats and payment graph
        /// </summary>
        [HttpGet("state")]
        public async Task<IActionResult> GetStateDashboard(
            [FromQuery] int? districtid, [FromQuery] string? dateid,
            [FromQuery] DateTime? fromdate, [FromQuery] DateTime? todate)
            => Ok(await _repo.GetDashboardAsync("state", districtid, null, dateid, fromdate, todate));

        /// <summary>
        /// Get state level membership statistics only (no graph)
        /// </summary>
        [HttpGet("state/membership")]
        public async Task<IActionResult> GetStateMembership(
            [FromQuery] int? districtid, [FromQuery] string? dateid,
            [FromQuery] DateTime? fromdate, [FromQuery] DateTime? todate)
            => Ok(await _repo.GetDashboardAsync("stateAA", districtid, null, dateid, fromdate, todate));

        /// <summary>
        /// Get state level payment graph only
        /// </summary>
        [HttpGet("state/graph")]
        public async Task<IActionResult> GetStateGraph(
            [FromQuery] int? districtid, [FromQuery] string? dateid,
            [FromQuery] DateTime? fromdate, [FromQuery] DateTime? todate)
            => Ok(await _repo.GetDashboardAsync("graph/stateAA", districtid, null, dateid, fromdate, todate));

        /// <summary>
        /// Get district level dashboard with membership stats and payment graph
        /// </summary>
        [HttpGet("district")]
        public async Task<IActionResult> GetDistrictDashboard(
            [FromQuery] int? districtid, [FromQuery] int? unitid, [FromQuery] string? dateid,
            [FromQuery] DateTime? fromdate, [FromQuery] DateTime? todate)
            => Ok(await _repo.GetDashboardAsync("district", districtid, unitid, dateid, fromdate, todate));

        /// <summary>
        /// Get district level membership statistics only (no graph)
        /// </summary>
        [HttpGet("district/membership")]
        public async Task<IActionResult> GetDistrictMembership(
            [FromQuery] int? districtid, [FromQuery] int? unitid, [FromQuery] string? dateid,
            [FromQuery] DateTime? fromdate, [FromQuery] DateTime? todate)
            => Ok(await _repo.GetDashboardAsync("districtAA", districtid, unitid, dateid, fromdate, todate));

        /// <summary>
        /// Get district level payment graph only
        /// </summary>
        [HttpGet("district/graph")]
        public async Task<IActionResult> GetDistrictGraph(
            [FromQuery] int? districtid, [FromQuery] int? unitid, [FromQuery] string? dateid,
            [FromQuery] DateTime? fromdate, [FromQuery] DateTime? todate)
            => Ok(await _repo.GetDashboardAsync("graph/districtAA", districtid, unitid, dateid, fromdate, todate));

        /// <summary>
        /// Get unit level membership statistics only
        /// </summary>
        [HttpGet("unit/membership")]
        public async Task<IActionResult> GetUnitMembership(
            [FromQuery] int? unitid, [FromQuery] string? dateid,
            [FromQuery] DateTime? fromdate, [FromQuery] DateTime? todate)
            => Ok(await _repo.GetDashboardAsync("unitAA", null, unitid, dateid, fromdate, todate));
    }
}