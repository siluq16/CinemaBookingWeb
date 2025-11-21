using CinemaBookingWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;   
using CinemaBookingWeb.Models;

namespace CinemaBookingWeb.Controllers
{
    public class ContactController : Controller
    {
        private readonly CinemaBookingWebContext _context;
        public ContactController(CinemaBookingWebContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendFeedback(FeedbackViewModel model)
        {
            if (ModelState.IsValid)
            {
                var lienHeMoi = new LienHe
                {
                    TenNguoiGui = model.Name,
                    Email = model.Email,
                    SoDienThoai = model.Phone,
                    NoiDung = model.Message,
                    NgayGui = DateTime.Now,
                    TrangThai = false
                };
                _context.Add(lienHeMoi);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cảm ơn bạn đã phản hồi. Chúng tôi sẽ liên hệ lại sớm nhất!";

                return LocalRedirect("/Contact/Index#lienhe");
            }

            TempData["Error"] = "Vui lòng kiểm tra lại thông tin.";
            return LocalRedirect("/Contact/Index#lienhe");
        }

    }
}
