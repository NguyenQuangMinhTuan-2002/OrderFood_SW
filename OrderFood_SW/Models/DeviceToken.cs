namespace OrderFood_SW.Models
{
    public class DeviceToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }               // FK tới user trong hệ thống của bạn
        public string Token { get; set; }             // FCM registration token
        public string Platform { get; set; }          // "web" | "android" | "ios"
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
