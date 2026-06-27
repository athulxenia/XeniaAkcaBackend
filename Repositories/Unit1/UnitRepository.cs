// Repositories/UnitRepository.cs

using Microsoft.EntityFrameworkCore;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Models;

namespace XeniaAkcaBackend.Repositories
{
    public class UnitRepository : IUnitRepository
    {
        private readonly ApplicationDbContext _context;

        public UnitRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== CREATE UNIT ====================
        public async Task<UnitResponse> CreateUnitAsync(UnitRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingUser = await _context.Users
                    .Where(u => u.UserName == request.UnitContactPerson)
                    .Select(u => u.UserName)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                    return new UnitResponse { Status = "failed", Message = "Username already exists" };

                // ✅ Fix: Max returns int? so use ?? 0
                var maxMembership = await _context.Units
                    .MaxAsync(u => (int?)Convert.ToInt32(u.LastMembershipNumber)) ?? 0;

                string nextMembershipNumber = (maxMembership + 1).ToString().PadLeft(3, '0');
                int unitDistrictId = request.UnitDistrictId > 0 ? request.UnitDistrictId : 0;

                var newUnit = new Unit
                {
                    UnitName = request.UnitName ?? "",
                    UnitCode = request.UnitCode ?? "",
                    UnitDistrictId = unitDistrictId,
                    UnitContactPerson = request.UnitContactPerson ?? "",
                    UnitContactPerson2 = request.UnitContactPerson2 ?? "",
                    UnitContactNumber = request.UnitContactNumber ?? "",
                    UnitContactNumber2 = request.UnitContactNumber2 ?? "",
                    UnitEmailAddress = request.UnitEmailAddress ?? "",
                    UnitMemNumberPrefix = "AKCA",
                    LastMembershipNumber = nextMembershipNumber,
                    Status = true
                };

                _context.Units.Add(newUnit);
                await _context.SaveChangesAsync();

                var newUser = new User
                {
                    UserGroupId = 3,
                    CompanyId = 1,
                    UserDistrictId = unitDistrictId,
                    UserUnitId = newUnit.UnitId,
                    UserName = request.UnitContactPerson ?? "",
                    Password = request.Password ?? "",
                    UserStatus = true
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return new UnitResponse { Status = "success", Message = "Unit created successfully" };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ==================== UPDATE UNIT ====================
        public async Task<UnitResponse> UpdateUnitAsync(int unitId, UnitRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var unit = await _context.Units.FindAsync(unitId);
                if (unit == null)
                    return new UnitResponse { Status = "failed", Message = "Unit not found" };

                unit.UnitName = request.UnitName ?? unit.UnitName;
                unit.UnitCode = request.UnitCode ?? unit.UnitCode;
                unit.UnitDistrictId = request.UnitDistrictId;
                unit.UnitContactPerson = request.UnitContactPerson ?? unit.UnitContactPerson;
                unit.UnitContactPerson2 = request.UnitContactPerson2 ?? unit.UnitContactPerson2;
                unit.UnitContactNumber = request.UnitContactNumber ?? unit.UnitContactNumber;
                unit.UnitContactNumber2 = request.UnitContactNumber2 ?? unit.UnitContactNumber2;
                unit.UnitEmailAddress = request.UnitEmailAddress ?? unit.UnitEmailAddress;
                unit.Status = request.Status;

                await _context.SaveChangesAsync();

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserUnitId == unitId);

                if (user != null)
                {
                    user.UserName = request.UnitContactPerson ?? user.UserName;
                    user.Password = request.Password ?? user.Password;
                    user.UserStatus = request.UserStatus;
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return new UnitResponse { Status = "success", Message = "Unit Updated successfully" };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ==================== DELETE UNIT ====================
        public async Task<UnitResponse> DeleteUnitAsync(int unitId)
        {
            var unit = await _context.Units.FindAsync(unitId);
            if (unit == null)
                return new UnitResponse { Status = "failed", Message = "Unit not found" };

            _context.Units.Remove(unit);
            await _context.SaveChangesAsync();

            return new UnitResponse { Status = "success", Message = "Unit Deleted successfully" };
        }

   
        public async Task<PaginatedUnitResponse> GetDistrictUnitsAsync(int userId, int page, int limit, string search)
        {
 
            var userDistrictId = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => (int?)u.UserDistrictId)
                .FirstOrDefaultAsync();

            int districtId = userDistrictId ?? 0;
            int offset = (page - 1) * limit;

            var query = _context.Units
                .Where(u => u.UnitDistrictId == districtId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    (u.UnitName != null && u.UnitName.Contains(search)) ||
                    (u.UnitContactPerson != null && u.UnitContactPerson.Contains(search)) ||
                    (u.UnitEmailAddress != null && u.UnitEmailAddress.Contains(search)));
            }

            int totalRecords = await query.CountAsync();

            var units = await query
                .OrderBy(u => u.UnitId)
                .Skip(offset)
                .Take(limit)
                .Select(u => new
                {
                    u.UnitId,
                    u.UnitName,
                    u.UnitCode,
                    u.UnitDistrictId,
                    u.UnitContactPerson,
                    u.UnitContactPerson2,
                    u.UnitContactNumber,
                    u.UnitContactNumber2,
                    u.UnitEmailAddress,
                    u.UnitMemNumberPrefix,
                    u.LastMembershipNumber,
                    u.Status
                })
                .ToListAsync();

            return new PaginatedUnitResponse
            {
                TotalPages = (int)Math.Ceiling((double)totalRecords / limit),
                CurrentPage = page,
                Limit = limit,
                TotalRecords = totalRecords,
                Units = units
            };
        }

        
        public async Task<PaginatedUnitResponse> GetAllStateUnitsAsync(string search, int page, int limit, int districtId)
        {
            int offset = (page - 1) * limit;

            var query = from unit in _context.Units
                        join district in _context.Districts on unit.UnitDistrictId equals district.DistrictId
                        where unit.UnitDistrictId == districtId
                        select new UnitWithDistrictDto
                        {
                            UnitId = unit.UnitId,
                            UnitName = unit.UnitName ?? "",
                            UnitCode = unit.UnitCode ?? "",
                            UnitDistrictId = unit.UnitDistrictId ?? 0,
                            DistrictName = district.DistrictName ?? "",
                            UnitContactPerson = unit.UnitContactPerson ?? "",
                            UnitContactPerson2 = unit.UnitContactPerson2 ?? "",
                            UnitContactNumber = unit.UnitContactNumber ?? "",
                            UnitContactNumber2 = unit.UnitContactNumber2 ?? "",
                            UnitEmailAddress = unit.UnitEmailAddress ?? "",
                            UnitMemNumberPrefix = unit.UnitMemNumberPrefix ?? "",
                            LastMembershipNumber = unit.LastMembershipNumber ?? "",
                            Status = unit.Status
                        };

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    (u.UnitName != null && u.UnitName.Contains(search)) ||
                    (u.UnitContactPerson != null && u.UnitContactPerson.Contains(search)) ||
                    (u.UnitEmailAddress != null && u.UnitEmailAddress.Contains(search)));
            }

