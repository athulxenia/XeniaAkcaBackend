using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Repositories;
using XeniaAkcaBackend.Repositories.Member;

namespace XeniaAkcaBackend.Controllers
{
    [ApiController]
    [Route("api/member")]
    public class MemberController : ControllerBase
    {
        private readonly IMemberRepository _repo;

        public MemberController(IMemberRepository repo)
        {
            _repo = repo;
        }

      
        private int GetUserIdFromToken()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

      
        private int GetDistrictIdFromToken()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "UserDistrictId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }


        private int GetUnitIdFromToken()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "UserUnitId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

       
        [HttpGet("verify/{membershipNumber}")]
        public async Task<IActionResult> GetMember(string membershipNumber)
        {
            var isMobileNumber = Regex.IsMatch(membershipNumber, @"^\d+$");
            string prefix = "", number = membershipNumber;

            if (!isMobileNumber)
            {
                prefix = $"KH/{membershipNumber.Substring(2, 2)}/{membershipNumber.Substring(4, 2)}/";
                number = membershipNumber.Substring(6);
            }

            var result = await _repo.GetMemberAsync(prefix, number);
            return Ok(result);
        }

        [HttpGet("state/{status:int}/{pending?}")]
        public async Task<IActionResult> GetAllStateWiseMember(
            int status, string? pending,
            [FromQuery] int page = 1, [FromQuery] int limit = 10,
            [FromQuery] string? searchText = null,
            [FromQuery] int? districtid = null,
            [FromQuery] int? unitid = null)
        {
            int active = status;
            var result = await _repo.GetAllStateWiseMembersAsync(active, pending, page, limit, searchText, districtid, unitid);

            return Ok(new
            {
                status = "success",
                data = result.Records,
                totalPages = (int)Math.Ceiling((double)result.Total / limit),
                currentPage = page,
                limit,
                totalRecords = result.Total
            });
        }

        [HttpGet("checkMember/{membershipNumber}")]
        public async Task<IActionResult> GetOwnerDetails(string membershipNumber)
        {
            var isMobileNumber = Regex.IsMatch(membershipNumber, @"^\d+$");
            string? prefix = null, number = null, suffix = null;

            if (!isMobileNumber && membershipNumber.Length == 12)
            {
                prefix = $"KL{membershipNumber.Substring(2, 2)}/{membershipNumber.Substring(4, 2)}/";
                number = membershipNumber.Substring(6, 4).PadLeft(4, '0');
                suffix = $"/{membershipNumber.Substring(10, 2)}";
            }
            else if (isMobileNumber)
            {
                number = membershipNumber;
            }
            else
            {
                return BadRequest(new { message = "Invalid membership number format" });
            }

            var result = await _repo.GetOwnerDetailsAsync(prefix, number, suffix);

            if (result is List<object> list && list.Count > 0)
                return Ok(list[0]);

            return NotFound(new { message = "Member not found" });
        }

        // ==================== AUTHORIZED ENDPOINTS (Token UserId) ====================

        [Authorize]
        [HttpGet("district/{status:int}")]
        public async Task<IActionResult> GetAllDistrictWiseMember(
            int status,
            [FromQuery] int page = 1, [FromQuery] int limit = 10,
            [FromQuery] string? searchText = null)
        {
            int districtId = GetDistrictIdFromToken(); // ✅ Token-ൽ നിന്ന്
            var result = await _repo.GetAllDistrictWiseMembersAsync(status, districtId, page, limit, searchText);

            return Ok(new
            {
                status = "success",
                data = result.Records,
                totalPages = (int)Math.Ceiling((double)result.Total / limit),
                currentPage = page,
                limit,
                totalRecords = result.Total
            });
        }

        [Authorize]
        [HttpGet("unit/{status:int}")]
        public async Task<IActionResult> GetAllUnitWiseMember(
            int status,
            [FromQuery] int page = 1, [FromQuery] int limit = 10,
            [FromQuery] string? searchText = null)
        {
            int unitId = GetUnitIdFromToken(); 
            var result = await _repo.GetAllUnitWiseMembersAsync(status, unitId, page, limit, searchText);

            return Ok(new
            {
                status = "success",
                data = result.Records,
                totalPages = (int)Math.Ceiling((double)result.Total / limit),
                currentPage = page,
                limit,
                totalRecords = result.Total
            });
        }

        [Authorize]
        [HttpGet("search/memberDtls")]
        public async Task<IActionResult> GetMemberDetails()
        {
            int userId = GetUserIdFromToken();

           
            int memberId = await _repo.GetMemberIdByUserIdAsync(userId);

            if (memberId == 0)
                return NotFound(new { error = "Member not found" });

            var result = await _repo.GetMemberDetailsAsync(memberId);
            return result != null ? Ok(result) : NotFound();
        }

