using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaTokenBackend.Repositories.Dashboard;

namespace XeniaTokenBackend.Controllers
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

        // GET /api/dashboard/stateAA
        [HttpGet("stateAA")]
        public async Task<IActionResult> GetAllStateWiseDetails(
            [FromQuery] int? districtid,
            [FromQuery] string? dateid,
            [FromQuery] DateTime? fromdate,
            [FromQuery] DateTime? todate)
        {
            var result = await _repo.GetAllStateWiseDetailsAsync(districtid, dateid, fromdate, todate);
            return Ok(result);
        }

        // GET /api/dashboard/districtAA
        [HttpGet("districtAA")]
        public async Task<IActionResult> GetAllDistrictWiseDetails(
            [FromQuery] int? districtid,
            [FromQuery] int? unitid,
            [FromQuery] string? dateid,
            [FromQuery] DateTime? fromdate,
            [FromQuery] DateTime? todate)
        {
            var result = await _repo.GetAllDistrictWiseDetailsAsync(districtid, unitid, dateid, fromdate, todate);
            return Ok(result);
        }

        // GET /api/dashboard/unitAA
        [HttpGet("unitAA")]
        public async Task<IActionResult> GetAllUnitWiseDetails(
            [FromQuery] int? unitid,
            [FromQuery] string? dateid,
            [FromQuery] DateTime? fromdate,
            [FromQuery] DateTime? todate)
        {
            var result = await _repo.GetAllUnitWiseDetailsAsync(unitid, dateid, fromdate, todate);
            return Ok(result);
        }

        // GET /api/dashboard/graph/stateAA
        [HttpGet("graph/stateAA")]
        public async Task<IActionResult> GetAllStateWiseGraphDetails(
            [FromQuery] int? districtid,
            [FromQuery] string? dateid,
            [FromQuery] DateTime? fromdate,
            [FromQuery] DateTime? todate)
        {
            var result = await _repo.GetAllStateWiseGraphDetailsAsync(districtid, dateid, fromdate, todate);
            return Ok(result);
        }

        // GET /api/dashboard/graph/districtAA
        [HttpGet("graph/districtAA")]
        public async Task<IActionResult> GetAllDistrictWiseGraphDetails(
            [FromQuery] int? districtid,
            [FromQuery] int? unitid,
            [FromQuery] string? dateid,
            [FromQuery] DateTime? fromdate,
            [FromQuery] DateTime? todate)
        {
            var result = await _repo.GetAllDistrictWiseGraphDetailsAsync(districtid, unitid, dateid, fromdate, todate);
            return Ok(result);
        }

        // GET /api/dashboard/state  ← main dashboard
        [HttpGet("state")]
        public async Task<IActionResult> GetAllStateWiseDetailsAndGraph(
            [FromQuery] int? districtid,
            [FromQuery] string? dateid,
            [FromQuery] DateTime? fromdate,
            [FromQuery] DateTime? todate)
        {
            var result = await _repo.GetAllStateWiseDetailsAndGraphAsync(districtid, dateid, fromdate, todate);
            return Ok(result);
        }

        // GET /api/dashboard/district  ← main dashboard
        [HttpGet("district")]
        public async Task<IActionResult> GetAllDistrictWiseDetailsAndGraph(
            [FromQuery] int? districtid,
            [FromQuery] int? unitid,
            [FromQuery] string? dateid,
            [FromQuery] DateTime? fromdate,
            [FromQuery] DateTime? todate)
        {
            var result = await _repo.GetAllDistrictWiseDetailsAndGraphAsync(districtid, unitid, dateid, fromdate, todate);
            return Ok(result);
        }
    }
}