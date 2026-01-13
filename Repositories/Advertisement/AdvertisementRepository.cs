using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using XeniaCatalogueApi.Service.Common;
using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Models;

namespace XeniaTokenBackend.Repositories.Advertisement
{
    public class AdvertisementRepository : IAdvertisementRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtHelperService _jwtHelperService;

        public AdvertisementRepository(ApplicationDbContext context, IConfiguration configuration, JwtHelperService jwtHelperService)
        {
            _context = context;
            _jwtHelperService = jwtHelperService;
        }

        public async Task<object> CreateAdvertisementAsync(CreateAdvertisementDto dto)
        {
            var advertisement = new xtm_Advertisement
            {
                CompanyID = dto.CompanyID,
                DepID = dto.DepID,
                AdvName = dto.AdvName,
                AdvOrder = dto.AdvOrder,
                AdvFileUrl = dto.AdvFileUrl,
                AdvModifiedDate = dto.AdvModifiedDate,
                AdvModifiedUserID = dto.AdvModifiedUserID,
                Status = dto.Status
            };

            _context.xtm_Advertisement.Add(advertisement);
            var rowsAffected = await _context.SaveChangesAsync();

            if (rowsAffected > 0)
            {
                return new { status = "success", message = "Advertisement created successfully" };
            }

            throw new Exception("Failed to create advertisement. No rows affected.");
        }

        public async Task<List<AdvertisementDto>> GetCompanyAdvertisementsAsync(int companyId)
        {
            var ads = await (from a in _context.xtm_Advertisement
                             join d in _context.xtm_Department
                             on a.DepID equals d.DepID
                             where a.CompanyID == companyId
                             select new AdvertisementDto
                             {
                                 AdvID = a.AdvID,
                                 CompanyID = a.CompanyID,
                                 DepID = a.DepID,
                                 AdvName = a.AdvName,
                                 AdvOrder = a.AdvOrder,
                                 AdvFileUrl = a.AdvFileUrl,
                                 AdvModifiedDate = a.AdvModifiedDate,
                                 AdvModifiedUserID = a.AdvModifiedUserID,
                                 Status = a.Status,
                                 DepName = d.DepName
                             }).ToListAsync();

            return ads;
        }

        public async Task<AdvertisementResponseDto> GetAdvertisementsByUserAsync(int userId)
        {
            var ads = await (
                from a in _context.xtm_Advertisement
                join um in _context.xtm_UserMap on a.DepID equals um.DepID
                join d in _context.xtm_Department on a.DepID equals d.DepID
                where um.UserID == userId && a.Status == true
                select new AdvertisementDto
                {
                    AdvID = a.AdvID,
                    CompanyID = a.CompanyID,
                    DepID = a.DepID,
                    AdvName = a.AdvName,
                    AdvOrder = a.AdvOrder,
                    AdvFileUrl = a.AdvFileUrl,
                    AdvModifiedDate = a.AdvModifiedDate,
                    AdvModifiedUserID = a.AdvModifiedUserID,
                    Status = a.Status,
                    DepName = d.DepName
                }
            ).ToListAsync();

            return new AdvertisementResponseDto
            {
                Status = "success",
                Advertisement = ads
            };
        }

        public async Task<object> UpdateAdvertisementAsync(int advId, UpdateAdvertisementDto dto)
        {
            var advertisement = await _context.xtm_Advertisement
                .FirstOrDefaultAsync(a => a.AdvID == advId);

            if (advertisement == null)
                throw new Exception("Advertisement not found");

            advertisement.CompanyID = dto.CompanyID;
            advertisement.DepID = dto.DepID;
            advertisement.AdvName = dto.AdvName;
            advertisement.AdvOrder = dto.AdvOrder;
            advertisement.AdvFileUrl = dto.AdvFileUrl;
            advertisement.AdvModifiedDate = dto.AdvModifiedDate;
            advertisement.AdvModifiedUserID = dto.AdvModifiedUserID;
            advertisement.Status = dto.Status;

            var rowsAffected = await _context.SaveChangesAsync();

            if (rowsAffected > 0)
            {
                return new { status = "success", message = "Advertisement updated successfully" };
            }

            throw new Exception("Failed to update advertisement. No rows affected.");
        }

        public async Task<object> DeleteAdvertisementAsync(int advId)
        {
            var advertisement = await _context.xtm_Advertisement
                .FirstOrDefaultAsync(a => a.AdvID == advId);

            if (advertisement == null)
                throw new Exception("Advertisement not found");

            _context.xtm_Advertisement.Remove(advertisement);
            var rowsAffected = await _context.SaveChangesAsync();

            if (rowsAffected > 0)
                return new { status = "success", message = "Advertisement deleted successfully" };

            throw new Exception("Failed to delete advertisement. No rows affected.");
        }


    }
}
