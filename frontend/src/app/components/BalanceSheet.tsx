import { useEffect, useState } from "react";
import axios from "axios";
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, PieChart, Pie, Cell, Legend } from "recharts";

interface BalanceSheet {
    fiscalYear: number;
    totalAssets: number;
    totalLiabilities: number;
    cashAndCashEquivalents: number;
    shortTermInvestments: number;
    inventory: number;
    propertyPlantEquipment: number;
    intangibleAssets: number;
    totalShareholderEquity: number;
}

export default function BalanceSheet({ ticker }: { ticker: string }) {
    const [balanceSheets, setBalanceSheets] = useState<BalanceSheet[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    
    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
    
    useEffect(() => {
        const fetchBalanceSheets = async () => {
            try {
                const response = await axios.get(`${API_BASE_URL}/api/financials/balancesheets/${ticker}`);
                console.log("Balance Sheets Response:", response.data);

                if (Array.isArray(response.data) && response.data.length > 0) {
                    const sortedBalanceSheets = response.data.sort((a, b) => b.fiscalYear - a.fiscalYear);
                    setBalanceSheets(sortedBalanceSheets);
                } else {
                    setError("No balance sheet data available.");
                }
            } catch (err) {
                setError("Failed to load balance sheet.");
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        fetchBalanceSheets();
    }, [ticker]);

    if (loading) return <p>Loading balance sheet...</p>;
    if (error) return <p className="text-red-500">{error}</p>;

    const latestBalanceSheet = balanceSheets[0];
    const formatMillions = (num: number) => (num / 1_000_000).toFixed(2) + "M";

    // Pie chart data for asset breakdown
    const assetBreakdown = [
        { name: "Cash & Equivalents", value: latestBalanceSheet.cashAndCashEquivalents / 1_000_000 },
        { name: "Short-Term Investments", value: latestBalanceSheet.shortTermInvestments / 1_000_000 },
        { name: "Inventory", value: latestBalanceSheet.inventory / 1_000_000 },
        { name: "Property & Equipment", value: latestBalanceSheet.propertyPlantEquipment / 1_000_000 },
        { name: "Intangible Assets", value: latestBalanceSheet.intangibleAssets / 1_000_000 }
    ];

    const COLORS = ["#0088FE", "#00C49F", "#FFBB28", "#FF8042", "#8884d8"];

    return (
        <div className="mb-8 text-black w-full p-6 flex flex-col gap-11">
            {/* Key Financials */}
            <div className="flex flex-col gap-5">
                <p><strong className={"text-green-800"}>Total Assets:</strong> ${formatMillions(latestBalanceSheet.totalAssets)}</p>
                <p><strong className={"text-red-800"}>Total Liabilities:</strong> ${formatMillions(latestBalanceSheet.totalLiabilities)}</p>
                <p><strong>Shareholder Equity:</strong> ${formatMillions(latestBalanceSheet.totalShareholderEquity)}</p>
            </div>

            {/* Assets vs. Liabilities Over Time */}
            <h3 className="text-lg font-semibold mt-4">Assets/Liabilities Over Time</h3>
            <ResponsiveContainer width="100%" height={300}>
                <BarChart data={balanceSheets
                    .map(bs => ({
                        ...bs,
                        totalAssets: bs.totalAssets / 1_000_000,
                        totalLiabilities: bs.totalLiabilities / 1_000_000
                    }))
                    .sort((a, b) => a.fiscalYear - b.fiscalYear)} // Sort fiscalYear in descending order
                >
                    <XAxis dataKey="fiscalYear" />
                    <Tooltip formatter={(value) => `${value}M`} />
                    <Bar dataKey="totalAssets" fill="#2CCE2C" name="Total Assets" />
                    <Bar dataKey="totalLiabilities" fill="#FF0000" name="Total Liabilities" />
                </BarChart>
            </ResponsiveContainer>

            {/* Asset Breakdown */}
            <h3 className="text-lg font-semibold mt-4">Asset Breakdown</h3>
            <ResponsiveContainer width="100%" height={500}>
                <PieChart>
                    <Pie
                        data={assetBreakdown}
                        cx="50%"
                        cy="50%"
                        outerRadius={200}
                        fill="#8884d8"
                        dataKey="value"
                        //label={({ name, value }) => `${name}: ${value}M`}
                    >
                        {assetBreakdown.map((entry, index) => (
                            <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                        ))}
                    </Pie>
                    <Tooltip />
                    <Legend
                        verticalAlign="middle"
                        align="right" 
                        layout="vertical" 
                        iconSize={10}  
                    />
                </PieChart>
            </ResponsiveContainer>
        </div>
    );
}
