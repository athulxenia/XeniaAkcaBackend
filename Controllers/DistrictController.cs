using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Repositories.Districts;

namespace XeniaAkcaBackend.Controllers
{
    [ApiController]
    [Route("api/district")]
    public class DistrictController : ControllerBase
    {
        private readonly IDistrictRepository _repo;

        public DistrictController(IDistrictRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDistrict()
        {
            var result = await _repo.GetAllDistrictAsync();
            return Ok(result);
        }

        [Authorize]
        [HttpPut("{districtid:int}")]
        public async Task<IActionResult> UpdateDistrict(
            int districtid,
            [FromBody] UpdateDistrictRequest request)
        {
            var result = await _repo.UpdateDistrictAsync(districtid, request);

            return result.Status switch
            {
                "success" => Ok(result),
                "partial success" => Ok(result),
                _ => NotFound(result)
            };
        }
    }
}