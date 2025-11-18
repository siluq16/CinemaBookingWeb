using System;
using System.Collections.Generic;

namespace CinemaBookingWeb.Models;

public partial class HoaDon
{
    public string MaHd { get; set; } = null!;

    public string MaKh { get; set; } = null!;

    public string? MaKm { get; set; }

    public DateTime NgayLap { get; set; }

    public decimal TongTien { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? PhuongThucThanhToan { get; set; }

    public DateTime? ThoiGianHetHan { get; set; }

    public virtual ICollection<ChiTietDoAn> ChiTietDoAns { get; set; } = new List<ChiTietDoAn>();

    public virtual KhachHang MaKhNavigation { get; set; } = null!;

    public virtual KhuyenMai? MaKmNavigation { get; set; }

    public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
}
