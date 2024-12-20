using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace УП
{
    internal class DataBase
    {
        SqlConnection sqlConnection = new SqlConnection(@"Data Source=192.168.188.11;Initial Catalog=KomarovUP0101;Integrated Security=True");
       // SqlConnection sqlConnection = new SqlConnection(@"Data Source=LAPTOP-O0E8Q1IU\LIZA;Initial Catalog=rybalchenko;Integrated Security=True");

        public void openConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                sqlConnection.Open();
            }
        }

        public void closeConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Open)
            {
                sqlConnection.Close();
            }
        }

        public SqlConnection getConnection()
        {
            return sqlConnection;
        }

    }

}