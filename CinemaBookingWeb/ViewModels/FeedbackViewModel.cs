using System.ComponentModel.DataAnnotations;

namespace CinemaBookingWeb.ViewModels
{
    public class FeedbackViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
        public string? Message { get; set; }
    }
}
