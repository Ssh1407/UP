using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace уп
{
    public partial class processingRequests : Form
    {
        private SqlConnection connection;
        private int requestStatusID; // ID заявки, которую мы редактируем

        public event EventHandler RequestAssigned; // Событие для уведомления об успешном сохранении

        public processingRequests(int requestStatusID)
        {
            InitializeComponent();
            connection = new SqlConnection(@"Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"); // Замените на свою строку подключения
            this.requestStatusID = requestStatusID;
            LoadMechanics(); // Загружаем механиков в comboBox1
        }

        private void LoadMechanics()
        {
            using (SqlConnection conn = new SqlConnection("Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT staffID, fio FROM Staff WHERE staffID IN (SELECT staffID FROM Staff WHERE type = 'Мастер')";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable mechanicsTable = new DataTable();
                    adapter.Fill(mechanicsTable);
                    comboBox1.DataSource = mechanicsTable;
                    comboBox1.DisplayMember = "fio"; // Отображаем ФИО механиков
                    comboBox1.ValueMember = "staffID"; // Сохраняем clientID механиков
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке мастеров: {ex.Message}");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection("Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"))
            {
                if (comboBox1.SelectedValue != null)
                {
                    int selectedstaffID = (int)comboBox1.SelectedValue;

                    try
                    {
                        conn.Open();
                        // Обновляем заявку с назначением механика
                        string query = "UPDATE Requests SET staffID = @staffID WHERE requestID = @requestID";
                        using (SqlCommand command = new SqlCommand(query, conn))
                        {
                            command.Parameters.AddWithValue("@staffID", selectedstaffID);
                            command.Parameters.AddWithValue("@requestID", requestStatusID);
                            command.ExecuteNonQuery();
                        }

                        MessageBox.Show("Мастер успешно назначен!");
                        //RequestAssigned?.Invoke(this, EventArgs.Empty); // Уведомляем об успешном сохранении
                        this.Close(); // Закрываем форму редактирования
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Пожалуйста, выберите мастера.");
                }
            }
        }
    }
}
