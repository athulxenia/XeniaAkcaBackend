using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Repositories.Report;

namespace XeniaAkcaBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportRepository _repo;

        public ReportController(IReportRepository repo) => _repo = repo;

        // Node.js: router.get('/contribution/state/:status', ...)
        [HttpGet("contribution/state/{status}")]
        public async Task<IActionResult> StateContribution(int status, [FromQuery] ContributionRequest request)
        {
            request.Status = status;
            var result = await _repo.GetContributionReportAsync("state", status, request);
            return Ok(result);
        }

        // Node.js: router.get('/contribution/district/:status', ...)
        [HttpGet("contribution/district/{status}")]
        public async Task<IActionResult> DistrictContribution(int status, [FromQuery] ContributionRequest request)
        {
            request.Status = status;
            var result = await _repo.GetContributionReportAsync("district", status, request);
            return Ok(result);
        }

        // Node.js: router.get('/contribution/unit/:status', ...)
        [HttpGet("contribution/unit/{status}")]
        public async Task<IActionResult> UnitContribution(int status, [FromQuery] ContributionRequest request)
        {
            request.Status = status;
            var result = await _repo.GetContributionReportAsync("unit", status, request);
            return Ok(result);
        }

        // Node.js: router.get('/payment', ...)
        [HttpGet("payment")]
        public async Task<IActionResult> Payment([FromQuery] PaymentRequest request)
        {
            var result = await _repo.GetPaymentReportAsync(request);
            return Ok(result);
        }

        // Node.js: router.get('/event', ...)
        [HttpGet("event")]
        public async Task<IActionResult> GetEvent([FromQuery] string? searchText)
        {
            var result = await _repo.GetEventsAsync(searchText);
            return Ok(new { status = "success", data = result });
        }
    }
}
