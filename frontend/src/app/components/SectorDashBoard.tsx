"use client";

import Image from "next/image";
import Link from "next/link";
import { useState, useEffect } from "react";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import SectorAnalysis from "./SectorAnalysis";
import Treemap from "./TreeMap";
import Footer from "./Footer";
import { ResponsiveContainer, PieChart, Pie, Cell, Tooltip, Legend } from "recharts";
import CircularProgress from "@mui/material/CircularProgress";

interface SectorPageProps {
    sectorName: string;
}

interface StockWithChange {
    symbol: string;
    marketCap: number;
    change: number;
}

interface SectorSentiment {
    sectorName: string;
    sentiment: number;
    label: string;
    date: string;
}

const sectorMap: Record<string, { name: string }> = {
    "technology": { name: "Information Technology" },
    "energy": { name: "Energy" },
    "healthcare": { name: "Healthcare" },
    "financials": { name: "Financials" },
    "consumer-discretionary": { name: "Consumer Discretionary" },
    "consumer-staples": { name: "Consumer Staples" },
    "industrials": { name: "Industrials" },
    "materials": { name: "Materials" },
    "real-estate": { name: "Real Estate" },
    "utilities": { name: "Utilities" },
    "communication-services": { name: "Communication Services" },
};

const COLORS = ["#FF5733", "#33A1FF", "#33FF57", "#FFD700", "#8B4513", "#807513", "#0F7513"];


