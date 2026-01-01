// Services/DonHangService.cs
using Admin.Extensions;
using Admin.Models;
using DataAccessTool;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Admin.Services
{
    public class DonHangService
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public DonHangService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách đơn hàng (kèm tên Khách và tên Trạng thái)
        public async Task<PaginatedList<DonHang>> GetPagedListAsync(DonHangSearchModel search)
        {
            var query = _context.DonHangs
                .Include(d => d.KhachHang)       // JOIN bảng Khách hàng
                .Include(d => d.TrangThaiDon)    // JOIN bảng Trạng thái
                .AsQueryable();

            if (!string.IsNullOrEmpty(search.Keyword))
            {
                // Tìm theo Tên khách hoặc Mã đơn hàng
                query = query.Where(d => d.KhachHang != null && d.KhachHang.HoTen.Contains(search.Keyword)
                                      || d.MaDonHang.ToString() == search.Keyword);
            }

            if (search.MaTrangThai.HasValue)
            {
                query = query.Where(d => d.MaTrangThai == search.MaTrangThai);
            }

            if (search.TuNgay.HasValue)
            {
                query = query.Where(d => d.NgayDat >= search.TuNgay.Value);
            }

            if (search.DenNgay.HasValue)
            {
                // Thêm 1 ngày để lấy trọn vẹn ngày kết thúc
                query = query.Where(d => d.NgayDat < search.DenNgay.Value.AddDays(1));
            }

            int count = await query.CountAsync();

            var items = await query
                .OrderByDescending(d => d.NgayDat) // Đơn mới nhất lên đầu
                .Skip((search.Page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return new PaginatedList<DonHang>(items, count, search.Page, PageSize);
        }

        // 2. Lấy chi tiết đơn hàng (Kèm danh sách sản phẩm bên trong)
        public async Task<DonHang?> GetDetailAsync(int id)
        {
            return await _context.DonHangs
                .Include(d => d.KhachHang)
                .Include(d => d.TrangThaiDon)
                .Include(d => d.TinhThanh) // Nếu có bảng Tỉnh thành
                .Include(d => d.DonHangChiTiets)       // Lấy chi tiết
                    .ThenInclude(ct => ct.MatHang)     // Từ chi tiết lấy tiếp thông tin Mặt hàng
                .FirstOrDefaultAsync(m => m.MaDonHang == id);
        }

        // 3. Cập nhật trạng thái đơn hàng
        public async Task<bool> UpdateStatusAsync(int maDonHang, int maTrangThaiMoi)
        {
            var donHang = await _context.DonHangs.FindAsync(maDonHang);
            if (donHang == null) return false;

            // 1. Cập nhật trạng thái mới
            donHang.MaTrangThai = maTrangThaiMoi;

            // 2. Cập nhật ngày tương ứng (Ghi đè thời gian hiện tại)
            DateTime currentTime = DateTime.Now;

            switch (maTrangThaiMoi)
            {
                case 3: // Trạng thái: Đang giao hàng
                    donHang.NgayDi = currentTime;
                    break;

                case 4: // Trạng thái: Giao thành công
                    donHang.NgayDen = currentTime;

                    // Logic phụ (tùy chọn): Nếu chưa có Ngày đi, tự động điền luôn để dữ liệu logic
                    if (donHang.NgayDi == null) donHang.NgayDi = currentTime;
                    break;

                case 5: // Trạng thái: Đã hủy
                    donHang.NgayHuy = currentTime;
                    break;

                    // Case 1 (Mới) và 2 (Đã xác nhận): Thường không cập nhật lại NgayDat
                    // Nhưng nếu muốn reset các ngày kia khi quay ngược trạng thái, bạn có thể set null tại đây.
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        // Admin/Services/DonHangService.cs

        // ... (Giữ nguyên các hàm cũ)

        public async Task<int> CreateAsync(DonHang model)
        {
            // 1. Tính tổng tiền từ chi tiết đơn hàng gửi lên
            model.TongTien = 0;
            if (model.DonHangChiTiets != null)
            {
                foreach (var item in model.DonHangChiTiets)
                {
                    // Lấy giá hiện tại từ CSDL để đảm bảo chính xác (hoặc dùng giá gửi lên nếu cho phép sửa giá)
                    var matHang = await _context.MatHangs.FindAsync(item.MaMatHang);
                    if (matHang != null)
                    {
                        item.DonGia = matHang.GiaBan; // Gán giá bán hiện tại
                        model.TongTien += item.DonGia * item.SoLuong;
                    }
                }
            }

            // 2. Gán ngày tạo nếu chưa có
            if (model.NgayDat == null) model.NgayDat = DateTime.Now;

            _context.Add(model);
            await _context.SaveChangesAsync();
            return model.MaDonHang;
        }

        public async Task<bool> UpdateAsync(DonHang model)
        {
            var existingOrder = await _context.DonHangs
                                              .Include(d => d.DonHangChiTiets)
                                              .FirstOrDefaultAsync(d => d.MaDonHang == model.MaDonHang);

            if (existingOrder == null) return false;

            // 1. Cập nhật thông tin chung
            existingOrder.MaKhachHang = model.MaKhachHang;
            existingOrder.MaTrangThai = model.MaTrangThai;
            existingOrder.NgayDat = model.NgayDat;
            existingOrder.NgayDi = model.NgayDi;
            existingOrder.NgayDen = model.NgayDen;
            existingOrder.NgayHuy = model.NgayHuy;
            existingOrder.DiaChiGiaoHang = model.DiaChiGiaoHang;
            existingOrder.MaTinh = model.MaTinh;

            // 2. Xử lý Chi tiết đơn hàng (Xóa cũ -> Thêm mới để đơn giản hóa logic đồng bộ)
            // Xóa hết chi tiết cũ
            _context.DonHangChiTiets.RemoveRange(existingOrder.DonHangChiTiets);

            // Thêm chi tiết mới từ form gửi lên
            decimal tongTien = 0;
            if (model.DonHangChiTiets != null)
            {
                foreach (var item in model.DonHangChiTiets)
                {
                    var matHang = await _context.MatHangs.FindAsync(item.MaMatHang);
                    if (matHang != null)
                    {
                        var chiTietMoi = new DonHangChiTiet
                        {
                            MaDonHang = existingOrder.MaDonHang,
                            MaMatHang = item.MaMatHang,
                            SoLuong = item.SoLuong,
                            DonGia = matHang.GiaBan // Lấy giá mới nhất
                        };
                        tongTien += chiTietMoi.DonGia * chiTietMoi.SoLuong;
                        _context.Add(chiTietMoi);
                    }
                }
            }

            existingOrder.TongTien = tongTien;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}