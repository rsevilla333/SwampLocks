"use client";

import { useState, useMemo, useEffect } from "react";
import axios from "axios";
import { ResponsiveContainer, LineChart, Line, XAxis, YAxis, Tooltip } from "recharts";

const timeframes = [
    { label: "1W", value: "1w" },
    { label: "1M", value: "1mo" },
    { label: "YTD", value: "ytd" },
    { label: "6M", value: "6mo" },
    { label: "1Y", value: "1y" },
    { label: "5Y", value: "5y" },
    { label: "Max", value: "max" }
];

interface StockData {
    date: string;
    price: number;
}

interface StockSearchProps {
    ticker: string;
}

export default function StockSearch({ ticker }: StockSearchProps) {
    const [symbol, setSymbol] = useState(ticker);
    const [data, setData] = useState<StockData[]>([]);
    const [filteredData, setFilteredData] = useState<StockData[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");
    const [timeframe, setTimeframe] = useState("max");
    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
    
    // Fetch stock data from the API
    const fetchStockData = async () => {
        setLoading(true);
        setError("");
        try {
            console.log(` Trying ${API_BASE_URL}/api/financials/stocks/${symbol}/data`);
            const response = await axios.get(
                `${API_BASE_URL}/api/financials/stocks/${symbol}/data`
            );
            const processedData = response.data.map((item: any) => ({
                date: new Date(item.date).toLocaleDateString(),
                price: item.closingPrice
            }));
            setData(processedData);
            setTimeframe("max");
            setFilteredData(processedData);
        } catch (err) {
            setError("Error fetching stock data");
            console.error("Error fetching stock data:", err);
        } finally {
            setLoading(false);
        }
    };

    // Filter data based on selected timeframe
    const filterDataByTimeframe = (timeframe: string) => {
        const now = new Date();
        let filtered = [...data];

        if (timeframe === "1w") {
            const oneWeekAgo = new Date();
            oneWeekAgo.setDate(now.getDate() - 8);
            filtered = filtered.filter((item: any) => new Date(item.date) > oneWeekAgo);
        } else if (timeframe === "1mo") {
            const oneMonthAgo = new Date();
            oneMonthAgo.setMonth(now.getMonth() - 1);
            filtered = filtered.filter((item: any) => new Date(item.date) > oneMonthAgo);
        } else if (timeframe === "6mo") {
            const sixMonthsAgo = new Date();
            sixMonthsAgo.setMonth(now.getMonth() - 6);
            filtered = filtered.filter((item: any) => new Date(item.date) > sixMonthsAgo);
        } else if (timeframe === "1y") {
            const oneYearAgo = new Date();
            oneYearAgo.setFullYear(now.getFullYear() - 1);
            filtered = filtered.filter((item: any) => new Date(item.date) > oneYearAgo);
        } else if (timeframe === "5y") {
            const fiveYearsAgo = new Date();
            fiveYearsAgo.setFullYear(now.getFullYear() - 5);
            filtered = filtered.filter((item: any) => new Date(item.date) > fiveYearsAgo);
        } else if (timeframe === "ytd") {
            const startOfYear = new Date(now.getFullYear(), 0, 1);
            filtered = filtered.filter((item: any) => new Date(item.date) > startOfYear);
        }

        setFilteredData(filtered);
    };

    const lineColor = useMemo(() => {
        if (filteredData.length < 2) return "#8884d8"; // Default color if not enough data

        const lastPrice = filteredData[filteredData.length - 1].price;
        const priceChange = lastPrice - filteredData[0].price;
        return priceChange >= 0 ? "#2CCE2C" : "#FF0000"; // Green if price is up, Red if down
    }, [filteredData]);

    return (
        <div className="flex flex-col items-center p-6 w-full bg-transparent text-black">
            {/*<h2 className="text-2xl font-bold mb-4 text-black">Search Stock History</h2>*/}
            <div className="flex gap-2 mb-4">
                <input
                    type="text"
                    placeholder="Symbol/Ticker"
                    value={symbol}
                    onChange={(e) => setSymbol(e.target.value.toUpperCase())}
                    className="border border-gray-300 p-2 rounded-md text-black"
                />
                <button
                    onClick={fetchStockData}
                    className="bg-primary text-black px-4 py-2 rounded-sm hover:bg-accent"
                >
                    Search
                </button>
            </div>

            <div className="flex gap-2 mb-4">
                {timeframes.map((t) => (
                    <button
                        key={t.value}
                        className={`px-3 py-1 rounded-md ${timeframe === t.value ? "bg-secondary text-black" : "bg-accent text-white"}`}
                        onClick={() => {
                            setTimeframe(t.value);
                            filterDataByTimeframe(t.value);
                        }}
                    >
                        {t.label}
                    </button>
                ))}
            </div>

            <ResponsiveContainer width="100%" height={400}>
                <LineChart data={filteredData}>
                    <XAxis
                        dataKey="date"
                        tickFormatter={(tick) => {
                            const date = new Date(tick);
                            const year = "\'" + String(date.getFullYear()).slice(-2);
                            if (timeframe === "max") {
                                return year;
                            } else if (["5y", "1y"].includes(timeframe)) {
                                return `${year}-${(date.getMonth() + 1).toString().padStart(2, "0")}`;
                            } else {
                                return `${(date.getMonth() + 1).toString().padStart(2, "0")}-${date.getDate().toString().padStart(2, "0")}`;
                            }
                        }}
                    />
                    <YAxis domain={["auto", "auto"]} />
                    <Tooltip />
                    <Line
                        type="linear"
                        dataKey="price"
                        stroke={lineColor}
                        strokeWidth={2}
                        dot={{ r: 0.3 }}
                    />
                </LineChart>
            </ResponsiveContainer>
        </div>
    );
}
