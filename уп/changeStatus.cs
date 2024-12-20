using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace уп
{
    public partial class changeStatus : Form
    {
        private SqlConnection connection;
        private int requestStatusID; // ID заявки, которую мы редактируем

        public event EventHandler StatusChanged; // Событие для уведомления об успешном изменении статуса

        public changeStatus(int requestStatusID)
        {
            InitializeComponent();
            connection = new SqlConnection(@"Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"); // Замените на свою строку подключения
            this.requestStatusID = requestStatusID;
            LoadStatuses(); // Загружаем статусы в comboBox1
        }

        private void LoadStatuses()
        {
            using (SqlConnection conn = new SqlConnection("Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT requestStatusID, requestStatus FROM RequestStatus";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable statusesTable = new DataTable();
                    adapter.Fill(statusesTable);
                    comboBox1.DataSource = statusesTable;
                    comboBox1.DisplayMember = "requestStatus"; // Отображаем название статуса
                    comboBox1.ValueMember = "requestStatusID"; // Сохраняем requestStatusID статусов
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке статусов: {ex.Message}");
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
                    int selectedrequestStatusID = (int)comboBox1.SelectedValue;

                    try
                    {
                        conn.Open();
                        // Обновляем заявку с новым статусом
                        string query = "UPDATE Requests SET requestStatusID = @requestStatusID, completionDate = @completionDate WHERE requestID = @requestID";
                        using (SqlCommand command = new SqlCommand(query, conn))
                        {
                            command.Parameters.AddWithValue("@requestStatusID", selectedrequestStatusID);
                            command.Parameters.AddWithValue("@requestID", requestStatusID);
                            if (selectedrequestStatusID == 2)
                                command.Parameters.AddWithValue("@completionDate", DateTime.Now.ToLocalTime());
                            else
                                command.Parameters.AddWithValue("@completionDate", DBNull.Value);
                            command.ExecuteNonQuery();
                        }
                            
                        MessageBox.Show("Статус заявки успешно изменен!");
                        StatusChanged?.Invoke(this, EventArgs.Empty); // Уведомляем об успешном изменении статуса
                        this.Close(); // Закрываем форму изменения статуса
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
                    MessageBox.Show("Пожалуйста, выберите статус.");
                }
            }
        }
    }
}
