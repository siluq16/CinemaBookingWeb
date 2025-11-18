namespace CinemaBookingWeb.Areas.Admin.Models
{
    public class UpdateSeatRowRequest
    {
        public List<SeatUpdateDto> Seats { get; set; } = new List<SeatUpdateDto>();
    }

    public class SeatUpdateDto
    {
        public string? MaGhe { get; set; }
        public string? LoaiGhe { get; set; }
    }
}
