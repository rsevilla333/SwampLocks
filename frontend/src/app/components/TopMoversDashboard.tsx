"use client";

import { useEffect, useState } from "react";
import { Bar } from "react-chartjs-2";
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend } from "chart.js";
import axios from "axios";
import CompactStockChart from "./CompactStockChart";



ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

interface MarketMover {
    ticker?: string;
    price?: number;
    change?: number;
    changePercent?: number;
    volume?: number;
}

export default function TopMoversDashboard() {
    const [movers, setMovers] = useState<MarketMover[] | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

    useEffect(() => {
        const fetchMovers = async () => {
            try {
                const response = await axios.get(`${API_BASE_URL}/api/financials/top_movers`);
                console.log("Top Movers:", response.data);
                
                setMovers(response.data);

            } catch (err) {
                setError("Failed to fetch market movers.");
            } finally {
                setLoading(false);
            }
        };

        fetchMovers();
    }, []);

    if (loading) {
        return <p className="text-center text-gray-500">Loading top movers...</p>;
    }

    if (error) {
        return <p className="text-center text-red-500">{error}</p>;
    }

    if (!movers || movers.length === 0) {
        return <p className="text-center text-gray-500">No market movers available.</p>;
    }

    const chartData = {
        labels: movers.map((mover) => mover.ticker ?? "N/A"),
        datasets: [
            {
                label: "Change Percentage",
                data: movers.map((mover) => mover.changePercent ?? 0),
                backgroundColor: movers.map((mover) =>
                    (mover.change ?? 0) >= 0 ? "rgba(75, 192, 192, 0.6)" : "rgba(255, 99, 132, 0.6)"
                ),
                borderColor: "rgba(0, 0, 0, 0.2)",
                borderWidth: 1,
            },
        ],
    };

    return (
        <div className="overflow-hidden max-h-full rounded-lg">
            {/* Table with scrollable body and fixed header */}
            <div className="overflow-y-auto max-h-[540px]">
                <table className="min-w-full bg-transparent border border-gray-200 rounded-lg shadow-md">
                    <thead className="bg-gray-700 text-white sticky top-0 z-10">
                    <tr>
                        <th className="py-3 px-4 text-left border-2 border-black">Ticker</th>
                        <th className="py-3 px-4 text-left border-2 border-black">Price</th>
                        <th className="py-3 px-4 text-left border-2 border-black">Volume</th>
                        <th className="py-3 px-4 text-left border-2 border-black">Change %</th>
                        <th className="py-3 px-4 text-left border-2 border-black">Today's Chart</th>
                    </tr>
                    </thead>
                    <tbody className={"text-black bg-transparent"}>
                    {movers.map((mover, index) => (
                        <tr key={index}>
                            <td className="py-2 px-4 border-2 border-black">{mover.ticker ?? "N/A"}</td>
                            <td className="py-2 px-4 border-2 border-black">${mover.price ? mover.price.toFixed(2) : "N/A"}</td>
                            <td className="py-2 px-4 border-2 border-black">{mover.volume ? mover.volume.toLocaleString() : "N/A"}</td>
                            <td className={`py-2 px-4 border-2 border-black  ${mover.change && mover.change >= 0 ? "text-green-800" : "text-red-800 "}`}>
                                {mover.changePercent ? `${mover.changePercent.toFixed(2)}%` : "N/A"}
                            </td>
                            <td className="py-2 px-4 border-2 border-black max-w-2 max-h-0.5">
                                <CompactStockChart ticker={mover.ticker ? mover.ticker.toLocaleString() : "N/A"} />
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}
