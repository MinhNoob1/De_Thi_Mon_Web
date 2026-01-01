using Admin.Models;
using DataAccessTool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GioHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: GioHang/Index
        public async Task<IActionResult> Index(int maKhachHang)
        {
            var khachHang = await _context.KhachHangs.FindAsync(maKhachHang);
            if (khachHang == null) return NotFound("Khách hàng không tồn tại");

            var cartItems = new List<GioHangItemViewModel>();

            var viewModel = new GioHangViewModel
            {
                KhachHang = khachHang,
                Items = cartItems
            };

            return View(viewModel);
        }
    }
}