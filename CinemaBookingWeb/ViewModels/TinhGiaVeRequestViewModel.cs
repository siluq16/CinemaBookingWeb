namespace CinemaBookingWeb.ViewModels
{
    public class TinhGiaVeRequestViewModel
    {
        public string MaLichChieu { get; set; } = null!;
        public List<string> DanhSachLoaiGhe { get; set; } = new List<string>();
    }
}
