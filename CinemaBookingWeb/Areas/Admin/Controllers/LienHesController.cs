using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaBookingWeb.Models;

namespace CinemaBookingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LienHesController : Controller
    {
        private readonly CinemaBookingWebContext _context;

        public LienHesController(CinemaBookingWebContext context)
        {
            _context = context;
        }

        // GET: Admin/LienHes
        public async Task<IActionResult> Index()
        {
            var listLienHe = await _context.LienHes
                                           .OrderByDescending(x => x.NgayGui) // Sắp xếp ngày giảm dần
                                           .ToListAsync();
            return View(listLienHe);
        }

        public async Task<IActionResult> DoiTrangThai(int id)
        {
            var item = await _context.LienHes.FindAsync(id);
            if (item != null)
            {
                item.TrangThai = !item.TrangThai;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Xoa(int id)
        {
            var item = await _context.LienHes.FindAsync(id);
            if (item != null)
            {
                _context.LienHes.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
