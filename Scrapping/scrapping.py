import requests
from selenium import webdriver
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from bs4 import BeautifulSoup
import pandas as pd
import time
import datetime


SECTORS = ["Technology", "Financial Services", "Consumer Cyclical", "Healthcare", "Communication Services", "Industrials", "Consumer Defensive", "Energy", "Basic Materials", "Real Estate", "Utilities"]
SECTOR_URLS = {
    "Technology": "https://finance.yahoo.com/sectors/technology/",
    "Financial Services": "https://finance.yahoo.com/sectors/financial-services/",
    "Consumer Cyclical": "https://finance.yahoo.com/sectors/consumer-cyclical/",
    "Healthcare": "https://finance.yahoo.com/sectors/healthcare/",
    "Communication Services": "https://finance.yahoo.com/sectors/communication-services/",
    "Industrials": "https://finance.yahoo.com/sectors/industrials/",
    "Consumer Defensive": "https://finance.yahoo.com/sectors/consumer-defensive/",
    "Energy": "https://finance.yahoo.com/sectors/energy/",
    "Basic Materials": "https://finance.yahoo.com/sectors/basic-materials/",
    "Real Estate": "https://finance.yahoo.com/sectors/real-estate/",
    "Utilities": "https://finance.yahoo.com/sectors/utilities/"
}
SECTOR_TICKERS = {
    "Technology": "^YH311",
    "Financial Services": "^YH103",
    "Consumer Cyclical": "^YH102",
    "Healthcare": "^YH206",
    "Communication Services": "^YH308",
    "Industrials": "^YH310",
    "Consumer Defensive": "^YH205",
    "Energy": "^YH309",
    "Basic Materials": "^YH101",
    "Real Estate": "^YH104",
    "Utilities": "^YH207"
}

def fetch_sector_stocks(sector_name, sector_url):
    chrome_options = Options()
    chrome_options.add_argument("--headless")
    chrome_options.add_argument("--disable-gpu")
    chrome_options.add_argument("--no-sandbox")
    driver = webdriver.Chrome(options=chrome_options)
    driver.get(sector_url)
    try:
        WebDriverWait(driver, 15).until(
            EC.presence_of_element_located((By.CLASS_NAME, "table-container"))
        )
        soup = BeautifulSoup(driver.page_source, 'html.parser')
        table = soup.find('table', {'data-testid': 'table-container'})
        if not table:
            print(f"No stock data table found for {sector_name}.")
            return pd.DataFrame()
        headers = [header.text.strip() for header in table.find_all('th')]
        rows = []
        for row in table.find('tbody').find_all('tr'):
            cols = [col.text.strip() for col in row.find_all('td')]
            if cols:
                rows.append(cols)
        if headers and rows:
            return pd.DataFrame(rows, columns=headers)
        else:
            print(f"Headers or rows missing for {sector_name}")
            return pd.DataFrame()
    except Exception as e:
        print(f"Error fetching data for {sector_name}: {e}")
        return pd.DataFrame()
    finally:
        driver.quit()
        time.sleep(2)
def fetch_all_sectors():
    sector_stock_data = {}
    for sector, url in SECTOR_URLS.items():
        print(f"Fetching stocks for {sector}...")
        sector_stock_data[sector] = fetch_sector_stocks(sector, url)
    print("All sectors fetched successfully!")
    return sector_stock_data


def fetch_sector_data(ticker, period="1y", interval="1d"):
    url = f"https://query1.finance.yahoo.com/v8/finance/chart/{ticker}?range={period}&interval={interval}"
    headers = {"User-Agent": "Mozilla/5.0"}
    response = requests.get(url, headers=headers)
    data = response.json()

    try:
        result = data["chart"]["result"][0]
    except (KeyError, IndexError):
        print(f"No data found for ticker: {ticker}")
        return pd.DataFrame()
    timestamps = result.get("timestamp", [])
    dates = pd.to_datetime(timestamps, unit="s") if timestamps else []
    closes = result["indicators"]["quote"][0].get("close", [])
    df = pd.DataFrame({
        "Date": dates,
        "Close": closes
    })
    return df

