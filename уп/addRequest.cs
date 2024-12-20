using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace уп
{
    public partial class addRequest : Form
    {
        private int clientID;

        public addRequest(int clientID)
        {
            InitializeComponent();
            LoadProblems();
            this.clientID = clientID;
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

        // Метод для валидации введённых данных
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(textBoxtechnicType.Text))
            {
                MessageBox.Show("Введите вид оргтехники.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxtechnicModel.Text))
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

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close(); // Закрываем текущую форму
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                string technicType = textBoxtechnicType.Text.Trim();
                string technicModel = textBoxtechnicModel.Text.Trim();
                string problem = comboBox1.SelectedItem.ToString();
                DateTime startDate = DateTime.Now; // Дата создания заявки

                string connectionString = "Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true";

                // Подключаемся к базе данных и сохраняем заявку
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Сначала проверим, существует ли уже автомобиль
                    int technicID;
                    string carCheckQuery = "SELECT technicID FROM Technic WHERE technicType = @technicType AND technicModel = @technicModel";

                    using (SqlCommand carCheckCmd = new SqlCommand(carCheckQuery, conn))
                    {
                        carCheckCmd.Parameters.AddWithValue("@technicType", technicType);
                        carCheckCmd.Parameters.AddWithValue("@technicModel", technicModel);

                        object result = carCheckCmd.ExecuteScalar();
                        if (result != null) // Если автомобиль уже существует
                        {
                            technicID = (int)result;
                        }
                        else // Если нет, добавим новый автомобиль
                        {
                            string insertCarQuery = "INSERT INTO Technic (technicType, technicModel) OUTPUT INSERTED.technicID VALUES (@technicType, @technicModel)";
                            using (SqlCommand insertCarCmd = new SqlCommand(insertCarQuery, conn))
                            {
                                insertCarCmd.Parameters.AddWithValue("@technicType", technicType);
                                insertCarCmd.Parameters.AddWithValue("@technicModel", technicModel);
                                technicID = (int)insertCarCmd.ExecuteScalar();
                            }
                        }
                    }

                    // Теперь добавим заявку
                    string requestQuery = "INSERT INTO Requests (startDate, technicID, problemDescryption, requestStatusID, clientID) " +
                                          "VALUES (@startDate, @technicID, @problemDescryption, 1, @clientID)";

                    using (SqlCommand requestCmd = new SqlCommand(requestQuery, conn))
                    {
                        requestCmd.Parameters.AddWithValue("@startDate", startDate);
                        requestCmd.Parameters.AddWithValue("@technicID", technicID);
                        requestCmd.Parameters.AddWithValue("@problemDescryption", problem);
                        requestCmd.Parameters.AddWithValue("@clientID", clientID); // Используем переданный clientID

                        try
                        {
                            requestCmd.ExecuteNonQuery();
                            MessageBox.Show("Заявка успешно создана и передана оператору.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Close(); // Закрываем форму после успешного создания заявки
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show("Ошибка при создании заявки: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        // Метод для открытия формы просмотра заявок
        public void OpenViewingForm()
        {
            // Открываем форму для просмотра, передавая clientID
            showRequestClient viewingForm = new showRequestClient(clientID); // Передаем clientID
            viewingForm.Show(); // Используем Show() вместо ShowDialog()
        }
    }
}
