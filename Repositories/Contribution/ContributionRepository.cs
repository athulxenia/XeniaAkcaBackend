using Microsoft.EntityFrameworkCore;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using System.Text.RegularExpressions;
using XeniaAkcaBackend.Models;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Repositories;


namespace XeniaAkcaBackend.Repositories
{
    public class ContributionRepository : IContributionRepository
    {
        private readonly ApplicationDbContext _context;

        public ContributionRepository(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
        }

        private static string FormatDateToIst(DateTime utcDate)
        {
            var ist = utcDate.AddMinutes(330);
            return ist.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        private static string FormatMembershipNumber(string partialName)
        {
            if (partialName.Length == 10)
                return $"{partialName[..2]}/{partialName[2..4]}/{partialName[4..6]}/{partialName[6..]}";
            return partialName;
        }

        public async Task<ContributionResponse> CreateContributionAsync(CreateContributionRequest request)
        {
            var now = DateTime.UtcNow;
            var istNow = now.AddMinutes(330);

            var contribution = new Contribution
            {
                ContributionMemberId = request.ContributionMemberId,
                ContributionImgUrl = request.ContributionImgUrl,
                ContributionText = request.ContributionText,
                ContributionContent = request.ContributionContent,
                ContributionAmount = request.ContributionAmount,
                ContributionInitiatedDate = istNow,
                ContributionDueDate = istNow.AddDays(30),
                ContributionStatus = "Created",
                ActiveStatus = request.ActiveStatus == 1,
                ContributionApprovalStatus = request.ContributionApprovalStatus,
                ContributionType = request.ContributionType
            };

            await _context.Contributions.AddAsync(contribution);
            var result = await _context.SaveChangesAsync();

            return result > 0
                ? new ContributionResponse { Status = "success", Message = "Contribution created successfully." }
                : new ContributionResponse { Status = "fail", Message = "Contribution not created." };
        }

        //public async Task<List<object>> GetMembersByPartialNameAsync(string partialName)
        //{
        //    var isExactMobile = Regex.IsMatch(partialName, @"^\d{10}$");
        //    var formatted = isExactMobile ? partialName : $"%{FormatMembershipNumber(partialName)}%";

        //    IQueryable<Member> query = isExactMobile
        //        ? _context.Members.Where(m => m.MemberMobilenumber == formatted)
        //        : _context.Members.Where(m =>
        //            EF.Functions.Like(m.MemberName, formatted) ||
        //            EF.Functions.Like(m.MemberBusinessName, formatted) ||
        //            EF.Functions.Like(m.MembershipNumberPrefix + m.MembershipNumber, formatted) ||
        //            EF.Functions.Like(m.MemberMobilenumber, formatted));

        //    var rows = await query.Select(m => new
        //    {
        //        m.MemberId,
        //        m.MemberName,
        //        m.MemberBusinessName,
        //        MembershipNumber = m.MembershipNumberPrefix + m.MembershipNumber
        //    }).ToListAsync();

        //    return rows.Cast<object>().ToList();
        //}
        public async Task<List<object>> GetMembersByPartialNameAsync(string partialName)
        {
            bool isExactMobile = Regex.IsMatch(partialName, @"^\d{10}$");

            string formatted = isExactMobile
                ? partialName
                : $"%{FormatMembershipNumber(partialName)}%";

            IQueryable<XeniaAkcaBackend.Models.Member> query;

            if (isExactMobile)
            {
                query = _context.Members.Where(m => m.MemberMobilenumber == partialName);
            }
            else
            {
                query = _context.Members.Where(m =>
                    EF.Functions.Like(m.MemberName, formatted) ||
                    EF.Functions.Like(m.MemberBusinessName, formatted) ||
                    EF.Functions.Like(m.MembershipNumberPrefix + m.MembershipNumber, formatted) ||
                    EF.Functions.Like(m.MemberMobilenumber, formatted));
            }

            var rows = await query
                .Select(m => new
                {
                    m.MemberId,
                    m.MemberName,
                    m.MemberBusinessName,
                    MembershipNumber = m.MembershipNumberPrefix + m.MembershipNumber
                })
                .ToListAsync();

            return rows.Cast<object>().ToList();
        }

        public async Task<ContributionResponse> UpdateContributionAsync(int contributionId, UpdateContributionRequest request)
        {
            var contribution = await _context.Contributions.FindAsync(contributionId);
            if (contribution == null)
                return new ContributionResponse { Status = "fail", Message = "Contribution not found." };

            contribution.ContributionMemberId = request.ContributionMemberId;
            contribution.ContributionImgUrl = request.ContributionImgUrl;
            contribution.ContributionText = request.ContributionText;
            contribution.ContributionContent = request.ContributionContent;
            contribution.ContributionStatus = "Updated";

            await _context.SaveChangesAsync();
            return new ContributionResponse { Status = "success", Message = "Contribution updated successfully." };
        }

        public async Task<object?> GetContributionAsync(int contributionId)
        {
            return await _context.Contributions
                .Where(c => c.ContributionId == contributionId)
                .Join(_context.Members, c => c.ContributionMemberId, m => m.MemberId,
                    (c, m) => new
                    {
                        c.ContributionId,
                        c.ContributionText,
                        m.MemberName,
                        c.ContributionImgUrl,
                        c.ContributionContent,
                        m.MemberId
                    })
                .FirstOrDefaultAsync();
        }

        public async Task<PaginatedResult<object>> GetDistrictPendingContributionAsync(
            int page, int limit, string? searchText, int? districtId)
        {
            var query = from m in _context.Members
                        join mc in _context.MemberContributions on m.MemberId equals mc.MemberId
                        join c in _context.Contributions on mc.ContributionId equals c.ContributionId
                        where c.ContributionId != mc.ContributionId
                        where !_context.MemberContributions.Any(x => x.MemberId == c.ContributionMemberId)
                        select new { m, c };

            if (districtId.HasValue)
                query = query.Where(x => x.m.MemberDistrictId == districtId.Value);
            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(x => EF.Functions.Like(x.c.ContributionText, $"%{searchText}%"));

            var total = await query.Select(x => x.c.ContributionId).Distinct().CountAsync();
            var records = await query
                .Select(x => new { x.c.ContributionId, x.c.ContributionText, x.c.ContributionInitiatedDate })
                .Distinct()
                .OrderBy(x => x.ContributionId)
                .Skip((page - 1) * limit).Take(limit)
                .ToListAsync();

            return new PaginatedResult<object> { Records = records.Cast<object>().ToList(), Total = total };
        }

        public async Task<PaginatedResult<object>> GetDistrictApproveContributionAsync(
            int page, int limit, string? searchText, int? districtId)
        {
            var query = from m in _context.Members
                        join mc in _context.MemberContributions on m.MemberId equals mc.MemberId
                        join c in _context.Contributions on mc.ContributionId equals c.ContributionId
                        where c.ContributionId == mc.ContributionId
                        where _context.MemberContributions.Any(x => x.MemberId == c.ContributionMemberId && x.ContributionPaymentRef != null)
                        select new { m, c };

            if (districtId.HasValue)
                query = query.Where(x => x.m.MemberDistrictId == districtId.Value);
            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(x => EF.Functions.Like(x.c.ContributionText, $"%{searchText}%"));

            var total = await query.Select(x => x.c.ContributionId).Distinct().CountAsync();
            var records = await query
                .Select(x => new { x.c.ContributionId, x.c.ContributionText, x.c.ContributionInitiatedDate })
                .Distinct()
                .OrderBy(x => x.ContributionId)
                .Skip((page - 1) * limit).Take(limit)
                .ToListAsync();

            return new PaginatedResult<object> { Records = records.Cast<object>().ToList(), Total = total };
        }

        public async Task<PaginatedResult<object>> GetUnitPendingContributionAsync(
            int page, int limit, string? searchText, int? unitId)
        {
            var query = from m in _context.Members
                        join mc in _context.MemberContributions on m.MemberId equals mc.MemberId
                        join c in _context.Contributions on mc.ContributionId equals c.ContributionId
                        where c.ContributionId != mc.ContributionId
                        where !_context.MemberContributions.Any(x => x.MemberId == c.ContributionMemberId)
                        select new { m, c };

            if (unitId.HasValue)
                query = query.Where(x => x.m.MemberUnitId == unitId.Value);
            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(x => EF.Functions.Like(x.c.ContributionText, $"%{searchText}%"));

            var total = await query.Select(x => x.c.ContributionId).Distinct().CountAsync();
            var records = await query
                .Select(x => new { x.c.ContributionId, x.c.ContributionText, x.c.ContributionInitiatedDate })
                .Distinct()
                .OrderBy(x => x.ContributionId)
                .Skip((page - 1) * limit).Take(limit)
                .ToListAsync();

            return new PaginatedResult<object> { Records = records.Cast<object>().ToList(), Total = total };
        }

        public async Task<PaginatedResult<object>> GetUnitApproveContributionAsync(
            int page, int limit, string? searchText, int? unitId)
        {
            var query = from m in _context.Members
                        join mc in _context.MemberContributions on m.MemberId equals mc.MemberId
                        join c in _context.Contributions on mc.ContributionId equals c.ContributionId
                        where c.ContributionId == mc.ContributionId
                        where _context.MemberContributions.Any(x => x.MemberId == c.ContributionMemberId && x.ContributionPaymentRef != null)
                        select new { m, c };

            if (unitId.HasValue)
                query = query.Where(x => x.m.MemberUnitId == unitId.Value);
            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(x => EF.Functions.Like(x.c.ContributionText, $"%{searchText}%"));

            var total = await query.Select(x => x.c.ContributionId).Distinct().CountAsync();
            var records = await query
                .Select(x => new { x.c.ContributionId, x.c.ContributionText, x.c.ContributionInitiatedDate })
                .Distinct()
                .OrderBy(x => x.ContributionId)
                .Skip((page - 1) * limit).Take(limit)
                .ToListAsync();

            return new PaginatedResult<object> { Records = records.Cast<object>().ToList(), Total = total };
        }

        public async Task<PaginatedResult<object>> GetStatePendingContributionAsync(int page, int limit, string? searchText)
        {
            var query = _context.Contributions.Where(c => !c.ActiveStatus && c.ContributionApprovalStatus == 0);
            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(c => EF.Functions.Like(c.ContributionText, $"%{searchText}%"));

            var total = await query.CountAsync();
            var records = await query
                .OrderBy(c => c.ContributionId)
                .Skip((page - 1) * limit).Take(limit)
                .Select(c => (object)new { c.ContributionId, c.ContributionText, c.ContributionInitiatedDate })
                .ToListAsync();

            return new PaginatedResult<object> { Records = records, Total = total };
        }

        public async Task<PaginatedResult<object>> GetStateApproveContributionAsync(int page, int limit, string? searchText)
        {
            var query = _context.Contributions.Where(c => c.ActiveStatus && c.ContributionApprovalStatus == 1);
            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(c => EF.Functions.Like(c.ContributionText, $"%{searchText}%"));

            var total = await query.CountAsync();
            var records = await query
                .OrderBy(c => c.ContributionId)
                .Skip((page - 1) * limit).Take(limit)
                .Select(c => (object)new { c.ContributionId, c.ContributionText, c.ContributionInitiatedDate })
                .ToListAsync();

            return new PaginatedResult<object> { Records = records, Total = total };
        }

        public async Task<List<object>> ConPendingDetailsAsync(int userId)
        {
            var memberId = await _context.Members
                .Where(m => m.MemberUserId == userId)
                .Select(m => m.MemberId)
                .FirstOrDefaultAsync();

            if (memberId == 0)
                return new List<object>();

            var selfPending = await (
                from m in _context.Members
                from c in _context.Contributions
                where m.MemberId == memberId
                where c.ContributionMemberId != m.MemberId
                where !_context.MemberContributions.Any(mc =>
                    mc.ContributionId == c.ContributionId && mc.MemberId == m.MemberId)
                select new
                {
                    OwnerBusinessName = (string?)null,
                    OwnerName = (string?)null,
                    m.MemberName,
                    c.ContributionAmount,
                    c.ContributionId,
                    c.ContributionText,
                    c.ContributionContent,
                    c.ContributionImgUrl,
                    c.ContributionMemberId,
                    m.MemberUserId,
                    m.MemberParentId
                }
            ).ToListAsync();

            var childrenPending = await (
                from parent in _context.Members
                join child in _context.Members on parent.MemberId equals child.MemberParentId
                from c in _context.Contributions
                where parent.MemberId == memberId
                where c.ContributionMemberId != child.MemberId
                where !_context.MemberContributions.Any(mc =>
                    mc.ContributionId == c.ContributionId && mc.MemberId == child.MemberId)
                select new
                {
                    OwnerBusinessName = (string?)null,
                    OwnerName = (string?)null,
                    child.MemberName,
                    c.ContributionAmount,
                    c.ContributionId,
                    c.ContributionText,
                    c.ContributionContent,
                    c.ContributionImgUrl,
                    c.ContributionMemberId,
                    child.MemberUserId,
                    child.MemberParentId
                }
            ).ToListAsync();

            var allRows = selfPending.Concat(childrenPending).ToList();

            var memberInfo = await _context.Members
                .Where(m => m.MemberId == memberId)
                .Select(m => new { m.MemberDistrictId, m.MemberUnitId })
                .FirstOrDefaultAsync();

            var districtName = memberInfo != null
                ? await _context.Districts.Where(d => d.DistrictId == memberInfo.MemberDistrictId)
                    .Select(d => d.DistrictName).FirstOrDefaultAsync()
                : null;

            var unitName = memberInfo?.MemberUnitId != null
                ? await _context.Units.Where(u => u.UnitId == memberInfo.MemberUnitId)
                    .Select(u => u.UnitName).FirstOrDefaultAsync()
                : null;

            var enrichedRows = new List<dynamic>();
            foreach (var row in allRows)
            {
                var owner = await _context.Members.FindAsync(row.ContributionMemberId);
                enrichedRows.Add(new
                {
                    row.ContributionId,
                    row.ContributionText,
                    row.ContributionContent,
                    row.MemberName,
                    OwnerName = owner?.MemberName,
                    owner?.MemberBusinessName,
                    DistrictName = districtName,
                    UnitName = unitName,
                    row.ContributionAmount,
                    row.ContributionImgUrl,
                    ContributionDetail = $"{row.ContributionText} ({owner?.MemberName})",
                    ContributionPaymentRef = (string?)null,
                    PaidDate = (string?)null,
                    row.MemberUserId,
                    row.MemberParentId,
                    row.ContributionMemberId
                });
            }

            var grouped = enrichedRows
                .GroupBy(r => (int)r.ContributionId)
                .Select(g =>
                {
                    var first = g.First();
                    return (object)new
                    {
                        OwnerData = new
                        {
                            first.ContributionId,
                            first.ContributionText,
                            first.ContributionContent,
                            first.MemberName,
                            first.MemberBusinessName,
                            first.DistrictName,
                            first.UnitName,
                            first.ContributionAmount,
                            first.ContributionImgUrl,
                            first.ContributionDetail,
                            first.ContributionPaymentRef,
                            first.PaidDate
                        },
                        MemberSubData = g.Select(r => new
                        {
                            r.ContributionId,
                            r.MemberName,
                            r.ContributionAmount,
                            r.MemberUserId
                        }).ToList()
                    };
                }).ToList();

            return grouped;
        }
        public async Task<List<object>> ConPayedDetailsAsync(int userId)
        {
            var result = await (
                from u in _context.Users
                join m in _context.KaruthalMembers on u.UserId equals m.MemberUserId
                join mc in _context.MemberContributions on m.MemberId equals mc.MemberId
                join c in _context.Contributions on mc.ContributionId equals c.ContributionId
                join m2 in _context.KaruthalMembers on c.ContributionMemberId equals m2.MemberId
                join d in _context.Districts on mc.PaidDistrict equals d.DistrictId into districtGroup
                from d in districtGroup.DefaultIfEmpty()
                join ut in _context.Units on mc.PaidUnit equals ut.UnitId into unitGroup
                from ut in unitGroup.DefaultIfEmpty()
                where m.MemberUserId == userId
                where mc.PaymentStatus == "success"
                orderby mc.PaidDate descending
                select new
                {
                    ContributionId = c.ContributionId,
                    ContributionText = c.ContributionText,
                    ContributionContent = c.ContributionContent,
                    ContributionAmount = c.ContributionAmount,
                    ContributionImgUrl = c.ContributionImgUrl,
                    PaidDate = mc.PaidDate,
                    ContributionDetail = c.ContributionText + "(" + m2.MemberName + ")",
                    ContributionPaymentRef = mc.PaymentTxnRefNo,
                    PayMode = mc.PayMode,
                    PaymentStatus = mc.PaymentStatus,
                    MemberName = m2.MemberName,
                    MemberBusinessName = m2.MemberBusinessName,
                    DistrictName = d != null ? d.DistrictName : null,
                    UnitName = ut != null ? ut.UnitName : null
                }
            ).ToListAsync();

            return result.Cast<object>().ToList();
        }

        public async Task<ContributionResponse> ApproveContributionAsync(int contributionId, bool activeStatus)
        {
            var contribution = await _context.Contributions.FindAsync(contributionId);
            if (contribution == null)
                return new ContributionResponse { Status = "fail", Message = "Contribution not found." };

            contribution.ActiveStatus = activeStatus;
            contribution.ContributionApprovalStatus = activeStatus ? 1 : 10;
            contribution.ContributionStatus = activeStatus ? "Approved" : "Rejected";

            await _context.SaveChangesAsync();

            return new ContributionResponse
            {
                Status = "success",
                Message = activeStatus ? "Contribution Approved Successfully" : "Contribution Rejected Successfully"
            };
        }

        public async Task<object?> GetContributionDetailsAsync(int contributionId)
        {
            return await (
                from c in _context.Contributions
                join m in _context.Members on c.ContributionMemberId equals m.MemberId into memberGroup
                from m in memberGroup.DefaultIfEmpty()
                where c.ContributionId == contributionId
                select new
                {
                    c.ContributionId,
                    c.ContributionText,
                    MemberName = m != null ? m.MemberName : null,
                    c.ContributionImgUrl,
                    c.ContributionContent,
                    c.ContributionType,
                    c.ContributionAmount,
                    c.ContributionRemarks,
                    c.ContributionDueDate,
                    c.ContributionHandOveredTo,
                    c.ContributionChequeNo,
                    MemberId = m != null ? (int?)m.MemberId : null
                }
            ).FirstOrDefaultAsync();
        }

        public async Task<object> ContributionAmountNotificationAsync(int memberUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.MemberUserId == memberUserId)
                    ?? throw new Exception("Member not found.");

                decimal walletBalance = member.MemberKaruthalWallet ?? 0;
                decimal openingBalance = walletBalance;

                var paidIds = await _context.MemberContributions
                    .Where(mc => mc.MemberId == member.MemberId && mc.PaymentStatus == "success")
                    .Select(mc => mc.ContributionId)
                    .ToListAsync();

                var contributions = await _context.Contributions
                    .Where(c => c.ActiveStatus == true
                             && c.ContributionApprovalStatus == 1
                             && !paidIds.Contains(c.ContributionId))
                    .OrderBy(c => c.ContributionAmount)
                    .ToListAsync();

                if (!contributions.Any())
                    return new { Message = "No pending contributions found." };

                var paid = new List<object>();
                var unpaid = new List<object>();

                foreach (var contrib in contributions)
                {
                    if (walletBalance < contrib.ContributionAmount)
                    {
                        unpaid.Add(new
                        {
                            contrib.ContributionId,
                            contrib.ContributionText,
                            contrib.ContributionAmount
                        });
                        continue;
                    }

                    var now = FormatDateToIst(DateTime.UtcNow);

                    var exists = await _context.MemberContributions.AnyAsync(mc =>
                        mc.ContributionId == contrib.ContributionId &&
                        mc.MemberId == member.MemberId &&
                        mc.PaymentStatus == "success");

                    if (exists) continue;

                    _context.MemberContributions.Add(new MemberContribution
                    {
                        ContributionId = contrib.ContributionId,
                        MemberId = member.MemberId,
                        ContributionAmount = contrib.ContributionAmount,
                        PaidDate = DateTime.Now,
                        PaidBy = memberUserId,
                        PaidDistrict = member.MemberDistrictId,
                        PaidUnit = member.MemberUnitId ?? 0,
                        PayMode = "wallet",
                        PaymentStatus = "success",
                        IsCallbackStatus = 1
                    });

                    await _context.Database.ExecuteSqlRawAsync(@"
                        INSERT INTO AKCA_MemberWallet
                        (walletAmount, walletMemberId, walletDate, walletTransaction, walletPurpose, walletType)
                        VALUES ({0},{1},{2},'Dr',{3},9)",
                        contrib.ContributionAmount, member.MemberId, now, contrib.ContributionText);

                    walletBalance -= contrib.ContributionAmount;
                    paid.Add(new
                    {
                        contrib.ContributionId,
                        contrib.ContributionText,
                        contrib.ContributionAmount
                    });
                }

                member.MemberKaruthalWallet = walletBalance;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new
                {
                    Message = "Wallet auto-deduction completed successfully",
                    OpeningWalletBalance = openingBalance,
                    RemainingWalletBalance = walletBalance,
                    PaidContributions = paid,
                    UnpaidContributions = unpaid
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<object> ProcessAllContributionPaymentsAsync()
        {
            var allowedStatuses = new List<int?> { 5, 6, 9 };

            var members = await _context.Members
                .Where(m => allowedStatuses.Contains(m.MemberStatus)
                         && m.MemberKaruthalWallet > 0)
                .ToListAsync();

            var globalPaid = new List<object>();
            var globalUnpaid = new List<object>();

            foreach (var member in members)
            {
                var paidIds = await _context.MemberContributions
                    .Where(mc => mc.MemberId == member.MemberId && mc.PaymentStatus == "success")
                    .Select(mc => mc.ContributionId)
                    .ToListAsync();

                var contributions = await _context.Contributions
                    .Where(c => c.ActiveStatus == true
                             && c.ContributionApprovalStatus == 1
                             && c.ContributionMemberId != member.MemberId
                             && !paidIds.Contains(c.ContributionId))
                    .OrderBy(c => c.ContributionAmount)
                    .ToListAsync();

                decimal walletBalance = member.MemberKaruthalWallet ?? 0;

                foreach (var contrib in contributions)
                {
                    if (walletBalance < contrib.ContributionAmount)
                    {
                        globalUnpaid.Add(new
                        {
                            member.MemberId,
                            contrib.ContributionId,
                            contrib.ContributionText
                        });
                        continue;
                    }

                    using var tx = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var now = FormatDateToIst(DateTime.UtcNow);

                        var exists = await _context.MemberContributions.AnyAsync(mc =>
                            mc.ContributionId == contrib.ContributionId &&
                            mc.MemberId == member.MemberId &&
                            mc.PaymentStatus == "success");

                        if (exists)
                        {
                            await tx.RollbackAsync();
                            continue;
                        }

                        _context.MemberContributions.Add(new MemberContribution
                        {
                            ContributionId = contrib.ContributionId,
                            MemberId = member.MemberId,
                            ContributionAmount = contrib.ContributionAmount,
                            PaidDate = DateTime.Now,
                            PaidBy = member.MemberUserId,
                            PaidDistrict = member.MemberDistrictId,
                            PaidUnit = member.MemberUnitId ?? 0,
                            PayMode = "wallet",
                            PaymentStatus = "success",
                            IsCallbackStatus = 1,
                            PaymentTxnRefNo = ""
                        });

                        await _context.Database.ExecuteSqlRawAsync(@"
                            INSERT INTO AKCA_MemberWallet
                            (walletAmount, walletMemberId, walletDate, walletTransaction, walletPurpose, walletType)
                            VALUES ({0},{1},{2},'Dr',{3},9)",
                            contrib.ContributionAmount, member.MemberId, now, contrib.ContributionText);

                        member.MemberKaruthalWallet = (member.MemberKaruthalWallet ?? 0) - contrib.ContributionAmount;

                        await _context.SaveChangesAsync();
                        await tx.CommitAsync();

                        walletBalance -= contrib.ContributionAmount;
                        globalPaid.Add(new
                        {
                            member.MemberId,
                            contrib.ContributionId,
                            contrib.ContributionText
                        });
                    }
                    catch
                    {
                        await tx.RollbackAsync();
                        throw;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return new
            {
                Message = "All member contributions processed.",
                PaidContributions = globalPaid,
                UnpaidContributions = globalUnpaid
            };
        }

        public async Task<object?> DetailsOfContributionAsync(int contributionId)
        {
            return await (
                from c in _context.Contributions
                join m in _context.Members on c.ContributionMemberId equals m.MemberId into memberGroup
                from m in memberGroup.DefaultIfEmpty()
                join d in _context.Districts on m != null ? m.MemberDistrictId : (int?)null equals d.DistrictId into districtGroup
                from d in districtGroup.DefaultIfEmpty()
                join u in _context.Units on m != null ? m.MemberUnitId : null equals u.UnitId into unitGroup
                from u in unitGroup.DefaultIfEmpty()
                where c.ContributionId == contributionId
                select new
                {
                    c.ContributionId,
                    c.ContributionText,
                    c.ContributionContent,
                    MemberName = m != null ? m.MemberName : null,
                    MemberBusinessName = m != null ? m.MemberBusinessName : null,
                    DistrictName = d != null ? d.DistrictName : null,
                    UnitName = u != null ? u.UnitName : null,
                    c.ContributionAmount,
                    c.ContributionImgUrl
                }
            ).FirstOrDefaultAsync();
        }

        public async Task<ContributionResponse> ContributionUpdationAsync(int contributionId, ContributionUpdationRequest request)
        {
            var contribution = await _context.Contributions.FindAsync(contributionId);
            if (contribution == null)
                return new ContributionResponse { Status = "fail", Message = "Contribution not found." };

            contribution.ContributionChequeNo = request.ContributionChequeNo;
            contribution.ContributionImgUrl = request.ContributionImgUrl;
            contribution.ContributionHandOveredTo = request.ContributionHandOveredTo;
            contribution.ContributionContent = request.ContributionContent;
            contribution.ContributionStatus = request.ContributionStatus;
            contribution.ContributionRemarks = request.ContributionRemarks;
            bool isApproved = request.ContributionStatus == "Approved";
            contribution.ActiveStatus = isApproved;
            contribution.ContributionApprovalStatus = isApproved ? 1 : 0;

            await _context.SaveChangesAsync();

            return new ContributionResponse
            {
                Status = "success",
                Message = "Contribution updated successfully."
            };
        }

        public async Task<object> SendFirebaseNotificationAsync(int contributionMemberId)
        {
            var allowedStatuses = new List<int?> { 5, 6, 9 };

            var tokens = await (
                from u in _context.Users
                join m in _context.Members on u.UserId equals m.MemberUserId
                where u.UserStatus == true
                   && allowedStatuses.Contains(m.MemberStatus)
                   && u.FirebaseToken != null
                   && u.UserId != contributionMemberId
                select u.FirebaseToken
            ).Distinct().ToListAsync();

            if (!tokens.Any())
                return new { SuccessCount = 0, FailureCount = 0 };

            int successCount = 0, failureCount = 0;
            const int chunkSize = 500;

            for (int i = 0; i < tokens.Count; i += chunkSize)
            {
                var chunk = tokens.Skip(i).Take(chunkSize).ToList();
                var messages = chunk.Select(token => new Message
                {
                    Notification = new Notification
                    {
                        Title = "AKCA Contribution",
                        Body = "A new contribution has been added that needs your attention."
                    },
                    Token = token
                }).ToList();

                var response = await FirebaseMessaging.DefaultInstance.SendEachAsync(messages);
                successCount += response.SuccessCount;
                failureCount += response.FailureCount;
            }

            return new { SuccessCount = successCount, FailureCount = failureCount };
        }
    }
}