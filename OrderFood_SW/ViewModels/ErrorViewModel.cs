namespace OrderFood_SW.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string? Message { get; set; }
        public string? Path { get; set; }
        public string? StackTrace { get; set; }
        
        // New properties for status code handling
        public int StatusCode { get; set; }
        public bool IsStatusCodeError => StatusCode > 0 && string.IsNullOrEmpty(StackTrace);
        
        public string GetStatusCodeMessage()
        {
            return StatusCode switch
            {
                404 => "Trang bạn đang tìm kiếm không tồn tại.",
                403 => "Bạn không có quyền truy cập vào trang này.",
                401 => "Bạn cần đăng nhập để truy cập trang này.",
                400 => "Yêu cầu không hợp lệ.",
                405 => "Phương thức không được phép.",
                500 => "Đã xảy ra lỗi máy chủ nội bộ.",
                502 => "Lỗi cổng kết nối.",
                503 => "Dịch vụ tạm thời không khả dụng.",
                _ => $"Đã xảy ra lỗi với mã {StatusCode}."
            };
        }
        
        public string GetStatusCodeTitle()
        {
            return StatusCode switch
            {
                404 => "Không tìm thấy trang",
                403 => "Truy cập bị từ chối", 
                401 => "Chưa được xác thực",
                400 => "Yêu cầu không hợp lệ",
                405 => "Phương thức không được phép",
                500 => "Lỗi máy chủ",
                502 => "Lỗi cổng kết nối",
                503 => "Dịch vụ không khả dụng",
                _ => $"Lỗi {StatusCode}"
            };
        }
        
        public string GetStatusCodeIcon()
        {
            return StatusCode switch
            {
                404 => "🔍",
                403 => "🚫", 
                401 => "🔐",
                400 => "❌",
                405 => "⛔",
                500 => "⚠️",
                502 => "🔌",
                503 => "🔧",
                _ => "⚠️"
            };
        }
    }
}
