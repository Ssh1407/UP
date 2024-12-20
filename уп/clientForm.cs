using QRCoder;
using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace уп
{
    public partial class clientForm : Form
    {
        private int clientID; // Переменная для хранения идентификатора клиента

        // Конструктор, который принимает clientID
        public clientForm(int clientID)
        {
            InitializeComponent();
            this.clientID = clientID; // Сохраняем clientID для использования внутри формы
            pictureBox2.Image = Image.FromFile("H:\\УП01.01\\Programm\\image.png");
            LoadClientFIO();
        }
        private void LoadClientFIO()
        {
            string query = "SELECT fio FROM Clients WHERE clientID = @clientID";

            using (SqlConnection connection = new SqlConnection("Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@clientID", clientID);

                try
                {
                    connection.Open();
                    string clientFIO = (string)command.ExecuteScalar();
                    lblClientFIO.Text = clientFIO; // Отображаем ФИО клиента в Label
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при получении данных: " + ex.Message);
                }
            }
        }

        // Открываем форму для добавления заявки, текущая остаётся открытой
        private void button1_Click(object sender, EventArgs e)
        {
            addRequest addRequestForm = new addRequest(clientID); // Передаем clientID в форму создания заявки
            addRequestForm.Show();
        }

        // Открываем форму просмотра заявок, закрываем текущую
        private void button2_Click(object sender, EventArgs e)
        {
            showRequestClient showRequestClientForm = new showRequestClient(clientID); // Передаем clientID в форму просмотра заявок
            showRequestClientForm.Show();
            this.Close(); // Закрываем текущую форму
        }

        // Открываем форму авторизации, закрываем текущую
        private void button3_Click(object sender, EventArgs e)
        {
            avtorizatia avtorizatiaForm = new avtorizatia();
            avtorizatiaForm.Show();
            this.Close(); // Закрываем текущую форму
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string url = "https://docs.google.com/spreadsheets/d/1WfPVTqLY6EKSZhXll9o9mZR0kYYH_EiYkf7yceugJ4E/edit?gid=124862915#gid=124862915";
            GenerateQRCode(url);
        }

        private void GenerateQRCode(string url)
        {
            // Генерируем QR-код
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            // Устанавливаем размер QR-кода в зависимости от размера pictureBox1
            int qrSize = Math.Min(pictureBox1.Width, pictureBox1.Height); // Минимальный размер для квадратного QR-кода
            Bitmap qrCodeImage = qrCode.GetGraphic(20); // Генерация QR-кода

            // Изменяем размер изображения под размеры pictureBox1
            Bitmap resizedQRCode = new Bitmap(qrCodeImage, new Size(pictureBox1.Width, pictureBox1.Height));

            // Устанавливаем изображение в pictureBox1
            pictureBox1.Image = resizedQRCode;
        }

        private void clientForm_Load(object sender, EventArgs e)
        {
            pictureBox2.Image = Image.FromFile("H:\\УП01.01\\Programm\\image.png");
        }
    }
}
