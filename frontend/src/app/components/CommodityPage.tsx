"use client";

import { useState, useEffect } from "react";
import { ResponsiveContainer, LineChart, XAxis, YAxis, Tooltip, Line, Legend } from "recharts";

type CommodityData = {
    date: string;
    price: number;
    commodityName: string;
};

type Indicator = {
    name: string;
    interval: string;
    unit: string;
};

const colorList = [
    "#FF5733", "#33A1FF", "#33FF57", "#FFD700", "#8B4513",
    "#FFA500", "#FF69B4", "#D2691E", "#654321", "#4B0082",
    "#A52A2A", "#20B2AA", "#DC143C", "#8A2BE2", "#32CD32"
];

const assignedColors: Record<string, string> = {};

export default function CommodityPage() {
    const [commodityData, setCommodityData] = useState<Record<string, CommodityData[]>>({});
    const [indicators, setIndicators] = useState<Record<string, Indicator>>({});
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        async function fetchCommodityData() {
            try {
                const response = await fetch(`http://localhost:5196/api/financials/commodities/indicators`);
                const indicatorList: Indicator[] = await response.json();

                // Assign colors dynamically
                indicatorList.forEach((indicator, index) => {
                    assignedColors[indicator.name] = colorList[index % colorList.length];
                });

                const dataPromises = indicatorList.map(async (indicator) => {
                    const res = await fetch(`http://localhost:5196/api/financials/commodities/${indicator.name}`);
                    if (!res.ok) return null;

                    const rawData: CommodityData[] = await res.json();
                    const filteredData = rawData.filter((entry) => entry.price > 0);

                    return { name: indicator.name, unit: indicator.unit, data: filteredData };
                });

                const results = (await Promise.all(dataPromises)).filter(
                    (res): res is { name: string; unit: string; data: CommodityData[] } => res !== null
                );

                const dataMap: Record<string, CommodityData[]> = {};
                const indicatorMap: Record<string, Indicator> = {};

                results.forEach((result) => {
                    dataMap[result.name] = result.data;
                    indicatorMap[result.name] = {
                        name: result.name,
                        interval: indicatorList.find((ind) => ind.name === result.name)?.interval || "N/A",
                        unit: result.unit,
                    };
                });

                setCommodityData(dataMap);
                setIndicators(indicatorMap);
            } catch (err) {
                setError((err as Error).message);
            } finally {
                setIsLoading(false);
            }
        }

        fetchCommodityData();
    }, []);

    if (isLoading) return <p className="text-center text-gray-500">Loading...</p>;
    if (error) return <p className="text-center text-red-500">{error}</p>;

    return (
        <div className="w-full max-w-7xl mx-auto p-6 bg-transparent">
            <h2 className="text-3xl text-black font-extrabold text-center mb-6">Commodities</h2>

            {/* Grid Layout for Charts */}
            <div className="flex flex-col gap-6">
                {Object.entries(commodityData).map(([commodity, data]) => {
                    const indicator = indicators[commodity];

                    return (
                        <div key={commodity} className="bg-transparent p-4 rounded-md border-2 border-accent ">
                            <h3 className="text-xl text-secondary font-semibold text-center">{indicator.name}</h3>
                            <ResponsiveContainer width="100%" height={300}>
                                <LineChart data={data}>
                                    <XAxis  />
                                    <YAxis  />
                                    <Tooltip />
                                    <Legend />
                                    <Line
                                        type="monotone"
                                        dataKey="price"
                                        stroke = {assignedColors[commodity]}
                                        strokeWidth={2}
                                        name={indicator.unit} 
                                        dot={{ r: 0.3 }}
                                    />
                                </LineChart>
                            </ResponsiveContainer>
                        </div>
                    );
                })}
            </div>
        </div>
    );
}
