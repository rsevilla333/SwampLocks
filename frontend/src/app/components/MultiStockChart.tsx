"use client";

import { useState, useEffect, useMemo } from "react";
import axios from "axios";
import { ResponsiveContainer, LineChart, Line, XAxis, YAxis, Tooltip } from "recharts";


const initialColors: Record<string, string> = {
  AAPL: "#8884d8",
  GOOGL: "#82ca9d",
  MSFT: "#ff7300",
};


function generateRandomColor(): string {
  const letters = "0123456789ABCDEF";
  let color = "#";
  for (let i = 0; i < 6; i++) {
    color += letters[Math.floor(Math.random() * 16)];
  }
  return color;
}

const timeframes = [
  { label: "Today", value: "1d" },
  { label: "1W", value: "1w" },
  { label: "1M", value: "1mo" },
  { label: "YTD", value: "ytd" },
  { label: "6M", value: "6mo" },
  { label: "1Y", value: "1y" },
  { label: "5Y", value: "5y" },
  { label: "Max", value: "max" }
];


interface MergedRow {
  date: string;
  [ticker: string]: string | number;
}

export default function MultiStockSearchChart() {
  const [searchQuery, setSearchQuery] = useState("");
  const [suggestions, setSuggestions] = useState<string[]>([]);
  const [selectedTickers, setSelectedTickers] = useState<string[]>([]);
  const [tickerColors, setTickerColors] = useState<Record<string, string>>(initialColors);

  const [mergedData, setMergedData] = useState<MergedRow[]>([]);
  const [filteredData, setFilteredData] = useState<MergedRow[]>([]);
  const [timeframe, setTimeframe] = useState("max");

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);


  const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;


  useEffect(() => {
    async function fetchSuggestions() {
      if (!searchQuery) {
        setSuggestions([]);
        return;
      }
      try {
        const resp = await axios.get(
          `${API_BASE_URL}/api/financials/stocks/autocomplete?query=${searchQuery}`
        );
        setSuggestions(resp.data);
      } catch (err) {
        console.error(err);
        setSuggestions([]);
      }
    }
    fetchSuggestions();
  }, [searchQuery, API_BASE_URL]);


  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    addTicker(searchQuery);
  }

  async function addTicker(ticker: string) {
    const uppercaseTicker = ticker.toUpperCase().trim();
    if (!uppercaseTicker) return;
    if (selectedTickers.includes(uppercaseTicker)) {
      // Already in list
      setSearchQuery("");
      setSuggestions([]);
      return;
    }
    setError(null);

    if (!tickerColors[uppercaseTicker]) {
      setTickerColors((prev) => ({
        ...prev,
        [uppercaseTicker]: generateRandomColor()
      }));
    }
    setSelectedTickers((prev) => [...prev, uppercaseTicker]);
    setSearchQuery("");
    setSuggestions([]);
  }


  function removeTicker(ticker: string) {
    setSelectedTickers((prev) => prev.filter((t) => t !== ticker));
  }

  useEffect(() => {
    if (selectedTickers.length === 0) {
      setMergedData([]);
      return;
    }

    let cancelled = false;
    async function fetchAll() {
      setLoading(true);
      setError(null);

      try {
        const results = await Promise.all(
          selectedTickers.map(async (ticker) => {
            const [respFiltered, respDaily] = await Promise.all([
              axios.get(`${API_BASE_URL}/api/financials/stocks/${ticker}/filtered_data`),
              axios.get(`${API_BASE_URL}/api/financials/stocks/${ticker}/todays_data`),
            ]);

            const combinedPoints = [...respFiltered.data, ...respDaily.data].map((item: any) => ({
              date: new Date(item.date).toISOString().slice(0, 10), // "YYYY-MM-DD"
              price: item.closingPrice
            }));

            return { ticker, data: combinedPoints };
          })
        );

        if (cancelled) return;

        const dataMap: Record<string, any> = {};
        for (const { ticker, data } of results) {
          data.forEach((pt) => {
            const dStr = pt.date;
            if (!dataMap[dStr]) {
              dataMap[dStr] = { date: dStr };
            }
            dataMap[dStr][ticker] = pt.price;
          });
        }
        const mergedArray = Object.values(dataMap).sort(
          (a: any, b: any) => (a.date as string).localeCompare(b.date as string)
        ) as MergedRow[];

        setMergedData(mergedArray);
      } catch (err) {
        console.error(err);
        if (!cancelled) setError("Error fetching multi-stock data.");
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    fetchAll();
    return () => { cancelled = true; };
  }, [selectedTickers, API_BASE_URL]);


  useEffect(() => {
    function filterByTimeframe(tf: string, dataArray: MergedRow[]): MergedRow[] {
      if (tf === "max") return dataArray;
      const now = new Date();
      let filtered = [...dataArray];

      if (tf === "1d") {
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        filtered = filtered.filter((item) => new Date(item.date) >= today);
      } else if (tf === "1w") {
        const oneWeekAgo = new Date(now);
        oneWeekAgo.setDate(now.getDate() - 7);
        filtered = filtered.filter((item) => new Date(item.date) >= oneWeekAgo);
      } else if (tf === "1mo") {
        const oneMonthAgo = new Date(now);
        oneMonthAgo.setMonth(now.getMonth() - 1);
        filtered = filtered.filter((item) => new Date(item.date) >= oneMonthAgo);
      } else if (tf === "6mo") {
        const sixMonthsAgo = new Date(now);
        sixMonthsAgo.setMonth(now.getMonth() - 6);
        filtered = filtered.filter((item) => new Date(item.date) >= sixMonthsAgo);
      } else if (tf === "1y") {
        const oneYearAgo = new Date(now);
        oneYearAgo.setFullYear(now.getFullYear() - 1);
        filtered = filtered.filter((item) => new Date(item.date) >= oneYearAgo);
      } else if (tf === "5y") {
        const fiveYearsAgo = new Date(now);
        fiveYearsAgo.setFullYear(now.getFullYear() - 5);
        filtered = filtered.filter((item) => new Date(item.date) >= fiveYearsAgo);
      } else if (tf === "ytd") {
        const startOfYear = new Date(now.getFullYear(), 0, 1);
        filtered = filtered.filter((item) => new Date(item.date) >= startOfYear);
      }

      return filtered;
    }

    setFilteredData(filterByTimeframe(timeframe, mergedData));
  }, [timeframe, mergedData]);


  const standardizedData = useMemo(() => {
    if (!filteredData.length || selectedTickers.length === 0) return [];

    const firstRow = filteredData[0];
    const baselines: Record<string, number> = {};
    selectedTickers.forEach((t) => {
      const val = firstRow[t];
      if (typeof val === "number") baselines[t] = val;
    });

    return filteredData.map((row) => {
      const out: MergedRow = { date: row.date };
      selectedTickers.forEach((t) => {
        const rawVal = row[t];
        if (typeof rawVal === "number" && baselines[t]) {
          out[t] = (rawVal / baselines[t]) * 100;
        } else {
          out[t] = null;
        }
      });
      return out;
    });
  }, [filteredData, selectedTickers]);


  const returnsData = useMemo(() => {
    if (filteredData.length < 2 || selectedTickers.length < 1) return [];
    const result: Array<{ date: string; [t: string]: number }> = [];
    for (let i = 1; i < filteredData.length; i++) {
      const prev = filteredData[i - 1];
      const curr = filteredData[i];
      const row: any = { date: curr.date };
      selectedTickers.forEach((ticker) => {
        const pVal = prev[ticker];
        const cVal = curr[ticker];
        if (typeof pVal === "number" && typeof cVal === "number" && pVal !== 0) {
          row[ticker] = (cVal - pVal) / pVal;
        } else {
          row[ticker] = 0;
        }
      });
      result.push(row);
    }
    return result;
  }, [filteredData, selectedTickers]);

  function computeCorrelation(arr1: number[], arr2: number[]): number {
    const n = arr1.length;
    if (n === 0) return 0;
    const mean1 = arr1.reduce((s, v) => s + v, 0) / n;
    const mean2 = arr2.reduce((s, v) => s + v, 0) / n;
    let numerator = 0, denom1 = 0, denom2 = 0;
    for (let i = 0; i < n; i++) {
      const diff1 = arr1[i] - mean1;
      const diff2 = arr2[i] - mean2;
      numerator += diff1 * diff2;
      denom1 += diff1 * diff1;
      denom2 += diff2 * diff2;
    }
    return denom1 && denom2 ? numerator / Math.sqrt(denom1 * denom2) : 0;
  }

  const pairwiseCorr: Record<string, number> = useMemo(() => {
    if (returnsData.length === 0 || selectedTickers.length < 2) return {};
    const returnsMap: Record<string, number[]> = {};
    selectedTickers.forEach((t) => { returnsMap[t] = []; });
    returnsData.forEach((row) => {
      selectedTickers.forEach((t) => {
        returnsMap[t].push(row[t]);
      });
    });

    const corrObj: Record<string, number> = {};
    for (let i = 0; i < selectedTickers.length; i++) {
      for (let j = i + 1; j < selectedTickers.length; j++) {
        const s1 = selectedTickers[i];
        const s2 = selectedTickers[j];
        const [a, b] = s1 < s2 ? [s1, s2] : [s2, s1];
        const r = computeCorrelation(returnsMap[s1], returnsMap[s2]);
        corrObj[`${a}-${b}`] = r;
      }
    }
    return corrObj;
  }, [returnsData, selectedTickers]);


  const allValues = standardizedData.flatMap((row) =>
    selectedTickers.map((t) => {
      const v = row[t];
      return typeof v === "number" ? v : null;
    })
  ).filter((x) => x !== null) as number[];

  const yMin = allValues.length ? Math.min(...allValues) : 0;
  const yMax = allValues.length ? Math.max(...allValues) : 100;

  //
  // 9) PERCENTAGE CHANGES
  //
  const percentageChanges: Record<string, number> = {};
  if (standardizedData.length > 1) {
    const firstRow = standardizedData[0];
    const lastRow = standardizedData[standardizedData.length - 1];
    selectedTickers.forEach((ticker) => {
      const fVal = firstRow[ticker];
      const lVal = lastRow[ticker];
      if (typeof fVal === "number" && typeof lVal === "number" && fVal !== 0) {
        percentageChanges[ticker] = ((lVal - fVal) / fVal) * 100;
      } else {
        percentageChanges[ticker] = 0;
      }
    });
  } else {
    selectedTickers.forEach((t) => { percentageChanges[t] = 0; });
  }

  return (
    <div className="w-full flex flex-col gap-4 relative text-black">

      {/* "Info" icon and disclaimer */}
      <div className="absolute top-0 right-0 mt-2 mr-2 group inline-block text-center cursor-pointer">
        <span className="font-bold border border-black rounded-full px-2 py-1 bg-gray-100 text-sm text-black">
          i
        </span>
        <div className="absolute hidden group-hover:block bg-white border border-gray-300 rounded p-2 w-60 text-black text-xs right-0 mt-1 z-10">
          <p className="font-semibold">Disclaimer</p>
          <p>
            This chart is a correlation visualization tool and does NOT reflect real price data.
          </p>
        </div>
      </div>

      {/* Search Bar */}
      <div className="relative w-full max-w-xl">
        <form onSubmit={handleSubmit} className="flex items-center bg-gray-100 rounded-md">
          <input
            type="text"
            placeholder="Enter stock ticker..."
            className="flex-grow p-2 outline-none rounded-l-md text-black caret-black placeholder-gray-600"
            value={searchQuery}
            onChange={(e) => {
              setSearchQuery(e.target.value);
              setError(null);
            }}
          />
          <button className="px-4 py-2 bg-gray-300 rounded-r-md text-black" type="submit">
            Add
          </button>
        </form>
        {suggestions.length > 0 && (
          <div className="absolute bg-white border border-gray-300 rounded-md w-full top-12 z-10 max-h-60 overflow-y-auto text-black">
            {suggestions.map((sugg) => (
              <div
                key={sugg}
                className="p-2 cursor-pointer hover:bg-gray-200"
                onClick={() => addTicker(sugg)}
              >
                {sugg}
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Error */}
      {error && <div className="text-red-500">{error}</div>}

      {/* Selected Tickers */}
      {selectedTickers.length > 0 && (
        <div className="flex flex-wrap gap-2 text-black">
          {selectedTickers.map((t) => (
            <div key={t} className="bg-gray-200 px-2 py-1 rounded-md flex items-center">
              {t}
              <button className="ml-2 text-red-600 font-bold" onClick={() => removeTicker(t)}>
                x
              </button>
            </div>
          ))}
        </div>
      )}

      {/* Timeframe Buttons */}
      <div className="flex gap-2 mb-2 text-black">
        {timeframes.map((tf) => (
          <button
            key={tf.value}
            className={`px-3 py-1 rounded-md ${
              timeframe === tf.value ? "bg-secondary" : "bg-accent"
            }`}
            onClick={() => setTimeframe(tf.value)}
          >
            {tf.label}
          </button>
        ))}
      </div>

      {/* Loading */}
      {loading && <p className="text-black">Loading...</p>}

      {/* Chart */}
      {selectedTickers.length === 0 && (
        <p className="text-gray-700">No tickers added yet.</p>
      )}
      {selectedTickers.length > 0 && standardizedData.length > 0 && (
        <ResponsiveContainer width="100%" height={400}>
          <LineChart data={standardizedData} className="text-black">
            <XAxis
              dataKey="date"
              tick={{ fill: "black" }}
              tickFormatter={(tick) => {
                const d = new Date(tick);
                const month = d.getMonth() + 1;
                const day = d.getDate();
                return `${month}/${day}`;
              }}
            />
            <YAxis
              tick={false}
              axisLine={false}
              tickLine={false}
              domain={[yMin - 5, yMax + 5]}
            />
            <Tooltip
              contentStyle={{ backgroundColor: "#ffffff", color: "#000000" }}
              itemStyle={{ color: "#000000" }}
              labelStyle={{ color: "#000000" }}
            />
            {selectedTickers.map((ticker) => (
              <Line
                key={ticker}
                type="linear"
                dataKey={ticker}
                stroke={tickerColors[ticker] || "#000000"}
                strokeWidth={2}
                dot={false}
              />
            ))}
          </LineChart>
        </ResponsiveContainer>
      )}

      {/* Legend & Correlations */}
      {selectedTickers.length > 0 && standardizedData.length > 0 && (
        <div className="mt-4 flex flex-col gap-4 text-black">
          {/* Legend: net % change */}
          <div>
            <h3 className="text-xl font-bold">Stocks</h3>
            <ul className="flex flex-wrap gap-4">
              {selectedTickers.map((ticker) => {
                const color = tickerColors[ticker] || "#000";
                const pct = (percentageChanges[ticker] ?? 0).toFixed(2);
                return (
                  <li key={ticker} className="flex items-center gap-2">
                    <div style={{ width: 20, height: 4, backgroundColor: color }}/>
                    <span>{ticker} ({pct}%)</span>
                  </li>
                );
              })}
            </ul>
          </div>

          {/* Pairwise correlations */}
          {selectedTickers.length > 1 && (
            <div>
              <h3 className="text-xl font-bold">Pairwise Correlations</h3>
              {Object.keys(pairwiseCorr).length === 0 ? (
                <p className="text-black">No correlation data (need at least 2 days in timeframe).</p>
              ) : (
                <ul className="flex flex-col gap-1">
                  {Object.entries(pairwiseCorr)
                    .sort(([keyA], [keyB]) => keyA.localeCompare(keyB))
                    .map(([pair, corr]) => (
                      <li key={pair}>
                        {pair}: {corr.toFixed(2)}
                      </li>
                    ))
                  }
                </ul>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
