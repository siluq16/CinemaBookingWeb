namespace CinemaBookingWeb.Areas.Admin.Models
{
    public class DashBoardViewModel
    {
        public decimal DoanhThuNgay { get; set; }
        public decimal TongDoanhThu { get; set; }
        public int KhachHangMoi { get; set; }
        public int VeBanRa { get; set; }
        public int Ngay { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }

        public List<DoanhThuPhimVM>? DoanhThuPhim { get; set; }
        public List<DoanhThuThangVM>? DoanhThuPhong { get; set; }
        public List<DoanhThuChiTietPhim> DoanhThuChiTietPhim { get; set; } = new();
    }

    public class DoanhThuPhimVM
    {
        public string? TenPhim { get; set; }
        public decimal TongDoanhThu { get; set; }
    }

    public class DoanhThuThangVM
    {
        public string? Thang { get; set; }
        public decimal TongDoanhThu { get; set; }
    }

    public class DoanhThuChiTietPhim
    {
        public string TenPhim { get; set; } = string.Empty;
        public decimal TongDoanhThu { get; set; }
        public int TongVeBanRa { get; set; }
    }
}
