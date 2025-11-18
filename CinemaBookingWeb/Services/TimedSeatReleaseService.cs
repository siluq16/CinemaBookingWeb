using Microsoft.EntityFrameworkCore;
using CinemaBookingWeb.Models;
using Timer = System.Threading.Timer;

namespace CinemaBookingWeb.Services
{
    public class TimedSeatReleaseService : IHostedService, IDisposable
    {
        private readonly ILogger<TimedSeatReleaseService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer = null;

        public TimedSeatReleaseService(ILogger<TimedSeatReleaseService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Dịch vụ giải phóng ghế hết hạn đang chạy.");
            _timer = new Timer(CheckAndReleaseSeats, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
            return Task.CompletedTask;
        }

        private async void CheckAndReleaseSeats(object? state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CinemaBookingWebContext>();
                var expiredTime = DateTime.Now;

                var expiredHolds = await context.HoaDons
                    .Include(h => h.Ves)         // Tải các Vé liên quan
                    .Include(h => h.ChiTietDoAns) // Tải ChiTietDoAn liên quan
                    .Where(h => h.TrangThai == "Holding" && h.ThoiGianHetHan.HasValue && h.ThoiGianHetHan.Value < expiredTime) // Thêm .HasValue cho an toàn
                    .ToListAsync();

                if (expiredHolds.Count > 0)
                {
                    _logger.LogWarning($"Tìm thấy {expiredHolds.Count} giao dịch hết hạn. Tiến hành giải phóng ghế.");

                    foreach (var hoaDon in expiredHolds)
                    {
                        context.ChiTietDoAns.RemoveRange(hoaDon.ChiTietDoAns);

                        context.Ves.RemoveRange(hoaDon.Ves);

                        context.HoaDons.Remove(hoaDon);
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation($"Đã giải phóng thành công {expiredHolds.Count} giao dịch và ghế tương ứng.");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Dịch vụ giải phóng ghế hết hạn đang dừng.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
