using System;
using System.Collections.Generic;

namespace CinemaBookingWeb.Models;

public partial class Account
{
    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? MaDoiTuong { get; set; }

    public string VaiTro { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public virtual KhachHang? MaDoiTuongNavigation { get; set; }
}
