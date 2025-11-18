using System;
using System.Collections.Generic;

namespace CinemaBookingWeb.Models;

public partial class Phim
{
    public string MaPhim { get; set; } = null!;

    public string TenPhim { get; set; } = null!;

    public string TheLoai { get; set; } = null!;

    public string? DienVien { get; set; }

    public int ThoiLuong { get; set; }

    public DateOnly NgayKhoiChieu { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public string? DaoDien { get; set; }

    public string? Trailer { get; set; }

    public string? Poster { get; set; }

    public string? MoTa { get; set; }

    public int? DoTuoi { get; set; }

    public virtual ICollection<LichChieu> LichChieus { get; set; } = new List<LichChieu>();
}
