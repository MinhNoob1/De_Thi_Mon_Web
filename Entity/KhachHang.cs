using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class KhachHang
    {
        public int MaKhachHang { get; set; }
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public string MatKhau { get; set; } = "";
        public string? DienThoai { get; set; }
        public string? HinhAnh { get; set; }
        public string? DiaChi { get; set; }
        public string? VaiTro { get; set; }
    }
}
