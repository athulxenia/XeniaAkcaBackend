using Microsoft.EntityFrameworkCore;
using XeniaAkcaBackend.Dto;
using System.Text.RegularExpressions;
using XeniaAkcaBackend.Models;
using XeniaAkcaBackend.Repositories.Member;

namespace XeniaAkcaBackend.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly ApplicationDbContext _context;

        public MemberRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object?> GetMemberAsync(string membershipNumberPrefix, string membershipNumber)
        {
            var query = from m in _context.KaruthalMembers
                        join n in _context.Nominees on m.MemberId equals n.NomineeMemberId
                        join u in _context.Users on m.MemberUserId equals u.UserId
                        join d in _context.Districts on m.MemberDistrictId equals d.DistrictId
                        join unit in _context.Units on m.MemberUnitId equals unit.UnitId
                        where n.NomineeStatus == 1   // ← true → 1
                        select new { m, d, unit };

            if (!string.IsNullOrEmpty(membershipNumberPrefix))
            {
                query = query.Where(x => x.m.MembershipNumberPrefix == membershipNumberPrefix
                                      && x.m.MembershipNumber == membershipNumber);
            }
            else
            {
                query = query.Where(x => x.m.MemberMobilenumber == membershipNumber);
            }

            var result = await query.Select(x => new
            {
                x.m.MemberBusinessName,
                x.m.MemberBusinessAddress,
                x.d.DistrictName,
                x.unit.UnitName,
                x.m.MemberUserId,
                x.m.MemberDistrictId,
                x.m.MemberUnitId
            }).FirstOrDefaultAsync();

            return result != null
                ? new { status = "success", data = new[] { result } }
                : new { status = "failed", data = (object?)null };
        }

        public async Task<PaginatedResult<object>> GetAllStateWiseMembersAsync(
            int active, string? pending, int page, int limit, string? searchText, int? districtId, int? unitId)
        {
            var query = from m in _context.KaruthalMembers
                        join u in _context.Users on m.MemberUserId equals u.UserId
                        join d in _context.Districts on m.MemberDistrictId equals d.DistrictId
                        join unit in _context.Units on m.MemberUnitId equals unit.UnitId
                        join g in _context.MemberGroups on m.MemberGroupId equals g.GroupId
                        where g.GroupId == u.UserGroupId
                        select new { m, d, unit, g };

            if (active == 1)
                query = query.Where(x => x.m.MemberActiveStatus == true && new[] { 9, 11 }.Contains(x.m.MemberStatus ?? 0));
            else
                query = query.Where(x => x.m.MemberActiveStatus == false && new[] { 2, 3, 4, 5, 6, 10 }.Contains(x.m.MemberStatus ?? 0));

            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(x =>
                    EF.Functions.Like(x.m.MemberBusinessName, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MemberName, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MemberMobilenumber, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MembershipNumberPrefix + x.m.MembershipNumber, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MemberOldMembership ?? "", $"%{searchText}%"));

            if (districtId.HasValue)
                query = query.Where(x => x.m.MemberDistrictId == districtId.Value);
            if (unitId.HasValue)
                query = query.Where(x => x.m.MemberUnitId == unitId.Value);

            var total = await query.CountAsync();

            var records = await query
                .OrderBy(x => x.m.MemberId)
                .Skip((page - 1) * limit).Take(limit)
                .Select(x => (object)new
                {
                    x.m.MemberId,
                    x.d.DistrictName,
                    x.unit.UnitName,
                    UserType = x.g.GroupLevel,
                    x.m.MemberBusinessName,
                    x.m.MemberName,
                    x.m.MemberMobilenumber,
                    x.m.MemberStatus,
                    MembershipNumber = x.m.MembershipNumberPrefix + x.m.MembershipNumber
                })
                .ToListAsync();

            return new PaginatedResult<object> { Records = records, Total = total };
        }

        public async Task<PaginatedResult<object>> GetAllDistrictWiseMembersAsync(
            int active, int districtId, int page, int limit, string? searchText)
        {
            var query = from m in _context.KaruthalMembers
                        join u in _context.Users on m.MemberUserId equals u.UserId
                        join d in _context.Districts on m.MemberDistrictId equals d.DistrictId
                        join unit in _context.Units on m.MemberUnitId equals unit.UnitId
                        join g in _context.MemberGroups on m.MemberGroupId equals g.GroupId
                        where g.GroupId == u.UserGroupId
                        where m.MemberActiveStatus == (active == 1)
                        where m.MemberDistrictId == districtId
                        select new { m, d, unit, g };

            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(x =>
                    EF.Functions.Like(x.m.MemberBusinessName, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MemberName, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MemberMobilenumber, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MembershipNumberPrefix + x.m.MembershipNumber, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MemberOldMembership ?? "", $"%{searchText}%"));

            if (active == 0)
                query = query.Where(x => x.m.MemberStatus == 7);

            var total = await query.CountAsync();

            var records = await query
                .OrderBy(x => x.m.MemberId)
                .Skip((page - 1) * limit).Take(limit)
                .Select(x => (object)new
                {
                    x.m.MemberId,
                    x.d.DistrictName,
                    x.unit.UnitName,
                    UserType = x.g.GroupLevel,
                    x.m.MemberBusinessName,
                    x.m.MemberName,
                    x.m.MemberMobilenumber,
                    x.m.MemberStatus,
                    MembershipNumber = x.m.MembershipNumberPrefix + x.m.MembershipNumber
                })
                .ToListAsync();

            return new PaginatedResult<object> { Records = records, Total = total };
        }

        public async Task<PaginatedResult<object>> GetAllUnitWiseMembersAsync(
            int active, int unitId, int page, int limit, string? searchText)
        {
            var query = from m in _context.KaruthalMembers
                        join u in _context.Users on m.MemberUserId equals u.UserId
                        join d in _context.Districts on m.MemberDistrictId equals d.DistrictId
                        join unit in _context.Units on m.MemberUnitId equals unit.UnitId
                        join g in _context.MemberGroups on m.MemberGroupId equals g.GroupId
                        where g.GroupId == u.UserGroupId
                        where m.MemberActiveStatus == (active == 1)
                        where m.MemberUnitId == unitId
                        select new { m, d, unit, g };

            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(x =>
                    EF.Functions.Like(x.m.MemberBusinessName, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MemberName, $"%{searchText}%"));

            if (active == 0)
                query = query.Where(x => new[] { 4, 5 }.Contains(x.m.MemberStatus ?? 0));

            var total = await query.CountAsync();

            var records = await query
                .OrderBy(x => x.m.MemberId)
                .Skip((page - 1) * limit).Take(limit)
                .Select(x => (object)new
                {
                    x.m.MemberId,
                    x.d.DistrictName,
                    x.unit.UnitName,
                    UserType = x.g.GroupLevel,
                    x.m.MemberBusinessName,
                    x.m.MemberName,
                    x.m.MemberMobilenumber,
                    x.m.MemberStatus,
                    MembershipNumber = x.m.MembershipNumberPrefix + x.m.MembershipNumber
                })
                .ToListAsync();

            return new PaginatedResult<object> { Records = records, Total = total };
        }

        public async Task<object?> GetMemberDetailsAsync(int memberId)
        {
            var member = await _context.KaruthalMembers.FindAsync(memberId);
            if (member == null) return null;

            var memberGroupId = member.MemberGroupId;

            var memberData = await (
                from m in _context.KaruthalMembers
                join n in _context.Nominees on m.MemberId equals n.NomineeMemberId into nomineeGroup
                from n in nomineeGroup.DefaultIfEmpty()
                join u in _context.Users on m.MemberUserId equals u.UserId
                join d in _context.Districts on m.MemberDistrictId equals d.DistrictId
                join unit in _context.Units on m.MemberUnitId equals unit.UnitId
                where m.MemberId == memberId
                select new { m, n, u, d, unit }
            ).FirstOrDefaultAsync();

            if (memberData == null) return null;

            if (memberGroupId == 4)
            {
                return new
                {
                    memberData = new
                    {
                        memberData.m.MemberId,
                        membername = memberData.m.MemberName,
                        memberData.m.MemberAddress,
                        memberData.m.MemberEmail,
                        memberData.m.MemberMobilenumber,
                        memberData.m.MemberIdProofNumber,
                        memberData.m.MemberBankAcName,
                        memberData.m.MemberBankName,
                        memberData.m.MemberBankAcNumber,
                        memberData.m.MemberBankBranch,
                        memberData.m.MemberIfsc,
                        memberImage = memberData.u.UserImageUrl,
                        memberData.m.MemberIdUrl1,
                        memberData.m.MemberIdUrl2,
                        nomineeName = memberData.n != null ? memberData.n.NomineeName : null,
                        nomineeAddress = memberData.n != null ? memberData.n.NomineeAddress : null,
                        nomineeMobilenumber = memberData.n != null ? memberData.n.NomineeMobilenumber : null,
                        nomineeEmail = memberData.n != null ? memberData.n.NomineeEmail : null,
                        relation = memberData.n != null ? memberData.n.NomineeRelation : null,
                        nomineeIdProofNumber = memberData.n != null ? memberData.n.NomineeIdProofNumber : null,
                        nomineeBankAcName = memberData.n != null ? memberData.n.NomineeBankAcName : null,
                        nomineeBankName = memberData.n != null ? memberData.n.NomineeBankName : null,
                        nomineeBankAcNumber = memberData.n != null ? memberData.n.NomineeBankAcNumber : null,
                        nomineeBankBranch = memberData.n != null ? memberData.n.NomineeBankBranch : null,
                        nomineeIfsc = memberData.n != null ? memberData.n.NomineeIfsc : null,
                        nomineeIdUrl1 = memberData.n != null ? memberData.n.NomineeIdUrl1 : null,
                        nomineeIdUrl2 = memberData.n != null ? memberData.n.NomineeIdUrl2 : null
                    },
                    membersubData = new { },
                    businessDetail = new
                    {
                        memberData.m.MemberBusinessName,
                        memberData.m.MemberBusinessAddress,
                        memberdistrictName = memberData.d.DistrictName,
                        memberData.m.MemberBusinessFSSAIno,
                        memberData.m.MemberBusinessCmpyType,
                        memberunitName = memberData.unit.UnitName
                    }
                };
            }
            else
            {
                object? membersubData = null;
                object? businessDetail = new
                {
                    memberData.m.MemberBusinessName,
                    memberData.m.MemberBusinessAddress,
                    memberdistrictName = memberData.d.DistrictName,
                    memberunitName = memberData.unit.UnitName
                };

                if (memberData.m.MemberParentId.HasValue)
                {
                    var parent = await (
                        from m in _context.KaruthalMembers
                        join n in _context.Nominees on m.MemberId equals n.NomineeMemberId into nomineeGroup
                        from n in nomineeGroup.DefaultIfEmpty()
                        join u in _context.Users on m.MemberUserId equals u.UserId
                        join d in _context.Districts on m.MemberDistrictId equals d.DistrictId
                        join unit in _context.Units on m.MemberUnitId equals unit.UnitId
                        where m.MemberId == memberData.m.MemberParentId.Value
                        select new { m, n, u, d, unit }
                    ).FirstOrDefaultAsync();

                    if (parent != null)
                    {
                        membersubData = new
                        {
                            parent.m.MemberId,
                            membername = parent.m.MemberName,
                            parent.m.MemberMobilenumber,
                            parent.m.MemberAddress,
                            parent.m.MemberEmail,
                            relation = parent.n != null ? parent.n.NomineeRelation : null,
                            parent.m.MemberIdProofNumber,
                            memberDob = parent.m.MemberDob,
                            age = parent.m.MemberAge,
                            parent.m.MemberBankAcName,
                            parent.m.MemberBankName,
                            parent.m.MemberBankAcNumber,
                            parent.m.MemberBankBranch,
                            parent.m.MemberIfsc,
                            memberImage = parent.u.UserImageUrl,
                            parent.m.MemberIdUrl1,
                            parent.m.MemberIdUrl2
                        };

                        businessDetail = new
                        {
                            parent.m.MemberBusinessName,
                            parent.m.MemberBusinessAddress,
                            memberdistrictName = parent.d.DistrictName,
                            memberunitName = parent.unit.UnitName
                        };
                    }
                }

                return new
                {
                    memberData = new
                    {
                        memberData.m.MemberId,
                        membername = memberData.m.MemberName,
                        memberData.m.MemberAddress,
                        memberData.m.MemberEmail,
                        memberData.m.MemberMobilenumber,
                        memberData.m.MemberIdProofNumber,
                        memberData.m.MemberBankAcName,
                        memberData.m.MemberBankName,
                        memberData.m.MemberBankAcNumber,
                        memberData.m.MemberBankBranch,
                        memberData.m.MemberIfsc,
                        memberImage = memberData.u.UserImageUrl,
                        memberData.m.MemberIdUrl1,
                        memberData.m.MemberIdUrl2,
                        nomineeName = memberData.n != null ? memberData.n.NomineeName : null,
                        nomineeAddress = memberData.n != null ? memberData.n.NomineeAddress : null,
                        nomineeMobilenumber = memberData.n != null ? memberData.n.NomineeMobilenumber : null,
                        nomineeEmail = memberData.n != null ? memberData.n.NomineeEmail : null,
                        relation = memberData.n != null ? memberData.n.NomineeRelation : null,
                        nomineeIdProofNumber = memberData.n != null ? memberData.n.NomineeIdProofNumber : null,
                        nomineeBankAcName = memberData.n != null ? memberData.n.NomineeBankAcName : null,
                        nomineeBankName = memberData.n != null ? memberData.n.NomineeBankName : null,
                        nomineeBankAcNumber = memberData.n != null ? memberData.n.NomineeBankAcNumber : null,
                        nomineeBankBranch = memberData.n != null ? memberData.n.NomineeBankBranch : null,
                        nomineeIfsc = memberData.n != null ? memberData.n.NomineeIfsc : null,
                        nomineeIdUrl1 = memberData.n != null ? memberData.n.NomineeIdUrl1 : null,
                        nomineeIdUrl2 = memberData.n != null ? memberData.n.NomineeIdUrl2 : null
                    },
                    membersubData = membersubData ?? new { },
                    businessDetail
                };
            }
        }

        public async Task<object> UpdateMemberStatusAsync(int userId, string memberStatus, string? memberReviseRemarks)
        {
            var member = await _context.KaruthalMembers.FindAsync(userId);
            if (member == null)
                return new { status = "failure", message = "No rows were updated" };

            member.MemberStatus = int.Parse(memberStatus);
            member.MemberReviseRemarks = memberReviseRemarks;

            if (memberStatus == "9")
                member.MemberActiveStatus = true;

            await _context.SaveChangesAsync();
            return new { status = "success", message = "Member status updated successfully" };
        }

        //public async Task<object?> GetMemberOutstandingAsync(int userId)
        //{
        //    var memberId = await _context.KaruthalMembers
        //        .Where(m => m.MemberUserId == userId)
        //        .Select(m => m.MemberId)
        //        .FirstOrDefaultAsync();

        //    if (memberId == 0) throw new Exception("No memberId found for the given userId");

        //    var outstanding = await (
        //        from parent in _context.KaruthalMembers
        //        join child in _context.KaruthalMembers on parent.MemberId equals child.MemberParentId
        //        from c in _context.Contributions
        //        where parent.MemberId == memberId
        //        where c.ContributionMemberId != child.MemberId
        //        where !_context.MemberContributions.Any(mc =>
        //            mc.ContributionId == c.ContributionId && mc.MemberId == child.MemberId)
        //        select c.ContributionAmount
        //    ).SumAsync();

        //    return new { OutstandingAmount = outstanding };
        //}
        public async Task<object?> GetMemberOutstandingAsync(int userId)
        {
            // Try KaruthalMembers first
            var memberId = await _context.KaruthalMembers
                .Where(m => m.MemberUserId == userId)
                .Select(m => m.MemberId)
                .FirstOrDefaultAsync();

            // If not found, try Members table
            if (memberId == 0)
            {
                memberId = await _context.Members
                    .Where(m => m.MemberUserId == userId)
                    .Select(m => m.MemberId)
                    .FirstOrDefaultAsync();
            }

            // If still not found, throw exception with clear message
            if (memberId == 0)
            {
                throw new Exception($"No member found for userId: {userId} in either Members or KaruthalMembers");
            }

            // Calculate outstanding amount
            var outstanding = await (
                from parent in _context.KaruthalMembers
                join child in _context.KaruthalMembers on parent.MemberId equals child.MemberParentId
                from c in _context.Contributions
                where parent.MemberId == memberId
                where c.ContributionMemberId != child.MemberId
                where !_context.MemberContributions.Any(mc =>
                    mc.ContributionId == c.ContributionId && mc.MemberId == child.MemberId)
                select c.ContributionAmount
            ).SumAsync();

            return new { OutstandingAmount = outstanding };
        }
        public async Task<object?> GetPendingApproveDetailsAsync(int userId)
        {
            var memberId = await _context.KaruthalMembers
                .Where(m => m.MemberUserId == userId)
                .Select(m => m.MemberId)
                .FirstOrDefaultAsync();

            var pendingMembers = await GetMembersByStatusAndParentId(memberId, new[] { 6 });
            var approvedMembers = await GetMembersByStatusAndParentId(memberId, new[] { 7, 8, 9 });

            return new
            {
                status = "success",
                data = new[]
                {
                    new { pending = pendingMembers, approve = approvedMembers }
                }
            };
        }

        private async Task<List<object>> GetMembersByStatusAndParentId(int parentMemberId, int[] statuses)
        {
            return await _context.KaruthalMembers
                .Where(m => statuses.Contains(m.MemberStatus ?? 0) && m.MemberParentId == parentMemberId)
                .Join(_context.MemberGroups, m => m.MemberGroupId, g => g.GroupId, (m, g) => new
                {
                    m.MemberId,
                    m.MemberName,
                    g.GroupLevel
                })
                .Select(x => (object)new
                {
                    x.MemberId,
                    x.MemberName,
                    x.GroupLevel
                })
                .ToListAsync();
        }

        public async Task<object> ChildMemberApproveAsync(int userId, string memberStatus, bool memberAction)
        {
            var member = await _context.KaruthalMembers.FindAsync(userId);
            if (member == null) return new { success = false };

            member.MemberStatus = int.Parse(memberStatus);
            await _context.SaveChangesAsync();

            return new { success = true };
        }

        public async Task<PaginatedResult<object>> GetMemberStatusDetailsAsync(int status, int page, int limit, string? searchText)
        {
            var query = from m in _context.KaruthalMembers
                        join u in _context.Users on m.MemberUserId equals u.UserId
                        join d in _context.Districts on m.MemberDistrictId equals d.DistrictId
                        join unit in _context.Units on m.MemberUnitId equals unit.UnitId
                        join g in _context.MemberGroups on m.MemberGroupId equals g.GroupId
                        where g.GroupId == u.UserGroupId
                        select new { m, d, unit, g, u };

            if (status == 2)
                query = query.Where(x => !x.m.MemberActiveStatus && x.m.MemberStatus == 2);
            else if (status == 3)
                query = query.Where(x => !x.m.MemberActiveStatus && x.m.MemberStatus == 3);
            else if (status == 4)
                query = query.Where(x => !x.m.MemberActiveStatus && x.m.MemberStatus == 4);
            else if (status == 5)
                query = query.Where(x => !x.m.MemberActiveStatus && x.m.MemberStatus == 5);
            else if (status == 6)
                query = query.Where(x => !x.m.MemberActiveStatus && x.m.MemberStatus == 6);

            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(x =>
                    EF.Functions.Like(x.m.MemberBusinessName, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MemberName, $"%{searchText}%"));

            var total = await query.CountAsync();

            var records = await query
                .OrderBy(x => x.m.MemberId)
                .Skip((page - 1) * limit).Take(limit)
                .Select(x => (object)new
                {
                    x.m.MemberId,
                    x.d.DistrictName,
                    x.unit.UnitName,
                    UserType = x.g.GroupLevel,
                    x.m.MemberBusinessName,
                    x.m.MemberName,
                    x.m.MemberMobilenumber,
                    x.m.MemberStatus,
                    MembershipNumber = x.m.MembershipNumberPrefix + x.m.MembershipNumber,
                    x.m.MemberUserId,
                    memberActive = x.u.UserStatus
                })
                .ToListAsync();

            return new PaginatedResult<object> { Records = records, Total = total };
        }

        // ─── Karuthal Methods ─────────────────────────────────

        public async Task<object?> GetOwnerDetailsAsync(string? prefix, string? number, string? suffix)
        {
            var query = from m in _context.KaruthalMembers
                        join d in _context.Districts on m.MemberDistrictId equals d.DistrictId
                        join unit in _context.Units on m.MemberUnitId equals unit.UnitId
                        join n in _context.Nominees on m.MemberId equals n.NomineeMemberId
                        join u in _context.Users on m.MemberUserId equals u.UserId
                        where d.DistrictId == unit.UnitDistrictId
                        select new { m, d, unit, n, u };

            if (!string.IsNullOrEmpty(prefix))
                query = query.Where(x => x.m.MembershipNumberPrefix == prefix
                                      && x.m.MembershipNumber == number
                                      && x.m.MembershipNumberSuffix == suffix);
            else
                query = query.Where(x => x.m.MemberMobilenumber == number);

            var result = await query.Select(x => new
            {
                x.m.MemberUserId,
                x.m.MemberId,
                x.u.UserImageUrl,
                x.m.MemberName,
                x.m.MemberMobilenumber,
                x.m.MemberAddress,
                x.m.MemberEmail,
                x.m.MemberIdProofNumber,
                AccountName = x.m.MemberBankAcName,
                BankName = x.m.MemberBankName,
                x.m.MemberBankAcNumber,
                x.m.MemberBankBranch,
                x.m.MemberIfsc,
                x.m.MemberBusinessName,
                x.m.MemberBusinessAddress,
                x.m.MemberBusinessDetails,
                x.m.MemberIdUrl1,
                x.m.MemberIdUrl2,
                x.m.MemberGstCertificateUrl,
                x.m.MemberPartnershipDeedUrl,
                x.unit.UnitName,
                x.unit.UnitId,
                x.m.MemberBusinessFSSAIno,
                x.d.DistrictName,
                x.d.DistrictId,
                State = "KERALA",
                x.m.MemberBusinessCmpyType,
                x.n.NomineeName,
                x.n.NomineeMobilenumber,
                x.n.NomineeAddress,
                x.n.NomineeEmail,
                x.n.NomineeRelation,
                x.n.NomineeIdProofNumber,
                x.n.NomineeBankAcName,
                x.n.NomineeBankAcNumber,
                x.n.NomineeBankBranch,
                x.n.NomineeIfsc,
                x.n.NomineeIdUrl1,
                x.n.NomineeIdUrl2,
                x.n.NomineeBankName,
                x.m.MemberDob,
                x.m.MemberIdProof,
                x.n.NomineeIdProof
            }).ToListAsync();

            return result;
        }

        public async Task<PaginatedResult<object>> GetAllStateKaruthalMemberAsync(
            int active, string? pending, int page, int limit, string? searchText, int? districtId, int? unitId)
        {
            var query = from m in _context.KaruthalMembers
                        join u in _context.Users on m.MemberUserId equals u.UserId
                        join d in _context.Districts on m.MemberDistrictId equals d.DistrictId
                        join unit in _context.Units on m.MemberUnitId equals unit.UnitId
                        join g in _context.MemberGroups on m.MemberGroupId equals g.GroupId
                        where g.GroupId == u.UserGroupId
                        select new { m, d, unit, g };

            if (active == 1)
                query = query.Where(x => x.m.MemberActiveStatus == true && new[] { 9, 11 }.Contains(x.m.MemberStatus ?? 0));
            else if (active == 2)
                query = query.Where(x => x.m.MembershipDate.AddYears(1) < DateTime.Now);
            else
                query = query.Where(x => x.m.MemberActiveStatus == false && new[] { 2, 3, 4, 5, 6, 10 }.Contains(x.m.MemberStatus ?? 0));

            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(x =>
                    EF.Functions.Like(x.m.MemberBusinessName, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MemberName, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MemberMobilenumber, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MembershipNumberPrefix + x.m.MembershipNumber, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MemberOldMembership ?? "", $"%{searchText}%"));

            if (districtId.HasValue)
                query = query.Where(x => x.m.MemberDistrictId == districtId.Value);
            if (unitId.HasValue)
                query = query.Where(x => x.m.MemberUnitId == unitId.Value);

            var total = await query.CountAsync();

            var records = await query
                .OrderBy(x => x.m.MemberId)
                .Skip((page - 1) * limit).Take(limit)
                .Select(x => (object)new
                {
                    x.m.MemberId,
                    x.d.DistrictName,
                    x.unit.UnitName,
                    UserType = x.g.GroupLevel,
                    x.m.MemberBusinessName,
                    x.m.MemberName,
                    x.m.MemberMobilenumber,
                    x.m.MemberKaruthalWallet,
                    x.m.MemberStatus,
                    MembershipNumber = x.m.MembershipNumberPrefix + x.m.MembershipNumber + x.m.MembershipNumberSuffix
                })
                .ToListAsync();

            return new PaginatedResult<object> { Records = records, Total = total };
        }

        public async Task<PaginatedResult<object>> GetAllDistrictKaruthalMemberAsync(
            int active, int districtId, int page, int limit, string? searchText)
        {
            var query = from m in _context.KaruthalMembers
                        join u in _context.Users on m.MemberUserId equals u.UserId
                        join d in _context.Districts on m.MemberDistrictId equals d.DistrictId
                        join unit in _context.Units on m.MemberUnitId equals unit.UnitId
                        join g in _context.MemberGroups on m.MemberGroupId equals g.GroupId
                        where g.GroupId == u.UserGroupId
                        where m.MemberActiveStatus == true
                        where m.MemberDistrictId == districtId
                        select new { m, d, unit, g, u };

            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(x =>
                    EF.Functions.Like(x.m.MemberBusinessName, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MemberName, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MemberMobilenumber, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MembershipNumberPrefix + x.m.MembershipNumber, $"%{searchText}%") ||
                    EF.Functions.Like(x.m.MemberOldMembership ?? "", $"%{searchText}%"));

            if (active == 0)
                query = query.Where(x => x.m.MemberStatus == 7);

            var total = await query.CountAsync();

            var records = await query
                .OrderBy(x => x.m.MemberId)
                .Skip((page - 1) * limit).Take(limit)
                .Select(x => (object)new
                {
                    x.m.MemberId,
                    x.d.DistrictName,
                    x.unit.UnitName,
                    UserType = x.g.GroupLevel,
                    x.m.MemberBusinessName,
                    x.m.MemberName,
                    x.m.MemberMobilenumber,
                    x.m.MemberStatus,
                    MembershipNumber = x.m.MembershipNumberPrefix + x.m.MembershipNumber + x.m.MembershipNumberSuffix,
                    x.m.MemberUserId,
                    memberActive = x.u.UserStatus
                })
                .ToListAsync();

            return new PaginatedResult<object> { Records = records, Total = total };
        }



        public async Task<List<object>?> MemberAccountDetailsAsync(int userId)
        {
            try
            {
                var query = from m in _context.KaruthalMembers
                            join u in _context.Users on m.MemberUserId equals u.UserId
                            join d in _context.Districts on m.MemberDistrictId equals d.DistrictId into districtGroup
                            from d in districtGroup.DefaultIfEmpty()
                            join unit in _context.Units on m.MemberUnitId equals unit.UnitId into unitGroup
                            from unit in unitGroup.DefaultIfEmpty()
                            join g in _context.MemberGroups on m.MemberGroupId equals g.GroupId into memberGroupJoin
                            from g in memberGroupJoin.DefaultIfEmpty()
                            join n in _context.Nominees on m.MemberId equals n.NomineeMemberId into nomineeGroup
                            from n in nomineeGroup.DefaultIfEmpty()
                            where m.MemberUserId == userId
                            select new
                            {
                                m.MemberId,
                                m.MemberUserId,
                                DistrictName = d != null ? d.DistrictName : "",
                                UnitName = unit != null ? unit.UnitName : "",
                                UserType = g != null ? g.GroupLevel : "",
                                MemberName = m.MemberName ?? "",
                                MemberMobilenumber = m.MemberMobilenumber ?? "",
                                MemberEmail = m.MemberEmail ?? "",
                                MemberStatus = m.MemberStatus ?? 0,
                                MemberAddress = m.MemberAddress ?? "",
                                MemberBusinessName = m.MemberBusinessName ?? "",
                                MemberBusinessAddress = m.MemberBusinessAddress ?? "",
                                MemberBusinessDetails = m.MemberBusinessDetails ?? "",
                                MemberBusinessFSSAIno = m.MemberBusinessFSSAIno ?? "",
                                MemberBusinessCmpyType = m.MemberBusinessCmpyType ?? "",
                                MemberAge = m.MemberAge ?? 0,
                                GroupLevel = g != null ? g.GroupLevel : "",
                                MembershipNumber = (m.MembershipNumberPrefix ?? "") + (m.MembershipNumber ?? ""),
                                NomineeName = n != null ? n.NomineeName : "",
                                NomineeRelation = n != null ? n.NomineeRelation : "",
                                NomineeMobilenumber = n != null ? n.NomineeMobilenumber : "",
                                NomineeEmail = n != null ? n.NomineeEmail : "",
                                MemberKaruthalWallet = m.MemberKaruthalWallet ?? 0,
                                MemberDistrictWallet = m.MemberDistrictWallet ?? 0,
                                MemberUnitWallet = m.MemberUnitWallet ?? 0,
                                MemberStateWallet = m.MemberStateWallet ?? 0
                            };

                var result = await query.FirstOrDefaultAsync();

                if (result != null)
                {
                    return new List<object> { result };
                }

                return new List<object> {
            new { error = "Member not found" }
        };
            }
            catch (Exception ex)
            {
                return new List<object> {
            new { error = ex.Message }
        };
            }
        }



        public async Task<object?> UpdateMemberFullDetailsAsync(UpdateMemberFullDetailsRequest data)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var member = await _context.KaruthalMembers.FindAsync(data.MemberId);
                if (member == null)
                    return new { success = false, message = "Member not found" };

            
                if (member.MemberMobilenumber != data.MemberMobilenumber)
                {
                    var phoneExists = await _context.Users.AnyAsync(u => u.UserId != member.MemberUserId && u.UserName == data.MemberMobilenumber)
                                    || await _context.KaruthalMembers.AnyAsync(m => m.MemberId != data.MemberId && m.MemberMobilenumber == data.MemberMobilenumber);

                    if (phoneExists)
                        return new { success = false, message = "Mobile number already exists" };
                }

 
                var user = await _context.Users.FindAsync(member.MemberUserId);
                if (user != null)
                {
                    user.UserName = data.MemberMobilenumber;
                    user.UserStatus = data.MemberActiveStatus;
                    user.UserImageUrl = data.UserImageUrl;
                }

       
                member.MemberName = data.MemberName ?? "";
                member.MemberAddress = data.MemberAddress ?? "";
                member.MemberEmail = data.MemberEmail ?? "";
                member.MemberMobilenumber = data.MemberMobilenumber ?? "";
                if (data.MemberDob.HasValue) member.MemberDob = data.MemberDob.Value;
                member.MemberAge = data.MemberAge;
                member.MemberIdProof = data.MemberIdProof;
                member.MemberIdProofNumber = data.MemberIdProofNumber;
                member.MemberBankName = data.MemberBankName ?? "";
                member.MemberBankAcName = data.MemberBankAcName ?? "";
                member.MemberBankAcNumber = data.MemberBankAcNumber ?? "";
                member.MemberBankBranch = data.MemberBankBranch ?? "";
                member.MemberIfsc = data.MemberIfsc ?? "";
                member.MemberIdUrl1 = data.MemberIdUrl1;
                member.MemberIdUrl2 = data.MemberIdUrl2;
                member.MemberBusinessName = data.MemberBusinessName ?? "";
                member.MemberBusinessAddress = data.MemberBusinessAddress ?? "";
                member.MemberBusinessDetails = data.MemberBusinessDetails;
                member.MemberBusinessFSSAIno = data.MemberBusinessFSSAIno;
                member.MemberBusinessCmpyType = data.MemberBusinessCmpyType;
                member.MemberActiveStatus = data.MemberActiveStatus;

               
                var nominee = await _context.Nominees.FirstOrDefaultAsync(n => n.NomineeMemberId == data.MemberId);
                if (nominee != null)
                {
                    nominee.NomineeName = data.NomineeName;
                    nominee.NomineeAddress = data.NomineeAddress;
                    nominee.NomineeEmail = data.NomineeEmail;
                    nominee.NomineeMobilenumber = data.NomineeMobilenumber;
                    nominee.NomineeIdProof = data.NomineeIdProof;
                    nominee.NomineeIdProofNumber = data.NomineeIdProofNumber;
                    nominee.NomineeBankName = data.NomineeBankName;
                    nominee.NomineeBankAcName = data.NomineeBankAcName;
                    nominee.NomineeBankAcNumber = data.NomineeBankAcNumber;
                    nominee.NomineeBankBranch = data.NomineeBankBranch;
                    nominee.NomineeIfsc = data.NomineeIfsc;
                    nominee.NomineeIdUrl1 = data.NomineeIdUrl1;
                    nominee.NomineeIdUrl2 = data.NomineeIdUrl2;
                    nominee.NomineeRelation = data.NomineeRelation;
                    nominee.NomineeStatus = data.NomineeStatus ?? 0;
                }
                else
                {
                    _context.Nominees.Add(new Nominee
                    {
                        NomineeMemberId = data.MemberId,
                        NomineeName = data.NomineeName,
                        NomineeAddress = data.NomineeAddress,
                        NomineeEmail = data.NomineeEmail,
                        NomineeMobilenumber = data.NomineeMobilenumber,
                        NomineeIdProof = data.NomineeIdProof,
                        NomineeIdProofNumber = data.NomineeIdProofNumber,
                        NomineeBankName = data.NomineeBankName,
                        NomineeBankAcName = data.NomineeBankAcName,
                        NomineeBankAcNumber = data.NomineeBankAcNumber,
                        NomineeBankBranch = data.NomineeBankBranch,
                        NomineeIfsc = data.NomineeIfsc,
                        NomineeIdUrl1 = data.NomineeIdUrl1,
                        NomineeIdUrl2 = data.NomineeIdUrl2,
                        NomineeRelation = data.NomineeRelation,
                        NomineeStatus = data.NomineeStatus ?? 0
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new { success = true, message = "Member account details updated successfully" };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}