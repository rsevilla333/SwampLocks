import requests
import pandas as pd
api_key = ''
topics = 'Technology,Financial Services,Consumer Cyclical,Healthcare,Communication Services,Industrials,Consumer Defensive,Energy,Basic Materials,Real Estate,Utilities'

# tickers = string Ex. 'AAPL, TSLA'
def get_all_news_data(tickers):
    url = 'https://www.alphavantage.co/query?function=NEWS_SENTIMENT&tickers=' + tickers + '&topics=' + topics + '&time_from=20170101T0000&time_to=20241231T2359&sort=EARLIEST&limit=1000&apikey=' + api_key
    r = requests.get(url)
    data = r.json()

    articles = data.get("feed", [])

    df = pd.DataFrame(
        articles, 
        columns=[
            "title",
            "time_published",
            "summary",
            "topics",
            "overall_sentiment_score",
            "overall_sentiment_label"
        ]
    )
    df["time_published"] = pd.to_datetime(df["time_published"], format="%Y%m%dT%H%M%S")
    pd.set_option('display.max_columns', None)

    print(df.head)
    return df

def get_earnings_news(tickers):
    url = 'https://www.alphavantage.co/query?function=BALANCE_SHEET&symbol=' + tickers + '&apikey=' + api_key
    r = requests.get(url)
    data = r.json()
    # need to retrieve the "fiscalDateEnding" and "commonStockSharesOutstanding" to calculate market cap over time
    print(data)
 
df = get_earnings_news('AAPL')
