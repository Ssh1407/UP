using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace уп
{
    public partial class refreshRequest : Form
    {
        private int requestID; // Идентификатор заявки
        private int clientID;  // Идентификатор клиента
        private showRequestClient parentForm; // Экземпляр родительской формы

        public refreshRequest(int requestID, int clientID, showRequestClient parentForm)
        {
            InitializeComponent();
            this.requestID = requestID;
            this.clientID = clientID;
            this.parentForm = parentForm; // Сохраняем ссылку на родительскую форму для обновления данных
            LoadProblems();     // Загружаем список проблем в ComboBox
            LoadRequestData();  // Загружаем данные заявки
        }

        // Метод для загрузки списка проблем в ComboBox
        private void LoadProblems()
        {
            comboBox1.Items.AddRange(new object[]
            {
               "Проблемы с настройками", "Системные ошибки и сбои", "Не загружается",
                "Проблемы с подключением к компьютеру", "Программное обеспечение не запускается", "Проблемы с электроникой",
                "Принтер не печатает", "Сканер не распознает документы", "Плохое качество передачи изображения",
                "Телефон не звонит или не принимает вызовы"
            });
        }

        // Метод для загрузки старых данных заявки
        private void LoadRequestData()
        {
            string connectionString = "Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true";
            string query = @"
SELECT 
    t.technicType, 
    t.technicModel, 
    r.problemDescryption, 
    c.fio, 
    c.phone
FROM 
    Requests r
JOIN 
    Technic t ON r.technicID = t.technicID
JOIN 
    Clients c ON r.clientID = c.clientID
WHERE 
    r.requestID = @requestID";

            /*string query = @"SELECT Cars.technicType, Cars.technicModel, Requests.problemDescryption, Users.fio, Users.phone 
                             FROM Requests 
                             JOIN Cars ON Requests.technicID = Cars.technicID
                             JOIN Users ON Requests.clientID = Users.userID  -- Обратите внимание на правильное имя столбца
                             WHERE Requests.requestStatusID = @requestStatusID";*/

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@requestID", requestID);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Заполняем поля формы данными из базы данных
                            textBox1.Text = reader["technicType"].ToString();
                            textBox2.Text = reader["technicModel"].ToString();
                            comboBox1.SelectedItem = reader["problemDescryption"].ToString();
                        }
                    }
                }
            }
        }

        // Метод для валидации введённых данных
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Введите вид оргтехники", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Введите модель оргтехники.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите проблему оргтехники.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        // Метод для обновления данных заявки
        private void UpdateRequest()
        {
            if (ValidateForm())
            {
                string technicType = textBox1.Text.Trim();
                string technicModel = textBox2.Text.Trim();
                string problem = comboBox1.SelectedItem.ToString();

                string connectionString = "Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Обновляем оргтехнику
                    string updateCarQuery = "UPDATE Technic SET technicType = @technicType, technicModel = @technicModel WHERE technicID = (SELECT TOP 1 technicID FROM Requests WHERE requestID = @requestID);";
                    using (SqlCommand updateCarCmd = new SqlCommand(updateCarQuery, conn))
                    {
                        updateCarCmd.Parameters.AddWithValue("@technicType", technicType);
                        updateCarCmd.Parameters.AddWithValue("@technicModel", technicModel);
                        updateCarCmd.Parameters.AddWithValue("@requestID", requestID);
                        updateCarCmd.ExecuteNonQuery();
                    }

                    // Обновляем заявку
                    string updateRequestQuery = "UPDATE Requests SET problemDescryption = @problemDescryption WHERE requestID = @requestID";
                    using (SqlCommand updateRequestCmd = new SqlCommand(updateRequestQuery, conn))
                    {
                        updateRequestCmd.Parameters.AddWithValue("@problemDescryption", problem);
                        updateRequestCmd.Parameters.AddWithValue("@requestID", requestID);
                        updateRequestCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Заявка успешно обновлена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Обновляем запись в DataGridView в форме просмотра
                    parentForm.RefreshDataGridView(); // Обновляем данные в родительской форме

                    // Закрываем форму после успешного обновления
                    this.Close();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateRequest();
        }
    }
}
