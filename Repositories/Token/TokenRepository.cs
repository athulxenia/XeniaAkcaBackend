using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using NAudio.Lame;
using NAudio.Wave;
using System.Globalization;
using System.Net.Http;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Models;

namespace XeniaTokenBackend.Repositories.Token
{
    public class TokenRepository : ITokenRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;



        public TokenRepository(ApplicationDbContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetAndUpdateCustomToken(TokenRequestDto tokenData)
        {
            string tokenStatus = string.Empty;

            var existingToken = await _context.xtm_TokenRegister
                .Where(t => t.CompanyID == tokenData.CompanyID &&
                            t.DepID == tokenData.DepID &&
                            t.TokenStatus == "onCall")
                .OrderBy(t => t.CreatedDate)
                .FirstOrDefaultAsync();

            if (existingToken != null)
            {
                if (tokenData.Status == 0)
                {
                    existingToken.TokenStatus = "completed";
                    existingToken.IsAnnounced = true;
                }
                else
                {
                    existingToken.TokenStatus = "onHold";
                    existingToken.IsAnnounced = false;
                }
                existingToken.StatusModifiedDate = DateTime.Now;
                existingToken.StatusModifiedUser = tokenData.StatusModifiedUser;

                await _context.SaveChangesAsync();

                var newToken = await _context.xtm_TokenRegister
                    .FirstOrDefaultAsync(t => t.CompanyID == tokenData.CompanyID &&
                                              t.DepID == tokenData.DepID &&
                                              t.TokenValue == tokenData.NewPrintValue);

                if (newToken != null)
                {
                    newToken.CustomerID = 0;
                    newToken.TokenStatus = tokenData.TokenStatus;
                    newToken.StatusModifiedDate = DateTime.Now;
                    newToken.StatusModifiedUser = tokenData.StatusModifiedUser;
                    newToken.IsAnnounced = false;
                    newToken.TokenActive = true;

                    await _context.SaveChangesAsync();
                    tokenStatus = "Token updated successfully";
                }
                else
                {
                    var newRecord = new xtm_TokenRegister
                    {
                        CompanyID = tokenData.CompanyID,
                        DepID = tokenData.DepID,
                        DepPrefix = tokenData.DepPrefix,
                        CounterID = tokenData.CounterID,
                        CustomerID = 0,
                        CreatedUserID = tokenData.CreatedUserID,
                        CreatedSource = "web",
                        CreatedDate = DateTime.Now,
                        TokenValue = tokenData.NewPrintValue,
                        TokenStatus = tokenData.TokenStatus,
                        StatusModifiedDate = DateTime.Now,
                        StatusModifiedUser = tokenData.StatusModifiedUser,
                        IsAnnounced = false,
                        TokenActive = true
                    };

                    await _context.xtm_TokenRegister.AddAsync(newRecord);
                    await _context.SaveChangesAsync();
                    tokenStatus = "Token inserted successfully";
                }
            }
            else
            {
                var duplicateToken = await _context.xtm_TokenRegister
                    .FirstOrDefaultAsync(t => t.CompanyID == tokenData.CompanyID &&
                                              t.DepID == tokenData.DepID &&
                                              t.TokenValue == tokenData.NewPrintValue &&
                                              t.TokenStatus == "onCall");

                if (duplicateToken != null)
                {
                    tokenStatus = "Duplicate token with 'onCall' status exists";
                }
                else
                {
                    var newRecord = new xtm_TokenRegister
                    {
                        CompanyID = tokenData.CompanyID,
                        DepID = tokenData.DepID,
                        DepPrefix = tokenData.DepPrefix,
                        CounterID = tokenData.CounterID,
                        CustomerID = 0,
                        CreatedUserID = tokenData.CreatedUserID,
                        CreatedSource = "web",
                        CreatedDate = tokenData.CreatedDate,
                        TokenValue = tokenData.NewPrintValue,
                        TokenStatus = tokenData.TokenStatus,
                        StatusModifiedDate = tokenData.ModifiedDate,
                        StatusModifiedUser = tokenData.StatusModifiedUser,
                        IsAnnounced = false,
                        TokenActive = true
                    };

                    await _context.xtm_TokenRegister.AddAsync(newRecord);
                    await _context.SaveChangesAsync();
                    tokenStatus = "Token called successfully";
                }
            }

            return tokenStatus;
        }
        public async Task<IEnumerable<TokenOnStatusDto>> GetTokensOnHold(int companyId, int userId)
        {
            var query = from t in _context.xtm_TokenRegister
                        join u in _context.xtm_UserMap on t.DepID equals u.DepID
                        join s in _context.xtm_Service on t.ServiceID equals s.SerID into serviceGroup
                        from s in serviceGroup.DefaultIfEmpty()
                        where t.TokenStatus == "onHold"
                              && t.CompanyID == companyId
                              && u.UserID == userId
                              && t.TokenActive
                              && u.Status == true
                        select new TokenOnStatusDto
                        {
                            TokenID = t.TokenID,
                            CounterID = t.CounterID,
                            CustomerID = t.CustomerID,
                            CreatedUserID = t.CreatedUserID,
                            CreatedSource = t.CreatedSource,
                            CreatedDate = t.CreatedDate,
                            TokenValue = t.TokenValue,
                            TokenStatus = t.TokenStatus,
                            StatusModifiedDate = t.StatusModifiedDate,
                            StatusModifiedUser = t.StatusModifiedUser,
                            DepID = t.DepID,
                            DepPrefix = t.DepPrefix,
                            ServiceID = t.ServiceID,
                            ServiceName = s.SerName
                        };

            return await query.ToListAsync();
        }
        public async Task<IEnumerable<TokenOnStatusDto>> GetTokensOnPending(int companyId, int userId)
        {
            var query = from tr in _context.xtm_TokenRegister
                        join um in _context.xtm_UserMap on tr.DepID equals um.DepID
                        join s in _context.xtm_Service on tr.ServiceID equals s.SerID into serviceGroup
                        from s in serviceGroup.DefaultIfEmpty()
                        where tr.TokenStatus == "Pending"
                              && tr.CompanyID == companyId
                              && tr.TokenActive
                              && um.UserID == userId
                              && um.Status == true
                        select new TokenOnStatusDto
                        {
                            DepID = tr.DepID,
                            DepPrefix = tr.DepPrefix,
                            TokenID = tr.TokenID,
                            CounterID = tr.CounterID,
                            CustomerID = tr.CustomerID,
                            CreatedUserID = tr.CreatedUserID,
                            CreatedSource = tr.CreatedSource,
                            CreatedDate = tr.CreatedDate,
                            TokenValue = tr.TokenValue,
                            TokenStatus = tr.TokenStatus,
                            StatusModifiedDate = tr.StatusModifiedDate,
                            StatusModifiedUser = tr.StatusModifiedUser,
                            ServiceID = s != null ? s.SerID : 0,
                            ServiceName = s != null ? s.SerName : null
                        };

            return await query.ToListAsync();
        }
        public async Task<IEnumerable<TokenOnStatusDto>> GetTokensByStatus(int companyId, int userId, string tokenStatus)
        {
            var query = from tr in _context.xtm_TokenRegister
                        join um in _context.xtm_UserMap on tr.DepID equals um.DepID
                        join s in _context.xtm_Service on tr.ServiceID equals s.SerID into serviceGroup
                        from s in serviceGroup.DefaultIfEmpty()
                        where tr.TokenStatus == tokenStatus
                              && tr.CompanyID == companyId
                              && tr.TokenActive
                              && um.UserID == userId
                              && um.Status == true
                        select new TokenOnStatusDto
                        {
                            DepID = tr.DepID,
                            DepPrefix = tr.DepPrefix,
                            TokenID = tr.TokenID,
                            CounterID = tr.CounterID,
                            CustomerID = tr.CustomerID,
                            CreatedUserID = tr.CreatedUserID,
                            CreatedSource = tr.CreatedSource,
                            CreatedDate = tr.CreatedDate,
                            TokenValue = tr.TokenValue,
                            TokenStatus = tr.TokenStatus,
                            StatusModifiedDate = tr.StatusModifiedDate,
                            StatusModifiedUser = tr.StatusModifiedUser,
                            ServiceID = s != null ? s.SerID : 0,
                            ServiceName = s != null ? s.SerName : null
                        };

            return await query.ToListAsync();
        }
        public async Task<TokenValuesDto> GetTokenValuesAsync(int companyId, int depId)
        {
            var latestToken = await _context.xtm_TokenRegister
                .Where(tr => tr.CompanyID == companyId
                          && tr.DepID == depId
                          && tr.TokenActive
                          && tr.TokenStatus != "Completed"
                          && tr.TokenStatus != "Deleted")
                .OrderByDescending(tr => tr.TokenValue)
                .Select(tr => new { tr.DepPrefix, tr.TokenValue })
                .FirstOrDefaultAsync();

            var latestCallToken = await _context.xtm_TokenRegister
                .Where(tr => tr.CompanyID == companyId
                          && tr.DepID == depId
                          && tr.TokenActive
                          && (tr.TokenStatus == "onCall" || tr.TokenStatus == "onHold"))
                .OrderByDescending(tr => tr.TokenValue)
                .Select(tr => new { tr.DepPrefix, tr.TokenValue })
                .FirstOrDefaultAsync();

            var nextToken = await (
                     from tr in _context.xtm_TokenRegister
                     join d in _context.xtm_Department
                         on tr.DepID equals d.DepID
                     where tr.CompanyID == companyId
                           && tr.DepID == depId
                           && tr.TokenActive
                           && tr.TokenStatus == "Pending"
                           && tr.DepPrefix == d.DepPrefix   
                     orderby tr.TokenValue
                     select new
                     {
                         tr.DepPrefix,
                         tr.TokenValue
                     }
                 ).FirstOrDefaultAsync();


            var latestCounterCallToken = await (
                     from c in _context.xtm_Counter
                     join tr in _context.xtm_TokenRegister
                         on c.CounterID equals tr.CounterID
                     join d in _context.xtm_Department     
                         on tr.DepID equals d.DepID
                     where tr.CompanyID == companyId
                           && tr.DepID == depId
                           && tr.TokenActive
                           && tr.TokenStatus == "onCall"
                           && tr.TokenValue == _context.xtm_TokenRegister
                                                 .Where(inner => inner.CounterID == c.CounterID
                                                              && inner.CompanyID == companyId
                                                              && inner.DepID == depId
                                                              && inner.TokenActive
                                                              && inner.TokenStatus == "onCall")
                                                 .Max(inner => (int?)inner.TokenValue)
                     select new CounterCallTokenDto
                     {
                         CounterID = c.CounterID,
                         CounterName = c.CounterName,
                         TokenID = tr.TokenID,
                         DepID = tr.DepID,
                         DepPrefix = tr.DepPrefix,
                         DepName = d.DepName,              
                         ServiceID = tr.ServiceID,
                         LastOnCallToken = tr.TokenValue
                     }
                 ).ToListAsync();


            return new TokenValuesDto
            {
                TotalToken = $"{latestToken?.DepPrefix ?? ""}{latestToken?.TokenValue ?? 0}",
                CallToken = $"{latestCallToken?.DepPrefix ?? ""}{latestCallToken?.TokenValue ?? 0}",
                NextToken = $"{nextToken?.DepPrefix ?? ""}{nextToken?.TokenValue ?? 0}",
                LatestCounterCallToken = latestCounterCallToken
            };
        }

        public async Task<TokenUpdateResponse> GetAndUpdateCounterTokenAsync(TokenUpdateRequest request)
        {
            var response = new TokenUpdateResponse();

            var currentToken = await _context.xtm_TokenRegister
                .Where(t => t.CompanyID == request.CompanyId &&
                            t.DepID == request.DepId &&
                            t.CounterID == request.CounterId &&
                            t.TokenStatus == "onCall" &&
                            t.TokenActive)
                .OrderByDescending(t => t.TokenValue)
                .FirstOrDefaultAsync();

            if (currentToken != null)
            {
                if (request.Status == 0)
                    currentToken.TokenStatus = "completed";
                else
                {
                    currentToken.TokenStatus = "onHold";
                    currentToken.IsAnnounced = false;
                }

                var history = new xtm_TokenHistory
                {
                    TokenID = currentToken.TokenID,
                    TokenValue = currentToken.TokenValue,
                    CompanyID = currentToken.CompanyID,
                    DepPrefix = currentToken.DepPrefix,
                    DepFrom = currentToken.DepID,
                    DepTo = currentToken.DepID,
                    ServiceID = currentToken.ServiceID,
                    CustomerID = currentToken.CustomerID,
                    CounterID = currentToken.CounterID,
                    CreatedSource = currentToken.CreatedSource,
                    TokenStatus = currentToken.TokenStatus,
                    StatusCreatedDate = DateTime.Now,
                    StatusCreatedUser = currentToken.StatusModifiedUser
                };
                _context.xtm_TokenHistory.Add(history);

                response.UpdatedCurrentToken = currentToken;
            }

            IQueryable<xtm_TokenRegister> nextTokenQuery;

            if (request.TokenValue.HasValue)
            {
                nextTokenQuery = _context.xtm_TokenRegister.Where(t =>
                    t.CompanyID == request.CompanyId &&
                    t.DepID == request.DepId &&
                    t.DepPrefix == request.DepPrefix &&
                    t.TokenValue == request.TokenValue &&
                    t.TokenActive &&
                    t.TokenStatus != "completed");
            }
            else
            {
                nextTokenQuery = _context.xtm_TokenRegister.Where(t =>
                    t.CompanyID == request.CompanyId &&
                    t.DepID == request.DepId &&
                    t.DepPrefix == request.DepPrefix &&
                    t.TokenActive &&
                    t.TokenStatus == "Pending");
            }

            var nextToken = await nextTokenQuery
                .OrderBy(t => t.TokenValue)
                .FirstOrDefaultAsync();

            if (nextToken != null)
            {
                nextToken.TokenStatus = "onCall";
                nextToken.CounterID = request.CounterId;
                nextToken.IsAnnounced = false;

                var history = new xtm_TokenHistory
                {
                    TokenID = nextToken.TokenID,
                    TokenValue = nextToken.TokenValue,
                    CompanyID = nextToken.CompanyID,
                    DepPrefix = nextToken.DepPrefix,
                    DepFrom = nextToken.DepID,
                    DepTo = nextToken.DepID,
                    ServiceID = nextToken.ServiceID,
                    CustomerID = nextToken.TokenID,
                    CounterID = nextToken.CounterID,
                    CreatedSource = nextToken.CreatedSource,
                    TokenStatus = "onCall",
                    StatusCreatedDate = DateTime.Now,
                    StatusCreatedUser = nextToken.StatusModifiedUser
                };
                _context.xtm_TokenHistory.Add(history);

                response.OnCallToken = nextToken;
            }

            await _context.SaveChangesAsync();
            return response;
        }

        public async Task<(bool Success, string Message)> UpdateTokenStatusAsync( int companyId, int depId,  string? depPrefix,  int tokenValue,  bool iscomplete, int userId, int serviceId, int? customerId,  int? counterId)
        {
            var token = await _context.xtm_TokenRegister
                        .FirstOrDefaultAsync(t =>
                            t.CompanyID == companyId &&
                            t.DepID == depId &&
                            (depPrefix == null || t.DepPrefix == depPrefix) &&
                            t.TokenValue == tokenValue);

            if (token == null)
                return (false, "Token not found");

        
            string status = iscomplete ? "completed" : "onhold";
            bool announced = iscomplete ? true : false;

            token.TokenStatus = status;
            token.IsAnnounced = announced;

            await _context.SaveChangesAsync();

       
            var history = new xtm_TokenHistory
            {
                TokenID = token.TokenID,
                TokenValue = token.TokenValue,
                CompanyID = companyId,
                DepPrefix = depPrefix,
                DepFrom = depId,
                DepTo = depId,
                ServiceID = serviceId,
                CustomerID = customerId,
                CounterID = counterId,
                CreatedSource = "web",
                TokenStatus = status,
                StatusCreatedDate = DateTime.Now,
                StatusCreatedUser = userId
            };

            _context.xtm_TokenHistory.Add(history);
            await _context.SaveChangesAsync();

            return (true, $"Token status updated as {status}");
        }

        public async Task<int> GetPendingTokenAsync(int companyId, int userId)
        {
            var token = await (from t in _context.xtm_TokenRegister
                               join u in _context.xtm_UserMap on t.DepID equals u.DepID
                               where t.TokenStatus == "Pending"
                                     && t.CompanyID == companyId
                                     && u.UserID == userId
                               orderby t.TokenValue ascending
                               select t.TokenValue)
                               .FirstOrDefaultAsync();

            return token == 0 ? 0 : token;
        }
        public async Task<IEnumerable<xtm_TokenRegister>> CheckTokenValueAsync(int companyId, int depId, int tokenValue)
        {
            return await _context.xtm_TokenRegister
                .Where(t => t.CompanyID == companyId
                            && t.DepID == depId
                            && t.TokenValue == tokenValue
                            && t.TokenStatus == "Pending"
                            && t.TokenActive)
                .OrderBy(t => t.TokenValue)
                .ToListAsync();
        }

        public async Task<TokenHistorySummaryDto> GetTokenHistorySummaryAsync( int companyId, DateTime date)
        {
            var fromDate = date.Date;
            var toDate = fromDate.AddDays(1);

            var tokenHistory = _context.xtm_TokenHistory
                .AsNoTracking()
                .Where(x =>
                    x.CompanyID == companyId &&
                    x.StatusCreatedDate >= fromDate &&
                    x.StatusCreatedDate < toDate);
   
            var totalTokensIssued = await tokenHistory
                .Where(x => x.TokenStatus == "pending")
                .CountAsync();

            var tokenTimes = await tokenHistory
                .Where(x => x.TokenStatus == "pending" || x.TokenStatus == "completed")
                .GroupBy(x => x.TokenID)
                .Select(g => new
                {
                    PendingTime = g
                        .Where(x => x.TokenStatus == "pending")
                        .Min(x => (DateTime?)x.StatusCreatedDate),

                    CompletedTime = g
                        .Where(x => x.TokenStatus == "completed")
                        .Max(x => (DateTime?)x.StatusCreatedDate)
                })
                .Where(x => x.PendingTime != null && x.CompletedTime != null)
                .ToListAsync();

            var averageWaitTimeMinutes = tokenTimes.Any()
                ? (int)Math.Round(
                    tokenTimes.Average(x =>
                        (x.CompletedTime.Value - x.PendingTime.Value).TotalMinutes))
                : 0;

            var peakHourData = await tokenHistory
                .Where(x => x.TokenStatus == "onCall")
                .GroupBy(x => x.StatusCreatedDate.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    TokensIssued = g.Count()
                })
                .OrderByDescending(x => x.TokensIssued)
                .FirstOrDefaultAsync();

            var peakHour = peakHourData != null
                ? new PeakHourDto
                {
                    Hour = DateTime.Today
                        .AddHours(peakHourData.Hour)
                        .ToString("hh tt"),
                    TokensIssued = peakHourData.TokensIssued
                }
                : new PeakHourDto { Hour = null, TokensIssued = 0 };

            var counterPerformanceData = await tokenHistory
                .Where(x => x.CounterID != null)
                .Join(
                    _context.xtm_Counter,
                    th => th.CounterID,
                    c => c.CounterID,
                    (th, c) => new { th.TokenID, c.CounterName }
                )
                .GroupBy(x => x.CounterName)
                .Select(g => new CounterPerformanceDto
                {
                    CounterName = g.Key,
                    TokensServed = g.Select(x => x.TokenID).Distinct().Count()
                })
                .ToListAsync();


            var tokensOnHold = await tokenHistory
                .GroupBy(x => x.TokenID)
                .Select(g => g
                    .OrderByDescending(x => x.StatusCreatedDate)
                    .First())
                .CountAsync(x => x.TokenStatus == "onHold");

  
            var tokensPending = await tokenHistory
                .GroupBy(x => x.TokenID)
                .Where(g =>
                    !g.Any(x => x.TokenStatus == "completed") &&
                    !g.Any(x => x.TokenStatus == "onCall"))
                .CountAsync();

            return new TokenHistorySummaryDto
            {
                TotalTokensIssued = totalTokensIssued,
                AverageWaitTimeMinutes = averageWaitTimeMinutes,
                PeakHour = peakHour,
                CounterPerformanceData = counterPerformanceData,
                TokensOnHold = tokensOnHold,
                TokensPending = tokensPending
            };
        }


