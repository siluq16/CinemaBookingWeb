using System;
using System.Collections.Generic;

namespace CinemaBookingWeb.Models;

public partial class KhuyenMai
{
    public string MaKm { get; set; } = null!;

    public string TenKm { get; set; } = null!;

    public string? MoTa { get; set; }

    public int? PhanTramGiam { get; set; }

    public decimal? SoTienGiam { get; set; }

    public DateOnly NgayBatDau { get; set; }

    public DateOnly NgayKetThuc { get; set; }

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
}
