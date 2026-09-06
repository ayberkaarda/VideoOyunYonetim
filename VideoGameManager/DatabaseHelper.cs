using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

public class DatabaseHelper
{
    // Connection string. Phase 2 moves this into appsettings.json.
    private static string connectionString = "Server=localhost,1433;Database=VideoGameManager;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;";

    /// <summary>
    /// Runs a statement that does not return rows, such as INSERT, UPDATE or DELETE.
    /// </summary>
    public static void ExecuteNonQuery(string query, params SqlParameter[] parameters)
    {
        using (var conn = new SqlConnection(connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddRange(parameters);
                cmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Runs a SELECT statement and returns the result as a DataTable.
    /// </summary>
    public static DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
    {
        using (var conn = new SqlConnection(connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddRange(parameters);
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }
    }
}
