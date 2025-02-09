// See https://aka.ms/new-console-template for more information

using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DotNetEnv;

class TestAzureSQL
{
    static async Task Main()
    {
        Console.WriteLine("Testting DB");
        
        Env.Load();
        
        // Replace with your Azure SQL Database connection string
        string? connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("❌ Connection string is missing. Make sure it's set in the .env file.");
            return;
        }

        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                Console.WriteLine("✅ Successfully connected to Azure SQL Database!");

                // Check if the database exists
                string checkDatabaseQuery = "SELECT DB_ID('swamp_db')";
                using (SqlCommand cmd = new SqlCommand(checkDatabaseQuery, conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != DBNull.Value && result != null)
                    {
                        Console.WriteLine("✅ Database exists!");
                    }
                    else
                    {
                        Console.WriteLine("❌ Database does NOT exist!");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Failed to connect to Azure SQL Database.");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