        [Authorize]
        [HttpPut("status")]
        public async Task<IActionResult> UpdateMemberStatus([FromBody] UpdateMemberStatusRequest request)
        {
            int userId = GetUserIdFromToken(); 
            var result = await _repo.UpdateMemberStatusAsync(userId, request.MemberStatus, request.MemberReviseRemarks);

            dynamic res = result;
            if (res.status == "success")
                return Ok(result);

            return NotFound(new { error = "Member not found or no changes were made" });
        }

        [Authorize]
        [HttpGet("childMembers/outstanding")]
        public async Task<IActionResult> GetMemberOutstanding()
        {
            int userId = GetUserIdFromToken(); 
            var result = await _repo.GetMemberOutstandingAsync(userId);
            return Ok(new { status = "success", data = result });
        }

        [Authorize]
        [HttpGet("childMembers/pendingApprove")]
        public async Task<IActionResult> GetPendingApproveDetails()
        {
            int userId = GetUserIdFromToken(); 
            var result = await _repo.GetPendingApproveDetailsAsync(userId);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("childMembers/approve")]
        public async Task<IActionResult> ChildMemberApprove([FromBody] ChildApproveRequest request)
        {
            int userId = GetUserIdFromToken(); 
            var result = await _repo.ChildMemberApproveAsync(userId, request.MemberStatus, request.MemberAction);

            dynamic res = result;
            if (res.success == true)
                return Ok(result);

            return NotFound(new { error = "Member not found or no changes were made" });
        }

        [Authorize]
        [HttpGet("accountInfo")]
        public async Task<IActionResult> MemberAccountDetails()
        {
            int userId = GetUserIdFromToken(); 
            if (userId == 0)
                return Unauthorized(new { error = "Invalid token" });

            var result = await _repo.MemberAccountDetailsAsync(userId);

            if (result != null && result.Count > 0)
            {
                var firstItem = result.FirstOrDefault();
                if (firstItem != null)
                {
                    var errorProp = firstItem.GetType().GetProperty("error");
                    if (errorProp != null)
                    {
                        var errorValue = errorProp.GetValue(firstItem)?.ToString();
                        if (!string.IsNullOrEmpty(errorValue))
                            return NotFound(result);
                    }
                }
                return Ok(result);
            }

            return NotFound(new { error = "Details not found" });
        }

        [Authorize]
        [HttpPut("account/update")]
        public async Task<IActionResult> UpdateMemberAccountDetails([FromBody] UpdateMemberFullDetailsRequest request)
        {
            int userId = GetUserIdFromToken(); 
            if (userId == 0) return Unauthorized();

            request.MemberId = userId;

            var result = await _repo.UpdateMemberFullDetailsAsync(request);
            return result != null ? Ok(result) : NotFound(new { error = "Member not found or no changes made" });
        }

        [HttpGet("memberStatus/{status:int?}")]
        public async Task<IActionResult> MemberStatusDetails(
            int? status,
            [FromQuery] int page = 1, [FromQuery] int limit = 10,
            [FromQuery] string? searchText = null)
        {
            var result = await _repo.GetMemberStatusDetailsAsync(status ?? 0, page, limit, searchText);

            return Ok(new
            {
                status = "success",
                data = result.Records,
                totalPages = (int)Math.Ceiling((double)result.Total / limit),
                currentPage = page,
                limit,
                totalRecords = result.Total
            });
        }

        [HttpGet("karuthalState/{status:int}/{pending?}")]
        public async Task<IActionResult> GetAllStateKaruthalMember(
            int status, string? pending,
            [FromQuery] int page = 1, [FromQuery] int limit = 10,
            [FromQuery] string? searchText = null,
            [FromQuery] int? districtid = null,
            [FromQuery] int? unitid = null)
        {
            var result = await _repo.GetAllStateKaruthalMemberAsync(status, pending, page, limit, searchText, districtid, unitid);

            return Ok(new
            {
                status = "success",
                data = result.Records,
                totalPages = (int)Math.Ceiling((double)result.Total / limit),
                currentPage = page,
                limit,
                totalRecords = result.Total
            });
        }

        [HttpGet("karuthalDistrict/{status:int}/{districtid:int?}")]
        public async Task<IActionResult> GetAllDistrictKaruthalMember(
            int status, int? districtid,
            [FromQuery] int page = 1, [FromQuery] int limit = 10,
            [FromQuery] string? searchText = null)
        {
            var result = await _repo.GetAllDistrictKaruthalMemberAsync(status, districtid ?? 0, page, limit, searchText);

            return Ok(new
            {
                status = "success",
                data = result.Records,
                totalPages = (int)Math.Ceiling((double)result.Total / limit),
                currentPage = page,
                limit,
                totalRecords = result.Total
            });
        }
    }
}