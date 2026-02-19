

using Microsoft.EntityFrameworkCore;
using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Models;


namespace XeniaTokenBackend.Repositories.Counter
{
    public class CounterRepository : ICounterRepository
    {
        private readonly ApplicationDbContext _context;
   

        public CounterRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;      
        }

        public async Task<object> CreateCounterAsync(CreateCounterRequestDto dto)
        {
            bool counterExists = await _context.xtm_Counter.AnyAsync(c =>
                c.CompanyID == dto.CompanyID &&
                c.DepID == dto.DepID &&
                c.CounterName == dto.CounterName);

            if (counterExists)
            {
                throw new Exception("Counter name already exists for this department");
            }

            var counter = new xtm_Counter
            {
                CompanyID = dto.CompanyID,
                DepID = dto.DepID,
                CounterName = dto.CounterName,
                Status = dto.Status
            };

            _context.xtm_Counter.Add(counter);
            var rows = await _context.SaveChangesAsync();

            if (rows > 0)
            {
                return new
                {
                    status = "success",
                    message = "Counter created successfully"
                };
            }

            throw new Exception("Failed to create counter. No rows affected.");
        }

        public async Task<List<CounterResponseDto>> GetCountersByCompanyAsync(int companyId)
        {
            return await (
                from c in _context.xtm_Counter
                join d in _context.xtm_Department on c.DepID equals d.DepID
                where c.CompanyID == companyId
                select new CounterResponseDto
                {
                    CounterID = c.CounterID,
                    CompanyID = c.CompanyID,
                    DepID = c.DepID,
                    CounterName = c.CounterName,
                    Status = c.Status,
                    DepName = d.DepName
                }
            ).ToListAsync();
        }


        public async Task<IEnumerable<CounterDto>> GetCountersByDepartmentAsync(int depId)
        {
            var counters = await (from c in _context.xtm_Counter
                                  join d in _context.xtm_Department on c.DepID equals d.DepID
                                  where c.DepID == depId
                                  select new CounterDto
                                  {
                                      CounterID = c.CounterID,
                                      CounterName = c.CounterName,
                                      Status = c.Status,
                                      DepName = d.DepName
                                  })
                                  .ToListAsync();

            return counters;
        }

        public async Task<object> UpdateCounterAsync(int counterId, UpdateCounterRequestDto dto)
        { 
            var counter = await _context.xtm_Counter
                .FirstOrDefaultAsync(c => c.CounterID == counterId);

            if (counter == null)
            {
                return new
                {
                    status = "error",
                    message = "Counter not found"
                };
            }

            bool counterExists = await _context.xtm_Counter.AnyAsync(c =>
                c.DepID == dto.DepID &&                                   
                c.CounterID != counterId &&                              
                c.CounterName.ToLower() == dto.CounterName.ToLower()     
            );

            if (counterExists)
            {
                return new
                {
                    status = "error",
                    message = $"Counter name '{dto.CounterName}' already exists in this department"
                };
            }

          
            counter.CounterName = dto.CounterName;
            counter.Status = dto.Status;
            counter.DepID = dto.DepID; 

            await _context.SaveChangesAsync();

            return new
            {
                status = "success",
                message = "Counter updated successfully"
            };
        }


        public async Task<object> DeleteCounterAsync(int counterId)
        {
            var counter = await _context.xtm_Counter
                .FirstOrDefaultAsync(c => c.CounterID == counterId);

            if (counter == null)
            {
                throw new Exception("Failed to delete counter. No rows affected.");
            }

            _context.xtm_Counter.Remove(counter);
            await _context.SaveChangesAsync();

            return new
            {
                status = "success",
                message = "Counter deleted successfully"
            };
        }



    }

}



