using Admin.Models;
using Admin.Services;
using DataAccessTool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class LoaiHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LoaiHangService _service;

        public LoaiHangController(ApplicationDbContext context, LoaiHangService service)
        {
            _context = context;
            _service = service;
        }

        public async Task<IActionResult> Index(LoaiHangSearchModel search)
        {
            var model = await _service.GetPagedListAsync(search);
            ViewBag.SearchModel = search;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("List", model);

            return View(model);
        }

        // GET: Create / Edit
        public async Task<IActionResult> Edit(int id = 0)
        {
            if (id == 0) return View(new LoaiHang()); // Tạo mới

            var item = await _context.LoaiHangs.FindAsync(id);
            if (item == null) return NotFound();

            return View(item); // Sửa
        }
        // POST: Create / Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LoaiHang model)
        {
            if (ModelState.IsValid)
            {
                if (model.MaLoaiHang == 0)
                    _context.Add(model);
                else
                    _context.Update(model);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.LoaiHangs.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Không tìm thấy!" });

            // Kiểm tra ràng buộc: Nếu loại hàng đang có sản phẩm thì không cho xóa
            bool hasProducts = _context.MatHangs.Any(m => m.MaLoaiHang == id);
            if (hasProducts)
                return Json(new { success = false, message = "Không thể xóa loại hàng này vì đang có sản phẩm!" });

            _context.LoaiHangs.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}