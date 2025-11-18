using System.Diagnostics;
using CinemaBookingWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBookingWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly CinemaBookingWebContext _context;

        public HomeController(CinemaBookingWebContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var top5Hot =
                (from p in _context.Phims
                 join lc in _context.LichChieus on p.MaPhim equals lc.MaPhim
                 join v in _context.Ves on lc.MaLich equals v.MaLich into gVes
                 from v in gVes.DefaultIfEmpty()
                 where p.NgayKhoiChieu <= today && p.NgayKetThuc >= today
                 group v by p into g
                 orderby g.Count(x => x != null) descending
                 select g.Key       // ❗ chỉ lấy PHIM, không lấy số vé
                )
                .Take(5)
                .ToList();


            var dangChieu = _context.Phims
                .Where(p => p.NgayKhoiChieu <= today)
                .OrderBy(p => p.NgayKhoiChieu)
                .ToList();

            ViewBag.FeaturedMovies = top5Hot;
            ViewBag.AllMovies = dangChieu;

            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = 403;
            ViewData["Title"] = "Truy Cập Bị Từ Chối";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