            int totalRecords = await query.CountAsync();

            var units = await query
                .OrderBy(u => u.UnitId)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return new PaginatedUnitResponse
            {
                TotalPages = (int)Math.Ceiling((double)totalRecords / limit),
                CurrentPage = page,
                Limit = limit,
                TotalRecords = totalRecords,
                Units = units
            };
        }

        // ==================== GET ALL UNITS ====================
        public async Task<PaginatedUnitResponse> GetAllUnitsAsync(string search, int page, int limit)
        {
            int offset = (page - 1) * limit;

            var query = from unit in _context.Units
                        join district in _context.Districts on unit.UnitDistrictId equals district.DistrictId
                        select new UnitWithDistrictDto
                        {
                            UnitId = unit.UnitId,
                            UnitName = unit.UnitName ?? "",
                            UnitCode = unit.UnitCode ?? "",
                            UnitDistrictId = unit.UnitDistrictId ?? 0,
                            DistrictName = district.DistrictName ?? "",
                            UnitContactPerson = unit.UnitContactPerson ?? "",
                            UnitContactPerson2 = unit.UnitContactPerson2 ?? "",
                            UnitContactNumber = unit.UnitContactNumber ?? "",
                            UnitContactNumber2 = unit.UnitContactNumber2 ?? "",
                            UnitEmailAddress = unit.UnitEmailAddress ?? "",
                            UnitMemNumberPrefix = unit.UnitMemNumberPrefix ?? "",
                            LastMembershipNumber = unit.LastMembershipNumber ?? "",
                            Status = unit.Status
                        };

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    (u.UnitName != null && u.UnitName.Contains(search)) ||
                    (u.UnitContactPerson != null && u.UnitContactPerson.Contains(search)) ||
                    (u.UnitEmailAddress != null && u.UnitEmailAddress.Contains(search)));
            }

            int totalRecords = await query.CountAsync();

            var units = await query
                .OrderBy(u => u.UnitId)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return new PaginatedUnitResponse
            {
                TotalPages = (int)Math.Ceiling((double)totalRecords / limit),
                CurrentPage = page,
                Limit = limit,
                TotalRecords = totalRecords,
                Units = units
            };
        }

        // ==================== GET ALL UNIT DETAILS ====================
        public async Task<List<UnitWithDistrictDto>> GetAllUnitDetailsAsync(int districtId)
        {
            return await (from unit in _context.Units
                          join district in _context.Districts on unit.UnitDistrictId equals district.DistrictId
                          where unit.UnitDistrictId == districtId
                          select new UnitWithDistrictDto
                          {
                              UnitId = unit.UnitId,
                              UnitName = unit.UnitName ?? "",
                              UnitCode = unit.UnitCode ?? "",
                              UnitDistrictId = unit.UnitDistrictId ?? 0, // ✅
                              DistrictName = district.DistrictName ?? "",
                              UnitContactPerson = unit.UnitContactPerson ?? "",
                              UnitContactPerson2 = unit.UnitContactPerson2 ?? "",
                              UnitContactNumber = unit.UnitContactNumber ?? "",
                              UnitContactNumber2 = unit.UnitContactNumber2 ?? "",
                              UnitEmailAddress = unit.UnitEmailAddress ?? "",
                              UnitMemNumberPrefix = unit.UnitMemNumberPrefix ?? "",
                              LastMembershipNumber = unit.LastMembershipNumber ?? "",
                              Status = unit.Status
                          }).ToListAsync();
        }

        // ==================== GET UNIT MEMBERS ====================
        public async Task<List<UnitWithDistrictDto>> GetUnitMembersAsync()
        {
            return await _context.Units
                .Select(u => new UnitWithDistrictDto
                {
                    UnitId = u.UnitId,
                    UnitName = u.UnitName ?? "",
                    UnitCode = u.UnitCode ?? "",
                    UnitDistrictId = u.UnitDistrictId ?? 0,
                    UnitContactPerson = u.UnitContactPerson ?? "",
                    UnitContactPerson2 = u.UnitContactPerson2 ?? "",
                    UnitContactNumber = u.UnitContactNumber ?? "",
                    UnitContactNumber2 = u.UnitContactNumber2 ?? "",
                    UnitEmailAddress = u.UnitEmailAddress ?? "",
                    UnitMemNumberPrefix = u.UnitMemNumberPrefix ?? "",
                    LastMembershipNumber = u.LastMembershipNumber ?? "",
                    Status = u.Status
                })
                .ToListAsync();
        }
    }
}