using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaKhraBackend.Dto;
using XeniaKhraBackend.Repositories.Payment;

namespace XeniaKhraBackend.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {

        private readonly IPaymentRepository _paymentRepository;


        public PaymentController(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }


     

    }
}
