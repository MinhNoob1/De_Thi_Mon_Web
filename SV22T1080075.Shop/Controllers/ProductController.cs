using BusinessLayers;
using BusinessLayers.Shared;
using DataAccessTool;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.SearchView;

namespace SV22T1080075.Shop.Controllers
{
    public class ProductController : Controller
    {
        private readonly MatHangService _matHangService;
        private readonly ApplicationDbContext _context;

        private const string PRODUCT_SEARCH_SESSION = "Shop_Product_Search_Condition";

        public ProductController(MatHangService matHangService, ApplicationDbContext context)
        {
            _matHangService = matHangService;
            _context = context;
        }

        public async Task<IActionResult> Index(MatHangSearchModel search)
        {
            // --- LOGIC XỬ LÝ SESSION ---

            // Trường hợp 1: Nếu search.Page == 0 nghĩa là người dùng vào trang Product từ Menu 
            // (không có tham số trên URL) -> Ta sẽ lấy lại trạng thái cũ từ Session.
            if (search.Page == 0)
            {
                var searchCondition = HttpContext.Session.GetObject<MatHangSearchModel>(PRODUCT_SEARCH_SESSION);
                if (searchCondition != null)
                {
                    search = searchCondition;
                }
                else
                {
                    // Nếu chưa có session thì thiết lập mặc định
                    search.Page = 1;
                    search.PageSize = 12;
                }
            }
            else
            {
                // Trường hợp 2: Người dùng vừa bấm nút Tìm kiếm/Chuyển trang/Sắp xếp
                // (có tham số gửi lên) -> Ta sẽ lưu trạng thái mới này vào Session.

                if (search.PageSize <= 0) search.PageSize = 12; // Đảm bảo PageSize luôn đúng
                HttpContext.Session.SetObject(PRODUCT_SEARCH_SESSION, search);
            }

            // 1. Xử lý phân trang mặc định
            if (search.Page == 0) search.Page = 1;
            if (search.PageSize == 0) search.PageSize = 12; // Shop thường hiện 12 sp/trang

            // 2. Gọi Service lấy danh sách sản phẩm (chỉ lấy hàng đang bán)
            var model = await _matHangService.GetPagedListAsync(search, onlyActive: true);

            // 3. Lấy danh sách loại hàng để hiển thị bên Sidebar
            ViewBag.LoaiHangs = await _context.LoaiHangs.ToListAsync();

            // 4. Lưu lại SearchModel để giữ trạng thái lọc trên View
            ViewBag.SearchModel = search;

            return View(model);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var matHang = await _matHangService.GetByIdAsync(id);
            if (matHang == null || matHang.DangBan == false)
            {
                return NotFound();
            }
            return View(matHang);
        }
    }
}