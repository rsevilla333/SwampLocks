"use client";

import { useState, useEffect } from "react";
import axios from "axios";
import { Pencil, Trash2 } from "lucide-react";
import {
    ResponsiveContainer,
    ScatterChart,
    Scatter,
    XAxis,
    YAxis,
    ZAxis,
    Tooltip,
    BarChart,
    Legend,
    CartesianGrid,
    Bar,
    Cell
} from "recharts";

import Link from "next/link";

interface Holding {
    ticker: string;
    shares: number;
    sectorName: string;
    holdingId: number,
    stockPrice: number;
    isETF: boolean;
}

export default function UserPortfolio({ userId }: { userId: string }) {
    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
    const ML_API_BASE_URL = process.env.NEXT_PUBLIC_ML_MODEL_BASE_URL;
    
    const [holdings, setHoldings] = useState<Holding[]>([]);
    const [query, setQuery] = useState("");
    const [suggestions, setSuggestions] = useState<string[]>([]);
    const [selectedTicker, setSelectedTicker] = useState<string | null>(null);
    const [shares, setShares] = useState<number>(0);
    const [isEditing, setIsEditing] = useState<boolean>(false);
    const [editingIndex, setEditingIndex] = useState<number | null>(null);
    const [sectorData, setSectorData] = useState<{ name: string, value: number }[]>([]);
    const [totals, setTotals] = useState({
        totalValue: 0,
        totalNonETFValue: 0,
        totalFutureValue: 0,
    });

    useEffect(() => {
        if (query.length > 0) {
            const fetchSuggestions = async () => {
                try {
                    const response = await axios.get(
                        `${API_BASE_URL}/api/financials/stocks/autocomplete?query=${query}`
                    );
                    setSuggestions(response.data);
                } catch (error) {
                    console.error("Error fetching suggestions", error);
                    setSuggestions([]);
                }
            };
            fetchSuggestions();
        } else {
            setSuggestions([]);
        }
    }, [query]);

    useEffect(() => {
        if (holdings.length === 0) return;

        const calculateTotals = async () => {
            const result = await getPortfolioTotalsWithFuture(holdings);
            setTotals(result);
        };

        calculateTotals();
    }, [holdings]);
    
    useEffect(() => {
        if (!userId || userId.length !== 36) return;
        const fetchHoldings = async () => {
            try {
                await loadAndMapHoldings();
                
            } catch (err) {
                console.error("Failed to load holdings:", err);
            }
        };

        if (userId) {
            fetchHoldings();
        }
    }, [userId]);
    
    
    const loadAndMapHoldings = async () => {
        try {
            const res = await axios.get(`${API_BASE_URL}/api/financials/user/holdings/${userId}/get-holdings`);

            const mappedHoldings = await Promise.all(
                res.data.map(async (h: any) => {
                    const priceRes = await axios.get(`${API_BASE_URL}/api/financials/stocks/${h.ticker}/latest-price`);
                    return {
                        ticker: h.ticker,
                        shares: h.shares,
                        sectorName: h.stock?.sectorName || "Unknown",
                        holdingId: h.holdingId,
                        stockPrice: priceRes.data,
                        isETF: h.stock?.isETF || false, // fix here: use proper isETF field
                    };
                })
            );
            
            console.log("Received holding payload:", mappedHoldings);

            setHoldings(mappedHoldings);
            await getSectorAllocation(mappedHoldings);
        } catch (err) {
            console.error("Failed to load and map holdings:", err);
        }
    };
    const getSectorAllocation = async (currentHoldings: Holding[]) => {
        const sectorMap: Record<string, number> = {};

        for (const h of currentHoldings) {
            try {
                const weight = h.stockPrice * h.shares;
                sectorMap[h.sectorName] = (sectorMap[h.sectorName] || 0) + weight;
            } catch (err) {
                console.error(`Error fetching sector for ${h.ticker}`, err);
            }
        }

        const formatted = Object.entries(sectorMap).map(([name, value]) => ({ name, value }));
        console.log("Updated sectorData:", formatted);
        setSectorData(formatted);
    };

    async function getPortfolioTotalsWithFuture(holdings: Holding[]) {
        let totalValue = 0;
        let totalNonETFValue = 0;
        let totalFutureValue = 0;

        for (const h of holdings) {
            const currentValue = h.shares * h.stockPrice;
            totalValue += currentValue;

            if (!h.isETF) {
                totalNonETFValue += currentValue;
            }

            try {
                const response = await axios.get(`${ML_API_BASE_URL}/api/MLModel?ticker=${h.ticker}`);
                console.log("future stock price:", response);
                const futurePrice = response.data?.price ?? h.stockPrice; 
                totalFutureValue += futurePrice * h.shares;
            } catch (err) {
                console.error(`Error fetching future price for ${h.ticker}`, err);
                totalFutureValue += h.stockPrice * h.shares; // fallback again if error
            }
        }

        return {
            totalValue,
            totalNonETFValue,
            totalFutureValue
        };
    }
    
    const addOrUpdateHolding = async () => {
        if (!selectedTicker || shares <= 0 || !userId || userId.length !== 36) return;

        try {
            const holdingPayload = {
                userId: userId,
                ticker: selectedTicker,
                shares: shares
            };
            
            if (isEditing && editingIndex !== null) {
                const holdingId = (holdings[editingIndex] as any).holdingId;
                await axios.put(`${API_BASE_URL}/api/financials/holdings/${holdingId}`, holdingPayload);
            } else {
                

                console.log("Sending holding payload:", holdingPayload);
                
                await axios.post(`${API_BASE_URL}/api/financials/holdings`, holdingPayload);
            }

            // Refresh holdings
            await loadAndMapHoldings();

            // Reset 
            setQuery("");
            setSelectedTicker(null);
            setShares(0);
            setIsEditing(false);
            setEditingIndex(null);
        } catch (err) {
            console.error("Error saving holding:", err);
        }
    };


    const deleteHolding = async (index: number) => {
        const holdingId = (holdings[index] as any).holdingId;
        try {
            await axios.delete(`${API_BASE_URL}/api/financials/holdings/${holdingId}`);
            await loadAndMapHoldings();
        } catch (err) {
            console.error("Error deleting holding:", err);
        }
    };
    
    const editHolding = (index: number) => {
        const h = holdings[index];
        setQuery(h.ticker);
        setSelectedTicker(h.ticker);
        setShares(h.shares);
        setIsEditing(true);
        setEditingIndex(index);
    };
    
    
    const CustomizedLabel = ({ cx, cy, name }: any) => {
        return (
            <text
                x={cx}
                y={cy}
                textAnchor="middle"
                dominantBaseline="middle"
                fontSize={14}
                fill="black"
                fontWeight="1000"
            >
                {name}
            </text>
        );
    };

    const chartData = [
        {
            name: "Current",
            value: totals.totalNonETFValue,
            color: "#4B5563", // gray
        },
        {
            name: "Predicted",
            value: totals.totalFutureValue,
            color: totals.totalFutureValue >= totals.totalNonETFValue ? "#16A34A" : "#DC2626", // green or red
        },
    ];
    
    return (
        <div className="flex flex-col items-center max-w mx-auto gap-4 p-4 text-black">
            <div className="mt-8 w-full max-w-xl text-center">
                <h1 className="text-3xl font-bold mb-2">
                    Total Portfolio Value: ${totals.totalValue.toFixed(2)}
                </h1>
                <p className="text-sm text-gray-600 mb-4">
                    Predicted values are for next quarter and apply only to individual (non-ETF) stocks.
                </p>

                <ResponsiveContainer width="100%" height={250}>
                    <BarChart data={chartData}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="name" />
                        <YAxis />
                        <Tooltip formatter={(val: number) => `$${val.toFixed(2)}`} />
                        <Legend />
                        <Bar dataKey="value" name="Value">
                            {chartData.map((entry, index) => (
                                <Cell key={`cell-${index}`} fill={entry.color} />
                            ))}
                        </Bar>
                    </BarChart>
                </ResponsiveContainer>
            </div>
            <div className="flex max-w mx-auto gap-4 p-4 text-black">
                {/*  Holdings */}
                <div className="w-1/4">
                    <h2 className="text-2xl font-bold mb-4">Your Holdings</h2>
    
                    <ul className="mb-6 space-y-2">
                        {holdings.map((h, index) => (
                            <li
                                key={index}
                                className="bg-gray-100 p-2 rounded shadow flex justify-between items-center"
                            >
                           <span>
                            <span className="font-semibold">
                                <Link href={`/stock/${h.ticker}`} className="hover:underline text-gray-800">
                                    {h.ticker}
                                </Link>
                            </span>{" "}
                                 — {h.shares} shares
                            </span>
                                <div className="space-x-2 flex items-center">
                                    <button onClick={() => editHolding(index)} className="text-blue-600 hover:text-blue-800">
                                        <Pencil size={18} />
                                    </button>
                                    <button onClick={() => deleteHolding(index)} className="text-red-600 hover:text-red-800">
                                        <Trash2 size={18} />
                                    </button>
                                </div>
                            </li>
                        ))}
                    </ul>
    
                    {/* Add/Edit Form */}
                    <div className="space-y-4">
                        <input
                            type="text"
                            className="w-full border p-2 rounded"
                            placeholder="Search ticker..."
                            value={query}
                            onChange={(e) => {
                                setQuery(e.target.value);
                                setSelectedTicker(null);
                            }}
                        />
    
                        {suggestions.length > 0 && (
                            <ul className="border rounded shadow text-black max-h-40 overflow-y-auto">
                                {suggestions.map((s, index) => (
                                    <li
                                        key={index}
                                        onClick={() => {
                                            setQuery(s);
                                            setSelectedTicker(s);
                                            setSuggestions([]);
                                        }}
                                        className="p-2 hover:bg-gray-200 cursor-pointer"
                                    >
                                        {s}
                                    </li>
                                ))}
                            </ul>
                        )}
    
                        <input
                            type="number"
                            min="0"
                            className="w-full border p-2 rounded"
                            placeholder="Enter shares"
                            value={shares}
                            onChange={(e) => setShares(Number(e.target.value))}
                        />
    
                        <button
                            onClick={addOrUpdateHolding}
                            className="bg-blue-600 px-4 py-2 rounded text-black hover:bg-blue-700 w-full"
                        >
                            {isEditing ? "Update Holding" : "Add to Portfolio"}
                        </button>
                    </div>
                </div>
    
                {/* Sector Bubble Chart */}
                <div className="flex flex-col w-full">
                <h2 className="font-semibold mb-4 text-2xl ">Sector Allocation</h2>
                <div className="w-[90%] border border-dashed border-gray-300 rounded-lg p-3">
                    <ResponsiveContainer width="100%" height={400}>
                        <ScatterChart margin={{ top: 20, bottom: 20 }}>
                            <XAxis type="number" dataKey="x" domain={[0, 100]} hide />
                            <YAxis type="number" dataKey="y" domain={[0, 100]} hide />
                            <ZAxis type="number" dataKey="z" range={[1000, 20000]} />
                            <Scatter
                                name="Sectors"
                                data={
                                    (() => {
                                        const max = Math.max(...sectorData.map(d => d.value), 1);
                                        return sectorData.map((d, i, arr) => {
                                            const cleanedName = d.name.startsWith("__") ? d.name.slice(2).trim() : d.name;
    
                                            return {
                                                x: (i + 1) * (100 / (arr.length + 1)),
                                                y: 50,
                                                z: (d.value / max) * 800,
                                                name: cleanedName,
                                            };
                                        });
                                    })()
                                }
                                fill="#808080"
                                label={<CustomizedLabel />}
                            />
                        </ScatterChart>
                    </ResponsiveContainer>
                </div>
                </div>
            </div>
        </div>
    );
}
