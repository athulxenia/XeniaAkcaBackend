using Microsoft.EntityFrameworkCore;
using XeniaAkcaBackend.Repositories.Auth;
using XeniaCatalogueApi.Service.Common;
using XeniaAkcaBackend.Models;
using MemberModel = XeniaAkcaBackend.Models.Member;
namespace XeniaAkcaBackend.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtHelperService _jwtHelper;

        public AuthRepository(ApplicationDbContext context, JwtHelperService jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var activeUsers = await _context.Users
                .Where(u => u.UserName == request.Username && u.UserStatus == true)
                .ToListAsync();

            if (!activeUsers.Any())
                return new LoginResponse { Status = "failed", Message = "Invalid username" };

            var user = activeUsers.Any(u => u.UserGroupId == 4)
                ? activeUsers.First(u => u.UserGroupId == 4)
                : activeUsers.First();

            if (user.Password != request.Password)
                return new LoginResponse { Status = "failed", Message = "Invalid password" };

            if (!string.IsNullOrEmpty(request.FirebaseToken))
            {
                user.FirebaseToken = request.FirebaseToken;
                await _context.SaveChangesAsync();
            }

            var tokenUser = (user.UserGroupId ?? 0) switch
            {
                1 => new User { UserId = user.UserId, UserName = user.UserName, UserGroupId = user.UserGroupId, UserStatus = user.UserStatus, CompanyId = user.CompanyId },
                2 => new User { UserId = user.UserId, UserName = user.UserName, UserGroupId = user.UserGroupId, UserDistrictId = user.UserDistrictId, UserStatus = user.UserStatus, CompanyId = user.CompanyId },
                _ => new User { UserId = user.UserId, UserName = user.UserName, UserGroupId = user.UserGroupId, UserDistrictId = user.UserDistrictId, UserUnitId = user.UserUnitId, UserStatus = user.UserStatus, CompanyId = user.CompanyId }
            };

            var token = _jwtHelper.GenerateJwtToken(tokenUser);
            return new LoginResponse { Status = "success", Token = token };
        }

     
        //public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        //{
        //    return request.RegType switch
        //    {
        //        1 => await CreateUserAsync(request),
        //        4 => await CreateKaruthalOwnerAsync(request),
        //        _ => await CreateKaruthalUserAsync(request)
        //    };
        //}

        private async Task<RegisterResponse> CreateUserAsync(RegisterRequest d)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
            
                if (await _context.Users.AnyAsync(u => u.UserName == d.FMYuserName))
                {
                    await tx.RollbackAsync();
                    return new RegisterResponse { StatusCode = 403, Status = "failed", Message = "Username already exists" };
                }

                var user = new User
                {
                    UserGroupId = d.FMYuserGroupId,
                    CompanyId = 1,
                    UserDistrictId = d.FMYuserDistrictId,
                    UserUnitId = d.FMYuserUnitId,
                    UserName = d.FMYuserName,
                    Password = d.FMYpassword,
                    UserImageUrl = d.FMYuserImageUrl,
                    FirebaseToken = d.FMYfirebaseToken,
                    UserStatus = true,
                    UserCreatedOn = string.IsNullOrEmpty(d.FMYCreatedOn) ? DateTime.UtcNow : DateTime.Parse(d.FMYCreatedOn)
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                var userId = user.UserId;

                
                var currentYear = DateTime.Now.Year;
                var suffix = "/" + currentYear.ToString()[^2..];
                var memberCheck = await _context.Members
                    .Where(m => m.MemberDistrictId == d.FMYuserDistrictId && m.MemberUnitId == d.FMYuserUnitId)
                    .ToListAsync();
                var maxNum = memberCheck.Any() ? memberCheck.Max(m => int.TryParse(m.MembershipNumber, out var n) ? n : 0) : 0;
                var newNumber = memberCheck.Count == 0 ? "0001" : (maxNum + 1).ToString().PadLeft(4, '0');

               
                var member = new MemberModel
                {
                    MemberGroupId = d.FMYuserGroupId,
                    MemberParentId = d.FMYmemberParentId,
                    MemberDistrictId = d.FMYuserDistrictId,
                    MemberUnitId = d.FMYuserUnitId,
                    MemberUserId = userId,
                    MemberStatus = 2,
                    MemberReviseRemarks = d.FMYmemberReviseRemarks,
                    MembershipNumberPrefix = "AKCA",
                    MembershipNumberSuffix = suffix,
                    MembershipNumber = newNumber,
                    MembershipDate = string.IsNullOrEmpty(d.FMYCreatedOn) ? DateTime.UtcNow : DateTime.Parse(d.FMYCreatedOn),
                    MemberActiveStatus = false,
                    MemberName = d.FMYmemberName ?? string.Empty,
                    MemberAddress = d.FMYmemberAddress ?? string.Empty,
                    MemberEmail = d.FMYmemberEmail ?? string.Empty,
                    MemberMobilenumber = d.FMYmemberMobilenumber ?? string.Empty,
                    MemberDob = string.IsNullOrEmpty(d.FMYmemberDob) ? DateTime.UtcNow : DateTime.Parse(d.FMYmemberDob),
                    MemberIdProofNumber = d.FMYmemberIdProofNumber,
                    MemberBankName = d.FMYmemberBankName ?? string.Empty,
                    MemberBankAcName = d.FMYmemberBankAcName ?? string.Empty,
                    MemberBankAcNumber = d.FMYmemberBankAcNumber ?? string.Empty,
                    MemberBankBranch = d.FMYmemberBankBranch ?? string.Empty,
                    MemberIfsc = d.FMYmemberIfsc ?? string.Empty,
                    MemberIdUrl1 = d.FMYmemberIdUrl1,
                    MemberIdUrl2 = d.FMYmemberIdUrl2,
                    MemberBusinessName = d.FMYmemberBusinessName ?? string.Empty,
                    MemberBusinessAddress = d.FMYmemberBusinessAddress ?? string.Empty,
                    MemberAge = d.FMYmemberAge,
                    MemberBusinessDetails = d.FMYmemberBusinessDetails,
                    MemberBusinessFSSAIno = d.FMYmemberBusinessFSSAIno,
                    MemberBusinessCmpyType = d.FMYmemberBusinessCmpyType,
                    MemberGstCertificateUrl = d.FMYmemberGstCertificateUrl,
                    MemberPartnershipDeedUrl = d.FMYmemberPartnershipDeedUrl,
                    MemberIdProof = d.FMYmemberIdProof
                };
                _context.Members.Add(member);
                await _context.SaveChangesAsync();
                var memberId = member.MemberId;
                var district = await _context.Districts.FindAsync(d.FMYuserDistrictId);
                var unit = await _context.Units.FindAsync(d.FMYuserUnitId);
                var newPrefix = "KL" + district?.DistrictMemberSchmPrefix + unit?.UnitMemNumberPrefix;

                member.MembershipNumberPrefix = newPrefix;
                await _context.SaveChangesAsync();

           
                var nominee = new Nominee
                {
                    NomineeMemberId = memberId,
                    NomineeName = d.FMYnomineeName,
                    NomineeAddress = d.FMYnomineeAddress,
                    NomineeEmail = d.FMYnomineeEmail,
                    NomineeMobilenumber = d.FMYnomineeMobilenumber,
                    NomineeIdProof = d.FMYnomineeIdProof,
                    NomineeIdProofNumber = d.FMYnomineeIdProofNumber,
                    NomineeBankName = d.FMYnomineeBankName,
                    NomineeBankAcName = d.FMYnomineeBankAcName,
                    NomineeBankAcNumber = d.FMYnomineeBankAcNumber,
                    NomineeBankBranch = d.FMYnomineeBankBranch,
                    NomineeIfsc = d.FMYnomineeIfsc,
                    NomineeIdUrl1 = d.FMYnomineeIdUrl1,
                    NomineeIdUrl2 = d.FMYnomineeIdUrl2,
                    NomineeApprovalStatus = 0,
                    NomineeStatus = d.FMYnomineeStatus,
                    NomineeRelation = d.FMYnomineeRelation
                };
                _context.Nominees.Add(nominee);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                var tokenUser = new User { UserId = userId, UserName = d.FMYuserName, UserGroupId = d.FMYuserGroupId, UserDistrictId = d.FMYuserDistrictId, UserUnitId = d.FMYuserUnitId, UserStatus = true, CompanyId = 1 };
                var token = _jwtHelper.GenerateJwtToken(tokenUser);
                return new RegisterResponse { StatusCode = 200, Status = "success", Message = "User created successfully", Token = token };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ─── regType 2/3 — AKCA_KaruthalMembers ──────────────────
        //private async Task<RegisterResponse> CreateKaruthalUserAsync(RegisterRequest d)
        //{
        //    using var tx = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        int userId;

        //        if (d.RegType != 2)
        //        {
        //            // Create new user
        //            if (await _context.Users.AnyAsync(u => u.UserName == d.FMYuserName))
        //            {
        //                await tx.RollbackAsync();
        //                return new RegisterResponse { StatusCode = 403, Status = "failed", Message = "Username already exists" };
        //            }

        //            var user = new User
        //            {
        //                UserGroupId = d.FMYuserGroupId,
        //                CompanyId = 1,
        //                UserDistrictId = d.FMYuserDistrictId,
        //                UserUnitId = d.FMYuserUnitId,
        //                UserName = d.FMYuserName,
        //                Password = d.FMYpassword,
        //                UserImageUrl = d.FMYuserImageUrl,
        //                FirebaseToken = d.FMYfirebaseToken,
        //                UserStatus = true,
        //                UserCreatedOn = string.IsNullOrEmpty(d.FYMCreatedOn) ? DateTime.UtcNow : DateTime.Parse(d.FYMCreatedOn)
        //            };
        //            _context.Users.Add(user);
        //            await _context.SaveChangesAsync();
        //            userId = user.UserId;
        //        }
        //        else
        //        {
                  
        //            if (await _context.Users.AnyAsync(u => u.UserName == d.FMYuserName))
        //            {
        //                await tx.RollbackAsync();
        //                return new RegisterResponse { StatusCode = 403, Status = "failed", Message = "Username already exists" };
        //            }

        //            if (d.FYMmemberId == null)
        //            {
        //                await tx.RollbackAsync();
        //                return new RegisterResponse { StatusCode = 400, Status = "failed", Message = "Request body not contain memberUserId" };
        //            }

        //            var existingMember = await _context.Members
        //                .Where(m => m.MemberId == d.FYMmemberId.Value)
        //                .Select(m => m.MemberUserId)
        //                .FirstOrDefaultAsync();

        //            if (existingMember == 0)
        //            {
        //                await tx.RollbackAsync();
        //                return new RegisterResponse { StatusCode = 400, Status = "failed", Message = "User id fetch failed" };
        //            }

        //            userId = existingMember;
        //        }

               
        //        var currentYear = DateTime.Now.Year;
        //        var suffix = "/" + currentYear.ToString()[^2..];
        //        var karuthalMembers = await _context.KaruthalMembers
        //            .Where(m => m.MemberDistrictId == d.FMYuserDistrictId && m.MemberUnitId == d.FMYuserUnitId)
        //            .ToListAsync();
        //        var maxNum = karuthalMembers.Any() ? karuthalMembers.Max(m => int.TryParse(m.MembershipNumber, out var n) ? n : 0) : 0;
        //        var newNumber = maxNum == 0 ? "0001" : (maxNum + 1).ToString().PadLeft(4, '0');

        //        // Build prefix
        //        var district = await _context.Districts.FindAsync(d.FMYuserDistrictId);
        //        var unit = await _context.Units.FindAsync(d.FMYuserUnitId);
        //        var basePrefix = district?.DistrictMemberSchmPrefix + unit?.UnitMemNumberPrefix;
        //        var newPrefix = d.RegType == 2 ? "KL" + basePrefix : "KKL" + basePrefix;

        //        // Insert KaruthalMember
        //        var member = new KaruthalMember
        //        {
        //            MemberGroupId = d.FMYuserGroupId,
        //            MemberParentId = d.FMYmemberParentId,
        //            MemberDistrictId = d.FMYuserDistrictId,
        //            MemberUnitId = d.FMYuserUnitId,
        //            MemberUserId = userId,
        //            MemberStatus = 2,
        //            MemberReviseRemarks = d.FMYmemberReviseRemarks,
        //            MembershipNumberPrefix = newPrefix,
        //            MembershipNumberSuffix = suffix,
        //            MembershipNumber = newNumber,
        //            MembershipDate = string.IsNullOrEmpty(d.FMYCreatedOn) ? DateTime.UtcNow : DateTime.Parse(d.FMYCreatedOn),
        //            MemberActiveStatus = false,
        //            MemberName = d.FMYmemberName ?? string.Empty,
        //            MemberAddress = d.FMYmemberAddress ?? string.Empty,
        //            MemberEmail = d.FMYmemberEmail ?? string.Empty,
        //            MemberMobilenumber = d.FMYmemberMobilenumber ?? string.Empty,
        //            MemberDob = string.IsNullOrEmpty(d.FMYmemberDob) ? DateTime.UtcNow : DateTime.Parse(d.FMYmemberDob),
        //            MemberIdProofNumber = d.FMYmemberIdProofNumber,
        //            MemberBankName = d.FMYmemberBankName ?? string.Empty,
        //            MemberBankAcName = d.FMYmemberBankAcName ?? string.Empty,
        //            MemberBankAcNumber = d.FMYmemberBankAcNumber ?? string.Empty,
        //            MemberBankBranch = d.FMYmemberBankBranch ?? string.Empty,
        //            MemberIfsc = d.FMYmemberIfsc ?? string.Empty,
        //            MemberIdUrl1 = d.FMYmemberIdUrl1,
        //            MemberIdUrl2 = d.FMYmemberIdUrl2,
        //            MemberBusinessName = d.FMYmemberBusinessName ?? string.Empty,
        //            MemberBusinessAddress = d.FMYmemberBusinessAddress ?? string.Empty,
        //            MemberAge = d.FMYmemberAge,
        //            MemberBusinessDetails = d.FMYmemberBusinessDetails,
        //            MemberBusinessFSSAIno = d.FMYmemberBusinessFSSAIno,
        //            MemberBusinessCmpyType = d.FMYmemberBusinessCmpyType,
        //            MemberGstCertificateUrl = d.FMYmemberGstCertificateUrl,
        //            MemberPartnershipDeedUrl = d.FMYmemberPartnershipDeedUrl,
        //            MemberIdProof = d.FMYmemberIdProof
        //        };
        //        _context.KaruthalMembers.Add(member);
        //        await _context.SaveChangesAsync();

           
        //        if (d.FMYmemberParentId != 0)
        //        {
        //            var nominee = new Nominee
        //            {
        //                NomineeMemberId = member.MemberId,
        //                NomineeName = d.FMYnomineeName,
        //                NomineeAddress = d.FMYnomineeAddress,
        //                NomineeEmail = d.FMYnomineeEmail,
        //                NomineeMobilenumber = d.FMYnomineeMobilenumber,
        //                NomineeIdProof = d.FMYnomineeIdProof,
        //                NomineeIdProofNumber = d.FMYnomineeIdProofNumber,
        //                NomineeBankName = d.FMYnomineeBankName,
        //                NomineeBankAcName = d.FMYnomineeBankAcName,
        //                NomineeBankAcNumber = d.FMYnomineeBankAcNumber,
        //                NomineeBankBranch = d.FMYnomineeBankBranch,
        //                NomineeIfsc = d.FMYnomineeIfsc,
        //                NomineeIdUrl1 = d.FMYnomineeIdUrl1,
        //                NomineeIdUrl2 = d.FMYnomineeIdUrl2,
        //                NomineeApprovalStatus = 0,
        //                NomineeStatus = d.FMYnomineeStatus,
        //                NomineeRelation = d.FMYnomineeRelation
        //            };
        //            _context.Nominees.Add(nominee);
        //            await _context.SaveChangesAsync();
        //        }

        //        await tx.CommitAsync();

        //        var tokenUser = new User { UserId = userId, UserName = d.FMYuserName, UserGroupId = d.FMYuserGroupId, UserDistrictId = d.FMYuserDistrictId, UserUnitId = d.FMYuserUnitId, UserStatus = true, CompanyId = 1 };
        //        var token = _jwtHelper.GenerateJwtToken(tokenUser);
        //        return new RegisterResponse { StatusCode = 201, Status = "success", Message = "User created successfully", Token = token };
        //    }
        //    catch
        //    {
        //        await tx.RollbackAsync();
        //        throw;
        //    }
        //}


        //private async Task<RegisterResponse> CreateKaruthalOwnerAsync(RegisterRequest d)
        //{
        //    using var tx = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        if (await _context.Users.AnyAsync(u => u.UserName == d.FMYuserName))
        //        {
        //            await tx.RollbackAsync();
        //            return new RegisterResponse { StatusCode = 403, Status = "failed", Message = "Username already exists" };
        //        }

        //        var user = new User
        //        {
        //            UserGroupId = d.FMYuserGroupId,
        //            CompanyId = 1,
        //            UserDistrictId = d.FMYuserDistrictId,
        //            UserUnitId = d.FMYuserUnitId,
        //            UserName = d.FMYuserName,
        //            Password = d.FMYpassword,
        //            UserImageUrl = d.FMYuserImageUrl,
        //            FirebaseToken = d.FMYfirebaseToken,
        //            UserStatus = true,
        //            UserCreatedOn = string.IsNullOrEmpty(d.FMYCreatedOn) ? DateTime.UtcNow : DateTime.Parse(d.FMYCreatedOn)
        //        };
        //        _context.Users.Add(user);
        //        await _context.SaveChangesAsync();
        //        var userId = user.UserId;

        //        var currentYear = DateTime.Now.Year;
        //        var suffix = "/" + currentYear.ToString()[^2..];
        //        var karuthalMembers = await _context.KaruthalMembers
        //            .Where(m => m.MemberDistrictId == d.FMYuserDistrictId && m.MemberUnitId == d.FMYuserUnitId)
        //            .ToListAsync();
        //        var maxNum = karuthalMembers.Any() ? karuthalMembers.Max(m => int.TryParse(m.MembershipNumber, out var n) ? n : 0) : 0;
        //        var newNumber = karuthalMembers.Count == 0 ? "0001" : (maxNum + 1).ToString().PadLeft(4, '0');

        //        var member = new KaruthalMember
        //        {
        //            MemberGroupId = d.FMYuserGroupId,
        //            MemberParentId = d.FMYmemberParentId,
        //            MemberDistrictId = d.FMYuserDistrictId,
        //            MemberUnitId = d.FMYuserUnitId,
        //            MemberUserId = userId,
        //            MemberStatus = 2,
        //            MemberReviseRemarks = d.FMYmemberReviseRemarks,
        //            MembershipNumberPrefix = "AKCA",
        //            MembershipNumberSuffix = suffix,
        //            MembershipNumber = newNumber,
        //            MembershipDate = string.IsNullOrEmpty(d.FMYCreatedOn) ? DateTime.UtcNow : DateTime.Parse(d.FMYCreatedOn),
        //            MemberActiveStatus = false,
        //            MemberName = d.FMYmemberName ?? string.Empty,
        //            MemberAddress = d.FMYmemberAddress ?? string.Empty,
        //            MemberEmail = d.FMYmemberEmail ?? string.Empty,
        //            MemberMobilenumber = d.FMYmemberMobilenumber ?? string.Empty,
        //            MemberDob = string.IsNullOrEmpty(d.FMYmemberDob) ? DateTime.UtcNow : DateTime.Parse(d.FMYmemberDob),
        //            MemberIdProofNumber = d.FMYmemberIdProofNumber,
        //            MemberBankName = d.FMYmemberBankName ?? string.Empty,
        //            MemberBankAcName = d.FMYmemberBankAcName ?? string.Empty,
        //            MemberBankAcNumber = d.FMYmemberBankAcNumber ?? string.Empty,
        //            MemberBankBranch = d.FMYmemberBankBranch ?? string.Empty,
        //            MemberIfsc = d.FMYmemberIfsc ?? string.Empty,
        //            MemberIdUrl1 = d.FMYmemberIdUrl1,
        //            MemberIdUrl2 = d.FMYmemberIdUrl2,
        //            MemberBusinessName = d.FMYmemberBusinessName ?? string.Empty,
        //            MemberBusinessAddress = d.FMYmemberBusinessAddress ?? string.Empty,
        //            MemberAge = d.FMYmemberAge,
        //            MemberBusinessDetails = d.FMYmemberBusinessDetails,
        //            MemberBusinessFSSAIno = d.FMYmemberBusinessFSSAIno,
        //            MemberBusinessCmpyType = d.FMYmemberBusinessCmpyType,
        //            MemberGstCertificateUrl = d.FMYmemberGstCertificateUrl,
        //            MemberPartnershipDeedUrl = d.FMYmemberPartnershipDeedUrl,
        //            MemberIdProof = d.FMYmemberIdProof
        //        };
        //        _context.KaruthalMembers.Add(member);
        //        await _context.SaveChangesAsync();

        //        // Update prefix
        //        var district = await _context.Districts.FindAsync(d.FMYuserDistrictId);
        //        var unit = await _context.Units.FindAsync(d.FMYuserUnitId);
        //        member.MembershipNumberPrefix = "KL" + district?.DistrictMemberSchmPrefix + unit?.UnitMemNumberPrefix;
        //        await _context.SaveChangesAsync();

        //        // Insert nominee
        //        var nominee = new Nominee
        //        {
        //            NomineeMemberId = member.MemberId,
        //            NomineeName = d.FMYnomineeName,
        //            NomineeAddress = d.FMYnomineeAddress,
        //            NomineeEmail = d.FMYnomineeEmail,
        //            NomineeMobilenumber = d.FMYnomineeMobilenumber,
        //            NomineeIdProof = d.FMYnomineeIdProof,
        //            NomineeIdProofNumber = d.FMYnomineeIdProofNumber,
        //            NomineeBankName = d.FMYnomineeBankName,
        //            NomineeBankAcName = d.FMYnomineeBankAcName,
        //            NomineeBankAcNumber = d.FMYnomineeBankAcNumber,
        //            NomineeBankBranch = d.FMYnomineeBankBranch,
        //            NomineeIfsc = d.FMYnomineeIfsc,
        //            NomineeIdUrl1 = d.FMYnomineeIdUrl1,
        //            NomineeIdUrl2 = d.FMYnomineeIdUrl2,
        //            NomineeApprovalStatus = 0,
        //            NomineeStatus = d.FMYnomineeStatus,
        //            NomineeRelation = d.FMYnomineeRelation
        //        };
        //        _context.Nominees.Add(nominee);
        //        await _context.SaveChangesAsync();

        //        await tx.CommitAsync();

        //        var tokenUser = new User { UserId = userId, UserName = d.FMYuserName, UserGroupId = d.FMYuserGroupId, UserDistrictId = d.FMYuserDistrictId, UserUnitId = d.FMYuserUnitId, UserStatus = true, CompanyId = 1 };
        //        var token = _jwtHelper.GenerateJwtToken(tokenUser);
        //        return new RegisterResponse { StatusCode = 200, Status = "success", Message = "User created successfully", Token = token };
        //    }
        //    catch
        //    {
        //        await tx.RollbackAsync();
        //        throw;
        //    }
        //}

     
        public async Task<object> ChangePasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new { Status = "error", Message = "User not found" };

            if (user.Password == newPassword)
                return new { Status = "error", Message = "You are trying to set the same password again" };

            user.Password = newPassword;
            await _context.SaveChangesAsync();
            return new { Status = "success", Message = "Password changed successfully" };
        }

        public async Task<object> PasswordResetAsync(int memberId)
        {
            var member = await _context.KaruthalMembers
                .Where(m => m.MemberId == memberId)
                .Select(m => new { m.MemberUserId, m.MemberMobilenumber })
                .FirstOrDefaultAsync();

            if (member == null)
                return new { Status = "error", Message = "User not found" };

            var user = await _context.Users.FindAsync(member.MemberUserId);
            if (user == null)
                return new { Status = "error", Message = "User not found" };

            user.Password = member.MemberMobilenumber;
            await _context.SaveChangesAsync();
            return new { Status = "success", Message = "Password has been reset successfully" };
        }

     
        public async Task<object> CheckAndSendSmsAsync(string mobileNumber)
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.UserName == mobileNumber);

            if (!userExists)
                return new { Success = false, Message = "Mobile number not found." };

        
            var otp = new Random().Next(1000, 9999).ToString();
            var expiry = DateTime.UtcNow.AddMinutes(10).AddMinutes(330)
                             .ToString("dd/MM/yyyy, HH:mm:ss");

         

            return new { Success = true, Message = "SMS sent successfully.", Otp = otp, Expiry = expiry };
        }

    
        public async Task<object> ForgotPasswordUpdateAsync(string mobileNumber, string newPassword)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == mobileNumber);

            if (user == null)
                return new { Status = "error", Message = "User not found" };

            user.Password = newPassword;
            await _context.SaveChangesAsync();
            return new { Status = "success", Message = "New password has been reset successfully" };
        }
    }
}