namespace OrderFood_SW.ViewModels
{
    public class EditUserViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ImageAvat { get; set; } = "nophoto1.png";
        public bool IsActive { get; set; }

        // Thêm field này để nhập password mới (tùy chọn)
        public string? NewPassword { get; set; }
    }
}
