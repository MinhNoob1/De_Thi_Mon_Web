using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entity
{
    public class DonHang
    {
        public int MaDonHang { get; set; }
        public int MaKhachHang { get; set; }
        public int? MaTrangThai { get; set; }
        public DateTime NgayDat { get; set; }
        public DateTime? NgayDi { get; set; }
        public DateTime? NgayDen { get; set; }
        public DateTime? NgayHuy { get; set; }
        public decimal? TongTien { get; set; }
        public string? DiaChiGiaoHang { get; set; }
    }
}
