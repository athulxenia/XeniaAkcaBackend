using Microsoft.AspNetCore.Mvc;
using XeniaTokenBackend.Repositories.Dashboard;

namespace XeniaTokenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardController(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }


        /*[HttpGet("dashboard/{companyId}")]
        public async Task<IActionResult> GetTokenCounts(int companyId)
        {
            try
            {
                var result = await _dashboardRepository.GetTokenCountsAsync(companyId);

                return Ok(new
                {
                    status = "success",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }*/

    }
}
