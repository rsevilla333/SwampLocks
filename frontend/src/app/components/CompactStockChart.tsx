"use client";

import { useEffect, useState, useMemo } from "react";
import axios from "axios";
import { ResponsiveContainer, LineChart, Line, XAxis, YAxis } from "recharts";

interface StockData {
    date: string;
    price: number;
}

interface StockChartProps {
    ticker: string;
}

export default function CompactStockChart({ ticker }: StockChartProps) {
    const [data, setData] = useState<StockData[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

    useEffect(() => {
        const fetchStockData = async () => {
            setLoading(true);
            setError(null);
            try {
                const response = await axios.get(`${API_BASE_URL}/api/financials/stocks/${ticker}/todays_data`);
                const processedData: StockData[] = response.data.map((item: any) => ({
                    date: new Date(item.date),
                    price: item.closingPrice,
                }));
                setData(processedData);
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

    const lineColor = useMemo(() => {
        if (data.length < 2) return "#8884d8"; // Default color if not enough data

        const lastPrice = data[data.length - 1]?.price;
        const priceChange = lastPrice - data[0]?.price;
        return priceChange >= 0 ? "#2CCE2C" : "#FF0000"; // Green if price is up, Red if down
    }, [data]);

    if (loading) return <div>...</div>;
    if (error) return <div className="text-red-500">{error}</div>;

    return (
        <div className="flex flex-col items-center w-full bg-transparent text-black w-full">
            <ResponsiveContainer width="100%" height={100}>
                <LineChart data={data}>
                    {/*<XAxis dataKey="date" tick={false} />*/}
                    <YAxis hide   
                           domain={(() => {
                                const min = Math.min(...data.map(item => item.price));
                                const max = Math.max(...data.map(item => item.price));
                                return [min,max];
                    })()} />
                    <Line type="linear" dataKey="price" stroke={lineColor} strokeWidth={2} dot={false} />
                </LineChart>
            </ResponsiveContainer>
        </div>
    );
}