        public async Task<(List<TokenHistoryReportDto> data, int totalCount)> GetTokenHistoryDetailsAsync( int companyId, DateTime startDate, DateTime endDate, int pageNumber, int pageSize, string searchParam)
        {
            var fromDate = startDate.Date;
            var toDate = endDate.Date.AddDays(1);

            var query =
                from th in _context.xtm_TokenHistory.AsNoTracking()

                join df in _context.xtm_Department on th.DepFrom equals df.DepID into dfj
                from depFrom in dfj.DefaultIfEmpty()

                join dt in _context.xtm_Department on th.DepTo equals dt.DepID into dtj
                from depTo in dtj.DefaultIfEmpty()

                join s in _context.xtm_Service on th.ServiceID equals s.SerID into sj
                from service in sj.DefaultIfEmpty()

                join c in _context.xtm_Customer on th.CustomerID equals c.CustomerID into cj
                from customer in cj.DefaultIfEmpty()

                join co in _context.xtm_Counter on th.CounterID equals co.CounterID into coj
                from counter in coj.DefaultIfEmpty()

                where th.CompanyID == companyId
                   && th.StatusCreatedDate >= fromDate
                   && th.StatusCreatedDate < toDate
                   && (
                        string.IsNullOrWhiteSpace(searchParam)
                        || (th.DepPrefix + th.TokenValue).Contains(searchParam)
                        || th.TokenID.ToString() == searchParam
                      )

                select new TokenHistoryReportDto
                {
                    TokenHistoryID = th.TokenHistoryID,
                    TokenID = th.TokenID,
                    TokenValue = th.TokenValue,
                    CreatedDate = th.StatusCreatedDate,
                    StatusCreatedTime =
                    _context.xtm_TokenHistory
                        .Where(x => x.TokenID == th.TokenID && x.TokenStatus == "Pending")
                        .OrderBy(x => x.StatusCreatedDate)
                        .Select(x => x.StatusCreatedDate.ToString("hh:mm tt"))
                        .FirstOrDefault() ?? "N/A",
                    StatusCreatedUser = th.StatusCreatedUser,

                    DepFromPrefix = th.DepPrefix,
                    DepFromName = depFrom != null ? depFrom.DepName : "N/A",
                    DepToName = depTo != null ? depTo.DepName : "N/A",

                    CounterName = counter != null ? counter.CounterName : "N/A",
                    ServiceName = service != null ? service.SerName : "N/A",
                    CustomerName = customer != null ? customer.CustomerName : "N/A",
                    CustomerMobileNumber = customer != null ? customer.CustomerMobileNumber : "N/A",

                    OnCallTime = new[] { "onCall", "onHold", "Transfer", "completed", "Recalled" }
                                .Contains(th.TokenStatus)
                                    ? th.StatusCreatedDate.ToString("hh:mm tt")
                                    : "N/A",

                    CurrentTokenStatus = th.TokenStatus
                };

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }


