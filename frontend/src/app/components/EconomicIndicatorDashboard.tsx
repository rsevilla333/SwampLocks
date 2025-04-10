"use client";

import { useEffect, useState } from "react";
import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer, BarChart, Bar } from "recharts";

interface Indicator {
    name: string;
    interval: string;
    unit: string;
}

interface DataPoint {
    date: string;
    value: number;
}

export default function EconomicIndicatorDashboard() {
    const [indicators, setIndicators] = useState<Indicator[]>([]);
    const [selectedIndicator, setSelectedIndicator] = useState<Indicator | null>(null);
    const [data, setData] = useState<DataPoint[]>([]);
    const [loading, setLoading] = useState(false);

    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
    
    useEffect(() => {
        const fetchIndicators = async () => {
            setLoading(true);
            try {
                const response = await fetch(`${API_BASE_URL}/api/financials/economic_data/indicators`);
                const indicatorList: Indicator[] = await response.json();

                // Set the indicators list in state
                setIndicators(indicatorList);

                // Set the default selected indicator (first one)
                if (indicatorList.length > 0) {
                    setSelectedIndicator(indicatorList[0]);
                }
            } catch (error) {
                console.error("Error fetching indicators:", error);
            } finally {
                setLoading(false);
            }
        };

        fetchIndicators();
    }, []);

    useEffect(() => {
        if (!selectedIndicator) return;

        const fetchIndicatorData = async () => {
            setLoading(true); 
            try {
                const response = await fetch(
                    `${API_BASE_URL}/api/financials/economic_data/${selectedIndicator.name}`
                );
                const indicatorData = await response.json();
                const filteredData = indicatorData.filter((dataPoint: DataPoint) => dataPoint.value !== 0);
                setData(filteredData);
            } catch (error) {
                console.error("Error fetching indicator data:", error);
            } finally {
                setLoading(false);
            }
        };

        fetchIndicatorData();
    }, [selectedIndicator]);

    const getChartType = () => {
        if (!selectedIndicator || data.length === 0) return null;

        if (["daily", "monthly", "quarterly", "annual"].includes(selectedIndicator.interval)) {
            return "line"; // Time-series data
        } else if (selectedIndicator.unit.includes("percent") || selectedIndicator.name === "Unemployment") {
            return "bar"; // Comparative
        } else {
            return "single"; // Single latest value
        }
    };

    const chartType = getChartType();

    return (
        <div className="w-full max-w-5xl mx-auto p-6">
            {/* Dropdown Selector */}
            <div className="mb-6 flex justify-center">
                <select
                    className="border border-gray-300 p-2 rounded-lg text-lg text-black"
                    value={selectedIndicator?.name || ""}
                    onChange={(e) => {
                        const indicator = indicators.find(ind => ind.name === e.target.value);
                        if (indicator) setSelectedIndicator(indicator);
                    }}
                >
                    {indicators.map((indicator) => (
                        <option key={indicator.name} value={indicator.name}>
                            {indicator.name} ({indicator.unit})
                        </option>
                    ))}
                </select>
            </div>

            {/* Display Indicator Data */}
            <div className="p-6 bg-white shadow-lg rounded-lg">
                {loading ? (
                    <p className="text-center text-gray-500">Loading data...</p>
                ) : chartType === "line" ? (
                    <LineChartComponent data={data} indicator={selectedIndicator!.name} />
                ) : chartType === "bar" ? (
                    <BarChartComponent data={data} indicator={selectedIndicator!.name} />
                ) : chartType === "single" ? (
                    <SingleValue data={data} indicator={selectedIndicator!.name} />
                ) : (
                    <p className="text-center text-gray-500">Select an indicator to view data</p>
                )}
            </div>
        </div>
    );
}

/* Line Chart */
const LineChartComponent = ({ data, indicator }: { data: DataPoint[], indicator: string }) => (
    <div className="w-full">
        <h2 className="text-xl font-semibold text-center mb-4">{indicator} Trend</h2>
        <ResponsiveContainer width="100%" height={400}>
            <LineChart data={data} >
                <XAxis dataKey="date" />
                <YAxis />
                <Tooltip />
                <Line type="monotone" dataKey="value" stroke="#3182CE" strokeWidth={2} dot={{ r: 0.3 }}/>
            </LineChart>
        </ResponsiveContainer>
    </div>
);

/* Bar Chart */
const BarChartComponent = ({ data, indicator }: { data: DataPoint[], indicator: string }) => (
    <div className="w-full">
        <h2 className="text-xl font-semibold text-center mb-4">{indicator} Overview</h2>
        <ResponsiveContainer width="100%" height={400}>
            <BarChart data={data}>
                <XAxis dataKey="date" />
                <YAxis />
                <Tooltip />
                <Bar dataKey="value" fill="#34D399" />
            </BarChart>
        </ResponsiveContainer>
    </div>
);

/* Single Value */
const SingleValue = ({ data, indicator }: { data: DataPoint[], indicator: string }) => {
    const latestValue = data.length > 0 ? data[data.length - 1].value : "N/A";

    return (
        <div className="text-center p-6 bg-white shadow-lg rounded-lg">
            <h2 className="text-2xl font-bold">{indicator}</h2>
            <p className="text-4xl font-bold text-green-500 mt-2">{latestValue}</p>
        </div>
    );
};
