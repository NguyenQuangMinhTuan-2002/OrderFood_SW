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
                404 => "The page you are looking for does not exist.",
                403 => "You do not have permission to access this page.",
                401 => "You need to log in to access this page.",
                400 => "Invalid request.",
                405 => "Method not allowed.",
                500 => "Internal server error occurred.",
                502 => "Bad gateway error.",
                503 => "Service temporarily unavailable.",
                _ => $"An error occurred with code {StatusCode}."
            };
        }
        
        public string GetStatusCodeTitle()
        {
            return StatusCode switch
            {
                404 => "Page Not Found",
                403 => "Access Denied", 
                401 => "Unauthorized",
                400 => "Bad Request",
                405 => "Method Not Allowed",
                500 => "Server Error",
                502 => "Bad Gateway",
                503 => "Service Unavailable",
                _ => $"Error {StatusCode}"
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
