using System;
using System.Collections.Generic;

namespace CinemaBookingWeb.Models;

public partial class GiaVe
{
    public string MaGia { get; set; } = null!;

    public string LoaiGhe { get; set; } = null!;

    public string LoaiNgay { get; set; } = null!;

    public decimal Gia { get; set; }
}
