using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public class DatabaseHelper
{
    // MSSQL bağlantı cümlesi
    private static string connectionString = "Server=MONSTER\\SQLEXPRESS;Database=VideoOyun;Trusted_Connection=True;";

    /// <summary>
    /// INSERT, UPDATE, DELETE gibi sorguları çalıştırır.
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
    /// SELECT sorgularını çalıştırır ve sonucu DataTable olarak döner.
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
