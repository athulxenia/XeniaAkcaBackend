using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Models;
using XeniaAkcaBackend.Repositories.Advertisements;

namespace XeniaAkcaBackend.Repositories
{
    public class AdvertisementRepository : IAdvertisementRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

        public AdvertisementRepository(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        private static string FormatDateToIst(DateTime utcDate)
        {
            var ist = utcDate.AddMinutes(330);
            return ist.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        // ─── Create ───────────────────────────────────────────────

        public async Task<AdvertisementResponse> CreateAsync(CreateAdvertisementRequest request)
        {
            var now = DateTime.UtcNow;
            var createdDate = FormatDateToIst(now);

            var advertisement = new Advertisement
            {
                AdvertisementName = request.AdvertisementName ?? "",
                DistrictId = request.DistrictId,
                FileUrl = request.FileUrl ?? "",
                AdvertisementCreatedUser = request.AdvertisementCreatedUser ?? "",
                AdvertisementCreatedDate = now,
                AdvertisementContent = request.AdvertisementContent ?? "",
                AdvertisementStartDate = request.AdvertisementStartDate ?? now,
                AdvertisementEndDate = request.AdvertisementEndDate ?? now.AddMonths(1),
                AdvertisementStatus = "Created",
                ActiveStatus = request.ActiveStatus,
                AdvertisementApproveStatus = request.AdvertisementApproveStatus
            };

            await _context.Advertisements.AddAsync(advertisement);
            var result = await _context.SaveChangesAsync();

            return result > 0
                ? new AdvertisementResponse { Status = "success", Message = "Advertisement created successfully" }
                : new AdvertisementResponse { Status = "fail", Message = "Advertisement not created" };
        }

        // ─── Update ───────────────────────────────────────────────

        public async Task<AdvertisementResponse> UpdateAsync(int advertisementId, UpdateAdvertisementRequest request)
        {
            var ad = await _context.Advertisements.FindAsync(advertisementId);
            if (ad == null)
                return new AdvertisementResponse { Status = "fail", Message = "Advertisement not found" };

            var now = DateTime.UtcNow;

            ad.AdvertisementName = request.AdvertisementName ?? ad.AdvertisementName;
            ad.DistrictId = request.DistrictId;
            ad.FileUrl = request.FileUrl ?? ad.FileUrl;
            ad.AdvertisementModifiedUser = request.AdvertisementModifiedUser;
            ad.AdvertisementModifiedDate = now;
            ad.AdvertisementContent = request.AdvertisementContent ?? ad.AdvertisementContent;
            ad.AdvertisementStartDate = request.AdvertisementStartDate ?? ad.AdvertisementStartDate;
            ad.AdvertisementEndDate = request.AdvertisementEndDate ?? ad.AdvertisementEndDate;
            ad.AdvertisementStatus = "Updated";

            await _context.SaveChangesAsync();

            return new AdvertisementResponse { Status = "success", Message = "Advertisement Updated successfully" };
        }

        // ─── Search ───────────────────────────────────────────────

        public async Task<List<object>> SearchAsync(string partialName)
        {
            var pattern = $"%{partialName}%";
            return await (
                from a in _context.Advertisements
                join d in _context.Districts on a.DistrictId equals d.DistrictId
                where EF.Functions.Like(a.AdvertisementName, pattern)
                   || EF.Functions.Like(d.DistrictName, pattern)
                select (object)new
                {
                    a.AdvertisementId,
                    a.AdvertisementName,
                    d.DistrictName
                }
            ).ToListAsync();
        }

        // ─── State List ───────────────────────────────────────────

        public async Task<object> GetStateAsync(int page, int limit, string? searchText, int? districtId, DateTime? fromDate, DateTime? toDate, int? advertisementId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var whereClauses = new List<string> { "1=1" };
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(searchText))
            {
                whereClauses.Add("(d.districtName LIKE @searchText OR t.advertisementName LIKE @searchText)");
                parameters.Add(new SqlParameter("@searchText", $"%{searchText}%"));
            }
            if (districtId.HasValue)
            {
                whereClauses.Add("t.districtId = @districtId");
                parameters.Add(new SqlParameter("@districtId", districtId.Value));
            }
            if (fromDate.HasValue && toDate.HasValue)
            {
                whereClauses.Add("t.advertisementCreatedDate BETWEEN @fromDate AND @toDate");
                parameters.Add(new SqlParameter("@fromDate", fromDate.Value));
                parameters.Add(new SqlParameter("@toDate", toDate.Value));
            }
            if (advertisementId.HasValue)
            {
                whereClauses.Add("t.advertisementId = @advertisementId");
                parameters.Add(new SqlParameter("@advertisementId", advertisementId.Value));
            }

            var whereClause = string.Join(" AND ", whereClauses);

            // Total count
            var totalSql = $"SELECT COUNT(*) FROM AKCA_Advertisement t LEFT JOIN AKCA_Districts d ON t.districtId = d.districtId WHERE {whereClause}";
            using var totalCmd = new SqlCommand(totalSql, connection);
            totalCmd.Parameters.AddRange(parameters.ToArray());
            var total = (int)await totalCmd.ExecuteScalarAsync();

            // Data query
            var selectFields = @"t.advertisementId, ISNULL(d.districtName,'All') as districtName, t.districtId, t.advertisementName, t.activeStatus, FORMAT(t.advertisementStartDate, 'yyyy-MM-dd') as advertisementStartDate, FORMAT(t.advertisementEndDate, 'yyyy-MM-dd') as advertisementEndDate";

            if (advertisementId.HasValue)
                selectFields += ", t.fileUrl, t.advertisementContent";

            var offset = (page - 1) * limit;
            var dataSql = $@"SELECT {selectFields} FROM AKCA_Advertisement t LEFT JOIN AKCA_Districts d ON t.districtId = d.districtId WHERE {whereClause} ORDER BY t.advertisementId ASC OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            using var dataCmd = new SqlCommand(dataSql, connection);
            dataCmd.Parameters.AddRange(parameters.ToArray());
            dataCmd.Parameters.AddWithValue("@offset", offset);
            dataCmd.Parameters.AddWithValue("@limit", limit);

            var records = new List<object>();
            using var reader = await dataCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (advertisementId.HasValue && records.Count == 0)
                {
                    records.Add(new
                    {
                        AdvertisementId = reader.GetInt32(0),
                        DistrictName = reader.GetString(1),
                        DistrictId = reader.GetInt32(2),
                        AdvertisementName = reader.GetString(3),
                        ActiveStatus = reader.GetBoolean(4),
                        AdvertisementStartDate = reader.GetString(5),
                        AdvertisementEndDate = reader.GetString(6),
                        FileUrl = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        AdvertisementContent = reader.IsDBNull(8) ? "" : reader.GetString(8)
                    });
                }
                else
                {
                    records.Add(new
                    {
                        AdvertisementId = reader.GetInt32(0),
                        DistrictName = reader.GetString(1),
                        DistrictId = reader.GetInt32(2),
                        AdvertisementName = reader.GetString(3),
                        ActiveStatus = reader.GetBoolean(4),
                        AdvertisementStartDate = reader.GetString(5),
                        AdvertisementEndDate = reader.GetString(6)
                    });
                }
            }

            return new
            {
                status = "success",
                data = advertisementId.HasValue && records.Count == 1 ? records[0] : records,
                totalPages = (int)Math.Ceiling((double)total / limit),
                currentPage = page,
                limit,
                totalRecords = total
            };
        }

