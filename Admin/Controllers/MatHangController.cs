using Admin.Models;
using Azure.Core;
using DataAccessTool;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Admin.Controllers
{
    public class MatHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MatHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MatHang (Danh sách + Tìm kiếm)
        public async Task<IActionResult> Index(string? searchName, int? maLoai, decimal? giaMin, decimal? giaMax, int page = 1)
        {
            int pageSize = 8; // Số sản phẩm mỗi trang

            // 1. Lấy toàn bộ mặt hàng, bao gồm thông tin Loại hàng (Eager Loading)
            var query = _context.MatHangs.Include(m => m.LoaiHang).AsQueryable();

            // 2. Lọc theo tên sản phẩm
            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(m => m.TenMatHang.Contains(searchName));
            }

            // 3. Lọc theo loại hàng
            if (maLoai.HasValue)
            {
                query = query.Where(m => m.MaLoaiHang == maLoai);
            }

            // 4. Lọc theo khoảng giá
            if (giaMin.HasValue)
            {
                query = query.Where(m => m.GiaBan >= giaMin);
            }
            if (giaMax.HasValue)
            {
                query = query.Where(m => m.GiaBan <= giaMax);
            }
            int count = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var model = new PaginatedList<MatHang>(items, count, page, pageSize);

            ViewBag.LoaiHangs = await _context.LoaiHangs.ToListAsync();

            // Nếu là yêu cầu AJAX (từ JavaScript), chỉ trả về phần danh sách sản phẩm
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("List", model);
            }

            return View(model);
        }
        // GET: MatHang/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var matHang = await _context.MatHangs.FindAsync(id);
            if (matHang == null) return NotFound();

            // Load danh mục để hiển thị trong Dropdown
            ViewBag.MaLoaiHang = new SelectList(_context.LoaiHangs, "MaLoaiHang", "TenLoaiHang", matHang.MaLoaiHang);
            return View(matHang);
        }

        // POST: MatHang/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaMatHang,TenMatHang,MaLoaiHang,GiaBan,DonViTinh,HinhAnh,MoTa,SoLuong,DangBan")] MatHang matHang)
        {
            if (id != matHang.MaMatHang) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(matHang);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.MatHangs.Any(e => e.MaMatHang == matHang.MaMatHang)) return NotFound();
                    else throw;
                }
            }
            ViewBag.MaLoaiHang = new SelectList(_context.LoaiHangs, "MaLoaiHang", "TenLoaiHang", matHang.MaLoaiHang);
            return View(matHang);
        }
    }
}
