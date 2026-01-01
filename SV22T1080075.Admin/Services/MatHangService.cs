using Admin.Extensions;
using Admin.Models;
using DataAccessTool;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Admin.Services
{
    public class MatHangService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const int PageSize = 10;

        public MatHangService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> SaveImageAsync(IFormFile image)
        {
            string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "MatHang");
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

        public async Task<PaginatedList<MatHang>> GetPagedListAsync(MatHangSearchModel search)
        {
            var query = _context.MatHangs.Include(m => m.LoaiHang).AsQueryable();

            if (!string.IsNullOrEmpty(search.SearchName))
                query = query.Where(m => m.TenMatHang.Contains(search.SearchName));

            if (search.MaLoai.HasValue)
                query = query.Where(m => m.MaLoaiHang == search.MaLoai);

            if (search.GiaMin.HasValue)
                query = query.Where(m => m.GiaBan >= search.GiaMin);

            if (search.GiaMax.HasValue)
                query = query.Where(m => m.GiaBan <= search.GiaMax);

            int count = await query.CountAsync();

            var items = await query
                .OrderBy(m => m.MaMatHang)
                .Skip((search.Page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return new PaginatedList<MatHang>(items, count, search.Page, PageSize);
        }
    }
}