        // ─── Approve / Reject ─────────────────────────────────────

        public async Task<AdvertisementResponse> ApproveAsync(int advertisementId, bool activeStatus)
        {
            var ad = await _context.Advertisements.FindAsync(advertisementId);
            if (ad == null)
                return new AdvertisementResponse { Status = "fail", Message = "Advertisement not found" };

            ad.ActiveStatus = activeStatus;
            ad.AdvertisementApproveStatus = activeStatus ? 1 : 11;
            ad.AdvertisementStatus = activeStatus ? "Approved" : "Rejected";

            await _context.SaveChangesAsync();

            return new AdvertisementResponse
            {
                Status = "success",
                Message = activeStatus ? "Advertisement Approved Successfully" : "Advertisement Rejected Successfully"
            };
        }

        // ─── Advertisement List (for app) ─────────────────────────

        public async Task<List<object>> GetListAsync(int districtId)
        {
            return await _context.Advertisements
                .Where(a => a.ActiveStatus == true && a.AdvertisementApproveStatus == 1 && a.DistrictId == 0)
                .Select(a => new
                {
                    a.AdvertisementName,
                    a.FileUrl,
                    a.AdvertisementContent
                })
                .Union(
                    _context.Advertisements
                        .Where(a => a.ActiveStatus == true && a.AdvertisementApproveStatus == 1 && a.DistrictId == districtId)
                        .Select(a => new
                        {
                            a.AdvertisementName,
                            a.FileUrl,
                            a.AdvertisementContent
                        })
                )
                .Select(x => (object)new
                {
                    x.AdvertisementName,
                    x.FileUrl,
                    x.AdvertisementContent
                })
                .ToListAsync();
        }
    }
}