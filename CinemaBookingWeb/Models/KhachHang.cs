using System;
using System.Collections.Generic;

namespace CinemaBookingWeb.Models;

public partial class KhachHang
{
    public string MaKh { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateOnly? NgaySinh { get; set; }

    public DateTime NgayTao { get; set; }

    public string? Avatar { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
}
