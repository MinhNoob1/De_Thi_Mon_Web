using Admin.Models;
using Admin.Services;
using DataAccessTool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class TinhThanhController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TinhThanhService _service;

        public TinhThanhController(ApplicationDbContext context, TinhThanhService service)
        {
            _context = context;
            _service = service;
        }

        public async Task<IActionResult> Index(TinhThanhSearchModel search)
        {
            var model = await _service.GetPagedListAsync(search);
            ViewBag.SearchModel = search;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("List", model);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id = 0)
        {
            if (id == 0) return View(new TinhThanh()); // Tạo mới

            var item = await _context.TinhThanhs.FindAsync(id);
            if (item == null) return NotFound();

            return View(item); // Sửa
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TinhThanh model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra ID đã tồn tại chưa
                var exists = await _context.TinhThanhs.AsNoTracking().AnyAsync(x => x.MaTinh == model.MaTinh);

                if (exists)
                {
                    _context.Update(model);
                }
                else
                {
                    // Nếu ID chưa có -> Thêm mới
                    _context.Add(model);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TinhThanhs.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Không tìm thấy!" });

            // 1. Kiểm tra bên Khách Hàng
            bool hasCustomer = await _context.KhachHangs.AnyAsync(k => k.MaTinh == id);
            if (hasCustomer)
                return Json(new { success = false, message = "Không thể xóa: Có khách hàng thuộc tỉnh/thành này!" });

            // 2. Kiểm tra bên Đơn Hàng (Địa chỉ giao hàng)
            bool hasOrder = await _context.DonHangs.AnyAsync(d => d.MaTinh == id);
            if (hasOrder)
                return Json(new { success = false, message = "Không thể xóa: Có đơn hàng giao tới tỉnh/thành này!" });

            _context.TinhThanhs.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}