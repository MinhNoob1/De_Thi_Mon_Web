using Admin.Models;
using DataAccessTool;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Admin.Services
{
    public class KhachHangService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const int PageSize = 10;

        public KhachHangService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<string> SaveImageAsync(IFormFile image)
        {
            string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "KhachHang");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            string filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return fileName;
        }
        public async Task<PaginatedList<KhachHang>> GetPagedListAsync(KhachHangSearchModel search)
        {
            var query = _context.KhachHangs
                        .Include(k => k.TinhThanh)
                        .AsQueryable();

            if (!string.IsNullOrEmpty(search.Keyword))
            {
                query = query.Where(k => k.HoTen.Contains(search.Keyword)
                                      || (k.DienThoai != null && k.DienThoai.Contains(search.Keyword)));
            }

            if (!string.IsNullOrEmpty(search.DiaChi))
            {
                query = query.Where(k => k.DiaChi != null && k.DiaChi.Contains(search.DiaChi));
            }

            int count = await query.CountAsync();

            var items = await query
                .OrderByDescending(k => k.MaKhachHang)
                .Skip((search.Page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return new PaginatedList<KhachHang>(items, count, search.Page, PageSize);
        }
    }
}