using Microsoft.AspNetCore.Mvc;
using XeniaCatalogueApi.Service.Common;
using XeniaTokenBackend.Repositories.Dashboard;

namespace XeniaTokenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly JwtHelperService _jwtHelperService;

        public DashboardController(IDashboardRepository dashboardRepository, JwtHelperService jwtHelperService)
        {
            _dashboardRepository = dashboardRepository;
            _jwtHelperService = jwtHelperService;
        }


        [HttpGet("dashboard")]
        public async Task<IActionResult> GetTokenValues()
        {
            try
            {
                int companyId = _jwtHelperService.GetCompanyId();


                var result = await _dashboardRepository.GetTokenValuesAsync(companyId);

                return Ok(new
                {
                    status = "success",
                    data = result
         
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "failed",
                    error = ex.Message
                });
            }
        }

    }
}
