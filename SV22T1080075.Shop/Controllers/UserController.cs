using BusinessLayers;
using DataAccessTool;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Security.Claims;

namespace SV22T1080075.Shop.Controllers
{
    [Authorize] // Mặc định tất cả các action đều yêu cầu đăng nhập
    public class UserController : Controller
    {
        private readonly KhachHangService _khachHangService;
        private readonly DonHangService _donHangService;
        private readonly ApplicationDbContext _context;

        public UserController(KhachHangService khachHangService, DonHangService donHangService, ApplicationDbContext context)
        {
            _khachHangService = khachHangService;
            _donHangService = donHangService;
            _context = context;
        }

        // --- HÀM PHỤ TRỢ: Lấy ID User từ Cookie ---
        private int GetUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out int id) ? id : 0;
        }

        // 1. DASHBOARD (TỔNG QUAN)
        public IActionResult Index()
        {
            return View();
        }

        // 2. ĐĂNG NHẬP
        [HttpGet]
        [AllowAnonymous] // Cho phép truy cập không cần login
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì đá về trang chủ
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Kiểm tra thông tin
            var user = await _khachHangService.Login(email, password);

            if (user == null)
            {
                ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
                return View();
            }

            // Tạo thông tin định danh (Claims)
            var claims = new List<Claim>
            {
                // QUAN TRỌNG: Dùng ClaimTypes.NameIdentifier để khớp với OrderController/CartController
                new Claim(ClaimTypes.NameIdentifier, user.MaKhachHang.ToString()),
                new Claim(ClaimTypes.Name, user.HoTen),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "Customer")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Ghi Cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return RedirectToAction("Index", "Home");
        }

        // 3. ĐĂNG XUẤT
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // 4. ĐĂNG KÝ
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(KhachHang model, string confirmPassword)
        {
            if (model.MatKhau != confirmPassword)
            {
                ModelState.AddModelError("confirmPassword", "Mật khẩu nhập lại không khớp.");
                return View(model);
            }

            model.TinhThanh = null; // Tránh lỗi validate
            int result = await _khachHangService.Register(model);

            if (result == -1)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                return View(model);
            }

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // 5. CẬP NHẬT HỒ SƠ (PROFILE)
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            int userId = GetUserId();
            var user = await _context.KhachHangs.FindAsync(userId);
            if (user == null) return RedirectToAction("Logout");
            ViewBag.ListTinhThanh = await _context.TinhThanhs.OrderBy(t => t.TenTinh).ToListAsync();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(KhachHang model, IFormFile? uploadPhoto)
        {
            int userId = GetUserId(); 

            // Lấy dữ liệu gốc từ DB để đảm bảo an toàn
            var userInDb = await _context.KhachHangs.FindAsync(userId); 
            if (userInDb == null) return RedirectToAction("Logout"); 

            // Cập nhật thông tin cơ bản
            userInDb.HoTen = model.HoTen; 
            userInDb.DienThoai = model.DienThoai; 
            userInDb.DiaChi = model.DiaChi;
            userInDb.MaTinh = model.MaTinh;

            // --- XỬ LÝ UPLOAD ẢNH SANG PROJECT ADMIN ---
            if (uploadPhoto != null && uploadPhoto.Length > 0)
            {
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}"; 

                // 1. Xác định đường dẫn tuyệt đối đến thư mục images/KhachHang của project Admin
                string adminRootPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "SV22T1080075.Admin", "wwwroot");
                string folderPath = Path.Combine(adminRootPath, "images", "KhachHang");

                // 2. Tạo thư mục nếu chưa tồn tại trong Admin
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath); 
                }

                // 3. Lưu file vật lý vào Admin
                string filePath = Path.Combine(folderPath, fileName); 
                using (var stream = new FileStream(filePath, FileMode.Create)) 
                {
                    await uploadPhoto.CopyToAsync(stream); 
                }

                // 4. Lưu đường dẫn tương đối vào Database
                userInDb.HinhAnh = $"/images/KhachHang/{fileName}"; 
            }

            await _context.SaveChangesAsync(); 
            TempData["Success"] = "Cập nhật hồ sơ thành công!";
            ViewBag.ListTinhThanh = await _context.TinhThanhs.OrderBy(t => t.TenTinh).ToListAsync();

            return View(userInDb); 
        }

        // 6. ĐỔI MẬT KHẨU
        [HttpGet]
        public IActionResult Password()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Password(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                return View();
            }

            int userId = GetUserId();
            bool result = await _khachHangService.ChangePassword(userId, oldPassword, newPassword);

            if (result)
            {
                TempData["Success"] = "Đổi mật khẩu thành công!";
                return View();
            }
            else
            {
                ModelState.AddModelError("", "Mật khẩu cũ không đúng.");
                return View();
            }
        }

        // 7. LỊCH SỬ ĐƠN HÀNG
        public async Task<IActionResult> Orders(int status = 0)
        {
            int userId = GetUserId();
            var list = await _donHangService.GetListByKhachHangID(userId, status);
            return View(list);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}