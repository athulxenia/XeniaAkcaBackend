using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using XeniaCatalogueApi.Service.Common;
using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Models;
using XeniaTokenBackend.Repositories.Token;

namespace XeniaTokenBackend.Repositories.Company
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtHelperService _jwtHelperService;

        public CompanyRepository(ApplicationDbContext context, IConfiguration configuration, JwtHelperService jwtHelperService)
        {
            _context = context;
            _jwtHelperService = jwtHelperService;
        }

        public async Task<int> CreateCompanyAsync(CreateCompanyDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
        
                if (await _context.xtm_Company.AnyAsync(c => c.CompanyName == dto.CompanyName))
                    throw new Exception("Company already exists");

          
                if (await _context.xtm_Users.AnyAsync(u => u.Username == dto.Username))
                    throw new Exception("Username already exists");

          
                var company = new xtm_Company
                {
                    CompanyName = dto.CompanyName,
                    LicenseKey = dto.LicenseKey,
                    Status = true,
                    Country = dto.Country,
                    Address = dto.Address,
                    Email = dto.Email,
                    Validity = DateTime.UtcNow.AddDays(14),
                    IsExpired = false
                };
                _context.xtm_Company.Add(company);
                await _context.SaveChangesAsync();

      
                var settings = new xtm_CompanySettings
                {
                    CompanyID = company.CompanyID,
                    CollectCustomerName = false,
                    PrintCustomerName = false,
                    CollectCustomerMobileNumber = false,
                    PrintCustomerMobileNumber = false,
                    IsCustomCall = false
                };
                _context.xtm_CompanySettings.Add(settings);
                await _context.SaveChangesAsync();

     
                var department = new xtm_Department
                {
                    CompanyID = company.CompanyID,
                    DepName = dto.CompanyName,
                    DepExpire = DateTime.UtcNow.AddDays(14),
                    DepPrefix = dto.DepPrefix,
                    Status = true
                };
                _context.xtm_Department.Add(department);
                await _context.SaveChangesAsync();


                var user = new xtm_Users
                {
                    CompanyID = company.CompanyID,
                    Username = dto.Username,
                    Password = dto.Password,
                    TokenResetAllowed = true,
                    UserType = "Administrator",
                    Status = true
                };
                _context.xtm_Users.Add(user);
                await _context.SaveChangesAsync();

         
                var userMap = new xtm_UserMap
                {
                    UserID = user.UserID,
                    DepID = department.DepID,
                    Status = true
                };
                _context.xtm_UserMap.Add(userMap);
                await _context.SaveChangesAsync();

           
                var tokenMaster = new xtm_TokenMaster
                {
                    CompanyID = company.CompanyID,
                    DepID = department.DepID,
                    PrintTokenValue = 0,
                    TriggerValue = 0,
                    MaximumToken = 999
                };
                _context.xtm_TokenMaster.Add(tokenMaster);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return company.CompanyID;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> UpdateCompanyAsync(int companyId, UpdateCompanyDto dto)
        {
            var company = await _context.xtm_Company
                .FirstOrDefaultAsync(c => c.CompanyID == companyId);

            if (company == null)
                throw new Exception("Company not found");

            company.CompanyName = dto.CompanyName;
            company.LicenseKey = dto.LicenseKey;
            company.Status = dto.Status;
            company.Country = dto.Country;
            company.Address = dto.Address;
            company.Email = dto.Email;
            company.Validity = DateTime.UtcNow;
            company.IsExpired = dto.IsExpired;

            var rowsAffected = await _context.SaveChangesAsync();
            return rowsAffected;
        }

        public async Task<List<xtm_Company>> GetAllCompanyAsync(string search = "")
        {
            IQueryable<xtm_Company> query = _context.xtm_Company;

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(c =>
                    c.CompanyName.ToLower().Contains(search) ||
                    c.LicenseKey.ToLower().Contains(search) ||
                    c.Address.ToLower().Contains(search) ||
                    c.Country.ToLower().Contains(search) ||
                    c.Email.ToLower().Contains(search));
            }

            return await query.ToListAsync();
        }

        public async Task<xtm_Company?> GetCompanyByIdAsync(int companyId)
        {
            var company = await _context.xtm_Company
                .FirstOrDefaultAsync(c => c.CompanyID == companyId);

            return company; 
        }

        public async Task<int> UpdateCompanySettingsAsync(int companySettingId, CompanySettingsUpdateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var settings = await _context.xtm_CompanySettings
                    .FirstOrDefaultAsync(x => x.CompSettingID == companySettingId);

                if (settings == null)
                    throw new Exception("Company settings not found");

                settings.CompanyID = dto.CompanyId;
                settings.CollectCustomerName = dto.CollectCustomerName;
                settings.PrintCustomerName = dto.PrintCustomerName;
                settings.CollectCustomerMobileNumber = dto.CollectCustomerMobileNumber;
                settings.PrintCustomerMobileNumber = dto.PrintCustomerMobileNumber;
                settings.IsCustomCall = dto.IsCustomCall;
                settings.IsServiceEnable = dto.IsServiceEnable;

                var rows = await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return rows;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<object> GetAllCompanySettingsAsync(int companyId, int userId)
        {
            var now = DateTime.UtcNow;

            var companySettings = await _context.xtm_CompanySettings
                .Where(c => c.CompanyID == companyId)
                .Select(c => new CompanySettingsDto
                {
                    CompSettingID = c.CompSettingID,
                    CompanyID = c.CompanyID,
                    CollectCustomerName = c.CollectCustomerName,
                    PrintCustomerName = c.PrintCustomerName,
                    CollectCustomerMobileNumber = c.CollectCustomerMobileNumber,
                    PrintCustomerMobileNumber = c.PrintCustomerMobileNumber,
                    IsCustomCall = c.IsCustomCall,
                    IsServiceEnable = c.IsServiceEnable,
                    hasExpiredDepartments = false 
                })
                .FirstOrDefaultAsync();

  
            var departmentData =
                from tm in _context.xtm_TokenMaster
                join um in _context.xtm_UserMap on tm.DepID equals um.DepID
                join d in _context.xtm_Department on tm.DepID equals d.DepID
                where um.UserID == userId
                      && um.Status == true
                      && d.CompanyID == companyId
                select new
                {
                    d.DepID,
                    d.DepName,
                    d.DepPrefix,
                    d.DepExpire,
                    tm.MaximumToken,
                    tm.PrintTokenValue,
                    isService = _context.xtm_Service.Any(s =>
                        s.SerDepID == d.DepID && s.SerStatus == true)
                };

            var departments = await departmentData.ToListAsync();

            bool hasExpiredDepartments = departments.Any(d => d.DepExpire <= now);

     
            var services = await _context.xtm_Service
                .Where(s => s.SerStatus == true)
                .Select(s => new
                {
                    s.SerDepID,
                    s.SerID,
                    s.SerName
                })
                .ToListAsync();

            var servicesByDepartment = services
                .GroupBy(s => s.SerDepID)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(s => new ServiceSettingsDto
                    {
                        ServiceID = s.SerID,
                        ServiceName = s.SerName
                    }).ToList()
                );

            var departmentSettings = departments.Select(d => new DepartmentSettingsDto
            {
                DepID = d.DepID,
                DepName = d.DepName,
                DepPrefix = d.DepPrefix,
                DepExpire = d.DepExpire.ToString("yyyy-MM-dd"), 
                maxToken = d.MaximumToken,
                printToken = d.PrintTokenValue,
                isService = d.isService,
                services = servicesByDepartment.ContainsKey(d.DepID)
                    ? servicesByDepartment[d.DepID]
                    : new List<ServiceSettingsDto>()
            }).ToList();

            if (companySettings != null)
                companySettings.hasExpiredDepartments = hasExpiredDepartments;

            return new CompanySettingsResponseDto
            {
                status = "success",
                DepartmentSettings = departmentSettings,
                companySettings = companySettings
            };
        }

    }
}
