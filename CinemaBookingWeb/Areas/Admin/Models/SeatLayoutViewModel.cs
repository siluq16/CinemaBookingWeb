using CinemaBookingWeb.Models;

namespace CinemaBookingWeb.Areas.Admin.Models
{
    public class SeatLayoutViewModel
    {
        public string? MaPhong { get; set; }
        public List<GheNgoi> GheNgois { get; set; } = new List<GheNgoi>();
    }
}
