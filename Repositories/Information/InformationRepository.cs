using Microsoft.EntityFrameworkCore;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Repositories.Informations;
using XeniaAkcaBackend.Models;

namespace XeniaAkcaBackend.Repositories
{
    public class InformationRepository : IInformationRepository
    {
        private readonly ApplicationDbContext _context;

        public InformationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        private static DateTime ToIst(DateTime utc) => utc.AddMinutes(330);

        public async Task<InformationResponse> CreateInformationAsync(CreateInformationRequest request)
        {
            var now = ToIst(DateTime.UtcNow);

            var info = new Information
            {
                InformationName = request.InformationName,
                DistrictId = request.DistrictId,
                InformationImgUrl = request.InformationImgUrl,
                InformationCreatedUser = request.InformationCreatedUser,
                InformationCreatedDate = now,
                InformationContent = request.InformationContent,
                InformationStartDate = request.InformationStartDate,
                InformationEndDate = request.InformationEndDate,
                InformationStatus = "Created",
                ActiveStatus = request.ActiveStatus,
                InformationApproveStatus = request.InformationApproveStatus
            };

            await _context.Informations.AddAsync(info);
            var result = await _context.SaveChangesAsync();

            return result > 0
                ? new InformationResponse { Status = "success", Message = "Information created successfully." }
                : new InformationResponse { Status = "fail", Message = "Information not created." };
        }


        public async Task<InformationResponse> UpdateInformationAsync(
            int informationId, UpdateInformationRequest request)
        {
            var info = await _context.Informations.FindAsync(informationId);
            if (info == null)
                return new InformationResponse { Status = "fail", Message = "Information not found." };

            info.InformationName = request.InformationName ?? info.InformationName;
            info.DistrictId = request.DistrictId ?? info.DistrictId;
            info.InformationImgUrl = request.InformationImgUrl ?? info.InformationImgUrl;
            info.InformationModifiedUser = request.InformationModifiedUser ?? info.InformationModifiedUser;
            info.InformationModifiedDate = ToIst(DateTime.UtcNow);
            info.InformationContent = request.InformationContent ?? info.InformationContent;
            info.InformationStartDate = request.InformationStartDate ?? info.InformationStartDate;
            info.InformationEndDate = request.InformationEndDate ?? info.InformationEndDate;
            info.InformationStatus = "Updated";

            await _context.SaveChangesAsync();
            return new InformationResponse { Status = "success", Message = "Information updated successfully." };
        }


        public async Task<List<object>> GetInformationByPartialNameAsync(string partialName)
        {
            var pattern = $"%{partialName}%";

            var result = await (
                from i in _context.Informations
                join d in _context.Districts on i.DistrictId equals d.DistrictId into dGroup
                from d in dGroup.DefaultIfEmpty()
                where EF.Functions.Like(i.InformationName, pattern)
                   || EF.Functions.Like(d.DistrictName, pattern)
                select (object)new
                {
                    i.InformationId,
                    i.InformationName,
                    DistrictName = d != null ? d.DistrictName : null
                }
            ).ToListAsync();

            return result;
        }

        public async Task<(List<object> Records, int Total)> GetStateInformationAsync(
            InformationListRequest req)
        {
            var query = from i in _context.Informations
                        join d in _context.Districts on i.DistrictId equals d.DistrictId into dGroup
                        from d in dGroup.DefaultIfEmpty()
                        select new { i, d };

            if (!string.IsNullOrEmpty(req.SearchText))
                query = query.Where(x =>
                    EF.Functions.Like(x.i.InformationName, $"%{req.SearchText}%") ||
                    EF.Functions.Like(x.d.DistrictName, $"%{req.SearchText}%"));

            if (req.DistrictId.HasValue)
                query = query.Where(x => x.i.DistrictId == req.DistrictId.Value);

            if (req.FromDate.HasValue && req.ToDate.HasValue)
                query = query.Where(x =>
                    x.i.InformationCreatedDate >= req.FromDate &&
                    x.i.InformationCreatedDate <= req.ToDate);

            if (req.InformationId.HasValue)
                query = query.Where(x => x.i.InformationId == req.InformationId.Value);

            var total = await query.CountAsync();

            List<object> records;

    
            if (req.InformationId.HasValue)
            {
                records = await query
                    .OrderBy(x => x.i.InformationId)
                    .Skip((req.Page - 1) * req.Limit)
                    .Take(req.Limit)
                    .Select(x => (object)new
                    {
                        x.i.InformationId,
                        DistrictName = x.d != null ? x.d.DistrictName : "All",
                        x.i.DistrictId,
                        x.i.InformationName,
                        x.i.ActiveStatus,
                        InformationStartDate = x.i.InformationStartDate.HasValue
                            ? x.i.InformationStartDate.Value.ToString("yyyy-MM-dd") : null,
                        InformationEndDate = x.i.InformationEndDate.HasValue
                            ? x.i.InformationEndDate.Value.ToString("yyyy-MM-dd") : null,
                        x.i.InformationImgUrl,
                        x.i.InformationContent
                    })
                    .ToListAsync();
            }
            else
            {
                records = await query
                    .OrderBy(x => x.i.InformationId)
                    .Skip((req.Page - 1) * req.Limit)
                    .Take(req.Limit)
                    .Select(x => (object)new
                    {
                        x.i.InformationId,
                        DistrictName = x.d != null ? x.d.DistrictName : "All",
                        x.i.DistrictId,
                        x.i.InformationName,
                        x.i.ActiveStatus,
                        InformationStartDate = x.i.InformationStartDate.HasValue
                            ? x.i.InformationStartDate.Value.ToString("yyyy-MM-dd") : null,
                        InformationEndDate = x.i.InformationEndDate.HasValue
                            ? x.i.InformationEndDate.Value.ToString("yyyy-MM-dd") : null
                    })
                    .ToListAsync();
            }

            return (records, total);
        }

        public async Task<InformationResponse?> ApproveInformationAsync(
            int informationId, bool activeStatus)
        {
            var info = await _context.Informations.FindAsync(informationId);
            if (info == null) return null;

            info.ActiveStatus = activeStatus;
            info.InformationApproveStatus = activeStatus ? 1 : 11;
            info.InformationStatus = activeStatus ? "Approved" : "Rejected";

          
            if (activeStatus)
                info.InformationApprovedDate = ToIst(DateTime.UtcNow);

            await _context.SaveChangesAsync();

            return new InformationResponse
            {
                Status = "success",
                Message = activeStatus
                    ? "Information Approved Successfully"
                    : "Information Rejected Successfully"
            };
        }

        public async Task<List<object>> GetInformationDetailsAsync(int districtId)
        {
            var now = ToIst(DateTime.UtcNow);

            var result = await (
                from i in _context.Informations
                join d in _context.Districts on i.DistrictId equals d.DistrictId into dGroup
                from d in dGroup.DefaultIfEmpty()
                where (i.DistrictId == 0 || i.DistrictId == districtId)
                   && i.InformationStartDate <= now
                   && i.InformationEndDate >= now
                select (object)new
                {
                    i.InformationId,
                    i.InformationContent
                }
            ).ToListAsync();

            return result;
        }
    }
}