using System;
using System.Collections.Generic;

namespace CinemaBookingWeb.Models;

public partial class PhongChieu
{
    public string MaPhong { get; set; } = null!;

    public string TenPhong { get; set; } = null!;

    public int SoHangGhe { get; set; }

    public int SoGheMoiHang { get; set; }

    public int? SoLuongGhe { get; set; }

    public virtual ICollection<GheNgoi> GheNgois { get; set; } = new List<GheNgoi>();

    public virtual ICollection<LichChieu> LichChieus { get; set; } = new List<LichChieu>();
}
