using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EmployeeManagementSystem.Utilities
{
    public static class SqlHelper
    {
        public static int ExecuteNonQuery(string connectionString, string query, List<SqlParameter> parameters)
        {
            using SqlConnection con = new SqlConnection(connectionString);
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddRange(parameters.ToArray());
            con.Open();
            int rows = cmd.ExecuteNonQuery();
            return rows;
        }

        public static DataTable ExecuteQuery(string connectionString, string query, List<SqlParameter> parameters)
        {
            DataTable dt = new DataTable();
            using SqlConnection con = new SqlConnection(connectionString);
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddRange(parameters.ToArray());
            con.Open();
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static bool EmployeeExists(string connectionString, int employeeId)
        {
            using SqlConnection con = new SqlConnection(connectionString);
            string query = "SELECT COUNT(1) FROM Employees WHERE employee_id = @employee_id";
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@employee_id", employeeId);
            con.Open();
            return (int)cmd.ExecuteScalar() > 0;
        }

        public static bool verifyEmployee(string connectionString, int employeeId, string password)
        {
            using SqlConnection con = new SqlConnection(connectionString);
            string query = "SELECT COUNT(1) FROM Employees WHERE employee_id = @employee_id AND password = @password";
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@employee_id", employeeId);
            cmd.Parameters.AddWithValue("@password", password);
            con.Open();
            return (int)cmd.ExecuteScalar() > 0;
            
        }

    }

}
