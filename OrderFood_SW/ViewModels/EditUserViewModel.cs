namespace OrderFood_SW.ViewModels
{
    public class EditUserViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }

        // Thêm field này để nhập password mới (tùy chọn)
        public string? NewPassword { get; set; }
    }
}
