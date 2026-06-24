using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using XeniaAkcaBackend.Models;

using XeniaTokenBackend.Repositories.Dashboard;

namespace XeniaTokenBackend.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public DashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─── Helpers ──────────────────────────────────────────────

        // Builds the dynamic CASE WHEN select columns based on dateId
        private static (string memberSelect, string suffix) BuildMemberSelectPart(string? dateId)
        {
            return dateId switch
            {
                "1" => (@"
                    COUNT(CASE WHEN t.memberStatus IN (7,9) AND CONVERT(date,t.membershipDate) = CONVERT(date,GETDATE()) THEN 1 END) AS NewMemberships,
                    COUNT(CASE WHEN t.memberStatus IN (2,3,4,5,8) AND CONVERT(date,t.membershipDate) = CONVERT(date,GETDATE()) THEN 1 END) AS PendingMemberships,
                    COUNT(CASE WHEN t.memberStatus = 6 AND CONVERT(date,t.membershipDate) = CONVERT(date,GETDATE()) THEN 1 END) AS PendingDistrictLevel",
                    "Today"),

                "2" => (@"
                    COUNT(CASE WHEN t.memberStatus IN (7,9) AND t.membershipDate >= DATEADD(day,-7,GETDATE()) THEN 1 END) AS NewMemberships,
                    COUNT(CASE WHEN t.memberStatus IN (2,3,4,5,8) AND t.membershipDate >= DATEADD(day,-7,GETDATE()) THEN 1 END) AS PendingMemberships,
                    COUNT(CASE WHEN t.memberStatus = 6 AND t.membershipDate >= DATEADD(day,-7,GETDATE()) THEN 1 END) AS PendingDistrictLevel",
                    "LastWeek"),

                "3" => (@"
                    COUNT(CASE WHEN t.memberStatus IN (7,9) AND t.membershipDate >= DATEADD(month,-1,GETDATE()) THEN 1 END) AS NewMemberships,
                    COUNT(CASE WHEN t.memberStatus IN (2,3,4,5,8) AND t.membershipDate >= DATEADD(month,-1,GETDATE()) THEN 1 END) AS PendingMemberships,
                    COUNT(CASE WHEN t.memberStatus = 6 AND t.membershipDate >= DATEADD(month,-1,GETDATE()) THEN 1 END) AS PendingDistrictLevel",
                    "LastMonth"),

                "4" => (@"
                    COUNT(CASE WHEN t.memberStatus IN (7,9) AND t.membershipDate >= DATEADD(year,-1,GETDATE()) THEN 1 END) AS NewMemberships,
                    COUNT(CASE WHEN t.memberStatus IN (2,3,4,5,8) AND t.membershipDate >= DATEADD(year,-1,GETDATE()) THEN 1 END) AS PendingMemberships,
                    COUNT(CASE WHEN t.memberStatus = 6 AND t.membershipDate >= DATEADD(year,-1,GETDATE()) THEN 1 END) AS PendingDistrictLevel",
                    "LastYear"),

                _ => (@"
                    COUNT(CASE WHEN t.memberStatus IN (7,9) THEN 1 END) AS NewMemberships,
                    COUNT(CASE WHEN t.memberStatus IN (2,3,4,5,8) THEN 1 END) AS PendingMemberships,
                    COUNT(CASE WHEN t.memberStatus = 6 THEN 1 END) AS PendingDistrictLevel",
                    "All")
            };
        }

        private static string BuildGraphSelectPart(string? dateId, string groupCol)
        {
            var amountExpr = dateId switch
            {
                "1" => "SUM(CASE WHEN CONVERT(date,t.paidDate) = CONVERT(date,GETDATE()) THEN t.paidAmount ELSE 0 END)",
                "2" => "SUM(CASE WHEN t.paidDate >= DATEADD(day,-7,GETDATE()) THEN t.paidAmount ELSE 0 END)",
                "3" => "SUM(CASE WHEN t.paidDate >= DATEADD(month,-1,GETDATE()) THEN t.paidAmount ELSE 0 END)",
                "4" => "SUM(CASE WHEN t.paidDate >= DATEADD(year,-1,GETDATE()) THEN t.paidAmount ELSE 0 END)",
                _ => "SUM(t.paidAmount)"
            };
            return $"{amountExpr} AS amount, {groupCol}, s.settingName";
        }

        // Adds SQL parameters safely
        private static void AddParams(SqlCommand cmd,
            int? districtId, int? unitId, DateTime? fromDate, DateTime? toDate)
        {
            if (districtId.HasValue)
                cmd.Parameters.AddWithValue("@districtid", districtId.Value);
            if (unitId.HasValue)
                cmd.Parameters.AddWithValue("@unitid", unitId.Value);
            if (fromDate.HasValue && toDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@fromdate", fromDate.Value);
                cmd.Parameters.AddWithValue("@todate", toDate.Value);
            }
        }

        private static string BuildMemberWhere(int? districtId, int? unitId, DateTime? fromDate, DateTime? toDate)
        {
            var where = "WHERE t.memberId IS NOT NULL";
            if (districtId.HasValue) where += " AND t.memberDistrictId = @districtid";
            if (unitId.HasValue) where += " AND t.memberUnitId = @unitid";
            if (fromDate.HasValue && toDate.HasValue)
                where += " AND t.membershipDate BETWEEN @fromdate AND @todate";
            return where;
        }

        private static string BuildPaymentWhere(int? districtId, int? unitId, DateTime? fromDate, DateTime? toDate)
        {
            var where = "WHERE t.paymentStatus = 'success'";
            if (districtId.HasValue) where += " AND t.paidDistrict = @districtid";
            if (unitId.HasValue) where += " AND t.paidUnit = @unitid";
            if (fromDate.HasValue && toDate.HasValue)
                where += " AND t.paidDate BETWEEN @fromdate AND @todate";
            return where;
        }

        // Executes raw SQL and returns first row as Dictionary
        private async Task<Dictionary<string, object?>> QueryFirstRowAsync(
            string sql, int? districtId, int? unitId, DateTime? fromDate, DateTime? toDate)
        {
            var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            AddParams((SqlCommand)cmd, districtId, unitId, fromDate, toDate);

            var result = new Dictionary<string, object?>();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                for (int i = 0; i < reader.FieldCount; i++)
                    result[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);

            await conn.CloseAsync();
            return result;
        }

        // Executes raw SQL and returns all rows
        private async Task<List<Dictionary<string, object?>>> QueryAllRowsAsync(
            string sql, int? districtId, int? unitId, DateTime? fromDate, DateTime? toDate)
        {
            var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            AddParams((SqlCommand)cmd, districtId, unitId, fromDate, toDate);

            var rows = new List<Dictionary<string, object?>>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                rows.Add(row);
            }

            await conn.CloseAsync();
            return rows;
        }

        // Maps first-row dict → MembershipStats
        private static MembershipStats MapStats(Dictionary<string, object?> data) => new()
        {
            NewMemberships = Convert.ToInt32(data.GetValueOrDefault("NewMemberships") ?? 0),
            PendingMemberships = Convert.ToInt32(data.GetValueOrDefault("PendingMemberships") ?? 0),
            PendingDistrictLevel = Convert.ToInt32(data.GetValueOrDefault("PendingDistrictLevel") ?? 0)
        };

        // Builds graph dictionary grouped by name column
        private static List<Dictionary<string, object?>> GroupGraphData(
            List<Dictionary<string, object?>> rows, string groupKey)
        {
            var grouped = new Dictionary<string, Dictionary<string, object?>>();
            foreach (var row in rows)
            {
                var name = row[groupKey]?.ToString() ?? "";
                var setting = row["settingName"]?.ToString() ?? "";
                var amount = row["amount"];

                if (!grouped.ContainsKey(name))
                    grouped[name] = new Dictionary<string, object?> { [groupKey] = name };

                grouped[name][setting] = amount;
            }
            return grouped.Values.Cast<Dictionary<string, object?>>().ToList();
        }

        // ─── 1. State Wise Details ────────────────────────────────

        public async Task<DashboardResponse> GetAllStateWiseDetailsAsync(
            int? districtId, string? dateId, DateTime? fromDate, DateTime? toDate)
        {
            var (selectPart, suffix) = BuildMemberSelectPart(dateId);
            var where = BuildMemberWhere(districtId, null, fromDate, toDate);

            var sql = $@"
                SELECT {selectPart}
                FROM AKCA_KaruthalMembers t
                JOIN AKCA_Users F ON F.userId = t.memberUserId
                JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                {where}";

            var data = await QueryFirstRowAsync(sql, districtId, null, fromDate, toDate);
            var stats = MapStats(data);

            return new DashboardResponse
            {
                Data = new[] { new Dictionary<string, object> { [suffix] = stats } }
            };
        }

        // ─── 2. District Wise Details ─────────────────────────────

        public async Task<DashboardResponse> GetAllDistrictWiseDetailsAsync(
            int? districtId, int? unitId, string? dateId, DateTime? fromDate, DateTime? toDate)
        {
            var (selectPart, suffix) = BuildMemberSelectPart(dateId);
            var where = BuildMemberWhere(districtId, unitId, fromDate, toDate);

            var sql = $@"
                SELECT {selectPart}
                FROM AKCA_KaruthalMembers t
                JOIN AKCA_Users F ON F.userId = t.memberUserId
                JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                {where}";

            var data = await QueryFirstRowAsync(sql, districtId, unitId, fromDate, toDate);
            var stats = MapStats(data);

            return new DashboardResponse
            {
                Data = new[] { new Dictionary<string, object> { [suffix] = stats } }
            };
        }

        // ─── 3. Unit Wise Details ─────────────────────────────────

        public async Task<DashboardResponse> GetAllUnitWiseDetailsAsync(
            int? unitId, string? dateId, DateTime? fromDate, DateTime? toDate)
        {
            var (selectPart, suffix) = BuildMemberSelectPart(dateId);
            var where = BuildMemberWhere(null, unitId, fromDate, toDate);

            var sql = $@"
                SELECT {selectPart}
                FROM AKCA_KaruthalMembers t
                JOIN AKCA_Users F ON F.userId = t.memberUserId
                JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                {where}";

            var data = await QueryFirstRowAsync(sql, null, unitId, fromDate, toDate);
            var stats = MapStats(data);

            return new DashboardResponse
            {
                Data = new[] { new Dictionary<string, object> { [suffix] = stats } }
            };
        }

        // ─── 4. State Wise Graph Details ──────────────────────────

        public async Task<DashboardResponse> GetAllStateWiseGraphDetailsAsync(
            int? districtId, string? dateId, DateTime? fromDate, DateTime? toDate)
        {
            var selectPart = BuildGraphSelectPart(dateId, "d.districtName");
            var where = BuildPaymentWhere(districtId, null, fromDate, toDate);

            var sql = $@"
                SELECT {selectPart}
                FROM AKCA_MemberPayment t
                JOIN AKCA_Districts d ON t.paidDistrict = d.districtId
                JOIN AKCA_Settings s ON t.paymentTypeId = s.settingId
                {where}
                GROUP BY d.districtName, s.settingName";

            var rows = await QueryAllRowsAsync(sql, districtId, null, fromDate, toDate);
            var grouped = GroupGraphData(rows, "districtName");

            return new DashboardResponse
            {
                Data = new[] { grouped }
            };
        }

        // ─── 5. District Wise Graph Details ───────────────────────

        public async Task<DashboardResponse> GetAllDistrictWiseGraphDetailsAsync(
            int? districtId, int? unitId, string? dateId, DateTime? fromDate, DateTime? toDate)
        {
            var selectPart = BuildGraphSelectPart(dateId, "d.unitName");
            var where = BuildPaymentWhere(districtId, unitId, fromDate, toDate);

            var sql = $@"
                SELECT {selectPart}
                FROM AKCA_MemberPayment t
                JOIN AKCA_Units d ON t.paidUnit = d.unitId
                JOIN AKCA_Settings s ON t.paymentTypeId = s.settingId
                {where}
                GROUP BY d.unitName, s.settingName";

            var rows = await QueryAllRowsAsync(sql, districtId, unitId, fromDate, toDate);
            var grouped = GroupGraphData(rows, "unitName");

            return new DashboardResponse
            {
                Data = new[] { grouped }
            };
        }

        // ─── 6. State Wise Details + Graph (main dashboard) ───────

        public async Task<DashboardResponse> GetAllStateWiseDetailsAndGraphAsync(
            int? districtId, string? dateId, DateTime? fromDate, DateTime? toDate)
        {
            var (memberSelect, _) = BuildMemberSelectPart(dateId);
            var graphSelect = BuildGraphSelectPart(dateId, "d.districtName");
            var memberWhere = BuildMemberWhere(districtId, null, fromDate, toDate);
            var paymentWhere = BuildPaymentWhere(districtId, null, fromDate, toDate);

            var memberSql = $@"
                SELECT {memberSelect}
                FROM AKCA_KaruthalMembers t
                JOIN AKCA_Users F ON F.userId = t.memberUserId
                JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                {memberWhere}";

            var graphSql = $@"
                SELECT {graphSelect}
                FROM AKCA_MemberPayment t
                JOIN AKCA_Districts d ON t.paidDistrict = d.districtId
                JOIN AKCA_Settings s ON t.paymentTypeId = s.settingId
                {paymentWhere}
                GROUP BY d.districtName, s.settingName";

            var memberData = await QueryFirstRowAsync(memberSql, districtId, null, fromDate, toDate);
            var graphRows = await QueryAllRowsAsync(graphSql, districtId, null, fromDate, toDate);

            var stats = MapStats(memberData);
            var grouped = GroupGraphData(graphRows, "districtName");

            return new DashboardResponse
            {
                Data = new
                {
                    DashboardData = stats,
                    GraphData = grouped
                }
            };
        }

        // ─── 7. District Wise Details + Graph (main dashboard) ────

        public async Task<DashboardResponse> GetAllDistrictWiseDetailsAndGraphAsync(
            int? districtId, int? unitId, string? dateId, DateTime? fromDate, DateTime? toDate)
        {
            var (memberSelect, _) = BuildMemberSelectPart(dateId);
            var graphSelect = BuildGraphSelectPart(dateId, "d.unitName");
            var memberWhere = BuildMemberWhere(districtId, unitId, fromDate, toDate);
            var paymentWhere = BuildPaymentWhere(districtId, unitId, fromDate, toDate);

            var memberSql = $@"
                SELECT {memberSelect}
                FROM AKCA_KaruthalMembers t
                JOIN AKCA_Users F ON F.userId = t.memberUserId
                JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                {memberWhere}";

            var graphSql = $@"
                SELECT {graphSelect}
                FROM AKCA_MemberPayment t
                JOIN AKCA_Units d ON t.paidUnit = d.unitId
                JOIN AKCA_Settings s ON t.paymentTypeId = s.settingId
                {paymentWhere}
                GROUP BY d.unitName, s.settingName";

            var memberData = await QueryFirstRowAsync(memberSql, districtId, unitId, fromDate, toDate);
            var graphRows = await QueryAllRowsAsync(graphSql, districtId, unitId, fromDate, toDate);

            var stats = MapStats(memberData);
            var grouped = GroupGraphData(graphRows, "unitName");

            return new DashboardResponse
            {
                Data = new
                {
                    DashboardData = stats,
                    GraphData = grouped
                }
            };
        }
    }
}