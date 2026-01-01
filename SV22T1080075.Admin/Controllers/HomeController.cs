using Admin.Models;
using DataAccessTool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Lấy số liệu thống kê
            var viewModel = new DashboardViewModel
            {
                TongDonHang = await _context.DonHangs.CountAsync(),
                TongSanPham = await _context.MatHangs.CountAsync(),
                TongKhachHang = await _context.KhachHangs.CountAsync(),
                // Tính tổng tiền các đơn hàng (Chỉ tính đơn đã hoàn thành/không bị hủy nếu cần logic chặt chẽ hơn)
                DoanhThu = await _context.DonHangs.SumAsync(d => d.TongTien) ?? 0
            };

            // 2. Lấy 5 đơn hàng mới nhất
            viewModel.DonHangMoiNhat = await _context.DonHangs
                .Include(d => d.KhachHang)
                .Include(d => d.TrangThaiDon)
                .OrderByDescending(d => d.NgayDat)
                .Take(5)
                .ToListAsync();

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}