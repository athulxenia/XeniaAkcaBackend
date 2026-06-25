using Microsoft.EntityFrameworkCore;
using XeniaAkcaBackend.Models;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Repositories;

namespace XeniaAkcaBackend.Repositories
{
    public class NomineeRepository : INomineeRepository
    {
        private readonly ApplicationDbContext _context;

        public NomineeRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<object?> GetNomineeAsync(int userId)
        {
         
            var memberId = await _context.Members
                .Where(m => m.MemberUserId == userId)
                .Select(m => m.MemberId)
                .FirstOrDefaultAsync();

            if (memberId == 0)
            {
                memberId = await _context.KaruthalMembers
                    .Where(m => m.MemberUserId == userId)
                    .Select(m => m.MemberId)
                    .FirstOrDefaultAsync();
            }

            if (memberId == 0) return null;

            var nominees = await _context.Nominees
                .Where(n => n.NomineeMemberId == memberId)
                .ToListAsync();

            if (!nominees.Any()) return null;

            var current = nominees.FirstOrDefault(n => n.NomineeStatus == 1);
            var newNom = nominees.FirstOrDefault(n => n.NomineeStatus == 0);

            return new
            {
                CurrentNominee = current == null ? (object)new { } : MapNominee(current),
                NewNominee = newNom == null ? (object)new { } : MapNominee(newNom)
            };
        }

        private static object MapNominee(Nominee n) => new
        {
            n.NomineeId,
            n.NomineeMemberId,
            n.NomineeName,
            n.NomineeAddress,
            n.NomineeEmail,
            n.NomineeMobilenumber,
            n.NomineeIdProof,
            n.NomineeIdProofNumber,
            n.NomineeBankName,
            n.NomineeBankAcName,
            n.NomineeBankAcNumber,
            n.NomineeBankBranch,
            n.NomineeIfsc,
            n.NomineeIdUrl1,
            n.NomineeIdUrl2,
            n.NomineeApprovalStatus,
            n.NomineeStatus,
            n.NomineeRelation
        };

        
        public async Task<object> GetAllNomineesAsync(int page, int limit, string search, int? unitId)
        {
            var pattern = $"%{search}%";

            var query = from n in _context.Nominees
                        join m in _context.Members on n.NomineeMemberId equals m.MemberId
                        where n.NomineeApprovalStatus == 0
                           && n.NomineeStatus == 0
                           && (EF.Functions.Like(m.MemberBusinessName, pattern)
                            || EF.Functions.Like(m.MemberName, pattern)
                            || EF.Functions.Like(m.MemberMobilenumber, pattern))
                        select new { n, m };

            if (unitId.HasValue)
                query = query.Where(x => x.m.MemberUnitId == unitId.Value);

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / limit);

            var records = await query
                .OrderBy(x => x.m.MemberName)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(x => (object)new
                {
                    x.m.MemberUserId,
                    x.m.MemberBusinessName,
                    x.m.MemberName,
                    x.m.MemberMobilenumber
                })
                .ToListAsync();

            return new
            {
                Nominees = records,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = page,
                Limit = limit
            };
        }

        public async Task<NomineeResponse> UpdateNomineeAsync(int userId, UpdateNomineeRequest request)
        {
            var memberId = await _context.Members
                .Where(m => m.MemberUserId == userId)
                .Select(m => m.MemberId)
                .FirstOrDefaultAsync();


            if (memberId == 0)
            {
                memberId = await _context.KaruthalMembers
                    .Where(m => m.MemberUserId == userId)
                    .Select(m => m.MemberId)
                    .FirstOrDefaultAsync();
            }
            if (memberId == 0)
                return new NomineeResponse { Status = "error", Message = "Member not found." };

            var nominee = new Nominee
            {
                NomineeMemberId = memberId,
                NomineeName = request.NomineeName,
                NomineeAddress = request.NomineeAddress,
                NomineeEmail = request.NomineeEmail,
                NomineeMobilenumber = request.NomineeMobilenumber,
                NomineeIdProof = request.NomineeIdProof,
                NomineeIdProofNumber = request.NomineeIdProofNumber,
                NomineeBankName = request.NomineeBankName,
                NomineeBankAcName = request.NomineeBankAcName,
                NomineeBankAcNumber = request.NomineeBankAcNumber,
                NomineeBankBranch = request.NomineeBankBranch,
                NomineeIfsc = request.NomineeIfsc,
                NomineeIdUrl1 = request.NomineeIdUrl1,
                NomineeIdUrl2 = request.NomineeIdUrl2,
                NomineeApprovalStatus = 0,
                NomineeStatus = 0,              
                NomineeRelation = request.NomineeRelation
            };

            await _context.Nominees.AddAsync(nominee);
            var result = await _context.SaveChangesAsync();

            return result > 0
                ? new NomineeResponse { Status = "success", Message = "Nominee updated successfully." }
                : new NomineeResponse { Status = "error", Message = "Nominee not found or no changes made." };
        }


        public async Task<NomineeResponse> ApproveNomineeAsync(int memberUserId, bool memberStatus)
        {
            if (memberStatus)
            {
            
                var hasStatusOne = await _context.Nominees
                    .AnyAsync(n => n.NomineeStatus == 1
                               && n.NomineeMemberId == memberUserId);

                if (hasStatusOne)
                {
                    await _context.Nominees
                        .Where(n => n.NomineeStatus == 1
                                 && n.NomineeMemberId == memberUserId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(n => n.NomineeApprovalStatus, 1));
                }

                var count = await _context.Nominees
                    .CountAsync(n => n.NomineeMemberId == memberUserId);

                if (count > 1)
                {
                    var nominees = await _context.Nominees
                        .Where(n => n.NomineeMemberId == memberUserId)
                        .ToListAsync();

                    foreach (var n in nominees)
                    {
     
                        n.NomineeStatus = n.NomineeApprovalStatus == 1 ? 0 : 1;

                     
                        n.NomineeApprovalStatus = n.NomineeApprovalStatus == 0
                            ? 1
                            : n.NomineeApprovalStatus;
                    }

                    await _context.SaveChangesAsync();
                }

                return new NomineeResponse { Status = "success", Message = "Nominee approved successfully." };
            }
            else
            {
                
                await _context.Nominees
                    .Where(n => n.NomineeStatus == 0
                             && n.NomineeApprovalStatus == 0
                             && n.NomineeMemberId == memberUserId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(n => n.NomineeStatus, 0)
                        .SetProperty(n => n.NomineeApprovalStatus, 10));

                return new NomineeResponse { Status = "success", Message = "Nominee rejected." };
            }
        }
    }
}