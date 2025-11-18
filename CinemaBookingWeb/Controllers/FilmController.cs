using CinemaBookingWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingWeb.Controllers
{
    public class FilmController : Controller
    {
        private readonly CinemaBookingWebContext _context;

        public FilmController(CinemaBookingWebContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var phimDangChieu = await _context.Phims
                .Where(p =>
                    p.NgayKhoiChieu <= today && p.NgayKetThuc >= today
                )
                .ToListAsync();

            return View(phimDangChieu);
        }
        public async Task<IActionResult> Detail(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var film = await _context.Phims.FirstOrDefaultAsync(p => p.MaPhim == id);
            if (film == null)
                return NotFound();

            return View(film);
        }

        // API lấy phim sắp chiếu (cho AJAX)
        [HttpGet]
        public async Task<IActionResult> GetUpcomingMovies()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var upcoming = await _context.Phims
                .Where(p => p.NgayKhoiChieu > today)
                .Select(p => new
                {
                    p.MaPhim,
                    p.TenPhim,
                    p.Poster,
                    p.TheLoai,
                    p.DoTuoi,
                    p.NgayKhoiChieu
                })
                .ToListAsync();

            return Json(upcoming);
        }
    }
}
