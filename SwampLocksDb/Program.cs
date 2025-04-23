using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DotNetEnv;
using SwampLocksDb.Models;
using SwampLocksDb.Data;
using Microsoft.EntityFrameworkCore;
using  System.Security.Permissions;


public class Program
{
    public static void Main(string[] args)
    {
	    // Test Database
		ListSectors();
	    ListStocksBySector();
    }

	public static void ListStocksBySector()
	{
		using (var context = new FinancialContext())
        {
    		var sectors = context.Sectors.Include(s => s.Stocks).ToList();

    		foreach (var sector in sectors)
    		{
        		Console.WriteLine($"Sector: {sector.Name}");

        		if (sector.Stocks.Any())
        		{
            		foreach (var stock in sector.Stocks)
            		{
                		Console.WriteLine($"  - {stock.Ticker}");
            		}
        		}
        		else
        		{
           		 	Console.WriteLine(" No stocks found in this sector.");
        		}	

        		Console.WriteLine(); 
			}
		}
    }

	public static void ListStockOfSector(string sectorName)
	{
		using (var context = new FinancialContext())
        {
    		var sector = context.Sectors
       			.Include(s => s.Stocks)
       			.FirstOrDefault(s => s.Name == sectorName);

    		if (sector == null)
    		{
       			Console.WriteLine($"Sector '{sectorName}' not found.");
       			return;
    		}

    		Console.WriteLine($"Sector: {sector.Name}");

   	    	if (sector.Stocks.Any())
    		{
        		foreach (var stock in sector.Stocks)
        		{
         	  		Console.WriteLine($"  - {stock.Ticker}");
        		}
    		}
    		else
    		{
       	 		Console.WriteLine("  No stocks found in this sector.");
    		}
		}
	}

	public static void ListCurrencyExRate()
	{
		using (var context = new FinancialContext())
        {
			var exRates = context.ExchangeRates.ToList();

			foreach(var rate in exRates)
			{
				Console.WriteLine($"currency: {rate.TargetCurrency} rate: {rate.Rate} date: {rate.Date}");
			}
        }
	}

	public static void AddSector(string sector)
	{
		using (var context = new FinancialContext())
        {
			var sectorsList = new List<Sector>
            {
				new Sector { Name = sector }
            };

			context.Sectors.AddRange(sectorsList);
			context.SaveChanges();

			Console.WriteLine("Sector has been added to the db");
        }
	}

	public static void DeleteSector(string sector)
	{
    	using (var context = new FinancialContext())
    	{
       	 	var sectorToDelete = context.Sectors
        	    .Include(s => s.Stocks) // Ensure related stocks are loaded
        	    .FirstOrDefault(s => s.Name == sector);

        	if (sectorToDelete == null)
        	{
            	Console.WriteLine($"Sector '{sector}' not found.");
            	return;
        	}

        	// Remove associated stocks first
        	if (sectorToDelete.Stocks.Any())
        	{
            	context.Stocks.RemoveRange(sectorToDelete.Stocks);
        	}

        	context.Sectors.Remove(sectorToDelete);
        	context.SaveChanges();

        	Console.WriteLine($"Sector '{sector}' has been deleted from the database.");
    	}
}


	public static void ListSectors()
	{
		using (var context = new FinancialContext())
        {
            // Retrieve all sectors from the database
            var sectors = context.Sectors.ToList();

            // Check if any sectors exist
            if (sectors.Any())
            {
                Console.WriteLine("List of sectors in the database:");
                foreach (var sector in sectors)
                {
                    Console.WriteLine($"- {sector.Name}");
                }
            }
            else
            {
                Console.WriteLine("No sectors found in the database.");
            }
        }
	}


	// testDb
	public static async Task testDB()
	{
 		Console.WriteLine("Testting DB");
        
        Env.Load();
        
        // Replace with your Azure SQL Database connection string
        string? connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        Console.WriteLine(connectionString);

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
