using Microsoft.EntityFrameworkCore;
using XeniaAkcaBackend.Dto;
using XeniaAkcaBackend.Repositories.Districts;
using XeniaAkcaBackend.Models;

namespace XeniaAkcaBackend.Repositories
{
    public class DistrictRepository : IDistrictRepository
    {
        private readonly ApplicationDbContext _context;

        public DistrictRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<District>> GetAllDistrictAsync()
        {
            return await _context.Districts.ToListAsync();
        }

        public async Task<DistrictResponse> UpdateDistrictAsync(int districtId, UpdateDistrictRequest request)
        {
            var district = await _context.Districts
                .FirstOrDefaultAsync(d => d.DistrictId == districtId);

            if (district == null)
                return new DistrictResponse { Status = "failure", Message = "District not found." };

            district.ContactPerson1 = request.ContactPerson1 ?? district.ContactPerson1;
            district.ContactNumber1 = request.ContactNumber1 ?? district.ContactNumber1;
            district.EmailAddress1 = request.EmailAddress1 ?? district.EmailAddress1;
            district.ContactPerson2 = request.ContactPerson2 ?? district.ContactPerson2;
            district.ContactNumber2 = request.ContactNumber2 ?? district.ContactNumber2;
            district.Status = request.Status;

            var districtRows = await _context.SaveChangesAsync();

            if (districtRows == 0)
                return new DistrictResponse { Status = "failure", Message = "District update failed." };

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserDistrictId == districtId);

            if (user == null)
                return new DistrictResponse
                {
                    Status = "partial success",
                    Message = "District updated, but user not found."
                };

            user.UserName = request.ContactPerson1;
            user.Password = request.Password;

            var userRows = await _context.SaveChangesAsync();

            return userRows > 0
                ? new DistrictResponse { Status = "success", Message = "District updated successfully." }
                : new DistrictResponse { Status = "partial success", Message = "District updated, but user update failed." };
        }
    }
}