using System;
using System.Collections.Generic;

namespace CinemaBookingWeb.Models;

public partial class DoAn
{
    public string MaDoAn { get; set; } = null!;

    public string TenDoAn { get; set; } = null!;

    public decimal DonGia { get; set; }

    public string? Anh { get; set; }

    public string? MoTa { get; set; }

    public virtual ICollection<ChiTietDoAn> ChiTietDoAns { get; set; } = new List<ChiTietDoAn>();
}
