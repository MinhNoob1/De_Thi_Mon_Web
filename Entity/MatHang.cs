using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class MatHang
    {
        public int MaMatHang { get; set; }
        public string TenMatHang { get; set; } = "";
        public int MaLoaiHang { get; set; }
        public decimal GiaBan { get; set; }
        public string? DonViTinh { get; set; }
        public string? HinhAnh { get; set; }
        public string? MoTa { get; set; }
        public int SoLuong { get; set; }
        public bool DangBan { get; set; }
    }
}
