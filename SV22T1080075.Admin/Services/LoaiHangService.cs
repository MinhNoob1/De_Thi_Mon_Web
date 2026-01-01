using Admin.Extensions;
using Admin.Models;
using DataAccessTool;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Admin.Services
{
    public class LoaiHangService
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 20; // Loại hàng thường ít, hiện nhiều hơn chút

        public LoaiHangService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<LoaiHang>> GetPagedListAsync(LoaiHangSearchModel search)
        {
            var query = _context.LoaiHangs.AsQueryable();

            if (!string.IsNullOrEmpty(search.Keyword))
            {
                query = query.Where(x => x.TenLoaiHang.Contains(search.Keyword));
            }

            int count = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.TenLoaiHang)
                .Skip((search.Page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return new PaginatedList<LoaiHang>(items, count, search.Page, PageSize);
        }
    }
}