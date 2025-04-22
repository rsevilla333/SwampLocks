"use client";

import React, { useEffect, useState } from "react";
import axios from "axios";

interface Stock {
    ticker: string;
    val_mse: number;
    R: number;
}

const ML_API_BASE_URL = process.env.NEXT_PUBLIC_ML_MODEL_BASE_URL;
const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

// Fetch all tickers first, then model metrics
export const fetchStockMetrics = async (): Promise<Stock[]> => {
    const results: Stock[] = [];

    try {
        const tickerRes = await axios.get(`${API_BASE_URL}/api/financials/top-marketcap-with-change`);
        const tickers: string[] = tickerRes.data?.map((stock: any) => stock.symbol) ?? [];

        for (const ticker of tickers) {
            try {
                const url = `${ML_API_BASE_URL}/api/MLModelResults?ticker=${ticker}`;
                console.log("Fetching:", url);
                const res = await axios.get(url);
                const val_mse = res.data?.val_mse ?? 0;
                const R = res.data?.percentChange ?? 0;
                console.log(res.data?.percentChange);

                results.push({ ticker, val_mse, R });
            } catch (err) {
                console.error(`❌ Skipped ${ticker}:`, err);
            }
        }
    } catch (err) {
        console.error("❌ Failed to fetch tickers:", err);
    }

    console.log("✅ Final stock list:", results);
    return results;
};

// RMSE & Score calculations
const rmse = (mse: number): number => Math.sqrt(mse);
const accuracy = (mse: number): number => 1 / (1 + rmse(mse));
const score = (mse: number, R: number): number => accuracy(mse) * (1 + R) - 1;

const TopPicks: React.FC = () => {
    const [stocks, setStocks] = useState<Stock[]>([]);

    useEffect(() => {
        fetchStockMetrics().then(setStocks);
    }, []);

    const ranked = stocks
        .map((s) => ({
            ...s,
            rmse: rmse(s.val_mse),
            accuracy: accuracy(s.val_mse),
            score: score(s.val_mse, s.R),
        }))
        .filter(s => !isNaN(s.score))
        .sort((a, b) => b.score - a.score)
        .slice(0, 10);

    return (
        <div className="p-6 bg-white rounded-2xl shadow-lg max-w-4xl mx-auto text-black">
            <h2 className="text-2xl text-center font-bold mb-4">Top Locks 🔒</h2>

            {stocks.length === 0 && (
                <p className="text-center text-red-500">No data loaded</p>
            )}
            <div className="overflow-x-auto">
                <table className="min-w-full bg-white border border-gray-200">
                    <thead>
                    <tr className="bg-gray-100">
                        <th className="px-6 py-3 border-b text-left text-sm font-semibold">Rank</th>
                        <th className="px-6 py-3 border-b text-left text-sm font-semibold">Ticker</th>
                        <th className="px-6 py-3 border-b text-left text-sm font-semibold">%Diff</th>
                        <th className="px-6 py-3 border-b text-left text-sm font-semibold">Return</th>
                        <th className="px-6 py-3 border-b text-left text-sm font-semibold">Score</th>
                    </tr>
                    </thead>
                    <tbody>
                    {ranked.map((s, i) => (
                        <tr
                            key={s.ticker}
                            className={`transition-colors hover:bg-gray-50 ${i % 2 === 0 ? '' : 'bg-gray-50'}`}
                        >
                            <td className="px-6 py-4 border-b text-sm">{i + 1}</td>
                            <td className="px-6 py-4 border-b text-sm">{s.ticker}</td>
                            <td className="px-6 py-4 border-b text-sm">{s.rmse.toFixed(3)}</td>
                            <td className="px-6 py-4 border-b text-sm">{(s.R).toFixed(1)}%</td>
                            <td className="px-6 py-4 border-b text-sm">{s.score.toFixed(3)}</td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default TopPicks;
