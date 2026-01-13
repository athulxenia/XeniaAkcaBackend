
using Microsoft.EntityFrameworkCore;
using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Models;
using XeniaTokenBackend.Repositories.Token;
using XeniaTokenBackend.Service.Common;


namespace XeniaTokenBackend.Repositories.Department
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly CommonService _commonService;


        public DepartmentRepository(ApplicationDbContext context, IConfiguration configuration,CommonService commonService)
        {
            _context = context;  
            _commonService = commonService;
        }


        public async Task<List<DepartmentDto>> GetDepartmentWebByIdAsync(int userId)
        {
            var baseQuery = await (from d in _context.xtm_Department
                                   join um in _context.xtm_UserMap on d.DepID equals um.DepID
                                   where um.UserID == userId && um.Status
                                   select new
                                   {
                                       d.DepID,
                                       d.DepName,
                                       d.CompanyID,
                                       d.DepPrefix,
                                       d.DepExpire,
                                       d.Status
                                   }).ToListAsync();

            var depIds = baseQuery.Select(x => x.DepID).ToList();

            var counters = await _context.xtm_Counter
                .Where(c => depIds.Contains(c.DepID) && c.Status == true) 
                .Select(c => new
                {
                    c.CounterID,
                    c.CounterName,
                    c.Status,
                    c.DepID
                })
                .ToListAsync();

            var departments = baseQuery
                .Select(d => new DepartmentDto
                {
                    DepID = d.DepID,
                    DepName = d.DepName,
                    CompanyID = d.CompanyID,
                    DepPrefix = d.DepPrefix,
                    DepExpire = d.DepExpire,
                    Status = d.Status,
                    Counters = counters
                        .Where(c => c.DepID == d.DepID)
                        .Select(c => new xtm_Counter
                        {
                            CounterID = c.CounterID,
                            CounterName = c.CounterName,
                            Status = c.Status
                        })
                        .ToList()
                })
                .Where(d => d.Counters.Any()) 
                .ToList();

            return departments;
        }

        public async Task<List<xtm_Department>> GetDepartmentWebAll(int companyId)
        {
            var departments = await _context.xtm_Department
                                            .Where(d => d.CompanyID == companyId && d.Status == true)
                                            .AsNoTracking() 
                                            .ToListAsync();

            return departments;
        }

        public async Task<object> CreateDepartmentAsync(CreateDepartmentRequestDto dto)
        {
            bool departmentExists = await _context.xtm_Department
                .AnyAsync(d => d.DepName == dto.DepName);

            if (departmentExists)
                throw new Exception("Department already exists");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var department = new xtm_Department
                {
                    CompanyID = dto.CompanyID,
                    DepName = dto.DepName,
                    DepPrefix = dto.DepPrefix,
                    DepExpire = _commonService.CalculateValidityDate(),
                    Status = dto.Status
                };

                _context.xtm_Department.Add(department);
                await _context.SaveChangesAsync();

                var tokenMaster = new xtm_TokenMaster
                {
                    CompanyID = dto.CompanyID,
                    DepID = department.DepID,
                    PrintTokenValue = 0,
                    TriggerValue = 0,
                    MaximumToken = dto.MaximumToken
                };

                _context.xtm_TokenMaster.Add(tokenMaster);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new
                {
                    DepID = department.DepID
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<object> UpdateDepartmentAsync(int depId, UpdateDepartmentRequestDto dto)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
 
            bool prefixExists = await _context.xtm_Department
                .AnyAsync(d =>
                    d.CompanyID == dto.CompanyID &&
                    d.DepID != depId &&
                    (
                        d.DepPrefix == dto.DepPrefix ||
                        (dto.DepPrefix == null && d.DepPrefix == null)
                    )
                );

            if (prefixExists)
                throw new Exception("Department prefix already exists for this company.");

  
            var department = await _context.xtm_Department
                .FirstOrDefaultAsync(d => d.DepID == depId);

            if (department == null)
                throw new Exception("Department not found.");

            department.DepName = dto.DepName;
            department.DepPrefix = dto.DepPrefix;
            department.CompanyID = dto.CompanyID;
            department.Status = dto.Status;

  
            var tokenMaster = await _context.xtm_TokenMaster
                .FirstOrDefaultAsync(t => t.DepID == depId);

            if (tokenMaster != null)
            {
                tokenMaster.MaximumToken = dto.MaximumToken;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new
            {
                success = true,
                message = "Department updated successfully"
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

        public async Task<List<xtm_Department>> GetAllDepartmentsAsync(string? depNameSearch = null)
        {
            var query = _context.xtm_Department.AsQueryable();

            if (!string.IsNullOrWhiteSpace(depNameSearch))
            {
                query = query.Where(d => d.DepName.Contains(depNameSearch));
            }

            return await query.ToListAsync();
        }

        public async Task<object> GetAllDepartmentsByCompanyAsync(int companyId)
        {
            var departments = await (
                from d in _context.xtm_Department
                join t in _context.xtm_TokenMaster
                    on d.DepID equals t.DepID into tg
                from t in tg.DefaultIfEmpty()
                where d.CompanyID == companyId
                select new DepartmentWithTokenDto
                {
                    DepID = d.DepID,
                    CompanyID = d.CompanyID,
                    DepName = d.DepName,
                    DepPrefix = d.DepPrefix,
                    DepExpire = d.DepExpire,
                    Status = d.Status,
                    MaximumToken = t != null ? t.MaximumToken : null
                }
            ).ToListAsync();

            if (departments.Any())
                return departments;

            return 0;
        }

        public async Task<DepartmentResponseDto> GetAllDepartmentsAppByUserIdAsync(int userId)
        {
            var now = DateTime.UtcNow;

            var departmentList = await (
                from d in _context.xtm_Department
                join um in _context.xtm_UserMap on d.DepID equals um.DepID
                join tm in _context.xtm_TokenMaster on d.DepID equals tm.DepID into tmj
                from tm in tmj.DefaultIfEmpty()
                where um.UserID == userId
                      && um.Status == true
                      && d.DepExpire > now
                select new
                {
                    d.DepID,
                    d.CompanyID,
                    d.DepName,
                    d.DepPrefix,
                    d.DepExpire,
                    d.Status,
                    MaxToken = tm != null ? tm.MaximumToken : 0,
                    printTokenValue = tm != null ? tm.PrintTokenValue : 0,
                    isService = _context.xtm_Service
                        .Any(s => s.SerDepID == d.DepID && s.SerStatus == true)
                }
            ).ToListAsync();

            if (!departmentList.Any())
            {
                return new DepartmentResponseDto
                {
                    status = "success",
                    department = new List<DepartmentAppDto>()
                };
            }

            var depIds = departmentList.Select(d => d.DepID).ToList();

            var serviceList = await _context.xtm_Service
                .Where(s => depIds.Contains(s.SerDepID) && s.SerStatus == true)
                .Select(s => new
                {
                    s.SerDepID,
                    s.SerID,
                    s.SerName
                })
                .ToListAsync();

            var servicesByDepartment = serviceList
                .GroupBy(s => s.SerDepID)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(s => new ServiceDto
                    {
                        ServiceID = s.SerID,
                        ServiceName = s.SerName
                    }).ToList()
                );

            var departments = departmentList.Select(d => new DepartmentAppDto
            {
                DepID = d.DepID,
                CompanyID = d.CompanyID,
                DepName = d.DepName,
                DepPrefix = d.DepPrefix,
                DepExpire = d.DepExpire.ToString("yyyy-MM-ddTHH:mm:ss"), 
                MaxToken = d.MaxToken,
                printTokenValue = d.printTokenValue,
                Status = d.Status,
                isService = d.isService,
                services = servicesByDepartment.ContainsKey(d.DepID)
                    ? servicesByDepartment[d.DepID]
                    : null
            }).ToList();

            return new DepartmentResponseDto
            {
                status = "success",
                department = departments
            };
        }



        public async Task<int> DeleteDepartmentAsync(int depId)
        {
            var department = await _context.xtm_Department
                .FirstOrDefaultAsync(d => d.DepID == depId);

            if (department == null)
                return 0;

            _context.xtm_Department.Remove(department);
            var rowsAffected = await _context.SaveChangesAsync();

            return rowsAffected;
        }

    }

}



