using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace project_nyz
{
    public class ServerManager
    {
        private string serverAddress;
        private int serverPort = 9999; // المنفذ الافتراضي
        private TcpClient client;
        private NetworkStream stream;

        public ServerManager(string serverAddress, int port = 9999)
        {
            this.serverAddress = serverAddress;
            this.serverPort = port;
        }

        // الاتصال بالسيرفر
        public async Task<bool> ConnectAsync()
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(serverAddress, serverPort);
                stream = client.GetStream();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connection error: {ex.Message}");
                return false;
            }
        }

        // إرسال بيانات تسجيل الدخول
        public async Task<bool> SendLoginAsync(string username, string gamePath)
        {
            try
            {
                var loginData = new
                {
                    action = "login",
                    username = username,
                    gamePath = gamePath,
                    timestamp = DateTime.Now
                };

                string json = JsonConvert.SerializeObject(loginData);
                byte[] data = Encoding.UTF8.GetBytes(json);

                await stream.WriteAsync(data, 0, data.Length);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Send error: {ex.Message}");
                return false;
            }
        }

        // استقبال بيانات من السيرفر
        public async Task<string> ReceiveAsync()
        {
            try
            {
                byte[] buffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                return Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Receive error: {ex.Message}");
                return null;
            }
        }

        // الحصول على قائمة اللاعبين
        public async Task<string> GetPlayersListAsync()
        {
            try
            {
                var request = new { action = "getPlayers" };
                string json = JsonConvert.SerializeObject(request);
                byte[] data = Encoding.UTF8.GetBytes(json);

                await stream.WriteAsync(data, 0, data.Length);
                return await ReceiveAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        // تسجيل الخروج
        public async Task<bool> LogoutAsync(string username)
        {
            try
            {
                var logoutData = new
                {
                    action = "logout",
                    username = username,
                    timestamp = DateTime.Now
                };

                string json = JsonConvert.SerializeObject(logoutData);
                byte[] data = Encoding.UTF8.GetBytes(json);

                await stream.WriteAsync(data, 0, data.Length);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
                return false;
            }
        }

        // قطع الاتصال
        public void Disconnect()
        {
            try
            {
                stream?.Close();
                client?.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Disconnect error: {ex.Message}");
            }
        }
    }

    // نموذج بيانات اللاعب
    public class PlayerData
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("loginTime")]
        public DateTime LoginTime { get; set; }
    }
}
