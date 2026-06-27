// Path: Repositories/ReportRepository.cs

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Models;
using XeniaAkcaBackend.Repositories.Report;

namespace XeniaAkcaBackend.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<ReportResponse> GetContributionReportAsync(string level, int status, ContributionRequest request)
        {
            int offset = (request.Page - 1) * request.Limit;

            if (status == 0)
                return await GetPendingContribution(level, request, offset);
            else if (status == 1)
                return await GetPayedContribution(level, request, offset);
            else
                throw new ArgumentException("Invalid status value");
        }

   
        private async Task<ReportResponse> GetPendingContribution(string level, ContributionRequest request, int offset)
        {
            var parameters = new List<SqlParameter>();

  
            string totalQuery, dataQuery;

            switch (level.ToLower())
            {
                case "state":
                    (totalQuery, dataQuery) = BuildStatePendingQuery(request, parameters);
                    break;
                case "district":
                    (totalQuery, dataQuery) = BuildDistrictPendingQuery(request, parameters);
                    break;
                case "unit":
                    (totalQuery, dataQuery) = BuildUnitPendingQuery(request, parameters);
                    break;
                default:
                    throw new ArgumentException("Invalid level");
            }

            AddPaginationParams(parameters, offset, request.Limit);
            return await ExecuteQuery(totalQuery, dataQuery, parameters, request);
        }

      
        private async Task<ReportResponse> GetPayedContribution(string level, ContributionRequest request, int offset)
        {
            var parameters = new List<SqlParameter>();

            string totalQuery, dataQuery;

            switch (level.ToLower())
            {
                case "state":
                    (totalQuery, dataQuery) = BuildStatePayedQuery(request, parameters);
                    break;
                case "district":
                    (totalQuery, dataQuery) = BuildDistrictPayedQuery(request, parameters);
                    break;
                case "unit":
                    (totalQuery, dataQuery) = BuildUnitPayedQuery(request, parameters);
                    break;
                default:
                    throw new ArgumentException("Invalid level");
            }

            AddPaginationParams(parameters, offset, request.Limit);
            return await ExecuteQuery(totalQuery, dataQuery, parameters, request);
        }

    
        public async Task<ReportResponse> GetPaymentReportAsync(PaymentRequest request)
        {
            int offset = (request.Page - 1) * request.Limit;
            var parameters = new List<SqlParameter>();

            string totalQuery, dataQuery;

         
            if (request.PayType == "5")
            {
                (totalQuery, dataQuery) = BuildPaymentContributionQuery(request, parameters);
            }
        
            else if (request.PayType == "1" || request.PayType == "2" || request.PayType == "3" || request.PayType == "4")
            {
                (totalQuery, dataQuery) = BuildPaymentOthersQuery(request, parameters);
            }
    
            else
            {
                (totalQuery, dataQuery) = BuildPaymentAllQuery(request, parameters);
            }

            AddPaginationParams(parameters, offset, request.Limit);
            return await ExecuteQuery(totalQuery, dataQuery, parameters, request);
        }

    
        public async Task<object> GetEventsAsync(string? searchText)
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            try
            {
                string sql = @"
            SELECT 
                c.contributionId, 
                CONCAT(c.contributionText, '(', m.memberName, ')') AS contributionDetail
            FROM AKCA_KaruthalMembers m    
            JOIN AKCA_Contributions c ON m.memberId = c.contributionMemberId";

                using var command = connection.CreateCommand();

                if (!string.IsNullOrEmpty(searchText))
                {
                    sql += @" WHERE c.contributionText LIKE @searchText 
                OR m.memberName LIKE @searchText";

                    var param = command.CreateParameter();
                    param.ParameterName = "@searchText";
                    param.Value = $"%{searchText}%";
                    command.Parameters.Add(param);
                }

                command.CommandText = sql;

                using var reader = await command.ExecuteReaderAsync();

                var events = new List<object>();
                while (await reader.ReadAsync())
                {
                    events.Add(new
                    {
                        contributionId = reader.GetInt32(0),
                        contributionDetail = reader.GetString(1)
                    });
                }

                return events;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }


        private (string totalQuery, string dataQuery) BuildStatePendingQuery(ContributionRequest request, List<SqlParameter> parameters)
        {
            
            string baseWhere = @"
                WHERE k.contributionId NOT IN (
                    SELECT contributionId FROM AKCA_MemberContributions WHERE memberId = t.memberId
                )
                AND t.memberStatus IN (5,6,9)";

            baseWhere = AddCommonFilters(baseWhere, request, parameters, dateField: "k.contributionInitiatedDate", useNextDay: true);

            string totalQuery = $@"
                SELECT COUNT(*) as total, SUM(k.contributionAmount) as amount
                FROM AKCA_Contributions k
                JOIN AKCA_KaruthalMembers t ON t.memberId NOT IN (
                    SELECT memberId FROM AKCA_MemberContributions WHERE contributionId = k.contributionId
                )
                JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                {baseWhere}";

            string dataQuery = $@"
                SELECT 
                    s.districtName, u.unitName, k.contributionText AS event, 
                    t.memberBusinessName, t.memberMobilenumber, u.unitContactPerson, 
                    t.memberMobilenumber as unitContactNumber, 'Pending' AS paymentStatus, 
                    NULL AS paidDate, k.contributionAmount, NULL AS payMode, 
                    NULL AS PaymentTxnRefNo, k.contributionId, k.contributionText,
                    t.memberName, t.memberId, t.memberUserId
                FROM AKCA_Contributions k
                JOIN AKCA_KaruthalMembers t ON t.memberId NOT IN (
                    SELECT memberId FROM AKCA_MemberContributions WHERE contributionId = k.contributionId
                )
                JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                {baseWhere}
                ORDER BY t.memberId ASC
                OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            return (totalQuery, dataQuery);
        }


        private (string totalQuery, string dataQuery) BuildStatePayedQuery(ContributionRequest request, List<SqlParameter> parameters)
        {
            string baseWhere = "WHERE g.paymentStatus='success'";
            baseWhere = AddCommonFilters(baseWhere, request, parameters, dateField: "g.paidDate", useNextDay: true);

            string totalQuery = $@"
                SELECT COUNT(*) as total, SUM(g.contributionAmount) as amount
                FROM AKCA_KaruthalMembers t, AKCA_Users F, AKCA_Districts s, AKCA_Units u, AKCA_Contributions k, AKCA_MemberContributions g
                WHERE F.userId = t.memberUserId
                AND s.districtId = t.memberDistrictId
                AND u.unitId = t.memberUnitId
                AND k.contributionId = g.contributionId
                AND g.memberId = t.memberId
                AND g.paymentStatus='success'
                {baseWhere.Replace("WHERE g.paymentStatus='success'", "")}";

            string dataQuery = $@"
                SELECT 
                    s.districtName, u.unitName, k.contributionText AS event, 
                    t.memberBusinessName, t.memberMobilenumber, u.unitContactPerson, 
                    t.memberMobilenumber as unitContactNumber, g.paymentStatus, g.paidDate, 
                    g.contributionAmount, g.payMode, g.PaymentTxnRefNo,
                    k.contributionId, k.contributionText, t.memberName 
                FROM AKCA_KaruthalMembers t, AKCA_Users F, AKCA_Districts s, AKCA_Units u, AKCA_Contributions k, AKCA_MemberContributions g
                WHERE F.userId = t.memberUserId
                AND s.districtId = t.memberDistrictId
                AND u.unitId = t.memberUnitId
                AND k.contributionId = g.contributionId
                AND g.memberId = t.memberId
                AND g.paymentStatus='success'
                {baseWhere.Replace("WHERE g.paymentStatus='success'", "")}
                ORDER BY t.memberId ASC
                OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            return (totalQuery, dataQuery);
        }

  
        private (string totalQuery, string dataQuery) BuildDistrictPendingQuery(ContributionRequest request, List<SqlParameter> parameters)
        {
            string baseWhere = "WHERE g.memberContributionId IS NULL";
            baseWhere = AddCommonFilters(baseWhere, request, parameters, dateField: "k.contributionInitiatedDate");

            string totalQuery = $@"
                SELECT COUNT(*) as total, SUM(k.contributionAmount) as amount
                FROM AKCA_KaruthalMembers t
                JOIN AKCA_Users F ON F.userId = t.memberUserId
                JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                JOIN AKCA_Contributions k ON k.contributionMemberId = t.memberId
                LEFT JOIN AKCA_MemberContributions g ON g.contributionId = k.contributionId 
                    AND g.memberId = k.contributionMemberId
                {baseWhere}";

            string dataQuery = $@"
                SELECT 
                    s.districtName, u.unitName, k.contributionText AS event, 
                    t.memberBusinessName, u.unitContactPerson, u.unitContactNumber, 
                    ISNULL(g.paymentStatus, 'Pending') AS paymentStatus, g.paidDate, 
                    k.contributionAmount, g.payMode, g.PaymentTxnRefNo,
                    k.contributionId, k.contributionText
                FROM AKCA_KaruthalMembers t
                JOIN AKCA_Users F ON F.userId = t.memberUserId
                JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                JOIN AKCA_Contributions k ON k.contributionMemberId = t.memberId
                LEFT JOIN AKCA_MemberContributions g ON g.contributionId = k.contributionId 
                    AND g.memberId = k.contributionMemberId
                {baseWhere}
                ORDER BY t.memberId ASC
                OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            return (totalQuery, dataQuery);
        }

      
        private (string totalQuery, string dataQuery) BuildDistrictPayedQuery(ContributionRequest request, List<SqlParameter> parameters)
        {
            string baseWhere = @"
                WHERE k.contributionMemberId IN (
                    SELECT m.memberId FROM AKCA_MemberContributions m
                    JOIN (SELECT contributionId, memberId FROM AKCA_MemberContributions GROUP BY contributionId, memberId) c 
                    ON m.contributionId = c.contributionId AND m.memberId = c.memberId
                )";
            baseWhere = AddCommonFilters(baseWhere, request, parameters, dateField: "k.contributionInitiatedDate");

            string totalQuery = $@"
                SELECT COUNT(*) as total, SUM(g.contributionAmount) as amount
                FROM AKCA_KaruthalMembers t
                JOIN AKCA_Users F ON F.userId = t.memberUserId
                JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                JOIN AKCA_Contributions k ON k.contributionMemberId = t.memberId
                JOIN AKCA_MemberContributions g ON g.memberId = t.memberId AND g.contributionId = k.contributionId
                {baseWhere}";

            string dataQuery = $@"
                SELECT 
                    s.districtName, u.unitName, k.contributionText AS event, 
                    t.memberBusinessName, u.unitContactPerson, u.unitContactNumber, 
                    g.paymentStatus, g.paidDate, g.contributionAmount,
                    g.payMode, g.PaymentTxnRefNo, k.contributionId, k.contributionText 
                FROM AKCA_KaruthalMembers t
                JOIN AKCA_Users F ON F.userId = t.memberUserId
                JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                JOIN AKCA_Contributions k ON k.contributionMemberId = t.memberId
                JOIN AKCA_MemberContributions g ON g.memberId = t.memberId AND g.contributionId = k.contributionId
                {baseWhere}
                ORDER BY t.memberId ASC
                OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            return (totalQuery, dataQuery);
        }


        private (string totalQuery, string dataQuery) BuildUnitPendingQuery(ContributionRequest request, List<SqlParameter> parameters)
        {
            parameters.Add(new SqlParameter("@districtid", request.DistrictId ?? 0));
            if (request.UnitId.HasValue) parameters.Add(new SqlParameter("@unitid", request.UnitId.Value));
            if (!string.IsNullOrEmpty(request.SearchText)) parameters.Add(new SqlParameter("@searchText", $"%{request.SearchText}%"));
            if (request.FromDate.HasValue) parameters.Add(new SqlParameter("@fromdate", request.FromDate));
            if (request.ToDate.HasValue) parameters.Add(new SqlParameter("@todate", request.ToDate));
            if (!string.IsNullOrEmpty(request.Event)) parameters.Add(new SqlParameter("@event", int.Parse(request.Event)));

            string baseWhere = @"
                WHERE m.memberDistrictId = @districtid
                AND mc.memberContributionId IS NULL";

            if (!string.IsNullOrEmpty(request.SearchText))
                baseWhere += @" AND (m.memberName LIKE @searchText OR m.memberBusinessName LIKE @searchText OR u.unitName LIKE @searchText OR d.districtName LIKE @searchText OR cn.contributionText LIKE @searchText)";
            if (request.UnitId.HasValue)
                baseWhere += " AND m.memberUnitId = @unitid";
            if (request.FromDate.HasValue && request.ToDate.HasValue)
                baseWhere += " AND cn.contributionInitiatedDate BETWEEN @fromdate AND @todate";
            if (!string.IsNullOrEmpty(request.Event))
                baseWhere += " AND cn.contributionId = @event";

            string totalQuery = $@"
                SELECT COUNT(*) AS total, SUM(cn.contributionAmount) AS amount
                FROM AKCA_KaruthalMembers m
                INNER JOIN AKCA_Districts d ON d.districtId = m.memberDistrictId
                INNER JOIN AKCA_Units u ON u.unitId = m.memberUnitId
                CROSS JOIN AKCA_Contributions cn
                LEFT JOIN AKCA_MemberContributions mc ON mc.memberId = m.memberId AND mc.contributionId = cn.contributionId
                {baseWhere}";

            string dataQuery = $@"
                SELECT 
                    m.memberId, m.memberName, m.memberBusinessName,
                    m.memberMobilenumber AS unitContactNumber, m.memberName AS unitContactPerson,
                    m.memberUserId, d.districtName, u.unitName,
                    cn.contributionId, cn.contributionText AS event, cn.contributionAmount,
                    'Pending' AS paymentStatus
                FROM AKCA_KaruthalMembers m
                INNER JOIN AKCA_Districts d ON d.districtId = m.memberDistrictId
                INNER JOIN AKCA_Units u ON u.unitId = m.memberUnitId
                CROSS JOIN AKCA_Contributions cn
                LEFT JOIN AKCA_MemberContributions mc ON mc.memberId = m.memberId AND mc.contributionId = cn.contributionId
                {baseWhere}
                ORDER BY m.memberName ASC
                OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            return (totalQuery, dataQuery);
        }


        private (string totalQuery, string dataQuery) BuildUnitPayedQuery(ContributionRequest request, List<SqlParameter> parameters)
        {
            parameters.Add(new SqlParameter("@districtid", request.DistrictId ?? 0));
            if (request.UnitId.HasValue) parameters.Add(new SqlParameter("@unitid", request.UnitId.Value));
            if (!string.IsNullOrEmpty(request.SearchText)) parameters.Add(new SqlParameter("@searchText", $"%{request.SearchText}%"));
            if (request.FromDate.HasValue) parameters.Add(new SqlParameter("@fromdate", request.FromDate));
            if (request.ToDate.HasValue) parameters.Add(new SqlParameter("@todate", request.ToDate));

            string baseWhere = @"
                WHERE c.paymentStatus = 'success'
                AND m.memberDistrictId = @districtid";

            if (!string.IsNullOrEmpty(request.SearchText))
                baseWhere += @" AND (m.memberName LIKE @searchText OR m.memberBusinessName LIKE @searchText OR u.unitName LIKE @searchText OR k.contributionText LIKE @searchText)";
            if (request.UnitId.HasValue)
                baseWhere += " AND m.memberUnitId = @unitid";
            if (request.FromDate.HasValue && request.ToDate.HasValue)
                baseWhere += " AND c.paidDate BETWEEN @fromdate AND @todate";

            string totalQuery = $@"
                SELECT COUNT(*) AS total, SUM(c.contributionAmount) AS amount
                FROM AKCA_MemberContributions c
                INNER JOIN AKCA_KaruthalMembers m ON c.memberId = m.memberId
                INNER JOIN AKCA_Contributions k ON k.contributionId = c.contributionId
                {baseWhere}";

            string dataQuery = $@"
                SELECT 
                    c.memberContributionId, c.contributionId, c.memberId,
                    c.contributionAmount, c.paidDate, c.paidBy, c.payMode,
                    c.paymentStatus, c.PaymentTxnRefNo, k.contributionText AS event,
                    m.memberName, m.memberUserId, m.memberBusinessName,
                    m.memberDistrictId, m.memberUnitId, u.unitName,
                    m.memberName AS unitContactPerson, m.memberMobilenumber AS unitContactNumber,
                    d.districtName
                FROM AKCA_MemberContributions c
                INNER JOIN AKCA_KaruthalMembers m ON c.memberId = m.memberId
                INNER JOIN AKCA_Contributions k ON k.contributionId = c.contributionId
                INNER JOIN AKCA_Units u ON u.unitId = m.memberUnitId
                INNER JOIN AKCA_Districts d ON d.districtId = m.memberDistrictId
                {baseWhere}
                ORDER BY c.paidDate DESC
                OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            return (totalQuery, dataQuery);
        }

    
        private (string totalQuery, string dataQuery) BuildPaymentContributionQuery(PaymentRequest request, List<SqlParameter> parameters)
        {
            string baseWhere = @"
                WHERE g.memberId = t.memberId AND g.contributionPaymentId IS NOT NULL";

            baseWhere = AddPaymentFilters(baseWhere, request, parameters);

            string totalQuery = $@"
                SELECT COUNT(*) as total, SUM(g.contributionAmount) as amount
                FROM AKCA_KaruthalMembers t
                JOIN AKCA_Users F ON F.userId = t.memberUserId
                JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                JOIN AKCA_MemberContributions g ON g.memberId = t.memberId
                JOIN AKCA_Contributions k ON k.contributionId = g.contributionId
                {baseWhere}";

            string dataQuery = $@"
                SELECT 
                    g.paidDate, u.unitName, 'Contributions' as type, 
                    k.contributionText as event, t.memberBusinessName, 
                    t.memberName as unitContactPerson, t.memberMobilenumber as unitContactNumber, 
                    g.contributionPaymentId, g.contributionAmount as paidAmount
                FROM AKCA_KaruthalMembers t
                JOIN AKCA_Users F ON F.userId = t.memberUserId
                JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                JOIN AKCA_MemberContributions g ON g.memberId = t.memberId
                JOIN AKCA_Contributions k ON k.contributionId = g.contributionId
                {baseWhere}
                ORDER BY g.contributionId ASC
                OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            return (totalQuery, dataQuery);
        }

        private (string totalQuery, string dataQuery) BuildPaymentOthersQuery(PaymentRequest request, List<SqlParameter> parameters)
        {
            string baseWhere = @"
                WHERE g.memberId = t.memberId AND g.PaymentPaymentId IS NOT NULL";

            baseWhere = AddPaymentFilters(baseWhere, request, parameters);
            baseWhere += " AND g.paymentTypeId = @paytype";
            parameters.Add(new SqlParameter("@paytype", request.PayType));

            string totalQuery = $@"
                SELECT COUNT(*) as total, SUM(g.paidAmount) as amount
                FROM AKCA_KaruthalMembers t
                JOIN AKCA_Users F ON F.userId = t.memberUserId
                JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                JOIN AKCA_MemberPayment g ON g.memberId = t.memberId
                JOIN AKCA_Settings x ON x.settingId = g.paymentTypeId
                {baseWhere}";

            string dataQuery = $@"
                SELECT 
                    g.paidDate, u.unitName, x.settingName AS type, 
                    x.settingName AS event, t.memberBusinessName, 
                    t.memberName AS unitContactPerson, t.memberMobilenumber AS unitContactNumber, 
                    g.PaymentPaymentId, g.paidAmount
                FROM AKCA_KaruthalMembers t
                JOIN AKCA_Users F ON F.userId = t.memberUserId
                JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                JOIN AKCA_MemberPayment g ON g.memberId = t.memberId
                JOIN AKCA_Settings x ON x.settingId = g.paymentTypeId
                {baseWhere}
                ORDER BY g.transactionId ASC
                OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            return (totalQuery, dataQuery);
        }

   
        private (string totalQuery, string dataQuery) BuildPaymentAllQuery(PaymentRequest request, List<SqlParameter> parameters)
        {
            string baseWhere = AddPaymentFilters("", request, parameters, useCombinedData: true);

            string totalQuery = $@"
                SELECT COUNT(*) as total, SUM(paidAmount) as amount
                FROM (
                    SELECT 
                        g.paidDate, u.unitName, x.settingName AS type, x.settingName AS event,
                        t.memberBusinessName, t.memberMobilenumber, t.memberName AS unitContactPerson,
                        t.memberMobilenumber AS unitContactNumber, g.PaymentTxnRefNo, g.paidAmount,
                        t.memberDistrictId AS districtId, t.memberUnitId AS unitId
                    FROM AKCA_KaruthalMembers t
                    JOIN AKCA_Users F ON F.userId = t.memberUserId
                    JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                    JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                    JOIN AKCA_MemberPayment g ON g.memberId = t.memberId
                    JOIN AKCA_Settings x ON x.settingId = g.paymentTypeId
                    WHERE g.PaymentTxnRefNo IS NOT NULL AND g.paymentStatus = 'success' AND g.paidAmount > 0
                ) AS combined_data
                {baseWhere}";

            string dataQuery = $@"
                SELECT 
                    combined_data.paidDate, combined_data.unitName, combined_data.type, 
                    combined_data.event, combined_data.memberBusinessName, 
                    combined_data.unitContactPerson, combined_data.unitContactNumber, 
                    combined_data.memberMobilenumber, combined_data.PaymentTxnRefNo, combined_data.paidAmount
                FROM (
                    SELECT 
                        g.paidDate, u.unitName, x.settingName AS type, x.settingName AS event,
                        t.memberBusinessName, t.memberMobilenumber, t.memberName AS unitContactPerson,
                        t.memberMobilenumber AS unitContactNumber, g.PaymentTxnRefNo, g.paidAmount,
                        t.memberDistrictId AS districtId, t.memberUnitId AS unitId
                    FROM AKCA_KaruthalMembers t
                    JOIN AKCA_Users F ON F.userId = t.memberUserId
                    JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                    JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                    JOIN AKCA_MemberPayment g ON g.memberId = t.memberId
                    JOIN AKCA_Settings x ON x.settingId = g.paymentTypeId
                    WHERE g.PaymentTxnRefNo IS NOT NULL AND g.paymentStatus = 'success' AND g.paidAmount > 0
                ) AS combined_data
                {baseWhere}
                ORDER BY combined_data.paidDate
                OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            return (totalQuery, dataQuery);
        }

      
        private string AddCommonFilters(string baseWhere, ContributionRequest request, List<SqlParameter> parameters,
            string dateField, bool useNextDay = false)
        {
            if (!string.IsNullOrEmpty(request.SearchText))
            {
                baseWhere += @" AND (t.memberBusinessName LIKE @searchText OR u.unitName LIKE @searchText OR s.districtName LIKE @searchText OR k.contributionText LIKE @searchText OR t.memberName LIKE @searchText OR t.memberMobilenumber LIKE @searchText)";
                parameters.Add(new SqlParameter("@searchText", $"%{request.SearchText}%"));
            }
            if (request.DistrictId.HasValue && request.DistrictId > 0)
            {
                baseWhere += " AND t.memberDistrictId = @districtid";
                parameters.Add(new SqlParameter("@districtid", request.DistrictId));
            }
            if (request.UnitId.HasValue && request.UnitId > 0)
            {
                baseWhere += " AND t.memberUnitId = @unitid";
                parameters.Add(new SqlParameter("@unitid", request.UnitId));
            }
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                if (useNextDay)
                {
                    var nextDay = request.ToDate.Value.AddDays(1);
                    baseWhere += $" AND {dateField} >= @fromdate AND {dateField} < @nextDay";
                    parameters.Add(new SqlParameter("@fromdate", request.FromDate));
                    parameters.Add(new SqlParameter("@nextDay", nextDay));
                }
                else
                {
                    baseWhere += $" AND {dateField} BETWEEN @fromdate AND @todate";
                    parameters.Add(new SqlParameter("@fromdate", request.FromDate));
                    parameters.Add(new SqlParameter("@todate", request.ToDate));
                }
            }
            if (!string.IsNullOrEmpty(request.Event))
            {
                baseWhere += " AND k.contributionId = @event";
                parameters.Add(new SqlParameter("@event", request.Event));
            }
            return baseWhere;
        }

        private string AddPaymentFilters(string baseWhere, PaymentRequest request, List<SqlParameter> parameters, bool useCombinedData = false)
        {
            string prefix = useCombinedData ? "combined_data." : "";
            string memberPrefix = useCombinedData ? "" : "t.";

            if (!string.IsNullOrEmpty(request.SearchText))
            {
                baseWhere += $" AND ({prefix}unitName LIKE @searchText OR {prefix}memberBusinessName LIKE @searchText OR {prefix}unitContactPerson LIKE @searchText OR {prefix}memberMobilenumber LIKE @searchText)";
                parameters.Add(new SqlParameter("@searchText", $"%{request.SearchText}%"));
            }
            if (request.DistrictId.HasValue && request.DistrictId > 0)
            {
                baseWhere += $" AND {prefix}districtId = @districtid";
                parameters.Add(new SqlParameter("@districtid", request.DistrictId));
            }
            if (request.UnitId.HasValue && request.UnitId > 0)
            {
                baseWhere += $" AND {prefix}unitId = @unitid";
                parameters.Add(new SqlParameter("@unitid", request.UnitId));
            }
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                baseWhere += $" AND CONVERT(DATE, {prefix}paidDate) BETWEEN @fromdate AND @todate";
                parameters.Add(new SqlParameter("@fromdate", request.FromDate));
                parameters.Add(new SqlParameter("@todate", request.ToDate));
            }
            return baseWhere;
        }

        private void AddPaginationParams(List<SqlParameter> parameters, int offset, int limit)
        {
            parameters.Add(new SqlParameter("@offset", offset));
            parameters.Add(new SqlParameter("@limit", limit));
        }

        private async Task<ReportResponse> ExecuteQuery(string totalSql, string dataSql, List<SqlParameter> parameters, dynamic request)
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            try
            {
             
                using var totalCommand = connection.CreateCommand();
                totalCommand.CommandText = totalSql;
                totalCommand.Parameters.AddRange(CloneParameters(parameters));

                using var totalReader = await totalCommand.ExecuteReaderAsync();
                int total = 0;
                decimal amount = 0;

                if (await totalReader.ReadAsync())
                {
               
                    total = await totalReader.IsDBNullAsync(0) ? 0 : totalReader.GetInt32(0);
                    amount = await totalReader.IsDBNullAsync(1) ? 0 : totalReader.GetDecimal(1);
                }
                await totalReader.CloseAsync();

             
                using var dataCommand = connection.CreateCommand();
                dataCommand.CommandText = dataSql;
                dataCommand.Parameters.AddRange(CloneParameters(parameters));

                using var dataReader = await dataCommand.ExecuteReaderAsync();
                var records = new List<Dictionary<string, object>>();

                while (await dataReader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        
                        row[dataReader.GetName(i)] = await dataReader.IsDBNullAsync(i)
                            ? null!
                            : dataReader.GetValue(i);
                    }
                    records.Add(row);
                }

                int totalPages = (int)Math.Ceiling((double)total / request.Limit);

                return new ReportResponse
                {
                    Status = "success",
                    Data = records,
                    TotalPages = totalPages,
                    CurrentPage = request.Page,
                    Limit = request.Limit,
                    TotalRecords = total,
                    TotalAmount = amount
                };
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

     
        private SqlParameter[] CloneParameters(List<SqlParameter> parameters)
        {
            var clonedParams = new SqlParameter[parameters.Count];
            for (int i = 0; i < parameters.Count; i++)
            {
                var value = parameters[i].Value ?? DBNull.Value;
                clonedParams[i] = new SqlParameter(parameters[i].ParameterName, value);
            }
            return clonedParams;
        }



    }
}