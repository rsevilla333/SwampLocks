"use client";
import { useEffect, useState } from "react";
import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer } from "recharts";

interface ExchangeRate {
    date: string;
    targetCurrency: string;
    rate: number;
}

interface RateStats {
    day: string;
    weekly: string;
    monthly: string;
    ytd: string;
    yoy: string;
}

export default function ExRatesDashBoard() {
    const [exchangeRates, setExchangeRates] = useState<ExchangeRate[]>([]);
    const [latestRates, setLatestRates] = useState<Record<string, ExchangeRate>>({});
    const [historicalRates, setHistoricalRates] = useState<Record<string, ExchangeRate[]>>({});
    
    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

    useEffect(() => {
        const fetchExchangeRates = async () => {
            try {
                const response = await fetch(`${API_BASE_URL}/api/financials/ex_rates`);
                const data: ExchangeRate[] = await response.json();
                
                const ratesByCurrency: Record<string, ExchangeRate[]> = {};
                data.forEach(rate => {
                    if (!ratesByCurrency[rate.targetCurrency]) {
                        ratesByCurrency[rate.targetCurrency] = [];
                    }
                    ratesByCurrency[rate.targetCurrency].push(rate);
                });
                
                Object.keys(ratesByCurrency).forEach(currency => {
                    ratesByCurrency[currency].sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
                });

                // Get latest rate for each currency
                const latest: Record<string, ExchangeRate> = {};
                Object.keys(ratesByCurrency).forEach(currency => {
                    latest[currency] = ratesByCurrency[currency][0];
                });

                setExchangeRates(data);
                setLatestRates(latest);
                setHistoricalRates(ratesByCurrency);
            } catch (error) {
                console.error("Error fetching exchange rates:", error);
            }
        };

        fetchExchangeRates();
    }, []);

    const calculateStats = (currency: string): RateStats | undefined => {
        const rates = historicalRates[currency];
        if (!rates || rates.length < 2) return undefined;

        const latestRate = rates[0].rate;
        const previousDayRate = rates[1].rate;

        const dayChange = ((latestRate - previousDayRate) / previousDayRate) * 100;

        // Weekly, Monthly, YTD, and YoY can be calculated in a similar fashion
        const weeklyChange = ((latestRate - rates[7]?.rate || latestRate) / (rates[7]?.rate || latestRate)) * 100;
        const monthlyChange = ((latestRate - rates[30]?.rate || latestRate) / (rates[30]?.rate || latestRate)) * 100;
        const ytdChange = ((latestRate - rates[365]?.rate || latestRate) / (rates[365]?.rate || latestRate)) * 100;
        const yoyChange = ((latestRate - rates[365 * 2]?.rate || latestRate) / (rates[365 * 2]?.rate || latestRate)) * 100;

        return {
            day: dayChange.toFixed(2) + "%",
            weekly: weeklyChange.toFixed(2) + "%",
            monthly: monthlyChange.toFixed(2) + "%",
            ytd: ytdChange.toFixed(2) + "%",
            yoy: yoyChange.toFixed(2) + "%",
        };
    };

    const getColorForChange = (change: number): string => {
        if (change >= 2.5) return "#008000"; // Bright green (strong positive)
        else if (change >= 2) return "#43a047"; // Bright medium green (good positive)
        else if (change >= 1.5) return "#66bb6a"; // Lively green (moderate positive)
        else if (change >= 0.5) return "#81c784"; // Fresh light green (small positive)
        else if (change >= 0) return "#a5d6a7"; // Very light green (minimal positive)
        else if (change <= -2.5) return "#e53935"; // Bright red (strong negative)
        else if (change <= -2) return "#f44336"; // Vivid red (bad negative)
        else if (change <= -1.5) return "#ef5350"; // Strong red (moderate negative)
        else if (change <= -0.5) return "#ff7043"; // Lively red (small negative)
        else return "#ff8a65"; // Light red (very small negative)
    };

    return (
        <div>
            <table className="min-w-full border-collapse text-black">
                <thead>
                <tr className="text-white bg-black">
                    <th className="border px-4 py-2">Major</th>
                    <th className="border px-4 py-2">Price</th>
                    <th className="border px-4 py-2">Day</th>
                    <th className="border px-4 py-2">Weekly</th>
                    <th className="border px-4 py-2">Monthly</th>
                    <th className="border px-4 py-2">YTD</th>
                    {/*<th className="border px-4 py-2">YoY</th>*/}
                    <th className="border px-4 py-2">Date</th>
                </tr>
                </thead>
                <tbody>
                {Object.keys(latestRates).map(currency => {
                    const latest = latestRates[currency];
                    const stats = calculateStats(currency);
                    const formattedDate = new Date(latest.date).toLocaleDateString();
                    const dayChange = stats ? parseFloat(stats.day) : 0;
                    const weeklyChange = stats ? parseFloat(stats.weekly) : 0;
                    const monthlyChange = stats ? parseFloat(stats.monthly) : 0;
                    const ytdChange = stats ? parseFloat(stats.ytd) : 0;

                    return (
                        <tr key={currency} className="border-2 border-black">
                            <td className="border px-4 py-2" style={{ backgroundColor: "#f0f0f0" }}>
                                {currency}
                            </td>
                            <td className="border px-4 py-2" style={{ backgroundColor: "#f0f0f0" }}>
                                {latest.rate.toFixed(5)}
                            </td>
                            <td className="border px-4 py-2" style={{ backgroundColor: getColorForChange(dayChange) }}>
                                {stats?.day}
                            </td>
                            <td className="border px-4 py-2" style={{ backgroundColor: getColorForChange(weeklyChange) }}>
                                {stats?.weekly}
                            </td>
                            <td className="border px-4 py-2" style={{ backgroundColor: getColorForChange(monthlyChange) }}>
                                {stats?.monthly}
                            </td>
                            <td className="border px-4 py-2" style={{ backgroundColor: getColorForChange(ytdChange) }}>
                                {stats?.ytd}
                            </td>
                            {/*<td className="border px-4 py-2">{stats?.yoy}</td>*/}
                            <td className="border px-4 py-2" style={{ backgroundColor: "#f0f0f0" }}>
                                {formattedDate}
                            </td>
                        </tr>
                    );
                })}
                </tbody>
            </table>
        </div>
    );
}
