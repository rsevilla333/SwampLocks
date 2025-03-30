"use client";

import { useEffect, useState, useMemo } from "react";
import axios from "axios";
import { ResponsiveContainer, LineChart, Line, XAxis, YAxis, Tooltip } from "recharts";


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

interface StockData {
    date: string;
    price: number;
}

interface StockChartProps {
    ticker: string;
}

export default function StockChart({ ticker }: StockChartProps) {
    const [data, setData] = useState<StockData[]>([]);
    const [filteredData, setFilteredData] = useState<StockData[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [timeframe, setTimeframe] = useState("max");

    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

    const firstDate = filteredData.length > 0 ? filteredData[0].date : null;
    const lastDate = filteredData.length > 0 ? filteredData[filteredData.length - 1].date : null;

    const firstPrice = filteredData.length > 0 ? filteredData[0].price : null;
    const lastPrice = filteredData.length > 0 ? filteredData[filteredData.length - 1].price : null;

    const percentageChange = firstPrice && lastPrice ? ((lastPrice - firstPrice) / firstPrice) * 100 : null;
    

    useEffect(() => {
        const fetchStockData = async () => {
            setLoading(true);
            setError(null);
            try {
                console.log(`Fetching data from ${API_BASE_URL}/api/financials/stocks/${ticker}/filtered_data`);
                
                const response = await axios.get(`${API_BASE_URL}/api/financials/stocks/${ticker}/filtered_data`);
                const dailyResponse = await axios.get(`${API_BASE_URL}/api/financials/stocks/${ticker}/todays_data`);
                
                console.log(dailyResponse.data);
                
                const processedData = response.data.map((item: any) => ({
                    date: new Date(item.date).toLocaleDateString(),
                    price: item.closingPrice
                }));

                const processedDailyData = dailyResponse.data.map((item: any) => ({
                    date: new Date(item.date),
                    price: item.closingPrice
                }));
                
                const combinedData = [...processedData, ...processedDailyData];
                
                setData(combinedData);
                setFilteredData(combinedData);
            } catch (err) {
                setError("Stock data not found.");
                console.error("Error fetching stock data:", err);
            } finally {
                setLoading(false);
            }
        };

        if (ticker) {
            fetchStockData();
        }
    }, [ticker]);

    const filterDataByTimeframe = (timeframe: string) => {
        const now = new Date();
        let filtered = [...data];

        if (timeframe === "1d") {
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            
            if (today.getDay() === 6) { // Saturday
                today.setDate(today.getDate() - 1);
            } else if (today.getDay() === 0) { // Sunday
                today.setDate(today.getDate() - 2);
            }
            filtered = filtered.filter((item) => new Date(item.date) > today);
            console.log("Filtered data:", filtered);
        } else if (timeframe === "1w") {
            const oneWeekAgo = new Date();
            oneWeekAgo.setDate(now.getDate() - 7);
            filtered = filtered.filter((item) => new Date(item.date) > oneWeekAgo);
        } else if (timeframe === "1mo") {
            const oneMonthAgo = new Date();
            oneMonthAgo.setMonth(now.getMonth() - 1);
            filtered = filtered.filter((item) => new Date(item.date) > oneMonthAgo);
        } else if (timeframe === "6mo") {
            const sixMonthsAgo = new Date();
            sixMonthsAgo.setMonth(now.getMonth() - 6);
            filtered = filtered.filter((item) => new Date(item.date) > sixMonthsAgo);
        } else if (timeframe === "1y") {
            const oneYearAgo = new Date();
            oneYearAgo.setFullYear(now.getFullYear() - 1);
            filtered = filtered.filter((item) => new Date(item.date) > oneYearAgo);
        } else if (timeframe === "5y") {
            const fiveYearsAgo = new Date();
            fiveYearsAgo.setFullYear(now.getFullYear() - 5);
            filtered = filtered.filter((item) => new Date(item.date) > fiveYearsAgo);
        } else if (timeframe === "ytd") {
            const startOfYear = new Date(now.getFullYear(), 0, 1);
            filtered = filtered.filter((item) => new Date(item.date) > startOfYear);
        }

        setFilteredData(filtered);
    };

    const lineColor = useMemo(() => {
        if (filteredData.length < 2) return "#8884d8"; // Default color if not enough data

        const lastPrice = filteredData[filteredData.length - 1]?.price;
        const priceChange = lastPrice - filteredData[0]?.price;
        return priceChange >= 0 ? "#2CCE2C" : "#FF0000"; // Green if price is up, Red if down
    }, [filteredData]);

    if (loading) return <div>Loading stock data...</div>;
    if (error) return <div className="text-red-500">{error}</div>;
    
    return (
        <div className="flex flex-col items-center p-8 w-full bg-transparent text-black ">
            <div className="w-full flex flex-col justify-start">
                <h2 className="text-2xl font-extrabold ml-4 text-black">{ticker.toUpperCase()} ${lastPrice}</h2>
                {percentageChange !== null && (
                    <span className={`ml-4 text-lg font-semibold ${percentageChange >= 0 ? "text-green-500" : "text-red-500"}`}>
                    {percentageChange.toFixed(2)}%
                </span>
                )}
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
                            } else if (timeframe === '1d') {
                                return `${date.getHours()}`;
                            } else {
                                return `${(date.getMonth() + 1).toString().padStart(2, "0")}-${date.getDate().toString().padStart(2, "0")}`;
                            }
                        }}
                    />
                    <YAxis
                        domain={(() => {
                            if (timeframe === "1d") {
                                const min = Math.min(...filteredData.map(item => item.price));
                                const max = Math.max(...filteredData.map(item => item.price));
                                return [min, max]; 
                            } else if (["1m", "1w", "1d", "6m"].includes(timeframe)) {
                                const min = Math.min(...filteredData.map(item => item.price));
                                const max = Math.max(...filteredData.map(item => item.price));
                                return [min - 5, max + 5];
                            } else {
                                return [0, "auto"]; 
                            }
                        })()}
                    />
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
