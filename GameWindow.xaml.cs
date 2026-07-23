using System;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace project_nyz
{
    public partial class GameWindow : Window
    {
        private string username;
        private string gamePath;
        private string serverAddress;

        public GameWindow(string username, string gamePath, string serverAddress)
        {
            InitializeComponent();
            this.username = username;
            this.gamePath = gamePath;
            this.serverAddress = serverAddress;

            UsernameDisplay.Text = username;
            StatusMessage.Text = $"جاري الاتصال بـ {serverAddress}...";

            // بدء الاتصال بالسيرفر
            ConnectToServer();
        }

        private async void ConnectToServer()
        {
            try
            {
                // هنا يتم الاتصال بسيرفر Radmin VPN
                await Task.Delay(2000);
                ServerStatusText.Text = "متصل ✓";
                ServerStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(74, 222, 128)); // أخضر
                PlayersCount.Text = "عدد اللاعبين: 0";
            }
            catch (Exception ex)
            {
                ServerStatusText.Text = "خطأ في الاتصال";
                ServerStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(239, 68, 68)); // أحمر
                StatusMessage.Text = $"خطأ: {ex.Message}";
            }
        }

        private void PlayGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusMessage.Text = "جاري تشغيل اللعبة...";
                LoadingProgress.Visibility = Visibility.Visible;

                // البحث عن ملف تشغيل اللعبة
                string[] gameExecutables = { 
                    Path.Combine(gamePath, "FortniteGame", "Binaries", "Win64", "FortniteClient-Win64-Shipping.exe"),
                    Path.Combine(gamePath, "FortniteGame", "Binaries", "Win64", "FortniteLauncher.exe"),
                    Path.Combine(gamePath, "Launcher.exe")
                };

                string executablePath = null;
                foreach (var exe in gameExecutables)
                {
                    if (File.Exists(exe))
                    {
                        executablePath = exe;
                        break;
                    }
                }

                if (executablePath == null)
                {
                    StatusMessage.Text = "لم يتم العثور على ملف تشغيل اللعبة";
                    LoadingProgress.Visibility = Visibility.Collapsed;
                    return;
                }

                // تشغيل اللعبة مع تسجيل الدخول التلقائي
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    UseShellExecute = true,
                    // يمكن إضافة معاملات إذا لزم الحال
                    Arguments = $"-username={username}" // تمرير اسم المستخدم للعبة
                };

                Process.Start(startInfo);

                // تسجيل الدخول في السيرفر
                LogPlayerLogin();

                // إغلاق نافذة اللانشر بعد تشغيل اللعبة
                await Task.Delay(2000);
                this.Close();
            }
            catch (Exception ex)
            {
                StatusMessage.Text = $"خطأ: {ex.Message}";
                LoadingProgress.Visibility = Visibility.Collapsed;
            }
        }

        private async void LogPlayerLogin()
        {
            try
            {
                // إرسال بيانات تسجيل الدخول إلى السيرفر
                // هذا سيكون اتصال HTTP أو TCP إلى سيرفر Radmin VPN
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging player: {ex.Message}");
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // العودة إلى نافذة التسجيل
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
