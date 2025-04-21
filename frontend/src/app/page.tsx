"use client";

import { useRouter } from 'next/navigation';
import Treemap from "./components/TreeMap";
import Footer from "./components/Footer";
import StockSearchBar from "./components/StockSearchBar";
import Articles from "./components/Articles";
import TopMoverDashBoard from "./components/TopMoversDashboard";
import { Switch } from "@headlessui/react";
import MountainMap from "./components/MountainMap";
import {useState, useEffect} from "react";
import CircularProgress from '@mui/material/CircularProgress';
import {
    BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, LabelList, PieChart, Pie, Cell
} from "recharts";
import {router} from "next/client";


interface SectorSentiment {
    sectorName: string;
    sentiment: number;
    label: string;
    date: string;
}
interface StockWithChange {
    symbol: string;
    marketCap: number;
    change: number;
}

interface RankedSector {
    name: string;
    path: string;
    score: number;
}

export default function Home() {
    const router = useRouter();
    const [selectedMap, setSelectedMap] = useState<'mountain' | 'treemap'>('treemap');
    const [stocks, setStocks] = useState<StockWithChange[]>([]);
    const [loading, setLoading] = useState(true);
    const [rankedSectors, setRankedSectors] = useState<RankedSector[]>([]);
    const [marketSentiment, setMarketSentiment] = useState<SectorSentiment | null>(null);
    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
    const sectors = [
        { name: "Information Technology", path: "technology" },
        { name: "Energy", path: "energy" },
        { name: "Healthcare", path: "healthcare" },
        { name: "Financials", path: "financials" },
        { name: "Consumer Discretionary", path: "consumer-discretionary" },
        { name: "Consumer Staples", path: "consumer-staples" },
        { name: "Industrials", path: "industrials" },
        { name: "Materials", path: "materials" },
        { name: "Real Estate", path: "real-estate" },
        { name: "Utilities", path: "utilities" },
        { name: "Communication Services", path: "communication-services" }
    ];

    const fetchTopMarketCapWithChange = async (): Promise<StockWithChange[]> => {
        const count = 100;

        try {
            const response = await fetch(`${API_BASE_URL}/api/financials/top-marketcap-with-change?count=${count}`);
            console.log(response);
            if (!response.ok) {
                throw new Error("Failed to fetch market cap data");
            }

            const stocks: StockWithChange[] = await response.json();
            console.log("✅ Stocks with market cap & change loaded:", stocks);
            return stocks;
        } catch (err) {
            console.error("❌ Error fetching top market cap stocks:", err);
            return [];
        }
    };

    useEffect(() => {
        const fetchSectorScores = async () => {
            const results: RankedSector[] = await Promise.all(
                sectors.map(async (sector) => {
                    try {
                        const res = await fetch(`${API_BASE_URL}/api/financials/sector-growth?sectorName=${sector.name}`);
                        const score = await res.json();
                        return { ...sector, score };
                    } catch (err) {
                        console.error(`Failed to fetch score for ${sector.name}`, err);
                        return { ...sector, score: 0 };
                    }
                })
            );

            results.sort((a, b) => b.score - a.score);
            setRankedSectors(results);
        };

        fetchSectorScores();
    }, []);

    const fetchMarketSentiment = async (): Promise<SectorSentiment | null> => {
        try {
            const url = `${API_BASE_URL}/api/financials/sector/sentiment/__Market`;
            const response = await fetch(url);
            console.log(response);
            if (!response.ok) throw new Error("Failed to fetch sector sentiment");
            const sentiment: SectorSentiment = await response.json();
            return sentiment;
        } catch (err) {
            console.error("Error fetching sector sentiment:", err);
            return null;
        }
    };
    
    useEffect(() => {
        const load = async () => {
            setLoading(true);
            const stocks = await fetchTopMarketCapWithChange();
            setStocks(stocks);
            const sentiment = await fetchMarketSentiment();
            setMarketSentiment(sentiment);
            setLoading(false);
        };
        load();
    }, []);

    const barColors = (value: number) => (value >= 0 ? "#16A34A" : "#DC2626"); // green/red
        return (
            <div className="w-full h-full flex flex-row">
                {/* Right Sidebar for Articles */}
                <div className="w-full h-full flex flex-col items-center gap-y-10 w-3/4">
                    {/* Stock Search Bar */}
                    <div className="p-6 w-full mx-auto">
                        <StockSearchBar />
                    </div>
    
                    {/* Header Text */}
                    <header className="w-full flex flex-col items-center">
                        <p className="text-center text-lg text-gray-500 max-w-4xl mb-8 font-medium">
                            Your personalized portfolio optimization tool  powered by machine learning, sentiment analysis, and sector performance insights. Strategically allocate investments based on data-driven recommendations.
                        </p>
                    </header>

                    {/* Market Sentiment */}
                    {loading ? (
                        <CircularProgress />
                    ) : (
                        <div className="mb-8 p-4 bg-white rounded shadow text-center">
                            <h2 className="text-lg font-semibold text-gray-800">
                                Market Sentiment as of {new Date(marketSentiment?.date ?? "NULL").toLocaleDateString()}:
                            </h2>
                            <p
                                className={`text-xl mt-1 font-semibold ${
                                    marketSentiment?.label?.toLowerCase().includes("fear")
                                        ? "text-red-600"
                                        : marketSentiment?.label?.toLowerCase() === "neutral"
                                            ? "text-gray-500"
                                            : "text-green-600"
                                }`}
                            >
                                <span className="font-bold">{marketSentiment?.sentiment.toFixed(2)}</span> – {marketSentiment?.label}
                            </p>
                        </div>
                    )}
                    <div className="mb-4 flex flex-row items-center gap-2">
                        <h1 className="text-black text-4xl">The Market</h1>
                        <Switch
                            checked={selectedMap === 'treemap'}
                            onChange={() => setSelectedMap(selectedMap === 'mountain' ? 'treemap' : 'mountain')}
                            className={`${
                                selectedMap === 'treemap' ? 'bg-blue-500' : 'bg-gray-300'
                            } relative inline-flex items-center h-6 rounded-full w-12 transition-colors duration-200 ease-in-out`}
                        >
                        <span
                            className={`${
                                selectedMap === 'treemap' ? 'translate-x-6' : 'translate-x-1'
                            } inline-block w-4 h-4 transform bg-white rounded-full transition-transform duration-200 ease-in-out`}
                        />
                        </Switch>
                    </div>
    
    
                    {/* Conditionally render the selected map */}
                    {loading ? (
                        <CircularProgress />
                    ) : selectedMap === 'mountain' ? (
                        <MountainMap stocks={stocks} />
                    ) : (
                        <Treemap stocks={stocks} width ={1000} height = {600} />
                    )}
    
                    
                    <div className="flex w-full max-w-7xl gap-8 max-h-full">
                        
                        <div className="w-full flex flex-col gap-8">
                            {/* Sectors Section */}
                            <div className="w-full h-[600px]">
                                <ResponsiveContainer width="100%" height="100%">
                                    <BarChart
                                        layout="vertical"
                                        data={rankedSectors}
                                        margin={{ top: 20, right: 30, left: 100, bottom: 20 }}
                                    >
                                        <CartesianGrid strokeDasharray="3 3" />
                                        <XAxis type="number" />
                                        <YAxis
                                            dataKey="name"
                                            type="category"
                                            tick={{ fontSize: 14, fill: "#374151" }}
                                        />
                                        <Tooltip formatter={(val: number) => `${(val).toFixed(2)}%`} />
                                        <Bar 
                                            dataKey="score"
                                            onClick={(data) => {
                                                const match = rankedSectors.find(s => s.name === data.name);
                                                if (match) router.push(`/sector/${match.path}`);
                                            }}
                                            cursor="pointer">
                                            {rankedSectors.map((entry, index) => (
                                                <Cell key={`bar-${index}`} fill={barColors(entry.score)} />
                                            ))}
                                            <LabelList
                                                dataKey="score"
                                                position="right"
                                                formatter={(val: number) => `${(val).toFixed(2)}%`}
                                            />
                                        </Bar>
                                    </BarChart>
                                </ResponsiveContainer>
                            </div>
                            
                            {/* Top Movers */}
                            <div className="min-w-full max-h-[550px] bg-transparent">
                                <h2 className="text-black text-4xl items-center">Top Movers</h2>
                                <TopMoverDashBoard />
                            </div>
                        </div>
                    </div>
                    <Footer></Footer>
                </div>
                <div className="w-1/4 bg-transparent h-full">
                    <h2 className="text-black text-4xl ">News</h2>
                    <Articles /> 
                </div>
            </div>
        );
        }
/// 
