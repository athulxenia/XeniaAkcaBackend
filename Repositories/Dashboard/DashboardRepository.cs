using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using XeniaAkcaBackend.Models;

namespace XeniaAkcaBackend.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

        public DashboardRepository(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        public async Task<object> GetDashboardAsync(int userId, int? districtId, int? unitId, string? dateId, DateTime? fromDate, DateTime? toDate)
        {
            var stateData = await GetStateDashboardData(districtId, dateId, fromDate, toDate);
            var districtData = await GetDistrictDashboardData(districtId, unitId, dateId, fromDate, toDate);

            var wallet = await _context.KaruthalMembers
                .Where(m => m.MemberUserId == userId)
                .Select(m => m.MemberKaruthalWallet ?? 0m)
                .FirstOrDefaultAsync();

            var adFiles = await _context.Advertisements
     .Where(a => a.ActiveStatus && a.AdvertisementApproveStatus == 1)
     .Where(a => districtId == null || districtId == 0
         ? a.DistrictId == 0
         : (a.DistrictId == 0 || a.DistrictId == districtId))
     .OrderByDescending(a => a.AdvertisementStartDate)
     .Select(a => new
     {
         a.AdvertisementId,
         a.AdvertisementName,
         a.AdvertisementContent,
         a.FileUrl
     })
     .ToListAsync();

            var infos = await _context.Informations
                .Where(i => i.ActiveStatus
                         && (i.InformationApproveStatus == 1 || i.InformationApproveStatus == null))
                .Where(i => districtId == null || districtId == 0
                    ? i.DistrictId == 0
                    : (i.DistrictId == 0 || i.DistrictId == districtId))
                .OrderByDescending(i => i.InformationStartDate)
                .Select(i => new
                {
                    i.InformationId,
                    i.InformationContent,
                    InformationImgUrl = i.InformationImgUrl ?? ""
                })
                .ToListAsync();

            return new
            {
                status = "success",
                data = new
                {
                    StateDashboard = stateData,
                    DistrictDashboard = districtData,
                    WalletBalance = wallet,
                    AdvertisementFiles = adFiles,
                    Informations = infos
                }
            };
        }

        private async Task<object> GetStateDashboardData(int? districtId, string? dateId, DateTime? fromDate, DateTime? toDate)
        {
            var p = new Dictionary<string, object?>();
            if (districtId.HasValue) p["@districtid"] = districtId.Value;
            if (fromDate.HasValue) { p["@fromdate"] = fromDate.Value; p["@todate"] = toDate!.Value; }

            var memberSql = BuildMemberQuery(dateId, true) + BuildWhere(p, true);
            var graphSql = BuildGraphQuery(dateId, "d.districtName", "AKCA_Districts d ON t.paidDistrict = d.districtId") + BuildWhere(p, false) + " GROUP BY d.districtName, s.settingName";

            var memberTask = ExecuteFirstRowAsync(memberSql, p);
            var graphTask = ExecuteAllRowsAsync(graphSql, p);
            await Task.WhenAll(memberTask, graphTask);

            return new
            {
                DashboardData = MapDashboardData(await memberTask, dateId),
                GraphData = MapGraphData(await graphTask, "districtName")
            };
        }

        private async Task<object> GetDistrictDashboardData(int? districtId, int? unitId, string? dateId, DateTime? fromDate, DateTime? toDate)
        {
            var p = new Dictionary<string, object?>();
            if (districtId.HasValue) p["@districtid"] = districtId.Value;
            if (unitId.HasValue) p["@unitid"] = unitId.Value;
            if (fromDate.HasValue) { p["@fromdate"] = fromDate.Value; p["@todate"] = toDate!.Value; }

            var memberSql = BuildMemberQuery(dateId, true) + BuildWhere(p, true);
            var graphSql = BuildGraphQuery(dateId, "d.unitName", "AKCA_Units d ON t.paidUnit = d.unitId") + BuildWhere(p, false) + " GROUP BY d.unitName, s.settingName";

            var memberTask = ExecuteFirstRowAsync(memberSql, p);
            var graphTask = ExecuteAllRowsAsync(graphSql, p);
            await Task.WhenAll(memberTask, graphTask);

            return new
            {
                DashboardData = MapDashboardData(await memberTask, dateId),
                GraphData = MapGraphData(await graphTask, "unitName")
            };
        }

        private static string BuildMemberQuery(string? dateId, bool isDashboard)
        {
            var ns = isDashboard ? "IN (7,9)" : "IN (2,3,4,5,6,7)";
            var ps = isDashboard ? "IN (2,3,4,5,8)" : "IN (6,7)";
            var ds = isDashboard ? "= 6" : "= 7";
            var (dc, sf) = GetDateConfig(dateId);
            dc = dc.Replace("{0}", "t.membershipDate");
            return $@"SELECT COUNT(CASE WHEN t.memberStatus {ns} AND ({dc}) THEN 1 END) AS NewMemberships{sf},COUNT(CASE WHEN t.memberStatus {ps} AND ({dc}) THEN 1 END) AS PendingMemberships{sf},COUNT(CASE WHEN t.memberStatus {ds} AND ({dc}) THEN 1 END) AS PendingDistrictLevel{sf} FROM AKCA_KaruthalMembers t JOIN AKCA_Users F ON F.userId=t.memberUserId JOIN AKCA_Districts s ON s.districtId=t.memberDistrictId JOIN AKCA_Units u ON u.unitId=t.memberUnitId WHERE t.memberId IS NOT NULL";
        }

        private static string BuildGraphQuery(string? dateId, string groupCol, string joinClause)
        {
            var (dc, _) = GetDateConfig(dateId);
            dc = dc.Replace("{0}", "t.paidDate");
            return $@"SELECT SUM(CASE WHEN ({dc}) THEN t.paidAmount ELSE 0 END) AS amount,{groupCol} AS GroupName,s.settingName FROM AKCA_MemberPayment t JOIN {joinClause} JOIN AKCA_Settings s ON t.paymentTypeId=s.settingId WHERE t.paymentStatus='success'";
        }

        private static (string, string) GetDateConfig(string? dateId) => dateId switch
        {
            "1" => ("CONVERT(date,{0})=CONVERT(date,GETDATE())", "_Today"),
            "2" => ("{0}>=DATEADD(day,-7,GETDATE())", "_LastWeek"),
            "3" => ("{0}>=DATEADD(month,-1,GETDATE())", "_LastMonth"),
            "4" => ("{0}>=DATEADD(year,-1,GETDATE())", "_LastYear"),
            "5" => ("1=1", ""),
            _ => ("1=1", "_All")
        };

        private static string BuildWhere(Dictionary<string, object?> p, bool isMembership)
        {
            var c = new List<string>();
            if (p.ContainsKey("@districtid")) c.Add(isMembership ? "t.memberDistrictId=@districtid" : "t.paidDistrict=@districtid");
            if (p.ContainsKey("@unitid")) c.Add(isMembership ? "t.memberUnitId=@unitid" : "t.paidUnit=@unitid");
            if (p.ContainsKey("@fromdate")) c.Add(isMembership ? "t.membershipDate BETWEEN @fromdate AND @todate" : "t.paidDate BETWEEN @fromdate AND @todate");
            return c.Count > 0 ? " AND " + string.Join(" AND ", c) : "";
        }

        private async Task<Dictionary<string, object?>> ExecuteFirstRowAsync(string sql, Dictionary<string, object?> p)
        {
            using var c = new SqlConnection(_connectionString); await c.OpenAsync();
            using var cmd = new SqlCommand(sql, c);
            foreach (var kv in p) cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
            var r = new Dictionary<string, object?>();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) for (int i = 0; i < reader.FieldCount; i++) r[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            return r;
        }

        private async Task<List<Dictionary<string, object?>>> ExecuteAllRowsAsync(string sql, Dictionary<string, object?> p)
        {
            using var c = new SqlConnection(_connectionString); await c.OpenAsync();
            using var cmd = new SqlCommand(sql, c);
            foreach (var kv in p) cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
            var rows = new List<Dictionary<string, object?>>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) { var row = new Dictionary<string, object?>(); for (int i = 0; i < reader.FieldCount; i++) row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i); rows.Add(row); }
            return rows;
        }

        private static object MapDashboardData(Dictionary<string, object?> d, string? dateId)
        {
            var s = dateId switch { "1" => "_Today", "2" => "_LastWeek", "3" => "_LastMonth", "4" => "_LastYear", "5" => "", _ => "_All" };
            return new { NewMemberships = Convert.ToInt32(d.GetValueOrDefault($"NewMemberships{s}") ?? 0), PendingMemberships = Convert.ToInt32(d.GetValueOrDefault($"PendingMemberships{s}") ?? 0), PendingDistrictLevel = Convert.ToInt32(d.GetValueOrDefault($"PendingDistrictLevel{s}") ?? 0) };
        }

        private static List<object> MapGraphData(List<Dictionary<string, object?>> rows, string groupKey)
        {
            var g = new Dictionary<string, Dictionary<string, object?>>();
            foreach (var r in rows) { var n = r["GroupName"]?.ToString() ?? ""; var s = r["settingName"]?.ToString() ?? ""; if (!g.ContainsKey(n)) g[n] = new Dictionary<string, object?> { [groupKey] = n }; g[n][s] = Convert.ToDecimal(r["amount"] ?? 0); }
            return g.Values.Select(x => (object)x).ToList();
        }
    }
}