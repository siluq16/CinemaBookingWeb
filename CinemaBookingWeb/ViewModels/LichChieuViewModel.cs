namespace CinemaBookingWeb.ViewModels
{
    public class LichChieuItem
    {
        public string? MaLich { get; set; }
        public string? TenPhong { get; set; }
        public DateOnly NgayChieu { get; set; }
        public TimeOnly GioBatDau { get; set; }
        public TimeOnly GioKetThuc { get; set; }
        public string? DinhDang { get; set; }
    }

    public class LichChieuViewModel
    {
        public string? MaPhim { get; set; }
        public string? TenPhim { get; set; }
        public List<DateOnly> NgayChieuKhac { get; set; } = new();
        public List<LichChieuItem> LichChieuTrongNgay { get; set; } = new();
    }
}
