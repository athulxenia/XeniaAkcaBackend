using Microsoft.EntityFrameworkCore;
using XeniaAkcaBackend.Models;
using XeniaKhraBackend.Dto;
using XeniaKhraBackend.Models;
using XeniaKhraBackend.Service.Constants;
//using XeniaKhraBackend.Service.Payment;

namespace XeniaKhraBackend.Repositories.Payment
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;
       
        private readonly IConfiguration _configuration;

        public PaymentRepository(ApplicationDbContext context,  IConfiguration configuration)
        {
            _context = context;
      
            _configuration = configuration;
        }

      

    }

}