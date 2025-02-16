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
from datetime import datetime
import random
import pytz
from lxml.html import fromstring



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


def economic_calendar_us(from_date="01/01/2017", to_date=None):
    if to_date is None:
        to_date = datetime.now().strftime("%d/%m/%Y")
    TIME_FILTERS = {"time_remaining": "timeRemain", "time_only": "timeOnly"}
    TIMEZONES = {"GMT -5:00": [42, 8, 43]}
    USER_AGENTS = [
        "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.62 Safari/537.36",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.12; rv:58.0) Gecko/20100101 Firefox/58.0",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36",
    ]
    COUNTRY_ID_FILTERS = {"united states": 5}
    IMPORTANCE_RATINGS = {1: "low", 2: "medium", 3: "high"}
    dateFrom_str = datetime.strptime(from_date, "%d/%m/%Y").strftime("%Y-%m-%d")
    dateTo_str = datetime.strptime(to_date, "%d/%m/%Y").strftime("%Y-%m-%d")
    data = {
        "dateFrom": dateFrom_str,
        "dateTo": dateTo_str,
        "timeZone": random.choice(TIMEZONES["GMT -5:00"]),
        "timeFilter": TIME_FILTERS["time_only"],
        "currentTab": "custom",
        "submitFilters": 1,
        "limit_from": 0,
        "country[]": [COUNTRY_ID_FILTERS["united states"]],
    }

    url = "https://www.investing.com/economic-calendar/Service/getCalendarFilteredData"
    headers = {
        "User-Agent": random.choice(USER_AGENTS),
        "X-Requested-With": "XMLHttpRequest",
        "Accept": "text/html",
        "Accept-Encoding": "gzip, deflate",
        "Connection": "keep-alive",
    }

    results = []
    last_id = None
    while True:
        resp = requests.post(url, headers=headers, data=data)
        try:
            json_data = resp.json()["data"]
        except Exception as e:
            raise ValueError("Error parsing JSON response: " + str(e))
        root = fromstring(json_data)
        rows = root.xpath(".//tr")
        if not rows:
            break
        current_id = None
        for row in rows[::-1]:
            row_id = row.get("id")
            if row_id:
                current_id = row_id.replace("eventRowId_", "")
                break

        if current_id == last_id:
            break
        for row in rows:
            row_id = row.get("id")
            if row_id is None:
                timescope = int(row.xpath("td")[0].get("id").replace("theDay", ""))
                curr_date = datetime.fromtimestamp(
                    timescope, tz=pytz.timezone("GMT")
                ).strftime("%d/%m/%Y")
            else:
                row_id = row_id.replace("eventRowId_", "")
                event_time = None
                zone = None
                currency = None
                importance_rating = None
                event = None
                actual = None
                forecast = None
                previous = None
                for cell in row.xpath("td"):
                    cell_class = cell.get("class") or ""
                    if "first left" in cell_class:
                        event_time = cell.text_content().strip()
                    elif "flagCur" in cell_class:
                        zone = cell.xpath("span")[0].get("title").lower()
                        currency = cell.text_content().strip()
                    elif "sentiment" in cell_class:
                        img_key = cell.get("data-img_key")
                        if img_key:
                            importance_rating = img_key.replace("bull", "")
                    elif cell_class.strip() == "left event":
                        event = cell.text_content().strip()
                    elif cell.get("id") == "eventActual_" + row_id:
                        actual = cell.text_content().strip()
                    elif cell.get("id") == "eventForecast_" + row_id:
                        forecast = cell.text_content().strip()
                    elif cell.get("id") == "eventPrevious_" + row_id:
                        previous = cell.text_content().strip()
                results.append({
                    "id": row_id,
                    "date": curr_date,
                    "time": event_time,
                    "zone": zone,
                    "currency": currency if currency != "" else None,
                    "importance": IMPORTANCE_RATINGS[int(importance_rating)] if importance_rating else None,
                    "event": event,
                    "actual": actual if actual != "" else None,
                    "forecast": forecast if forecast != "" else None,
                    "previous": previous if previous != "" else None
                })
        last_id = results[-1]["id"]
        data["limit_from"] += 1

    return pd.DataFrame(results)