        public async Task<IEnumerable<TokenTimelineDto>> GetTokenTimelineAsync(int tokenId)
        {
            var timeline = await (from th in _context.xtm_TokenHistory
                                  join depFrom in _context.xtm_Department on th.DepFrom equals depFrom.DepID into depFromJoin
                                  from depFrom in depFromJoin.DefaultIfEmpty()
                                  join depTo in _context.xtm_Department on th.DepTo equals depTo.DepID into depToJoin
                                  from depTo in depToJoin.DefaultIfEmpty()
                                  join c in _context.xtm_Counter on th.CounterID equals c.CounterID into counterJoin
                                  from c in counterJoin.DefaultIfEmpty()
                                  where th.TokenID == tokenId
                                  orderby th.StatusCreatedDate
                                  select new TokenTimelineDto
                                  {
                                      TokenStatus = th.TokenStatus,
                                      TokenTime = th.StatusCreatedDate.ToString("hh:mm tt"),
                                      DepFromName = depFrom.DepName,
                                      DepToName = depTo.DepName,
                                      CounterName = c.CounterName
                                  }).ToListAsync();

            return timeline;
        }


        public async Task<bool> ResetTokenAsync(int companyId, int depId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
  
                var tokenRegisters = _context.xtm_TokenRegister
                    .Where(t => t.CompanyID == companyId && t.DepID == depId);

                _context.xtm_TokenRegister.RemoveRange(tokenRegisters);
                await _context.SaveChangesAsync();

                var tokenMasters = await _context.xtm_TokenMaster
                    .Where(m => m.CompanyID == companyId && m.DepID == depId)
                    .ToListAsync();

                foreach (var master in tokenMasters)
                {
                    master.TriggerValue = 0;
                    master.PrintTokenValue = 0;
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateIsAnnouncedAsync(int companyId, int depId, int tokenValue)
        {
            var token = await _context.xtm_TokenRegister
                .FirstOrDefaultAsync(t => t.CompanyID == companyId
                                       && t.DepID == depId
                                       && t.TokenValue == tokenValue);

            if (token == null)
                return false;

            token.IsAnnounced = true;
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<(bool Success, string Message)> UpdateDepartmentAsync(int companyId,int oldDepId, string depPrefix,int tokenValue, TokenUpdateDto tokenData)
        {
            try
            {
                var tokenRegister = await _context.xtm_TokenRegister
                    .FirstOrDefaultAsync(t => t.CompanyID == companyId
                                           && t.DepID == oldDepId
                                           && t.DepPrefix == depPrefix
                                           && t.TokenValue == tokenValue);

                if (tokenRegister == null)
                    return (false, "Token not found");

                var createdUser = tokenRegister.CreatedUserID;
                var customerId = tokenRegister.CustomerID;
                tokenRegister.DepID = tokenData.NewDepId;
                tokenRegister.TokenStatus = "Pending";
                tokenRegister.IsAnnounced = false;

                var transferHistory = new xtm_TokenHistory
                {
                    TokenID = tokenData.TokenId,
                    TokenValue = tokenValue,
                    CompanyID = companyId,
                    DepPrefix = depPrefix,
                    DepFrom = oldDepId,
                    DepTo = tokenData.NewDepId,
                    ServiceID = tokenData.ServiceId,
                    CustomerID = customerId,
                    CounterID = tokenData.CounterId,
                    CreatedSource = "web",
                    TokenStatus = "Transfer",
                    StatusCreatedDate = DateTime.Now,
                    StatusCreatedUser = createdUser
                };

                _context.xtm_TokenHistory.Add(transferHistory);

           
                var pendingHistory = new xtm_TokenHistory
                {
                    TokenID = tokenData.TokenId,
                    TokenValue = tokenValue,
                    CompanyID = companyId,
                    DepPrefix = depPrefix,
                    DepFrom = tokenData.NewDepId,
                    DepTo = tokenData.NewDepId,
                    ServiceID = tokenData.ServiceId,
                    CustomerID = customerId,
                    CounterID = tokenData.CounterId,
                    CreatedSource = "web",
                    TokenStatus = "Pending",
                    StatusCreatedDate = DateTime.Now,
                    StatusCreatedUser = createdUser
                };

                _context.xtm_TokenHistory.Add(pendingHistory);

                await _context.SaveChangesAsync();

                return (true, "DepID and token status updated, and history recorded successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task<IEnumerable<object>> GetTokensOnHoldAsync(int companyId, int userId)
        {
            var tokens = await (from t in _context.xtm_TokenRegister
                                join u in _context.xtm_UserMap on t.DepID equals u.DepID
                                join s in _context.xtm_Service on t.ServiceID equals s.SerID into serviceJoin
                                from s in serviceJoin.DefaultIfEmpty()
                                where t.TokenStatus == "onHold"
                                      && t.CompanyID == companyId
                                      && u.UserID == userId
                                      && t.TokenActive == true
                                      && u.Status == true
                                select new
                                {
                                    t.TokenID,
                                    t.CounterID,
                                    t.CustomerID,
                                    t.CreatedUserID,
                                    t.CreatedSource,
                                    t.CreatedDate,
                                    t.TokenValue,
                                    t.TokenStatus,
                                    t.StatusModifiedDate,
                                    t.StatusModifiedUser,
                                    t.DepID,
                                    t.DepPrefix,
                                    t.ServiceID,
                                    ServiceName = s != null ? s.SerName : null
                                })
                                .ToListAsync();

            // ✅ Convert UTC → IST manually in C#
            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            var converted = tokens.Select(t => new
            {
                t.TokenID,
                t.CounterID,
                t.CustomerID,
                t.CreatedUserID,
                t.CreatedSource,
                CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(t.CreatedDate, istZone),
                t.TokenValue,
                t.TokenStatus,
                StatusModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(t.CreatedDate, istZone),
                t.StatusModifiedUser,
                t.DepID,
                t.DepPrefix,
                t.ServiceID,
                t.ServiceName
            });

            return converted;
        }
        public async Task<string> RecallTokenAsync(TokenRecallDto tokenData)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var token = await _context.xtm_TokenRegister
                    .FirstOrDefaultAsync(t => t.TokenID == tokenData.TokenId);

                if (token == null)
                    throw new Exception("Token ID not found.");

                token.IsAnnounced = false;
                _context.xtm_TokenRegister.Update(token);
                await _context.SaveChangesAsync();

                var history = new xtm_TokenHistory
                {
                    TokenID = tokenData.TokenId,
                    TokenValue = tokenData.TokenValue,
                    CompanyID = tokenData.CompanyId,
                    DepPrefix = tokenData.DepPrefix,
                    DepFrom = tokenData.DepId,
                    DepTo = tokenData.DepId,
                    ServiceID = tokenData.ServiceId,
                    CustomerID = token.CustomerID,
                    CounterID = tokenData.CounterId,
                    CreatedSource = "web",
                    TokenStatus = "Recalled",
                    StatusCreatedDate = DateTime.Now,
                    StatusCreatedUser = tokenData.UserId
                };

                _context.xtm_TokenHistory.Add(history);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return "Token recalled successfully.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error recalling token: {ex.Message}");
            }
        }

        public async Task<object> UpsertTokenAsync(TokenUpsertDto tokenData)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int newPrintValue = 0;
                int? tokenId = null;
                int? customerId = null;
                if (!string.IsNullOrEmpty(tokenData.CustomerName) || !string.IsNullOrEmpty(tokenData.CustomerMobileNumber))
                {
                    var existingCustomer = await _context.xtm_Customer
                        .Where(c =>
                            c.CustomerName == tokenData.CustomerName ||
                            c.CustomerMobileNumber == tokenData.CustomerMobileNumber)
                        .FirstOrDefaultAsync();

                    if (existingCustomer != null)
                    {
                        customerId = existingCustomer.CustomerID;
                    }
                    else
                    {
                        var newCustomer = new xtm_Customer
                        {
                            CustomerName = tokenData.CustomerName,
                            CustomerMobileNumber = tokenData.CustomerMobileNumber,
                            Status = true
                        };
                        _context.xtm_Customer.Add(newCustomer);
                        await _context.SaveChangesAsync();
                        customerId = newCustomer.CustomerID;
                    }
                }

                var tokenMaster = await _context.xtm_TokenMaster
                    .Where(t => t.CompanyID == tokenData.CompanyID && t.DepID == tokenData.DepID)
                    .FirstOrDefaultAsync();

                if (tokenMaster != null)
                {
                    newPrintValue = tokenMaster.PrintTokenValue + 1;
                    tokenMaster.PrintTokenValue = newPrintValue;
                    _context.xtm_TokenMaster.Update(tokenMaster);
                }
                else
                {
                    newPrintValue = 1;
                    tokenMaster = new xtm_TokenMaster
                    {
                        CompanyID = tokenData.CompanyID,
                        DepID = tokenData.DepID,
                        PrintTokenValue = newPrintValue,
                        TriggerValue = 0,
                        MaximumToken = 999
                    };
                    _context.xtm_TokenMaster.Add(tokenMaster);
                }

                await _context.SaveChangesAsync();

             
                var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var nowIST = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ist);

                var newTokenRegister = new xtm_TokenRegister
                {
                    CompanyID = tokenData.CompanyID,
                    DepID = tokenData.DepID,
                    DepPrefix = tokenData.DepPrefix,
                    ServiceID = tokenData.ServiceID,
                    CounterID = tokenData.CounterID,
                    CustomerID = customerId,
                    CreatedUserID = tokenData.CreatedUserID,
                    CreatedSource = tokenData.CreatedSource,
                    CreatedDate = nowIST,
                    TokenValue = newPrintValue,
                    TokenStatus = tokenData.TokenStatus,
                    StatusModifiedDate = nowIST,
                    StatusModifiedUser = tokenData.StatusModifiedUser,
                    IsAnnounced = false,
                    TokenActive = true
                };
                _context.xtm_TokenRegister.Add(newTokenRegister);
                await _context.SaveChangesAsync();

                tokenId = newTokenRegister.TokenID;

             
                var newTokenHistory = new xtm_TokenHistory
                {
                    TokenID = tokenId.Value,
                    TokenValue = newPrintValue,
                    CompanyID = tokenData.CompanyID,
                    DepPrefix = tokenData.DepPrefix,
                    DepFrom = tokenData.DepID,
                    DepTo = tokenData.DepID,
                    ServiceID = tokenData.ServiceID,
                    CustomerID = customerId,
                    CounterID = tokenData.CounterID,
                    CreatedSource = tokenData.CreatedSource,
                    TokenStatus = tokenData.TokenStatus,
                    StatusCreatedDate = nowIST,
                    StatusCreatedUser = tokenData.StatusModifiedUser
                };
                _context.xtm_TokenHistory.Add(newTokenHistory);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new
                {
                    status = "success",
                    message = "Token register and history created successfully",
                    token = newPrintValue
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<byte[]> GetTokenAudioAsync(string tokenNumber, string counterName)
        {
            counterName = Uri.UnescapeDataString(counterName);

            counterName = Regex.Replace(counterName, @"[^a-zA-Z0-9\s]", "");

            string text = $"TokenNumber {tokenNumber} on {counterName}";

            const string baseUrl = "https://translate.google.com/translate_tts";

            var queryParams = new Dictionary<string, string>
            {
                ["ie"] = "UTF-8",
                ["client"] = "tw-ob",
                ["tl"] = "en-IN",
                ["q"] = text
            };

            var url = QueryHelpers.AddQueryString(baseUrl, queryParams);

            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"
            );

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

    }
}



