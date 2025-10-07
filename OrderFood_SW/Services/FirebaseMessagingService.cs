using FirebaseAdmin.Messaging;
using OrderFood_SW.Models;
using Notification = FirebaseAdmin.Messaging.Notification;

namespace OrderFood_SW.Services
{
    public class FirebaseMessagingService
    {
        public async Task<bool> SendNotificationAsync(string title, string body, string token)
        {
            try
            {
                var message = new Message()
                {
                    Token = token,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    }
                };

                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                Console.WriteLine($"[FCM] Sent notification: {response}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FCM] Error sending notification: {ex.Message}");
                return false;
            }
        }

        public async Task SendToMultipleAsync(string title, string body, List<string> tokens)
        {
            if (tokens == null || tokens.Count == 0) return;

            var messages = tokens.Select(token => new Message
            {
                Token = token,
                Notification = new Notification { Title = title, Body = body }
            }).ToList();

            var response = await FirebaseMessaging.DefaultInstance.SendEachAsync(messages);
            Console.WriteLine($"[FCM] Multicast sent: {response.SuccessCount}/{tokens.Count} success");
        }
    }
}
