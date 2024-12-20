using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace уп
{
    public partial class managerForm : Form
    {
        private string connectionString = @"Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true";
        private const int InProgressrequestStatusID = 2; // ID статуса "В процессе ремонта"

        public managerForm()
        {
            InitializeComponent();
            LoadRequests();
            LoadMechanics();
        }

        private void LoadRequests()
        {
            // Загрузка всех заявок в DataGridView
            string query = @"SELECT R.requestID AS [ID заявки], R.startDate AS [Дата начала], C.technicModel AS [Модель техники], R.problemDescryption AS [Описание проблемы], S.requestStatus AS [Статус заявки], U.fio AS [Мастер], R.completionDate AS [Дата окончания]
                             FROM Requests R
                             LEFT JOIN Technic C ON R.technicID = C.technicID
                             LEFT JOIN RequestStatus S ON R.requestStatusID = S.requestStatusID
                             LEFT JOIN Staff U ON R.staffID = U.staffID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;
                dataGridView1.Columns["ID заявки"].Visible = false; // Скрываем колонку с ID заявки
            }
        }

        private void LoadMechanics()
        {
            // Загрузка всех автомехаников в ComboBox
            string query = @"SELECT staffID, fio 
                 FROM Staff 
                 WHERE staffID IN (SELECT staffID FROM Staff WHERE type = 'Мастер')";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                comboBox1.DisplayMember = "fio";   // Отображаем ФИО автомехаников
                comboBox1.ValueMember = "clientID";  // Используем clientID как значение
                comboBox1.DataSource = dt;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Назначить выбранного автомеханика на выбранную заявку
            if (dataGridView1.SelectedRows.Count > 0 && comboBox1.SelectedValue != null)
            {
                int selectedrequestStatusID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID заявки"].Value);
                int selectedstaffID = (int)((DataRowView)comboBox1.SelectedValue).Row.ItemArray[0]; 

                AssignMechanicToRequest(selectedrequestStatusID, selectedstaffID);
                MessageBox.Show("Мастер успешно назначен на заявку!");

                // Обновляем данные заявок после изменения
                LoadRequests();
            }
            else
            {
                MessageBox.Show("Выберите заявку и мастера.");
            }
        }

        private void AssignMechanicToRequest(int requestStatusID, int staffID)
        {
            // SQL-запрос для назначения автомеханика на заявку
            string query = @"UPDATE Requests
                             SET staffID = @staffID
                             WHERE requestID = @requestID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@staffID", staffID);
                    cmd.Parameters.AddWithValue("@requestID", requestStatusID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Изменить дату завершения заявки и статус на "В процессе"
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedrequestStatusID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID заявки"].Value);
                DateTime selectedDate = dateTimePicker1.Value;

                UpdateRequestCompletionDateAndStatus(selectedrequestStatusID, selectedDate);
                MessageBox.Show("Дата завершения заявки успешно обновлена и статус изменен на 'В процессе'!");

                // Обновляем данные заявок после изменения
                LoadRequests();
            }
            else
            {
                MessageBox.Show("Выберите заявку.");
            }
        }

        private void UpdateRequestCompletionDateAndStatus(int requestStatusID, DateTime completionDate)
        {
            // SQL-запрос для обновления даты завершения и статуса заявки
            string query = @"UPDATE Requests
                             SET completionDate = @completionDate, requestStatusID = @requestStatusID
                             WHERE requestID = @requestID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@completionDate", completionDate);
                    cmd.Parameters.AddWithValue("@requestStatusID", InProgressrequestStatusID); // Статус "В процессе"
                    cmd.Parameters.AddWithValue("@requestID", requestStatusID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            avtorizatia avtorizatiaForm = new avtorizatia();
            avtorizatiaForm.Show();
            this.Close(); // Закрываем текущую форму

        }
    }
}
