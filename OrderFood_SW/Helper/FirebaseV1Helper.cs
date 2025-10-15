using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace OrderFood_SW.Helper
{
    public class FirebaseV1Helper
    {
        private readonly string _projectId;
        private readonly GoogleCredential _credential;
        private static readonly HttpClient _http = new HttpClient();

        public FirebaseV1Helper(string keyFilePath)
        {
            _credential = GoogleCredential.FromFile(keyFilePath)
                .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

            dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(keyFilePath));
            _projectId = json.project_id;
        }

        public async Task<string> SendAsync(string targetToken, string title, string body)
        {
            var accessToken = await _credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var message = new
            {
                message = new
                {
                    token = targetToken,
                    notification = new { title, body },
                    data = new { action = "RELOAD" }
                }
            };

            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"https://fcm.googleapis.com/v1/projects/{_projectId}/messages:send";
            var response = await _http.PostAsync(url, content);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
