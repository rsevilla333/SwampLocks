"use client";

import React, { useEffect, useState } from "react";
import axios from "axios";

interface StockMetrics {
  ticker: string;
  val_mse: number;
  R: number;
  price: number;
}

const ML_API_BASE_URL = process.env.NEXT_PUBLIC_ML_MODEL_BASE_URL;
const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

// Compute the date of the last calendar day at quarter's end
default function getLastQuarterEnd(): Date {
  const today = new Date();
  const year = today.getFullYear();
  const month = today.getMonth();

  let qYear = year;
  let qMonth: number;

  if (month <= 2) {
    qYear = year - 1;
    qMonth = 11;
  } else if (month <= 5) {
    qMonth = 2;
  } else if (month <= 8) {
    qMonth = 5;
  } else {
    qMonth = 8;
  }

  const lastDay = new Date(qYear, qMonth + 1, 0).getDate();
  return new Date(qYear, qMonth, lastDay);
}

// Fetch tickers, ML metrics, and quarter-end price
export const fetchStockMetrics = async (): Promise<StockMetrics[]> => {
  const results: StockMetrics[] = [];
  try {
    const tickerRes = await axios.get(`${API_BASE_URL}/api/financials/top-marketcap-with-change`);
    const tickers: string[] = tickerRes.data?.map((s: any) => s.symbol) ?? [];
    const quarterEnd = getLastQuarterEnd();

    for (const ticker of tickers) {
      try {
        const mlRes = await axios.get(`${ML_API_BASE_URL}/api/MLModelResults?ticker=${ticker}`);
        const val_mse = mlRes.data?.val_mse ?? 0;
        const R = mlRes.data?.percentChange ?? 0;

        const priceRes = await axios.get(`${API_BASE_URL}/api/financials/stocks/${ticker}/filtered_data`);
        const priceData: Array<{ date: string; close: number }> = priceRes.data;
        const sorted = priceData
          .map(p => ({ ...p, ts: new Date(p.date) }))
          .filter(p => p.ts <= quarterEnd)
          .sort((a, b) => b.ts.getTime() - a.ts.getTime());
        const price = sorted.length > 0 ? sorted[0].close : priceData[priceData.length - 1]?.close || 1;

        results.push({ ticker, val_mse, R, price });
      } catch (err) {
        console.error(`Skipped ${ticker}:`, err);
      }
    }
  } catch (err) {
    console.error("Failed to fetch tickers:", err);
  }
  return results;
};

// Helper functions
const rmse = (mse: number): number => Math.sqrt(mse);
const accuracy = (err: number): number => 1 / (1 + err);
const score = (err: number, R: number): number => accuracy(err) * (1 + R) - 1;

const TopPicks: React.FC = () => {
  const [data, setData] = useState<StockMetrics[]>([]);

  useEffect(() => {
    fetchStockMetrics().then(setData);
  }, []);

  const ranked = data
    .map(s => {
      const normalizedError = rmse(s.val_mse) / s.price;
      return {
        ...s,
        normalizedError,
        score: score(normalizedError, s.R),
      };
    })
    .filter(s => !isNaN(s.score) && s.score > 0)
    .sort((a, b) => b.score - a.score)
    .slice(0, 10);

  return (
    <div className="p-6 bg-white rounded-2xl shadow-lg max-w-4xl mx-auto text-black">
      <h2 className="text-2xl text-center font-bold mb-4">Top Locks 🔒</h2>
      {data.length === 0 && <p className="text-center text-gray-500">Loading...</p>}
      <div className="overflow-x-auto">
        <table className="min-w-full bg-white border border-gray-200">
          <thead>
            <tr className="bg-gray-100">
              <th className="px-6 py-3 border-b text-left text-sm font-semibold">Rank</th>
              <th className="px-6 py-3 border-b text-left text-sm font-semibold">Ticker</th>
              <th className="px-6 py-3 border-b text-left text-sm font-semibold">Normalized Error</th>
              <th className="px-6 py-3 border-b text-left text-sm font-semibold">Return</th>
              <th className="px-6 py-3 border-b text-left text-sm font-semibold">Score</th>
            </tr>
          </thead>
          <tbody>
            {ranked.map((s, i) => (
              <tr
                key={s.ticker}
                className={`transition-colors hover:bg-gray-50 ${i % 2 === 0 ? "" : "bg-gray-50"}`}
              >
                <td className="px-6 py-4 border-b text-sm">{i + 1}</td>
                <td className="px-6 py-4 border-b text-sm">{s.ticker}</td>
                <td className="px-6 py-4 border-b text-sm">{s.normalizedError.toFixed(4)}</td>
                <td className="px-6 py-4 border-b text-sm">{(s.R * 100).toFixed(1)}%</td>
                <td className="px-6 py-4 border-b text-sm">{s.score.toFixed(4)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default TopPicks;
