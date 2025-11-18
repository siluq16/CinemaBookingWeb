using System;
using System.Collections.Generic;

namespace CinemaBookingWeb.Models;

public partial class ChiTietDoAn
{
    public string MaHd { get; set; } = null!;

    public string MaDoAn { get; set; } = null!;

    public int SoLuong { get; set; }

    public decimal ThanhTien { get; set; }

    public virtual DoAn MaDoAnNavigation { get; set; } = null!;

    public virtual HoaDon MaHdNavigation { get; set; } = null!;
}
