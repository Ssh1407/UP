using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;

namespace уп
{
    public partial class avtorizatia : Form
    {
        public int attemptCount = 0;
        public bool isBlocked = false;
        private System.Timers.Timer blockTimer;
        private int blockTime = 180000; // 3 минуты блокировки
        private string captchaValue;
        private bool isPasswordVisible = false;

        public avtorizatia()
        {
            InitializeComponent();
            pictureBoxCaptcha.Visible = false;
            txtCaptcha.Visible = false;
            updatePictureBox.Visible = false;

            txtPassword.UseSystemPasswordChar = true;
            pictureBox1.Click += new EventHandler(TogglePasswordVisibility);

            blockTimer = new System.Timers.Timer(blockTime);
            blockTimer.Elapsed += UnblockUser;
            blockTimer.AutoReset = false;
        }

        public static class CaptchaGenerator
        {
            private static Random random = new Random();

            public static string Generate()
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                char[] captcha = new char[5];

                for (int i = 0; i < captcha.Length; i++)
                {
                    captcha[i] = chars[random.Next(chars.Length)];
                }

                return new string(captcha);
            }

            public static Bitmap RenderCaptchaImage(string captchaText, Size pictureBoxSize)
            {
                Bitmap bitmap = new Bitmap(pictureBoxSize.Width, pictureBoxSize.Height);

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.Gray);

                    int fontSize = Math.Min(pictureBoxSize.Width / captchaText.Length, pictureBoxSize.Height / 2);
                    Font font = new Font("Arial", fontSize, FontStyle.Bold);

                    for (int i = 0; i < captchaText.Length; i++)
                    {
                        int xOffset = i * fontSize + random.Next(-5, 5);
                        int yOffset = random.Next(0, pictureBoxSize.Height / 3);

                        PointF position = new PointF(xOffset, yOffset);
                        g.DrawString(captchaText[i].ToString(), font, Brushes.White, position);
                    }

                    for (int i = 0; i < 100; i++)
                    {
                        int x = random.Next(bitmap.Width);
                        int y = random.Next(bitmap.Height);
                        bitmap.SetPixel(x, y, Color.Black);
                    }
                }

                return bitmap;
            }
        }
        
        public void GenerateCaptcha()
        {
            captchaValue = CaptchaGenerator.Generate();
            pictureBoxCaptcha.Image = CaptchaGenerator.RenderCaptchaImage(captchaValue, pictureBoxCaptcha.Size);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isBlocked)
            {
                MessageBox.Show("Вход заблокирован. Подождите 3 минуты.", "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string login = txtLogin.Text;
            string password = txtPassword.Text;
            Console.WriteLine($"Login: {login}, Password: {password}"); // Вывод в консоль


            if (attemptCount >= 1 && pictureBoxCaptcha.Visible)
            {
                if (txtCaptcha.Text != captchaValue)
                {
                    //attemptCount++;
                    LogLoginAttempt(login, false);
                    MessageBox.Show("Неправильная CAPTCHA. Попробуйте снова.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                   /* if (attemptCount > 2)
                    {
                        BlockUser();
                        return;
                    }*/

                    return;
                }
            }

            if (ValidateUser(login, password))
            {
                attemptCount = 0;
                LogLoginAttempt(login, true);
                return;
            }
            else
            {
                attemptCount++;
                LogLoginAttempt(login, false);

                if (attemptCount == 1)
                {
                    MessageBox.Show("Неверный логин или пароль. Попробуйте снова.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ShowCaptcha();
                }
                else if (attemptCount >= 2)
                {
                    MessageBox.Show("Неверный логин или пароль. Попробуйте снова.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (attemptCount > 2)
                {
                    BlockUser();
                }
            }
        }

        public bool ValidateUser(string login, string password)
        {
            using (SqlConnection conn = new SqlConnection("Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"))
            {
                conn.Open();
                string query = @"
                SELECT s.type 
                FROM Staff s 
                WHERE s.[login] = @login AND s.[password] = @password";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    string role = reader.GetString(0);
                    OpenUserForm(role);
                    return true;
                }
                else
                {
                    reader.Close();
                    query = @"
                    SELECT c.fio
                    FROM Clients c 
                    WHERE c.[login] = @login AND c.[password] = @password";

                    SqlCommand cmd1 = new SqlCommand(query, conn);
                    cmd1.Parameters.AddWithValue("@login", login);
                    cmd1.Parameters.AddWithValue("@password", password);

                    SqlDataReader reader1 = cmd1.ExecuteReader();
                    if (reader1.HasRows)
                    {
                        reader1.Read();
                        OpenUserForm("Заказчик");
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        private void LogLoginAttempt(string login, bool pass)
        {
            using (SqlConnection conn = new SqlConnection("Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"))
            {
                conn.Open();
                string query = "INSERT INTO Logs (login, logDate, pass) VALUES (@login, @logDate, @pass)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@logDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@pass", pass);

                cmd.ExecuteNonQuery();
            }
        }

        private void OpenUserForm(string role)
        {
            this.Hide();

            pictureBoxCaptcha.Visible = false;
            txtCaptcha.Visible = false;
            updatePictureBox.Visible = false;

            switch (role)
            {
                case "Заказчик":
                    int clientID = GetClientID();  // Получение идентификатора клиента
                    clientForm clientForm = new clientForm(clientID);
                    clientForm.Show();
                    break;
                case "Оператор":
                    showRequestAdmin adminForm = new showRequestAdmin();
                    adminForm.Show();
                    break;
                case "Мастер":
                    int staffID = GetstaffID();  // Получаем идентификатор автомеханика
                    requestFormMaster masterForm = new requestFormMaster(staffID);  // Передаем ID в конструктор
                    masterForm.Show();
                    break;
                case "Менеджер":
                    managerForm managerForm = new managerForm();  // Создаем форму менеджера с маленькой буквы
                    managerForm.Show();  // Открываем форму менеджера
                    break;
                default:
                    MessageBox.Show("Неизвестная роль пользователя.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Show();
                    break;
            }
        }

        private int GetClientID()
        {
            using (SqlConnection conn = new SqlConnection("Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"))
            {
                conn.Open();
                string query = "SELECT clientID FROM Clients WHERE login = @login";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@login", txtLogin.Text);

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    throw new Exception("Не удалось получить clientID");
                }
            }
        }

        private int GetstaffID()
        {
            using (SqlConnection conn = new SqlConnection("Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"))
            {
                conn.Open();
                string query = "SELECT staffID FROM Staff WHERE login = @login";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@login", txtLogin.Text);

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    throw new Exception("Не удалось получить идентификатор мастера");
                }
            }
        }

        private void ShowCaptcha()
        {
            pictureBoxCaptcha.Visible = true;
            txtCaptcha.Visible = true;
            updatePictureBox.Visible = true;

            GenerateCaptcha();
        }

        public void BlockUser()
        {
            isBlocked = true;
            MessageBox.Show("Доступ заблокирован на 3 минуты из-за слишком большого количества неудачных попыток.", "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            blockTimer.Start();
        }

        public void UnblockUser(Object source, ElapsedEventArgs e)
        {
            isBlocked = false;
            attemptCount = 0;
            blockTimer.Stop();
        }

        private void TogglePasswordVisibility(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !isPasswordVisible;
            isPasswordVisible = !isPasswordVisible;
        }

        private void updatePictureBox_Click(object sender, EventArgs e)
        {
            GenerateCaptcha();
        }

    }
}
