using Microsoft.AspNetCore.Mvc;

namespace CinemaBookingWeb.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Xử lý phản hồi người dùng (POST)
        [HttpPost]
        public IActionResult SendFeedback([FromBody] FeedbackModel feedback)
        {
            if (feedback == null || string.IsNullOrEmpty(feedback.Name))
                return BadRequest("Thông tin không hợp lệ.");

            // 👉 Lưu hoặc xử lý ở đây
            Console.WriteLine($"Feedback: {feedback.Name} - {feedback.Email} - {feedback.Message}");

            return Ok("Success");
        }

        public class FeedbackModel
        {
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string? Message { get; set; }
        }

    }
}
