using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.Net;
using System.Text.Json;
using System;

public class FinancialsApiIntegrationTests
{
    private readonly HttpClient _client;

    public FinancialsApiIntegrationTests()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri("https://swamplocksapi.azurewebsites.net/")
        };
    }

    [Fact] public async Task PingTest_ReturnsOk() => await GetTest("api/financials/ping");
    [Fact] public async Task GetAllStocks_Returns() => await GetTest("api/financials/stocks");
    [Fact] public async Task GetAllSectors_Returns() => await GetTest("api/financials/sectors");
    [Fact] public async Task Login_CreatesOrFetchesUser() => await GetTest("api/financials/login/testuser%40example.com/TestUser");

    [Fact]
    public async Task GetStockData_ReturnsAAPL()
    {
        var response = await _client.GetAsync("api/financials/stocks/AAPL/data");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        foreach (var entry in doc.RootElement.EnumerateArray())
        {
            Assert.Equal("AAPL", entry.GetProperty("ticker").GetString());
        }
    }

    [Fact] public async Task GetStockExists_Returns() => await GetTest("api/financials/stocks/AAPL/exists");

    [Fact]
    public async Task GetLatestPrice_ReturnsAAPL()
    {
        var response = await _client.GetAsync("api/financials/stocks/AAPL/latest-price");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var price = await response.Content.ReadAsStringAsync();
        Assert.True(decimal.TryParse(price, out var _));
    }

    [Fact]
    public async Task GetFilteredData_ReturnsAAPL()
    {
        var response = await _client.GetAsync("api/financials/stocks/AAPL/filtered_data");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        foreach (var entry in doc.RootElement.EnumerateArray())
        {
            Assert.Equal("AAPL", entry.GetProperty("ticker").GetString());
        }
    }

    [Fact] public async Task GetHoldingsByUser_HandlesBadGuid() => await GetTest("api/financials/user/holdings/00000000-0000-0000-0000-000000000000/get-holdings");
    [Fact] public async Task GetMatchingTickers_Returns() => await GetTest("api/financials/stocks/autocomplete?query=A");

    [Fact]
    public async Task GetArticles_ReturnsAAPL()
    {
        var response = await _client.GetAsync("api/financials/stocks/AAPL/articles");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        foreach (var entry in doc.RootElement.EnumerateArray())
        {
            Assert.Equal("AAPL", entry.GetProperty("ticker").GetString());
        }
    }

    [Fact] public async Task GetAllArticles_Returns() => await GetTest("api/financials/stocks/articles/all");
    [Fact] public async Task GetCommodities_Returns() => await GetTest("api/financials/commodities/oil");
    [Fact] public async Task GetCommodityIndicators_Returns() => await GetTest("api/financials/commodities/indicators");
    [Fact] public async Task GetEconomicData_Returns() => await GetTest("api/financials/economic_data/GDP");
    [Fact] public async Task GetEconomicIndicators_Returns() => await GetTest("api/financials/economic_data/indicators");

    [Fact]
    public async Task GetBalanceSheets_ReturnsAAPL()
    {
        var response = await _client.GetAsync("api/financials/balancesheets/AAPL");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        foreach (var entry in doc.RootElement.EnumerateArray())
        {
            Assert.Equal("AAPL", entry.GetProperty("ticker").GetString());
        }
    }

    [Fact]
    public async Task GetCashflow_ReturnsAAPL()
    {
        var response = await _client.GetAsync("api/financials/cashflowstatements/AAPL");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        foreach (var entry in doc.RootElement.EnumerateArray())
        {
            Assert.Equal("AAPL", entry.GetProperty("ticker").GetString());
        }
    }

    [Fact]
    public async Task GetEarnings_ReturnsAAPL()
    {
        var response = await _client.GetAsync("api/financials/earnings/AAPL");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        foreach (var entry in doc.RootElement.EnumerateArray())
        {
            Assert.Equal("AAPL", entry.GetProperty("ticker").GetString());
        }
    }

    [Fact]
    public async Task GetIncomeStatements_ReturnsAAPL()
    {
        var response = await _client.GetAsync("api/financials/incomestatements/AAPL");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        foreach (var entry in doc.RootElement.EnumerateArray())
        {
            Assert.Equal("AAPL", entry.GetProperty("ticker").GetString());
        }
    }

    [Fact] public async Task GetSectorPerformance_Returns() => await GetTest("api/financials/sectorperformance/Technology");
    [Fact] public async Task GetStocksFromSector_Returns() => await GetTest("api/financials/sectorstocks/Technology");
    [Fact] public async Task GetExchangeRates_Returns() => await GetTest("api/financials/ex_rates");

    private async Task GetTest(string path)
    {
        var response = await _client.GetAsync(path);
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
    }
}
