using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class Class1
    {
        private SqlConnection connection;
        private int staffID; // ID авторизованного автомеханика
        private int totalRecords; // Общее количество записей

        public static string UpdateAverageCompletionTime1(DataTable requestsTable)
        {
            using (SqlConnection conn = new SqlConnection("Server=192.168.188.11;Database=KomarovUP0101;Integrated Security=true"))
            {
                conn.Open();
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
                    return $"Среднее время выполнения: {avgDuration:F1} дней";
                }
                else
                {
                    return "Среднее время выполнения: нет завершенных заявок";
                }
            }
        }
    }
}
