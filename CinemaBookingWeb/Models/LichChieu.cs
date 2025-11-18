using System;
using System.Collections.Generic;

namespace CinemaBookingWeb.Models;

public partial class LichChieu
{
    public string MaLich { get; set; } = null!;

    public string MaPhim { get; set; } = null!;

    public string MaPhong { get; set; } = null!;

    public DateOnly NgayChieu { get; set; }

    public TimeOnly GioBatDau { get; set; }

    public TimeOnly GioKetThuc { get; set; }

    public virtual Phim MaPhimNavigation { get; set; } = null!;

    public virtual PhongChieu MaPhongNavigation { get; set; } = null!;

    public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
}
