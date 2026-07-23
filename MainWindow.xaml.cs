using System;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace project_nyz
{
    public partial class MainWindow : Window
    {
        private string selectedGamePath = "";
        private string serverAddress = ""; // سيتم تعيينها من قبل المستخدم
        private string currentUsername = "";
        private string radminVpnPassword = "";

        public MainWindow()
        {
            InitializeComponent();
            LoadSavedUsers();
            UsernameComboBox.SelectionChanged += UsernameComboBox_SelectionChanged;
        }

        private void LoadSavedUsers()
        {
            string usersFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProjectNYZ", "users.txt");
            if (File.Exists(usersFile))
            {
                var lines = File.ReadAllLines(usersFile);
                UsernameComboBox.Items.Clear();
                foreach (var line in lines)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        UsernameComboBox.Items.Add(line);
                    }
                }
                UsernameComboBox.Items.Add("اضف مستخدم جديد");
            }
        }

        private void UsernameComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (UsernameComboBox.SelectedItem?.ToString() == "اضف مستخدم جديد")
            {
                NewUserLabel.Visibility = Visibility.Visible;
                NewUsernameTextBox.Visibility = Visibility.Visible;
                NewUsernameTextBox.Clear();
            }
            else
            {
                NewUserLabel.Visibility = Visibility.Collapsed;
                NewUsernameTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void SelectGameFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "اختر مجلد اللعبة (Fortnite)";
            
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                selectedGamePath = folderDialog.SelectedPath;
                GamePathTextBox.Text = selectedGamePath;
                
                // حفظ المسار
                SaveGamePath(selectedGamePath);
            }
        }

        private void SaveGamePath(string path)
        {
            string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProjectNYZ");
            Directory.CreateDirectory(configDir);
            File.WriteAllText(Path.Combine(configDir, "gamepath.txt"), path);
        }

        private string LoadGamePath()
        {
            string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProjectNYZ");
            string pathFile = Path.Combine(configDir, "gamepath.txt");
            if (File.Exists(pathFile))
            {
                return File.ReadAllText(pathFile).Trim();
            }
            return "";
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // التحقق من المدخلات
            string username = UsernameComboBox.SelectedItem?.ToString() ?? "";
            string password = PasswordBox.Password;

            if (username == "اضف مستخدم جديد")
            {
                username = NewUsernameTextBox.Text.Trim();
                if (string.IsNullOrEmpty(username))
                {
                    ShowError("من فضلك أدخل اسم مستخدم صحيح");
                    return;
                }
                SaveNewUser(username);
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("من فضلك أدخل اسم المستخدم وكلمة المرور");
                return;
            }

            if (string.IsNullOrEmpty(selectedGamePath))
            {
                selectedGamePath = LoadGamePath();
                if (string.IsNullOrEmpty(selectedGamePath))
                {
                    ShowError("من فضلك اختر مجلد اللعبة");
                    return;
                }
            }

            // التحقق من مجلد اللعبة
            if (!Directory.Exists(selectedGamePath))
            {
                ShowError("مجلد اللعبة غير موجود");
                return;
            }

            currentUsername = username;
            radminVpnPassword = password;

            // محاولة تسجيل الدخول
            PerformLogin(username, password);
        }

        private void SaveNewUser(string username)
        {
            string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProjectNYZ");
            Directory.CreateDirectory(configDir);
            string usersFile = Path.Combine(configDir, "users.txt");
            
            if (!File.Exists(usersFile))
            {
                File.WriteAllText(usersFile, username);
            }
            else
            {
                var users = File.ReadAllText(usersFile);
                if (!users.Contains(username))
                {
                    File.AppendAllText(usersFile, Environment.NewLine + username);
                }
            }
        }

        private async void PerformLogin(string username, string password)
        {
            try
            {
                StatusMessage.Text = "جاري تسجيل الدخول...";
                StatusMessage.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 212, 255));

                // هنا يتم التحقق من بيانات المستخدم
                bool loginSuccess = await VerifyCredentials(username, password);

                if (loginSuccess)
                {
                    // إخفاء نافذة التسجيل
                    this.Hide();

                    // فتح نافذة اللعبة الرئيسية
                    GameWindow gameWindow = new GameWindow(currentUsername, selectedGamePath, serverAddress);
                    gameWindow.Show();

                    // إغلاق نافذة التسجيل
                    this.Close();
                }
                else
                {
                    ShowError("بيانات الدخول غير صحيحة");
                }
            }
            catch (Exception ex)
            {
                ShowError($"خطأ: {ex.Message}");
            }
        }

        private async Task<bool> VerifyCredentials(string username, string password)
        {
            // محاكاة التحقق من البيانات
            // في نسخة حقيقية، ستتصل بالسيرفر للتحقق
            
            await Task.Delay(1000); // محاكاة تأخير الاتصال
            
            // للاختبار: أي مستخدم + أي كلمة مرور = نجاح
            // في الإنتاج، ستحتاج إلى التحقق من السيرفر
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
        }

        private void ShowError(string message)
        {
            StatusMessage.Text = message;
            StatusMessage.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 107, 107));
        }
    }

    public class LoginData
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("gamePath")]
        public string GamePath { get; set; }
    }
}
