using Microsoft.Data.SqlClient;
using System.Data;
using XeniaAkcaBackend.Repositories.Dashboard;

namespace XeniaAkcaBackend.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly string _conn;

        public DashboardRepository(IConfiguration config) => _conn = config.GetConnectionString("DefaultConnection")!;

        public async Task<object> GetDashboardAsync(string type, int? districtId, int? unitId, string? dateId, DateTime? fromDate, DateTime? toDate)
        {
            var (memberSql, graphSql) = type switch
            {
                "state" => BuildStateQueries(dateId),
                "stateAA" => BuildMembershipOnlyQuery(dateId),
                "district" => BuildDistrictQueries(dateId),
                "districtAA" => BuildMembershipOnlyQuery(dateId),
                "unitAA" => BuildMembershipOnlyQuery(dateId),
                "graph/stateAA" => (null, BuildGraphQuery(dateId, "d.districtName", "AKCA_Districts d ON t.paidDistrict = d.districtId")),
                "graph/districtAA" => (null, BuildGraphQuery(dateId, "d.unitName", "AKCA_Units d ON t.paidUnit = d.unitId")),
                _ => throw new ArgumentException("Invalid type")
            };

            var parameters = BuildParams(districtId, unitId, fromDate, toDate);
            var memberWhere = BuildWhere(parameters, true);
            var paymentWhere = BuildWhere(parameters, false);

            if (memberSql != null) memberSql += memberWhere;
            if (graphSql != null) graphSql += paymentWhere;

            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            var tasks = new List<Task<List<Dictionary<string, object?>>>>();
            if (memberSql != null) tasks.Add(ExecuteAsync(conn, memberSql, parameters));
            if (graphSql != null) tasks.Add(ExecuteAsync(conn, graphSql, parameters));

            await Task.WhenAll(tasks);

            var results = tasks.Select(t => t.Result).ToList();
            var memberData = results.Count > 0 && memberSql != null ? results[0].FirstOrDefault() : null;
            var graphData = results.Count > 1 || (results.Count == 1 && memberSql == null) ? results.Last() : null;

            return type switch
            {
                "state" => FormatDashboardResponse(memberData!, graphData!, dateId, "districtName"),
                "district" => FormatDashboardResponse(memberData!, graphData!, dateId, "unitName"),
                "stateAA" or "districtAA" or "unitAA" => FormatMembershipResponse(memberData!, dateId),
                "graph/stateAA" => FormatGraphResponse(graphData!, "districtName"),
                "graph/districtAA" => FormatGraphResponse(graphData!, "unitName"),
                _ => throw new ArgumentException("Invalid type")
            };
        }

        // ─── Query Builders ───────────────────────────────────────

        private static (string?, string?) BuildStateQueries(string? dateId) => (
            BuildMemberSelect(dateId, true), BuildGraphQuery(dateId, "d.districtName", "AKCA_Districts d ON t.paidDistrict = d.districtId"));

        private static (string?, string?) BuildDistrictQueries(string? dateId) => (
            BuildMemberSelect(dateId, true), BuildGraphQuery(dateId, "d.unitName", "AKCA_Units d ON t.paidUnit = d.unitId"));

        private static (string?, string?) BuildMembershipOnlyQuery(string? dateId) => (
            BuildMemberSelect(dateId, false), null);

        private static string BuildMemberSelect(string? dateId, bool isDashboard)
        {
            var (dateCheck, colSuffix) = GetDateConfig(dateId);
            var statusNew = isDashboard ? "IN (7,9)" : "IN (2,3,4,5,6,7)";
            var statusPending = isDashboard ? "IN (2,3,4,5,8)" : "IN (6,7)";
            var statusDistrict = isDashboard ? "= 6" : "= 7";

            return $@"
                SELECT 
                    COUNT(CASE WHEN t.memberStatus {statusNew} AND ({dateCheck}) THEN 1 END) AS NewMemberships{colSuffix},
                    COUNT(CASE WHEN t.memberStatus {statusPending} AND ({dateCheck}) THEN 1 END) AS PendingMemberships{colSuffix},
                    COUNT(CASE WHEN t.memberStatus {statusDistrict} AND ({dateCheck}) THEN 1 END) AS PendingDistrictLevel{colSuffix}
                FROM AKCA_KaruthalMembers t
                JOIN AKCA_Users F ON F.userId = t.memberUserId
                JOIN AKCA_Districts s ON s.districtId = t.memberDistrictId
                JOIN AKCA_Units u ON u.unitId = t.memberUnitId
                WHERE t.memberId IS NOT NULL";
        }

        private static string BuildGraphQuery(string? dateId, string groupCol, string joinClause)
        {
            var (dateCheck, _) = GetDateConfig(dateId);
            return $@"
                SELECT 
                    SUM(CASE WHEN ({dateCheck}) THEN t.paidAmount ELSE 0 END) AS amount,
                    {groupCol} AS GroupName,
                    s.settingName
                FROM AKCA_MemberPayment t
                JOIN {joinClause}
                JOIN AKCA_Settings s ON t.paymentTypeId = s.settingId
                WHERE t.paymentStatus = 'success'
                GROUP BY {groupCol}, s.settingName";
        }

        private static (string dateCheck, string colSuffix) GetDateConfig(string? dateId) => dateId switch
        {
            "1" => ("CONVERT(date,t.{0}) = CONVERT(date,GETDATE())", "_Today"),
            "2" => ("t.{0} >= DATEADD(day,-7,GETDATE())", "_LastWeek"),
            "3" => ("t.{0} >= DATEADD(month,-1,GETDATE())", "_LastMonth"),
            "4" => ("t.{0} >= DATEADD(year,-1,GETDATE())", "_LastYear"),
            "5" => ("1=1", ""),
            _ => ("1=1", "_All")
        };

        // ─── Parameter Builder ────────────────────────────────────

        private static Dictionary<string, object?> BuildParams(int? districtId, int? unitId, DateTime? fromDate, DateTime? toDate)
        {
            var p = new Dictionary<string, object?>();
            if (districtId.HasValue) p["@districtid"] = districtId;
            if (unitId.HasValue) p["@unitid"] = unitId;
            if (fromDate.HasValue) { p["@fromdate"] = fromDate; p["@todate"] = toDate; }
            return p;
        }

        private static string BuildWhere(Dictionary<string, object?> p, bool isMembership)
        {
            var conditions = new List<string>();
            if (p.ContainsKey("@districtid")) conditions.Add(isMembership ? "t.memberDistrictId = @districtid" : "t.paidDistrict = @districtid");
            if (p.ContainsKey("@unitid")) conditions.Add(isMembership ? "t.memberUnitId = @unitid" : "t.paidUnit = @unitid");
            if (p.ContainsKey("@fromdate")) conditions.Add(isMembership ? "t.membershipDate BETWEEN @fromdate AND @todate" : "t.paidDate BETWEEN @fromdate AND @todate");
            return conditions.Count > 0 ? " AND " + string.Join(" AND ", conditions) : "";
        }

        // ─── Data Access ──────────────────────────────────────────

        private static async Task<List<Dictionary<string, object?>>> ExecuteAsync(SqlConnection conn, string sql, Dictionary<string, object?> parameters)
        {
            using var cmd = new SqlCommand(sql, conn);
            foreach (var p in parameters)
                cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);

            var rows = new List<Dictionary<string, object?>>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                rows.Add(row);
            }
            return rows;
        }

        // ─── Response Formatters ──────────────────────────────────

        private static object FormatDashboardResponse(Dictionary<string, object?> member, List<Dictionary<string, object?>> graph, string? dateId, string groupKey)
        {
            var label = GetLabel(dateId);
            return new
            {
                status = "success",
                data = new
                {
                    DashboardData = new
                    {
                        NewMemberships = member[$"NewMemberships{label}"],
                        PendingMemberships = member[$"PendingMemberships{label}"],
                        PendingDistrictLevel = member[$"PendingDistrictLevel{label}"]
                    },
                    GraphData = GroupGraphData(graph, groupKey)
                }
            };
        }

        private static object FormatMembershipResponse(Dictionary<string, object?> member, string? dateId)
        {
            var label = GetLabel(dateId);
            var key = dateId switch { "1" => "Today", "2" => "LastWeek", "3" => "LastMonth", "4" => "LastYear", "5" => "CustomRange", _ => "All" };

            return new
            {
                status = "success",
                data = new[]
                {
                    new Dictionary<string, object>
                    {
                        [key] = new
                        {
                            NewMemberships = member[$"NewMemberships{label}"],
                            PendingMemberships = member[$"PendingMemberships{label}"],
                            PendingDistrictLevel = member[$"PendingDistrictLevel{label}"]
                        }
                    }
                }
            };
        }

        private static object FormatGraphResponse(List<Dictionary<string, object?>> graph, string groupKey) => new
        {
            status = "success",
            data = new[] { GroupGraphData(graph, groupKey) }
        };

        private static string GetLabel(string? dateId) => dateId switch { "1" => "_Today", "2" => "_LastWeek", "3" => "_LastMonth", "4" => "_LastYear", "5" => "", _ => "_All" };

        private static Dictionary<string, object> GroupGraphData(List<Dictionary<string, object?>> rows, string groupKey)
        {
            var grouped = new Dictionary<string, Dictionary<string, object?>>();
            foreach (var row in rows)
            {
                var name = row["GroupName"]?.ToString() ?? "";
                var setting = row["settingName"]?.ToString() ?? "";
                if (!grouped.ContainsKey(name))
                    grouped[name] = new Dictionary<string, object?> { [groupKey] = name };
                grouped[name][setting] = row["amount"];
            }
            return grouped.ToDictionary(x => x.Key, x => (object)x.Value);
        }
    }
}