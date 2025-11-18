using System;
using System.Collections.Generic;

namespace CinemaBookingWeb.Models;

public partial class Ve
{
    public string MaVe { get; set; } = null!;

    public string MaLich { get; set; } = null!;

    public string MaGhe { get; set; } = null!;

    public string MaHd { get; set; } = null!;

    public decimal GiaVe { get; set; }

    public virtual GheNgoi MaGheNavigation { get; set; } = null!;

    public virtual HoaDon MaHdNavigation { get; set; } = null!;

    public virtual LichChieu MaLichNavigation { get; set; } = null!;
}
