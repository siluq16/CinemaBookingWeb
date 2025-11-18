using System;
using System.Collections.Generic;

namespace CinemaBookingWeb.Models;

public partial class NhanVien
{
    public string MaNv { get; set; } = null!;

    public string? HoTen { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? GioiTinh { get; set; }

    public string? Sdt { get; set; }

    public string? DiaChi { get; set; }

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
}
