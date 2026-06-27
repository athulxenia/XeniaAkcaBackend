using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Repositories;

namespace XeniaAkcaBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UnitController : ControllerBase
    {
        private readonly IUnitRepository _unitRepository;

        public UnitController(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }


        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("UserId not found in token");
            return int.Parse(userIdClaim);
        }

       
        private int GetDistrictIdFromToken()
        {
            var districtIdClaim = User.FindFirst("UserDistrictId")?.Value;
            if (string.IsNullOrEmpty(districtIdClaim))
                throw new UnauthorizedAccessException("UserDistrictId not found in token");
            return int.Parse(districtIdClaim);
        }

    
        private int GetUnitIdFromToken()
        {
            var unitIdClaim = User.FindFirst("UserUnitId")?.Value;
            if (string.IsNullOrEmpty(unitIdClaim))
                return 0; 
            return int.Parse(unitIdClaim);
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateUnit([FromBody] UnitRequest request)
        {
            var result = await _unitRepository.CreateUnitAsync(request);
            return Ok(result);
        }

    
        [HttpGet("district")]
        public async Task<IActionResult> GetDistrictUnits(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string search = "")
        {
            int userId = GetUserIdFromToken(); 
            var result = await _unitRepository.GetDistrictUnitsAsync(userId, page, limit, search);
            return Ok(result);
        }


        [HttpGet("state/{districtId?}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllStateUnits(int? districtId, [FromQuery] string search = "", [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            object result;

            if (districtId.HasValue && districtId > 0)
            {
                result = await _unitRepository.GetAllStateUnitsAsync(search, page, limit, districtId.Value);
            }
            else
            {
                result = await _unitRepository.GetAllUnitsAsync(search, page, limit);
            }

            return Ok(result);
        }

      
        [HttpPut("{unitId}")]
        public async Task<IActionResult> UpdateUnit(int unitId, [FromBody] UnitRequest request)
        {
            var result = await _unitRepository.UpdateUnitAsync(unitId, request);
            return Ok(result);
        }

 
    
        [HttpDelete("{unitId}")]
        public async Task<IActionResult> DeleteUnit(int unitId)
        {
            var result = await _unitRepository.DeleteUnitAsync(unitId);
            return Ok(result);
        }

      
        [HttpGet("unitList")]
        public async Task<IActionResult> GetAllUnitDetails()
        {
            int districtId = GetDistrictIdFromToken(); 
            var result = await _unitRepository.GetAllUnitDetailsAsync(districtId);
            return Ok(result);
        }

       
        [HttpGet]
        public async Task<IActionResult> GetUnitMembers()
        {
            var result = await _unitRepository.GetUnitMembersAsync();
            return Ok(result);
        }

     
        [HttpGet("myunit")]
        public async Task<IActionResult> GetMyUnit()
        {
            int unitId = GetUnitIdFromToken(); 
           
            return Ok(new { UnitId = unitId, Message = "Token UnitId" });
        }
    }
}