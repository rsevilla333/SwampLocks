using System;
using Microsoft.Data.SqlClient;

class QuerySentiment
{
    static void Main()
    {
        string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("❌ CONNECTION_STRING not found in environment variables.");
            return;
        }

        Console.WriteLine("🔌 Connecting to the database...");

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            string query = @"
                SELECT Ticker, Date, SentimentScore
                FROM Articles
                WHERE Date >= '2025-04-08' AND Date < DATEADD(day, 1, '2025-04-08')
                ORDER BY Ticker";

            using (var command = new SqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                if (!reader.HasRows)
                {
                    Console.WriteLine("⚠️ No sentiment data found for 2025-04-08.");
                    return;
                }

                Console.WriteLine("\n📈 Sentiment Scores for 2025-04-08:\n");

                while (reader.Read())
                {
                    string ticker = reader.GetString(0);
                    DateTime date = reader.GetDateTime(1);
                    decimal score = reader.GetDecimal(2);

                    Console.WriteLine($"📌 {ticker} | {date:yyyy-MM-dd} | Score: {score}");
                }
            }
        }
    }
}

