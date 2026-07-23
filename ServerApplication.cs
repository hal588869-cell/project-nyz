using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace project_nyz
{
    /// <summary>
    /// سيرفر محلي يعمل على جهازك
    /// يتم تشغيله كخادم للعبة عبر Radmin VPN
    /// </summary>
    public class LocalGameServer
    {
        private TcpListener listener;
        private List<ClientHandler> connectedClients = new List<ClientHandler>();
        private int port = 9999;
        private bool isRunning = false;

        public LocalGameServer(int port = 9999)
        {
            this.port = port;
        }

        // بدء السيرفر
        public async Task StartAsync()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                isRunning = true;
                System.Diagnostics.Debug.WriteLine($"Server started on port {port}");

                while (isRunning)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    System.Diagnostics.Debug.WriteLine($"Client connected from {client.Client.RemoteEndPoint}");
                    
                    ClientHandler handler = new ClientHandler(client, this);
                    connectedClients.Add(handler);
                    _ = handler.HandleClientAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Server error: {ex.Message}");
            }
        }

        // إيقاف السيرفر
        public void Stop()
        {
            isRunning = false;
            listener?.Stop();
        }

        // الحصول على عدد اللاعبين المتصلين
        public int GetPlayerCount()
        {
            return connectedClients.Count;
        }

        // إرسال رسالة إلى جميع اللاعبين
        public async Task BroadcastMessageAsync(string message)
        {
            foreach (var client in connectedClients)
            {
                await client.SendMessageAsync(message);
            }
        }

        // إزالة عميل من القائمة
        public void RemoveClient(ClientHandler handler)
        {
            connectedClients.Remove(handler);
        }
    }

    /// <summary>
    /// معالج الاتصال لكل عميل
    /// </summary>
    public class ClientHandler
    {
        private TcpClient client;
        private NetworkStream stream;
        private LocalGameServer server;
        public string Username { get; set; }

        public ClientHandler(TcpClient client, LocalGameServer server)
        {
            this.client = client;
            this.stream = client.GetStream();
            this.server = server;
        }

        // معالجة الاتصال
        public async Task HandleClientAsync()
        {
            try
            {
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    await ProcessMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Client error: {ex.Message}");
            }
            finally
            {
                stream?.Close();
                client?.Close();
                server.RemoveClient(this);
                System.Diagnostics.Debug.WriteLine($"Client disconnected: {Username}");
            }
        }

        // معالجة الرسائل الواردة
        private async Task ProcessMessageAsync(string message)
        {
            try
            {
                dynamic data = JsonConvert.DeserializeObject(message);
                string action = data["action"];

                switch (action)
                {
                    case "login":
                        await HandleLoginAsync(data);
                        break;
                    case "logout":
                        await HandleLogoutAsync(data);
                        break;
                    case "getPlayers":
                        await HandleGetPlayersAsync();
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine($"Unknown action: {action}");
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Process error: {ex.Message}");
            }
        }

        // معالجة تسجيل الدخول
        private async Task HandleLoginAsync(dynamic data)
        {
            Username = data["username"];
            string gamePath = data["gamePath"];

            var response = new
            {
                status = "success",
                message = $"Welcome {Username}",
                playersOnline = server.GetPlayerCount()
            };

            await SendMessageAsync(JsonConvert.SerializeObject(response));
            System.Diagnostics.Debug.WriteLine($"{Username} logged in from {gamePath}");
        }

        // معالجة تسجيل الخروج
        private async Task HandleLogoutAsync(dynamic data)
        {
            string username = data["username"];
            var response = new { status = "success", message = "Logged out" };
            
            await SendMessageAsync(JsonConvert.SerializeObject(response));
            System.Diagnostics.Debug.WriteLine($"{username} logged out");
        }

        // الحصول على قائمة اللاعبين
        private async Task HandleGetPlayersAsync()
        {
            var response = new
            {
                status = "success",
                playersOnline = server.GetPlayerCount()
            };

            await SendMessageAsync(JsonConvert.SerializeObject(response));
        }

        // إرسال رسالة إلى العميل
        public async Task SendMessageAsync(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Send error: {ex.Message}");
            }
        }
    }
}
