using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace уп
{
    public partial class showRequestAdmin : Form
    {
        private SqlConnection connection;
        private DataTable requestsTable; // Таблица для хранения заявок
        private int totalRecords; // Общее количество записей

        public showRequestAdmin()
        {
            InitializeComponent();
           
            { // Замените на свою строку подключения
               
                LoadRequests();
            }
        }

        private void LoadRequests()
        {
            using (SqlConnection conn = new SqlConnection("Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"))
            {
                conn.Open();
                try
                {

                    // Запрос для получения всех заявок с присоединением к другим таблицам для отображения имен клиентов и механиков
                    string query = @"
SELECT 
    r.requestID AS [ID заявки],
    r.startDate AS [Дата начала],
    t.technicType AS [Тип техники], 
    t.technicModel AS [Модель техники],
    r.problemDescryption AS [Описание проблемы],
    rs.requestStatus AS [Статус заявки],
    r.completionDate AS [Дата завершения],
    s.fio AS [ФИО мастера],
    c.fio AS [ФИО клиента]
FROM 
    Requests r
LEFT JOIN 
    Technic t ON r.technicID = t.technicID
LEFT JOIN 
    RequestStatus rs ON r.requestStatusID = rs.requestStatusID
LEFT JOIN 
    Staff s ON r.staffID = s.staffID
LEFT JOIN 
    Clients c ON r.clientID = c.clientID";

                    /*string query = @"
                    SELECT 
                        r.requestID,
                        r.startDate,
                        c.technicID,
                        r.problemDescryption,
                        s.requestStatus,
                        r.completionDate,
                        u.clientID AS masterID,
                        u.fio AS masterName,
                        uc.fio AS clientName
                    FROM Requests r
                    LEFT JOIN Cars c ON r.technicID = c.technicID
                    LEFT JOIN Statuses s ON r.requestID = s.requestID
                    LEFT JOIN Users u ON r.masterID = u.clientID
                    LEFT JOIN Users uc ON r.clientID = uc.clientID";*/

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    requestsTable = new DataTable();
                    adapter.Fill(requestsTable);
                    dataGridView1.DataSource = requestsTable;

                    totalRecords = requestsTable.Rows.Count; // Сохраняем общее количество записей
                    UpdateRecordCount();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке заявок: {ex.Message}");
                }
                /*finally
                {
                    connection.Close();
                }*/
            }
        }

        private void UpdateRecordCount()
        {
            int filteredRecords = (dataGridView1.DataSource as DataTable)?.DefaultView.Count ?? 0;
            label2.Text = $"{filteredRecords} из {totalRecords}"; // Обновляем текст метки с количеством записей
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Открываем форму для назначения механика
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedrequestStatusID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID заявки"].Value);
                processingRequests processingRequestsForm = new processingRequests(selectedrequestStatusID); // Открытие формы назначения механика
                processingRequestsForm.RequestAssigned += (s, args) =>
                {
                    UpdateRequestStatusToInProgress(selectedrequestStatusID);
                    LoadRequests(); // Обновляем список заявок после редактирования
                };
                processingRequestsForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для редактирования.");
            }
        }

        private void UpdateRequestStatusToInProgress(int requestStatusID)
        {
            using (SqlConnection conn = new SqlConnection("Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE Requests SET requestID = (SELECT requestID FROM RequestStatus WHERE requestStatus = 'В процессе ремонта') WHERE requestID = @requestID";
                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@requestID", requestStatusID);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении статуса заявки: {ex.Message}");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Открываем форму для изменения статуса заявки
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedrequestStatusID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID заявки"].Value);
                changeStatus changeStatusForm = new changeStatus(selectedrequestStatusID); // Открытие формы изменения статуса
                changeStatusForm.ShowDialog();
                LoadRequests(); // Обновляем список заявок после изменения статуса
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для изменения статуса.");
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            // Открываем форму для изменения статуса заявки
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedrequestStatusID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID заявки"].Value);
                DeleteRequests(selectedrequestStatusID); 
                LoadRequests(); // Обновляем список заявок после удаления
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для удаление.");
            }
        }

        private void DeleteRequests(int requestID)
        {
            using (SqlConnection conn = new SqlConnection("Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"))
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM Requests WHERE requestID = @requestID"; // Используем DELETE вместо UPDATE
                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@requestID", requestID);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Заявка успешно удалена.");
                        }
                        else
                        {
                            MessageBox.Show("Заявка не найдена.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении заявки: {ex.Message}");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string searchText = textBox1.Text;
            // Фильтрация данных
            string filter = $"[ФИО мастера] LIKE '%{searchText}%' OR " +
                                  $"[ФИО клиента] LIKE '%{searchText}%' OR " +
                                  $"[Описание проблемы] LIKE '%{searchText}%' OR " +
                                  $"[Статус заявки] LIKE '%{searchText}%'";

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = filter;
            UpdateRecordCount(); // Обновляем количество записей после фильтрации
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Сбрасываем фильтр и обновляем данные
            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = string.Empty;
            LoadRequests(); // Перезагружаем все заявки
            textBox1.Clear(); // Очищаем текстовое поле
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Сортировка по заголовку столбца
            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (dataGridView1.SortOrder == System.Windows.Forms.SortOrder.Ascending) // Указали полное имя класса
            {
                dataGridView1.Sort(dataGridView1.Columns[columnName], System.ComponentModel.ListSortDirection.Descending);
            }
            else
            {
                dataGridView1.Sort(dataGridView1.Columns[columnName], System.ComponentModel.ListSortDirection.Ascending);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            avtorizatia avtorizatiaForm = new avtorizatia();
            avtorizatiaForm.Show();
            this.Close(); // Закрываем текущую форму
        }
        private void button6_Click(object sender, EventArgs e)
        {
            historyLoginForm historyForm = new historyLoginForm();
            historyForm.Show();
        }

    
    }
}
