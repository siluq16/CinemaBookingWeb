using System;
using System.Collections.Generic;

namespace CinemaBookingWeb.Models;

public partial class GheNgoi
{
    public string MaGhe { get; set; } = null!;

    public string HangGhe { get; set; } = null!;

    public int CotGhe { get; set; }

    public string LoaiGhe { get; set; } = null!;

    public string MaPhong { get; set; } = null!;

    public virtual PhongChieu MaPhongNavigation { get; set; } = null!;

    public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
}
