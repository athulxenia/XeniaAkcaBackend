using Microsoft.AspNetCore.Mvc;
using XeniaCatalogueApi.Service.Common;
using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Repositories.Token;
using XeniaTokenBackend.Service;

namespace XeniaTokenBackend.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly JwtHelperService _jwtHelperService;
        private readonly LiveTokenService _service;

        public TokenController(ITokenRepository tokenRepository, JwtHelperService jwtHelperService, LiveTokenService service)
        {
            _tokenRepository = tokenRepository;
            _jwtHelperService = jwtHelperService;
            _service = service;
        }


        [HttpPost("updateCustomTokenStatus")]
        public async Task<IActionResult> GetNextCustomTokenForCounter([FromBody] TokenRequestDto tokenData)
        {
            try
            {
                var result = await _tokenRepository.GetAndUpdateCustomToken(tokenData);
                return Ok(new { success = true, message = result });
            }
            catch
            {
                return StatusCode(500, new { success = false, message = "Internal server error occurred while retrieving and updating counter token." });
            }
        }


        [HttpGet("onHold")]
        public async Task<IActionResult> GetTokensOnHold()
        {
            try
            {
                var companyId = _jwtHelperService.GetCompanyId();
                var userId = _jwtHelperService.GetUserId();

  
                var tokens = await _tokenRepository.GetTokensOnHold(companyId, userId);
                return Ok(new { status = "success", tokensOnHold = tokens });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "failed", error = ex.Message });
            }
        }


        [HttpGet("onPending/{companyId}/{userId}")]
        public async Task<IActionResult> GetTokensOnPending(int companyId, int userId)
        {
            try
            {
                var tokens = await _tokenRepository.GetTokensOnPending(companyId, userId);
                return Ok(new { status = "success", tokensOnPending = tokens });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "failed", error = ex.Message });
            }
        }


        [HttpGet("onCompleted/{companyId}/{userId}")]
        public async Task<IActionResult> GetTokensOnCompleted(int companyId, int userId)
        {
            var tokens = await _tokenRepository.GetTokensByStatus(companyId, userId, "completed");
            return Ok(new { status = "success", tokensOnCompleted = tokens });
        }


        [HttpGet("tokenValue/{companyId}/{depId}")]
        public async Task<IActionResult> GetTokenValues(int companyId, int depId)
        {
            try
            {
                var tokenValues = await _tokenRepository.GetTokenValuesAsync(companyId, depId);
                return Ok(new { status = "success", tokenValues });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "failed", error = ex.Message });
            }
        }


        [HttpPost("updateTokenStatus")]
        public async Task<IActionResult> UpdateToken([FromBody] TokenUpdateRequest request)
        {
            var result = await _tokenRepository.GetAndUpdateCounterTokenAsync(request);

            if (result.OnCallToken == null)
                return NotFound(new { success = false, message = "No pending token found." });

            return Ok(new { success = true, result.UpdatedCurrentToken, result.OnCallToken });
        }


        [HttpPost("tokenStatusUpdate/{companyId}")]
        public async Task<IActionResult> UpdateTokenStatus(int companyId, int depId, string? depPrefix, int tokenValue, [FromBody] UpdateTokenStatusRequest request, bool iscomplete = true)
        {
            var result = await _tokenRepository.UpdateTokenStatusAsync(
                companyId, depId, depPrefix, tokenValue, iscomplete,
                request.UserId, request.ServiceId, request.CustomerId, request.CounterId);

            if (result.Success)
                return Ok(new { success = true, message = result.Message });

            return NotFound(new { success = false, message = result.Message });
        }


        [HttpGet("pendingToken/{companyId}/{userId}")]
        public async Task<IActionResult> GetPendingTokenValues(int companyId, int userId)
        {
            var pendingToken = await _tokenRepository.GetPendingTokenAsync(companyId, userId);

            return Ok(new
            {
                status = "success",
                PendingToken = pendingToken
            });
        }


        [HttpGet("checkToken/{companyId}/{depId}/{tokenValue}")]
        public async Task<IActionResult> CheckTokenValues(int companyId, int depId, int tokenValue)
        {
            try
            {
                var tokens = await _tokenRepository.CheckTokenValueAsync(companyId, depId, tokenValue);

                if (tokens != null && tokens.Any())
                {
                    return Ok(new { status = "success", PendingToken = tokens });
                }
                else
                {
                    return Ok(new { status = "success", PendingToken = 0 });
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { status = "failed", error = ex.Message });
            }
        }


        [HttpGet("summary/{companyId}")]
        public async Task<IActionResult> GetTokenHistorySummary( int companyId, [FromQuery] DateTime date)
        {
            if (date == default)
                return BadRequest(new { status = "failed", message = "Date is required" });

            var result = await _tokenRepository.GetTokenHistorySummaryAsync(companyId, date);

            return Ok(new
            {
                status = "success",
                summaryReport = result
            });
        }


        [HttpGet("report/tokenDetail/{companyId}")]
          public async Task<IActionResult> GetTokenHistoryReport(int companyId,[FromQuery] DateTime startDate,[FromQuery] DateTime endDate,[FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 10,[FromQuery] string searchParam = "")
          {
            if (startDate == default || endDate == default)
                return BadRequest(new { status = "failed", message = "Start date and end date are required." });

            var (data, totalCount) =
                await _tokenRepository.GetTokenHistoryDetailsAsync(
                    companyId, startDate, endDate, pageNumber, pageSize, searchParam);

                return Ok(new
                {
                    status = "success",
                    data,
                    totalCount
                });
        }


        [HttpGet("timeline/{tokenId}")]
        public async Task<IActionResult> GetTokenTimeline(int tokenId)
        {
            var data = await _tokenRepository.GetTokenTimelineAsync(tokenId);

            if (!data.Any())
                return Ok(new { status = "success", message = "No timeline data found", data });

            return Ok(new
            {
                status = "success",
                message = "Token timeline retrieved successfully",
                data
            });
        }


        [HttpPut("resetToken/{companyId}/{depId}")]
        public async Task<IActionResult> ResetToken(int companyId, int depId)
        {
            try
            {
                var message = await _tokenRepository.ResetTokenAsync(companyId, depId);
                return Ok(new { status = "success", message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "failed", error = ex.Message });
            }
        }


        [HttpPut("IsAnnounced/{companyId}/{depId}/{tokenValue}")]
        public async Task<IActionResult> UpdateIsAnnounced(int companyId, int depId, int tokenValue)
        {
            var result = await _tokenRepository.UpdateIsAnnouncedAsync(companyId, depId, tokenValue);

            if (!result)
                return NotFound(new { status = "failed", message = "Token not found" });

            return Ok(new { status = "success", message = "IsAnnounced updated successfully" });
        }


        [HttpPut("updateDepartment/{companyId}/{depId}/{depPrefix}/{tokenValue}")]
        public async Task<IActionResult> UpdateDepartment(int companyId, int depId, string depPrefix, int tokenValue, [FromBody] TokenUpdateDto tokenData)
        {
            var result = await _tokenRepository.UpdateDepartmentAsync(companyId, depId, depPrefix, tokenValue, tokenData);

            if (!result.Success)
                return BadRequest(new { status = "failed", message = result.Message });

            return Ok(new { status = "success", message = result.Message });
        }


        [HttpGet("onHold/{companyId}/{userId}")]
        public async Task<IActionResult> GetTokensOnHold(int companyId, int userId)
        {
            try
            {
                var tokensOnHold = await _tokenRepository.GetTokensOnHoldAsync(companyId, userId);

                return Ok(new
                {
                    status = "success",
                    tokensOnHold
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


        [HttpPut("recall")]
        public async Task<IActionResult> RecallToken([FromBody] TokenRecallDto tokenData)
        {
            if (tokenData == null)
                return BadRequest(new { message = "Invalid request data." });

            try
            {
                var result = await _tokenRepository.RecallTokenAsync(tokenData);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error recalling token", error = ex.Message });
            }
        }


        [HttpPost("upsert")]
        public async Task<IActionResult> UpsertToken([FromBody] TokenUpsertDto tokenData)
        {
            if (tokenData == null)
                return BadRequest(new { status = "error", message = "Token data is required" });

            try
            {
                var result = await _tokenRepository.UpsertTokenAsync(tokenData);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }


        [HttpGet("{tokenNumber}/{counterName}")]
        public async Task<IActionResult> GetTokenAudio(string tokenNumber,string counterName)
        {
            try
            {
                var mp3Bytes = await _tokenRepository
                    .GetTokenAudioAsync(tokenNumber, counterName);

                Response.Headers.Append("Accept-Ranges", "bytes");
                Response.Headers.Append("Cache-Control", "no-store");

                return File(mp3Bytes, "audio/mpeg", enableRangeProcessing: true);
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
