using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;

namespace уп
{
    public partial class requestFormMaster : Form
    {
        private SqlConnection connection;
        private DataTable requestsTable; // Таблица для хранения заявок
        private int staffID; // ID авторизованного автомеханика
        private int totalRecords; // Общее количество записей

        // Добавляем параметр staffID в конструктор
        public requestFormMaster(int loggedInstaffID)
        {
            InitializeComponent();
            connection = new SqlConnection(@"Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"); // Строка подключения к БД
            staffID = loggedInstaffID; // Присваиваем переданный ID автомеханика
            LoadRequests();
        }

        private void LoadRequests()
        {
            using (SqlConnection conn = new SqlConnection("Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"))
            {
                try
                {
                    conn.Open();
                    // Запрос для получения заявок только для текущего автомеханика
                    string query = @"
SELECT 
    r.requestID AS [ID заявки],
    r.startDate AS [Дата начала],
    t.technicModel AS [Техника],
    r.problemDescryption AS [Описание проблемы],
    s.requestStatus AS [Статус],
    r.completionDate AS [Дата завершения],
    c.fio AS [ФИО клиента]
FROM 
    Requests r
LEFT JOIN 
    Technic t ON r.technicID = t.technicID
LEFT JOIN 
    RequestStatus s ON r.requestStatusID = s.requestStatusID
LEFT JOIN 
    Clients c ON r.clientID = c.clientID
WHERE 
    r.staffID = @staffID";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@staffID", staffID);

                    requestsTable = new DataTable();
                    adapter.Fill(requestsTable);
                    dataGridView1.DataSource = requestsTable;

                    totalRecords = requestsTable.Rows.Count; // Сохраняем общее количество записей
                    label2.Text = ClassLibrary1.Class1.UpdateAverageCompletionTime1(requestsTable);/////////////////////////////////////////
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке заявок: {ex.Message}");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void UpdateAverageCompletionTime()
        {
            // Вычисляем среднее время выполнения заказов
            var completedRows = requestsTable.Select("[Дата завершения] IS NOT NULL");
            if (completedRows.Length > 0)
            {
                TimeSpan totalDuration = TimeSpan.Zero;
                foreach (DataRow row in completedRows)
                {
                    DateTime startDate = Convert.ToDateTime(row["Дата начала"]);
                    DateTime completionDate = Convert.ToDateTime(row["Дата завершения"]);
                    totalDuration += (completionDate - startDate);
                }
                double avgDuration = totalDuration.TotalDays / completedRows.Length;
                label2.Text = $"Среднее время выполнения: {avgDuration:F1} дней";
            }
            else
            {
                label2.Text = "Среднее время выполнения: нет завершенных заявок";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            avtorizatia avtorizatiaForm = new avtorizatia();
            avtorizatiaForm.Show();
            this.Close(); // Закрываем текущую форму
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Фильтрация данных по статусу, ФИО и другим текстовым полям (без даты)
            string filter = $"clientName LIKE '%{textBox1.Text}%' OR " +
                            $"problemDescryption LIKE '%{textBox1.Text}%' OR " +
                            $"requestStatus LIKE '%{textBox1.Text}%'";

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = filter;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Сбрасываем фильтр, очищаем текстовое поле и обновляем данные
            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = string.Empty;
            textBox1.Clear();
            LoadRequests(); // Перезагружаем заявки автомеханика
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Сортировка по столбцу, используем System.Windows.Forms.SortOrder
            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (dataGridView1.SortOrder == System.Windows.Forms.SortOrder.Ascending)
            {
                dataGridView1.Sort(dataGridView1.Columns[columnName], ListSortDirection.Descending);
            }
            else
            {
                dataGridView1.Sort(dataGridView1.Columns[columnName], ListSortDirection.Ascending);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Проверяем, выбрана ли заявка
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedrequestStatusID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID заявки"].Value);
                CompleteRequest(selectedrequestStatusID); // Завершение заявки
                LoadRequests(); // Обновляем список заявок
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для завершения.");
            }
        }

        private void CompleteRequest(int requestStatusID)
        {
            using (SqlConnection conn = new SqlConnection("Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"))
            {
                try
                {
                    conn.Open();
                    // Запрос на изменение статуса заявки и добавление даты завершения
                    string query = @"
                UPDATE Requests
                SET requestStatusID = (SELECT requestStatusID FROM RequestStatus WHERE requestStatus = 'Готова к выдаче'), 
                    completionDate = @completionDate
                WHERE requestID = @requestID";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@completionDate", DateTime.Now);
                        command.Parameters.AddWithValue("@requestID", requestStatusID);

                        command.ExecuteNonQuery();
                    }
                    MessageBox.Show("Заявка успешно завершена!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при завершении заявки: {ex.Message}");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedrequestStatusID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID заявки"].Value);
                var orderData = GetOrderData(selectedrequestStatusID);
                string report = GenerateReport(selectedrequestStatusID, orderData);
                SaveReportToFile(report);
                MessageBox.Show("Отчет успешно создан!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для создания отчета.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private (string ClientName, string OrderDescription, DateTime StartTime, DateTime EndTime) GetOrderData(int orderId)
        {
            using (SqlConnection conn = new SqlConnection("Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"))
            {
                conn.Open();
                string query = @"
                    SELECT c.fio AS ClientName, r.problemDescryption, r.startDate, r.completionDate
                    FROM Requests r
                    INNER JOIN Clients c ON r.clientID = c.clientID
                    WHERE r.requestID = @orderId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@orderId", orderId);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string clientName = reader.GetString(0);
                    string description = reader.GetString(1);
                    DateTime startTime = reader.GetDateTime(2);
                    DateTime endTime = reader.IsDBNull(3) ? DateTime.Now : reader.GetDateTime(3);

                    return (clientName, description, startTime, endTime);
                }
                else
                {
                    throw new Exception("Заявка не найдена.");
                }
            }
        }

        // Обновленный метод для генерации отчета
        private string GenerateReport(int requestStatusID, (string ClientName, string OrderDescription, DateTime StartTime, DateTime EndTime) orderData)
        {
            TimeSpan duration = orderData.EndTime - orderData.StartTime;
            double durationInDays = duration.TotalDays;

            string report = $"Отчет по заявке №{requestStatusID}\n\n" +
                            $"Клиент: {orderData.ClientName}\n" +
                            $"Описание заказа: {orderData.OrderDescription}\n" +
                            $"Время начала выполнения: {orderData.StartTime}\n" +
                            $"Время завершения выполнения: {orderData.EndTime}\n" +
                            $"Общее время выполнения: {durationInDays:F1} дней\n";

            return report;
        }

        private void SaveReportToFile(string report)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.DefaultExt = "txt";
                saveFileDialog.AddExtension = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, report);
                }
            }
        }
    }
}