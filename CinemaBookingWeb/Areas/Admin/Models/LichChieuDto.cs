namespace CinemaBookingWeb.Areas.Admin.Models
{
    public class LichChieuDto
    {
        public string? MaLich { get; set; }
        public string MaPhim { get; set; } = null!;
        public string MaPhong { get; set; } = null!;
        public string NgayChieu { get; set; } = null!;
        public string GioBatDau { get; set; } = null!;
        public string GioKetThuc { get; set; } = null!;
    }
}
