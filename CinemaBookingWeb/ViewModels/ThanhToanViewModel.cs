namespace CinemaBookingWeb.ViewModels
{
    public class ThanhToanViewModel
    {
        public string MaLichChieu { get; set; } = string.Empty;
        public List<string> DanhSachGhe { get; set; } = new();
        public List<ComboItem> ComboList { get; set; } = new();
        public decimal TongTien { get; set; }
        public string MaKH { get; set; } = string.Empty;
        public string MaKM { get; set; } = string.Empty;
        public string PhuongThucThanhToan { get; set; }
    }

    public class ComboItem
    {
        public string MaDoAn { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }
    }
}
