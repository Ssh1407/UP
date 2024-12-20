using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace уп
{
    public partial class showRequestClient : Form
    {
        private int clientID; // Идентификатор текущего авторизованного клиента

        public showRequestClient(int clientID)
        {
            InitializeComponent();
            this.clientID = clientID;
            RefreshDataGridView(); // Загружаем заявки клиента
        }

        private void showRequestClient_Load(object sender, EventArgs e)
        {
            RefreshDataGridView(); // Загружаем заявки при загрузке формы
        }

        // Метод для открытия формы добавления заявки
        public void OpenAddRequestForm()
        {
            addRequest addRequestForm = new addRequest(clientID);
            addRequestForm.FormClosed += (s, args) => RefreshDataGridView(); // Обновляем заявки после закрытия формы
            addRequestForm.ShowDialog();
        }

        // Метод для загрузки заявок клиента в DataGridView
        public void RefreshDataGridView()
        {
            string connectionString = "Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true";
            string query = "SELECT r.requestID AS[ID заявки],r.startDate AS[Дата начала], t.technicType AS[Тип техники], t.technicModel AS[Модель техники], r.problemDescryption AS[Описание проблемы], rs.requestStatus AS[Статус заявки] FROM Requests r JOIN Technic t ON r.technicID = t.technicID JOIN RequestStatus rs ON r.requestStatusID = rs.requestStatusID WHERE r.clientID = @clientID;";


            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@clientID", clientID);

                    try
                    {
                        conn.Open();
                        DataTable dataTable = new DataTable();
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dataTable);

                        dataGridView1.DataSource = dataTable; // Заполняем DataGridView
                        ResizeColumns(); // Настроить ширину столбцов
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Ошибка при загрузке заявок: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Метод для настройки ширины столбцов по содержимому
        private void ResizeColumns()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        // Кнопка для открытия формы редактирования заявки
        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedrequestStatusID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID заявки"].Value);
                refreshRequest refreshForm = new refreshRequest(selectedrequestStatusID, clientID, this); // Передаем ID заявки и клиента
                refreshForm.FormClosed += (s, args) =>
                {
                    RefreshDataGridView(); // Обновляем данные после редактирования
                    MessageBox.Show("Заявка успешно обновлена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
                refreshForm.ShowDialog(); // Показываем форму редактирования
            }
            else
            {
                MessageBox.Show("Выберите заявку для редактирования.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Кнопка для удаления заявки
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedrequestStatusID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID заявки"].Value);

                // Уведомление с вопросом о подтверждении удаления
                DialogResult dialogResult = MessageBox.Show("Вы уверены, что хотите удалить эту заявку?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.Yes)
                {
                    // Если пользователь подтвердил, вызываем метод удаления
                    DeleteRequest(selectedrequestStatusID);
                    RefreshDataGridView(); // Обновить данные после удаления
                }
            }
            else
            {
                MessageBox.Show("Выберите заявку для удаления.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Метод для удаления заявки из базы данных
        private void DeleteRequest(int requestID)
        {
            string connectionString = "Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true";
            string query = "DELETE FROM Requests WHERE requestID = @requestID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@requestID", requestID);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Заявка успешно удалена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Ошибка при удалении заявки: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Кнопка для возврата к форме клиента
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
            clientForm clientForm = new clientForm(clientID);
            clientForm.Show();
        }
    }
}
