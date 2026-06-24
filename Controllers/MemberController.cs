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


        [HttpGet("district/{status:int}/{districtid:int?}")]
        public async Task<IActionResult> GetAllDistrictWiseMember(
            int status, int? districtid,
            [FromQuery] int page = 1, [FromQuery] int limit = 10,
            [FromQuery] string? searchText = null)
        {
            var result = await _repo.GetAllDistrictWiseMembersAsync(status, districtid ?? 0, page, limit, searchText);

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
        [HttpGet("unit/{status:int}/{unitid:int?}")]
        public async Task<IActionResult> GetAllUnitWiseMember(
            int status, int? unitid,
            [FromQuery] int page = 1, [FromQuery] int limit = 10,
            [FromQuery] string? searchText = null)
        {
            var result = await _repo.GetAllUnitWiseMembersAsync(status, unitid ?? 0, page, limit, searchText);

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
        [HttpGet("search/memberDtls/{memberid:int}")]
        public async Task<IActionResult> GetMemberDetails(int memberid)
        {
            var result = await _repo.GetMemberDetailsAsync(memberid);
            return result != null ? Ok(result) : NotFound();
        }

        
        [Authorize]
        [HttpPut("{userId:int}")]
        public async Task<IActionResult> UpdateMemberStatus(int userId, [FromBody] UpdateMemberStatusRequest request)
        {
            var result = await _repo.UpdateMemberStatusAsync(userId, request.MemberStatus, request.MemberReviseRemarks);

            // Use dynamic to access properties
            dynamic res = result;
            if (res.status == "success")
                return Ok(result);

            return NotFound(new { error = "Member not found or no changes were made" });
        }

      
        [Authorize]
        [HttpGet("childMembers/outstanding/{userId:int}")]
        public async Task<IActionResult> GetMemberOutstanding(int userId)
        {
            var result = await _repo.GetMemberOutstandingAsync(userId);
            return Ok(new { status = "success", data = result });
        }


        [Authorize]
        [HttpGet("childMembers/pendingApprove/{userId:int}")]
        public async Task<IActionResult> GetPendingApproveDetails(int userId)
        {
            var result = await _repo.GetPendingApproveDetailsAsync(userId);
            return Ok(result);
        }

   
        [Authorize]
        [HttpPut("childMembers/approve/{userId:int}")]
        public async Task<IActionResult> ChildMemberApprove(int userId, [FromBody] ChildApproveRequest request)
        {
            var result = await _repo.ChildMemberApproveAsync(userId, request.MemberStatus, request.MemberAction);

            // Use dynamic to access properties
            dynamic res = result;
            if (res.success == true)
                return Ok(result);

            return NotFound(new { error = "Member not found or no changes were made" });
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

        // ─── Karuthal Routes ─────────────────────────────────────

        // GET /api/member/checkMember/{membershipNumber}
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

        // GET /api/member/karuthalState/{status}/{pending?}
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

        // GET /api/member/karuthalDistrict/{status}/{districtid?}
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