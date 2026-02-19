using Microsoft.AspNetCore.Mvc;
using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Repositories.Company;

namespace XeniaTokenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : Controller
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyController(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

    
        [HttpPut("update/{companyId}")]
        public async Task<IActionResult> UpdateCompany(int companyId, [FromBody] UpdateCompanyDto dto)
        {
            try
            {
                var rowsAffected = await _companyRepository.UpdateCompanyAsync(companyId, dto);

                if (rowsAffected > 0)
                    return Ok(new { status = "success", message = "Company updated successfully" });
                else
                    return NotFound(new { status = "error", message = "Company not found" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

     
        [HttpGet("{companyId}")]
        public async Task<IActionResult> GetCompanyById(int companyId)
        {
            try
            {
                var company = await _companyRepository.GetCompanyByIdAsync(companyId);

                if (company == null)
                    return NotFound(new { status = "error", message = "Company not found" });

                return Ok(new { status = "success", company });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }


        [HttpPut("companysettings/{companySettingId}")]
        public async Task<IActionResult> UpdateCompanySettings(int companySettingId,[FromBody] CompanySettingsUpdateDto dto)
        {
            try
            {
                var rows = await _companyRepository
                    .UpdateCompanySettingsAsync(companySettingId, dto);

                if (rows == 0)
                    return NotFound(new { status = "error", message = "No record updated" });

                return Ok(new
                {
                    status = "success",
                    message = "Company settings updated successfully"
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
        }


        [HttpGet("companysettings")]
        public async Task<IActionResult> GetAllCompanySettings( [FromQuery] int companyId,[FromQuery] int userId)
        {
            var result = await _companyRepository
                .GetAllCompanySettingsAsync(companyId, userId);

            return Ok(result);
        }

    }
}
