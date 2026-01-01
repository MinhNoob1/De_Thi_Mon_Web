namespace Admin.Models
{
    public class MatHangSearchModel
    {
        public string? SearchName { get; set; }
        public int? MaLoai { get; set; }
        public decimal? GiaMin { get; set; }
        public decimal? GiaMax { get; set; }
        public int Page { get; set; } = 1;
    }
}