export default function SectorDashboard({ sectorName }: SectorPageProps) {
    
    const [selectedDate, setSelectedDate] = useState<Date>(new Date());
    const [isCalendarOpen, setIsCalendarOpen] = useState(false);
    const [loading, setLoading] = useState(true);
    const [sectorGrowth, setSectorGrowth] = useState<number | null>(null);
    const [sectorSentiment, setSectorSentiment] = useState<SectorSentiment | null>(null);
    const [stocks, setStocks] = useState<StockWithChange[]>([]);
    const maxVisible = 20;
    const sortedStocks = [...stocks].sort((a, b) => b.marketCap - a.marketCap);
    const topStocks = sortedStocks.slice(0, maxVisible);
    const otherValue = sortedStocks
        .slice(maxVisible)
        .reduce((sum, stock) => sum + stock.marketCap, 0);
    
    const pieData = [...topStocks, { symbol: "Other", marketCap: otherValue }]
        
    const topMovers = [...stocks]
        .filter(stock => stock.change !== null && stock.change !== undefined)
        .sort((a, b) => Math.abs(b.change) - Math.abs(a.change))
        .slice(0, maxVisible); 
    
    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

    const fetchSectorGrowth = async (): Promise<number | null> => {
        try {
            const url = `${API_BASE_URL}/api/financials/sector-growth?sectorName=${sectorMap[sectorName].name}`;
            const response = await fetch(url);
            if (!response.ok) throw new Error("Failed to fetch sector growth");
            const growth = await response.json();
            return growth;
        } catch (err) {
            console.error("Error fetching sector growth:", err);
            return null;
        }
    };

    const fetchSectorSentiment = async (): Promise<SectorSentiment | null> => {
        try {
            const url = `${API_BASE_URL}/api/financials/sector/sentiment/${sectorMap[sectorName].name}`;
            const response = await fetch(url);
            if (!response.ok) throw new Error("Failed to fetch sector sentiment");
            const sentiment: SectorSentiment = await response.json();
            return sentiment;
        } catch (err) {
            console.error("Error fetching sector sentiment:", err);
            return null;
        }
    };
    
    const fetchTopMarketCapWithChange = async (): Promise<StockWithChange[]> => {
        const count = 100;
        
        let date = selectedDate;
        
        if (selectedDate.getDay() === 6 ) {
            date.setDate(selectedDate.getDate() - 1);
        } else if (selectedDate.getDay() === 0) {
            date.setDate(selectedDate.getDate() - 2);
        }
        
        try {
            const url = `${API_BASE_URL}/api/financials/top-marketcap-with-change?count=${count}&sectorName=${sectorMap[sectorName].name}&date=${date.toISOString().split("T")[0]}`;
            console.log(url);
            const response = await fetch(url);
            console.log(response);
            if (!response.ok) {
                throw new Error("Failed to fetch market cap data");
            }

            const stocks: StockWithChange[] = await response.json();
            console.log("Stocks with market cap & change loaded:", stocks);
            return stocks;
        } catch (err) {
            console.error("Error fetching top market cap stocks:", err);
            return [];
        }
    };

    useEffect(() => {
        const load = async () => {
            setLoading(true);
            const stocks = await fetchTopMarketCapWithChange();
            setStocks(stocks);
            const growth = await fetchSectorGrowth();
            setSectorGrowth(growth);
            const sentiment = await fetchSectorSentiment();
            setSectorSentiment(sentiment);
            setLoading(false);
        };
        if (selectedDate) {
            console.log("selectedDate", selectedDate);
            load();
        }
    }, [selectedDate]);

    // Handle the date change
    const handleDateChange = (date: Date | null) => {
        if (date) {
            setSelectedDate(date);
            setIsCalendarOpen(false); 
        }
    };

    // Toggle the calendar popup
    const toggleCalendar = () => {
        setIsCalendarOpen(!isCalendarOpen);
    };

    return (
        <div className="min-h-screen flex flex-col items-center p-8 bg-gray-50">
            <Link
                href="/"
                className="text-lg text-blue-600 font-medium hover:underline mb-6"
            >
                ← Back to Home
            </Link>

            {/* Sector Title */}
            <h1 className="text-4xl font-semibold text-gray-800 mb-8">{sectorMap[sectorName].name}</h1>

            
            {/* Date Picker Button */}
            <div className="mb-8 w-full">
                <button
                    onClick={toggleCalendar}
                    className="px-4 py-2 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition"
                >
                    Date: {selectedDate.toLocaleDateString()}
                </button>

                {/* Date Picker Popup */}
                {isCalendarOpen && (
                    <div className="mt-4 z-10 absolute top-16 bg-white shadow-lg rounded-lg border p-4">
                        <DatePicker
                            selected={selectedDate}
                            onChange={handleDateChange}
                            inline
                            dateFormat="MMMM d, yyyy"
                            maxDate={new Date()}
                        />
                    </div>
                )}
            </div>
            <div className="flex items-center flex flex-row p-5">
            <div className="mb-8 p-4 bg-white rounded shadow text-center">
                <h2 className="text-2xl font-semibold text-black mb-2"> Sector Growth Forecast (Next Quarter)</h2>
                {sectorGrowth === null ? (
                    <p className="text-gray-500">Loading forecast...</p>
                ) : (
                    <p className={`text-xl font-bold ${sectorGrowth > 0 ? "text-green-600" : sectorGrowth < 0 ? "text-red-500" : "text-gray-500"}`}>
                        {sectorGrowth > 0 ? "▲" : sectorGrowth < 0 ? "▼" : "–"} {Math.abs(sectorGrowth).toFixed(2)}%
                    </p>
                )}
            </div>
            {/* Market Sentiment */}
            {loading ? (
                <CircularProgress />
            ) : (
                <div className="mb-8 p-4 bg-white rounded shadow text-center">
                    <h2 className="text-2xl font-semibold text-black mb-2">
                        Market Sentiment as of {new Date(sectorSentiment?.date ?? "NULL").toLocaleDateString()}:
                    </h2>
                    <p
                        className={`text-xl mt-1 font-semibold ${
                            sectorSentiment?.label?.toLowerCase().includes("fear")
                                ? "text-red-600"
                                : sectorSentiment?.label?.toLowerCase() === "neutral"
                                    ? "text-gray-500"
                                    : "text-green-600"
                        }`}
                    >
                        <span className="font-bold">{sectorSentiment?.sentiment.toFixed(2)}</span> – {sectorSentiment?.label}
                    </p>
                </div>
            )}
            </div>

            {/* Heatmap Section */}
            <div className="w-full flex flex-col items-center">
                <Treemap stocks={stocks} width={1200} height={800} />
            </div>

            {/* Main Content */}
            <div className="w-full grid grid-cols-1 md:grid-cols-2 gap-8">
                
                {/* Top Movers Section */}
                <div className="p-6 bg-white shadow-md rounded-lg">
                    <h2 className="text-2xl font-semibold text-gray-700 mb-4">Top Movers</h2>
                    <p className="mb-4 text-gray-500">
                        (Data will be fetched for {selectedDate ? selectedDate.toDateString() : "the current date"})
                    </p>

                    <div className="overflow-x-auto">
                        <table className="min-w-full divide-y divide-gray-200 text-sm">
                            <thead className="bg-gray-100">
                            <tr>
                                <th className="px-4 py-2 text-left text-gray-600">Symbol</th>
                                <th className="px-4 py-2 text-left text-gray-600">Market Cap</th>
                                <th className="px-4 py-2 text-left text-gray-600">% Change</th>
                            </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-100">
                            {topMovers.map((stock) => (
                                <tr key={stock.symbol}>
                                    <Link href={`/stock/${stock.symbol}`} className="px-4 py-2 font-medium text-black p-4">
                                        {stock.symbol}
                                    </Link>
                                    <td className="px-4 py-2 text-gray-500">${(stock.marketCap / 1e9).toFixed(2)}B</td>
                                    <td className={`px-4 py-2 font-semibold ${stock.change >= 0 ? "text-green-600" : "text-red-500"}`}>
                                        {stock.change.toFixed(2)}%
                                    </td>
                                </tr>
                            ))}
                            </tbody>
                        </table>
                    </div>
                </div>


                {/* Top Market Cap Stocks Section */}
                <div className="p-6 bg-white shadow-md rounded-lg">
                    <h2 className="text-2xl font-semibold text-gray-700 mb-4">Top Market Cap Stocks</h2>
                    {/* Pie Chart for Market Cap */}
                    <div className="w-full flex justify-center mt-6">
                        <ResponsiveContainer width={800} height={600}>
                            <PieChart>
                                <Pie
                                    data={pieData}
                                    cx="50%"
                                    cy="50%"
                                    labelLine={true}
                                    outerRadius={360}
                                    fill="#8884d8"
                                    dataKey="marketCap"
                                    nameKey="symbol"
                                >
                                    {pieData.map((entry, index) => (
                                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                                    ))}
                                </Pie>
                                <Tooltip />
                                {/*<Legend />*/}
                            </PieChart>
                        </ResponsiveContainer>
                    </div>
                </div>
            </div>
           
            <div className="w-max">
                {/*<SectorAnalysis sectorName={sectorName} />*/}
            </div>
            
            <Footer />
        </div>
    );
}