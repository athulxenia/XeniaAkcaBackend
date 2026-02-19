using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaCatalogueApi.Service.Common;
using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Repositories.Department;


namespace XeniaTokenBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly JwtHelperService _jwtHelperService;

        public DepartmentController(IDepartmentRepository departmentRepository, JwtHelperService jwtHelperService)
        {
            _departmentRepository = departmentRepository;
            _jwtHelperService = jwtHelperService;
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto dto)
        {
            try
            {
                var depId = await _departmentRepository.CreateDepartmentAsync(dto);
                return Ok(new
                {
                    status = "success",
                    message = "Departments created successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("web/{userId}")]   
        public async Task<IActionResult> GetDepartmentWebById(int userId)
        {
            try
            {
                var departments = await _departmentRepository.GetDepartmentWebByIdAsync(userId);

                if (departments == null || departments.Count == 0)
                {
                    return NotFound(new
                    {
                        status = "error",
                        message = "No departments found"
                    });
                }

                return Ok(new
                {
                    status = "success",
                    department = departments
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }



        [HttpGet("web/all/{companyId}")]
        public async Task<IActionResult> GetDepartmentWebAll(int companyId)
        {
            try
            {
                var departments = await _departmentRepository.GetDepartmentWebAll(companyId);

                if (departments == null || departments.Count == 0)
                {
                    return NotFound(new
                    {
                        status = "error",
                        message = "No departments found"
                    });
                }

                return Ok(new
                {
                    status = "success",
                    department = departments
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("update/{depId}")]
        public async Task<IActionResult> UpdateDepartment(int depId, [FromBody] UpdateDepartmentRequestDto dto)
        {
            try
            {
                var result = await _departmentRepository.UpdateDepartmentAsync(depId, dto);
                return Ok(result);
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

        [HttpGet("departments")]
        public async Task<IActionResult> GetAllDepartments([FromQuery] string? depNameSearch = null)
        {
            try
            {
                var departments = await _departmentRepository.GetAllDepartmentsAsync(depNameSearch);
                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }

        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetAllDepartmentsByCompany(int companyId)
        {
            try
            {
                var result = await _departmentRepository.GetAllDepartmentsByCompanyAsync(companyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }

        [HttpGet("app/{userId}")]
        public async Task<IActionResult> GetAllDepartmentsAppByUser(int userId)
        {
            try
            {
                var result = await _departmentRepository.GetAllDepartmentsAppByUserIdAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }

        [HttpDelete("delete/{depId}")]
        public async Task<IActionResult> DeleteDepartment(int depId)
        {
            try
            {
                var rowsAffected = await _departmentRepository.DeleteDepartmentAsync(depId);

                if (rowsAffected > 0)
                {
                    return Ok(new
                    {
                        status = "success",
                        message = "Department deleted successfully"
                    });
                }

                return NotFound(new
                {
                    status = "error",
                    message = "Department not found"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }


    }
}
