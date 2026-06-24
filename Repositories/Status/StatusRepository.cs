using Microsoft.EntityFrameworkCore;
using XeniaAkcaBackend.Models;
using XeniaAkcaBackend.Dto;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace XeniaAkcaBackend.Repositories
{
    public class StatusRepository : IStatusRepository
    {
        private readonly ApplicationDbContext _context;

        public StatusRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object?> CheckServerStatusAsync()
        {
            return await _context.Servers
                .Select(s => new
                {
                    s.ServerId,
                    s.IosAppVersion,
                    s.AppVersion,
                    s.ServerUpdate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<object?> GetAccStatusAndPaymentHistoryAsync(int userId)
        {
            var userStatus = await (
                from s in _context.Statuses
                join m in _context.KaruthalMembers on s.StatusId equals m.MemberStatus
                where m.MemberUserId == userId
                select new { s.Status1, s.StatusId, m.MemberId }
            ).FirstOrDefaultAsync();

            if (userStatus == null) return null;

            int? paymentTypeId = userStatus.StatusId switch
            {
                2 => 1,
                3 => 2,
                4 => 3,
                5 => 4,
                6 => 5,
                _ => null
            };

            if (paymentTypeId.HasValue)
            {
                var pendingPayment = await _context.MemberPayments
                    .Where(p => p.PaymentStatus == "initiate"
                             && p.MemberId == userStatus.MemberId
                             && p.PaymentTypeId == paymentTypeId.Value)
                    .Select(p => new
                    {
                        p.PaymentTxnRefNo,
                        p.IsCallbackStatus,
                        p.PaymentStatus,
                        p.PaymentTypeId
                    })
                    .FirstOrDefaultAsync();

                if (pendingPayment != null)
                {
                    return new
                    {
                        status = userStatus,
                        pendingPayments = pendingPayment
                    };
                }
            }

            return new { status = userStatus };
        }

        public async Task<string?> GetTermsAndConditionsAsync(int statusId)
        {
            var column = statusId switch
            {
                1 => "assTermsAndConditionsENG",
                2 => "assTermsAndConditionsHND",
                3 => "assTermsAndConditionsML",
                4 => "assTermsAndConditionsTN",
                _ => throw new Exception("Invalid statusId")
            };

            // Use raw SQL to select dynamic column
            var sql = $"SELECT [{column}] AS TermsAndConditions FROM AKCA_CompanyProfile WHERE [{column}] IS NOT NULL";

            using var conn = new Microsoft.Data.SqlClient.SqlConnection(
                _context.Database.GetConnectionString());
            await conn.OpenAsync();
            using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn);

            var result = await cmd.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value) return null;

            var json = result.ToString();
            if (string.IsNullOrEmpty(json)) return null;

            try
            {
                var termsObject = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                if (termsObject == null) return null;

                var html = "<html><body>";
                foreach (var section in termsObject)
                {
                    html += $"<h2>{section.Key}</h2><ul>";
                    foreach (var item in section.Value)
                        html += $"<p>{item}</p>";
                    html += "</ul>";
                }
                html += "</body></html>";
                return html;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> GetPrivacyPolicyAsync(int statusId)
        {
            var column = statusId switch
            {
                1 => "assPrivacyPolicyENG",
                2 => "assPrivacyPolicyHND",
                3 => "assPrivacyPolicyML",
                4 => "assPrivacyPolicyTN",
                _ => throw new Exception("Invalid statusId")
            };

            // Use raw SQL to select dynamic column
            var sql = $"SELECT [{column}] AS PrivacyPolicy FROM AKCA_CompanyProfile WHERE [{column}] IS NOT NULL";

            using var conn = new Microsoft.Data.SqlClient.SqlConnection(
                _context.Database.GetConnectionString());
            await conn.OpenAsync();
            using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn);

            var result = await cmd.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value) return null;

            var json = result.ToString();
            if (string.IsNullOrEmpty(json)) return null;

            try
            {
                var policyObject = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                if (policyObject == null) return null;

                var html = "<html><body>";
                foreach (var section in policyObject)
                {
                    html += $"<h2>{section.Key}</h2><ul>";
                    foreach (var item in section.Value)
                    {
                        var formattedItem = System.Text.RegularExpressions.Regex.Replace(item, @"\*\*(.*?)\*\*", "<b>$1</b>");
                        html += $"<p>{formattedItem}</p>";
                    }
                    html += "</ul>";
                }
                html += "</body></html>";
                return html;
            }
            catch
            {
                return null;
            }
        }
        public async Task<object?> GetFamilyMemberAsync(int userId)
        {
            var result = await (
                from m in _context.KaruthalMembers
                join d in _context.Districts on m.MemberDistrictId equals d.DistrictId
                join u in _context.Units on m.MemberUnitId equals u.UnitId
                where m.MemberUserId == userId
                select new
                {
                    m.MemberBusinessName,
                    m.MemberBusinessAddress,
                    d.DistrictName,
                    u.UnitName,
                    m.MemberUserId,
                    m.MemberDistrictId,
                    m.MemberUnitId
                }
            ).ToListAsync();

            if (result.Count == 1) return result[0];
            if (result.Count > 1) return result;
            return null;
        }

        public async Task<object?> MemberDeactivationAsync(int userId, string? memberReviseRemarks)
        {
            var user = await _context.Users.FindAsync(userId);
            var member = await _context.KaruthalMembers.FirstOrDefaultAsync(m => m.MemberUserId == userId);

            if (user == null || member == null) return null;

            user.UserStatus = false;
            member.MemberStatus = 12;
            member.MemberReviseRemarks = memberReviseRemarks ?? "";
            member.MemberActiveStatus = false;

            await _context.SaveChangesAsync();

            return new { status = "success", message = "Member deactivated successfully" };
        }

        public async Task<object?> GetMemberDetailsAsync(int userId)
        {
            var result = await _context.KaruthalMembers
                .Where(m => m.MemberUserId == userId)
                .Select(m => new
                {
                    MembershipNumber = m.MembershipNumberPrefix + m.MembershipNumber + m.MembershipNumberSuffix,
                    m.MemberMobilenumber,
                    m.MemberEmail,
                    m.MemberName
                })
                .ToListAsync();

            if (result.Count == 1) return result[0];
            if (result.Count > 1) return result;
            return null;
        }

        public async Task<List<object>> GetReceiptDetailsAsync(int userId, string? tranId)
        {
            var memberId = await _context.KaruthalMembers
                .Where(m => m.MemberUserId == userId)
                .Select(m => m.MemberId)
                .FirstOrDefaultAsync();

            var sql = @"
                SELECT 
                    t.transactionId, 
                    m.memberName, 
                    m.memberMobilenumber, 
                    s.settingName as fundType, 
                    s.settingId as typeId,  
                    u.unitName, 
                    d.districtName, 
                    t.paidAmount, 
                    FORMAT(t.paidDate, 'dd-MMM-yyyy') as paidDate,
                    t.PaymentPaymentId 
                FROM AKCA_MemberPayment t
                JOIN AKCA_KaruthalMembers m ON t.memberId = m.memberId  
                JOIN AKCA_Settings s ON s.settingId = t.paymentTypeId
                JOIN AKCA_Units u ON u.unitId = t.paidUnit AND u.unitId = m.memberUnitId
                JOIN AKCA_Districts d ON d.districtId = t.paidDistrict AND d.districtId = m.memberDistrictId AND d.districtId = u.unitDistrictId
                WHERE t.paidBy = 1 AND t.memberId = {0}";

            var parameters = new List<object> { memberId };

            if (!string.IsNullOrEmpty(tranId))
            {
                sql += " AND t.PaymentPaymentId = {1}";
                parameters.Add(tranId);
            }

            sql += @"
                UNION ALL
                SELECT 
                    t.memberContributionId as transactionId, 
                    m.memberName, 
                    m.memberMobilenumber, 
                    (SELECT settingName FROM AKCA_Settings WHERE settingId = 3) as fundType, 
                    3 as typeId,
                    u.unitName, 
                    d.districtName, 
                    t.contributionAmount as paidAmount, 
                    FORMAT(t.paidDate, 'dd-MMM-yyyy') as paidDate,
                    t.contributionPaymentId as PaymentPaymentId 
                FROM AKCA_MemberContributions t
                JOIN AKCA_KaruthalMembers m ON t.memberId = m.memberId
                JOIN AKCA_Units u ON u.unitId = t.paidUnit AND u.unitId = m.memberUnitId
                JOIN AKCA_Districts d ON d.districtId = t.paidDistrict AND d.districtId = m.memberDistrictId AND d.districtId = u.unitDistrictId
                WHERE t.paidBy = 1 AND t.memberId = {0}";

            if (!string.IsNullOrEmpty(tranId))
            {
                sql += " AND t.contributionPaymentId = {" + parameters.Count + "}";
                parameters.Add(tranId);
            }

            var result = new List<object>();
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            for (int i = 0; i < parameters.Count; i++)
                cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter($"@{i}", parameters[i]));

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new
                {
                    TransactionId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    MemberName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    MemberMobilenumber = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    FundType = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    TypeId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    UnitName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    DistrictName = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    PaidAmount = reader.IsDBNull(7) ? 0m : reader.GetDecimal(7),
                    PaidDate = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    PaymentPaymentId = reader.IsDBNull(9) ? "" : reader.GetString(9)
                });
            }

            return result;
        }

        public async Task<object?> CompanyWhatsappMobAsync()
        {
            return await _context.CompanyProfiles
                .Select(c => new { c.CompanyPhone1 })
                .FirstOrDefaultAsync();
        }

        public async Task<List<object>?> MemberAccountDetailsAsync(int userId)
        {
            var result = await (
                from m in _context.KaruthalMembers
                join u in _context.Users on m.MemberUserId equals u.UserId
                join d in _context.Districts on m.MemberDistrictId equals d.DistrictId
                join unit in _context.Units on m.MemberUnitId equals unit.UnitId
                join g in _context.MemberGroups on m.MemberGroupId equals g.GroupId
                join n in _context.Nominees on m.MemberId equals n.NomineeMemberId into nomineeGroup
                from n in nomineeGroup.DefaultIfEmpty()
                where g.GroupId == u.UserGroupId
                where m.MemberUserId == userId
                select new
                {
                    m.MemberId,
                    d.DistrictName,
                    unit.UnitName,
                    UserType = g.GroupLevel,
                    m.MemberName,
                    m.MemberMobilenumber,
                    m.MemberEmail,
                    m.MemberStatus,
                    m.MemberAddress,
                    m.MemberBusinessName,
                    m.MemberBusinessDetails,
                    m.MemberAge,
                    GroupLevel = g.GroupLevel,
                    MembershipNumber = m.MembershipNumberPrefix + m.MembershipNumber,
                    NomineeName = n != null ? n.NomineeName : null,
                    NomineeRelation = n != null ? n.NomineeRelation : null
                }
            ).ToListAsync();

            return result.Count > 0 ? result.Cast<object>().ToList() : null;
        }

        public async Task<object?> UpdateMemberFullDetailsAsync(UpdateMemberFullDetailsRequest data)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var member = await _context.KaruthalMembers.FindAsync(data.MemberId);
                if (member == null)
                    return new { success = false, message = "Member not found" };

                // Check phone uniqueness
                if (member.MemberMobilenumber != data.MemberMobilenumber)
                {
                    var phoneExists = await _context.Users.AnyAsync(u => u.UserId != member.MemberUserId && u.UserName == data.MemberMobilenumber)
                                    || await _context.KaruthalMembers.AnyAsync(m => m.MemberId != data.MemberId && m.MemberMobilenumber == data.MemberMobilenumber);

                    if (phoneExists)
                        return new { success = false, message = "Mobile number already exists" };
                }

                // Update User
                var user = await _context.Users.FindAsync(member.MemberUserId);
                if (user != null)
                {
                    user.UserName = data.MemberMobilenumber;
                    user.UserStatus = data.MemberActiveStatus;
                    user.UserImageUrl = data.UserImageUrl;
                }

                // Update Member
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

                // Upsert Nominee
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