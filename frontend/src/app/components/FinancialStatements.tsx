import { useEffect, useState } from "react";
import axios from "axios";
import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer, BarChart, Bar } from "recharts";

interface CashFlowStatement {
    operatingCashFlow: number;
    cashFlowFromFinancing: number;
    cashFlowFromInvestment: number;
    netIncome: number;
    fiscalDateEnding: string;
    capitalExpenditures: number;
    dividendPayout: number;
    changeInCashAndCashEquivalents: number;
}

interface IncomeStatement {
    netIncome: number;
    grossProfit: number;
    operatingIncome: number;
    fiscalDateEnding: string;
}

export default function FinancialStatements({ ticker }: { ticker: string }) {
    const [cashFlowStatements, setCashFlowStatements] = useState<CashFlowStatement[]>([]);
    const [incomeStatements, setIncomeStatements] = useState<IncomeStatement[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [yAxisDomain, setYAxisDomain] = useState<[number, number]>([0, 5000]);

    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

    useEffect(() => {
        const fetchCashFlowStatements = async () => {
            try {
                const response = await axios.get(`${API_BASE_URL}/api/financials/cashflowstatements/${ticker}`);
                if (Array.isArray(response.data) && response.data.length > 0) {
                    const sortedCashFlowStatements = response.data.sort((a, b) => new Date(b.fiscalDateEnding).getTime() - new Date(a.fiscalDateEnding).getTime());
                    setCashFlowStatements(sortedCashFlowStatements);
                } else {
                    setError("No cash flow data available.");
                }
            } catch (err) {
                setError("Failed to load cash flow statement.");
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        fetchCashFlowStatements();

        const fetchIncomeStatements = async () => {
            try {
                const response = await axios.get(`${API_BASE_URL}/api/financials/incomestatements/${ticker}`);
                console.log("IncomeStatements Response:", response.data);
                if (Array.isArray(response.data) && response.data.length > 0) {
                    const sortedIncomeStatements = response.data.sort((a, b) => new Date(b.fiscalDateEnding).getTime() - new Date(a.fiscalDateEnding).getTime());
                    setIncomeStatements(sortedIncomeStatements);
                } else {
                    setError("No income statement data available.");
                }
            } catch (err) {
                setError("Failed to load income statements.");
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        fetchCashFlowStatements();
        fetchIncomeStatements();
    }, [ticker]);
    

    if (loading) return <p>Loading financial statements...</p>;
    if (error) return <p className="text-red-500">{error}</p>;

    const latestCashFlow = cashFlowStatements[0];
    const latestIncomeStatement = incomeStatements[0];
    const formatMillions = (num: number) => (num / 1_000_000).toFixed(2) + "M";
    
    if (!latestIncomeStatement) {
        return <p className="text-red-500">No latest income statement available.</p>;
    }

    // Line chart data for cash flow trends over time
    const cashFlowTrend = cashFlowStatements.map((statement) => ({
        fiscalDateEnding: new Date(statement.fiscalDateEnding).toLocaleDateString(),
        operatingCashFlow: statement.operatingCashFlow / 1_000_000,
        cashFlowFromFinancing: statement.cashFlowFromFinancing / 1_000_000,
        cashFlowFromInvestment: statement.cashFlowFromInvestment / 1_000_000,
        capitalExpenditures: statement.capitalExpenditures / 1_000_000,
    })) .sort((a, b) => new Date(a.fiscalDateEnding).getTime() - new Date(b.fiscalDateEnding).getTime()); 

    const incomeStatementTrend = incomeStatements.map((statement) => ({
        fiscalDateEnding: new Date(statement.fiscalDateEnding).toLocaleDateString(),
        netIncome: statement.netIncome / 1_000_000,
        grossProfit: statement.grossProfit / 1_000_000,
        operatingIncome: statement.operatingIncome / 1_000_000,
    }));

    // Combining data for cash flow and income statement for the bar chart
    const combinedData = cashFlowStatements.map((cashFlow, index) => ({
        fiscalDateEnding: new Date(cashFlow.fiscalDateEnding).toLocaleDateString(),
        operatingCashFlow: cashFlow.operatingCashFlow / 1_000_000,
        netIncome: incomeStatements[index]?.netIncome / 1_000_000 || 0,
        capitalExpenditures: cashFlow.capitalExpenditures / 1_000_000,
    })).sort((a, b) => new Date(a.fiscalDateEnding).getTime() - new Date(b.fiscalDateEnding).getTime());

    const zoomIn = () => {
        setYAxisDomain([yAxisDomain[0], yAxisDomain[1] * 0.8]); // Zoom out by 20%
    };

    const zoomOut = () => {
        setYAxisDomain([yAxisDomain[0], yAxisDomain[1] * 1.2]); // Zoom in by 20%
    };

    return (
        <div className="mb-8 text-black w-full p-6 flex flex-col gap-11">
            {/* Key Financials */}
            <div className="flex flex-col gap-5">
                <p>
                    <strong className={"text-green-800"}>Operating Cash Flow:</strong>
                    {latestCashFlow ? (
                        `$${formatMillions(latestCashFlow.operatingCashFlow ?? 0)}`
                    ) : (
                        'Loading...'
                    )}
                </p>

                <p>
                    <strong className={"text-red-800"}>Net Income:</strong>
                    {latestIncomeStatement ? (
                        `$${formatMillions(latestIncomeStatement.netIncome ?? 0)}`
                    ) : (
                        'Loading...'
                    )}
                </p>
            </div>

            {/* Cash Flow Trends */}
            <h3 className="text-lg font-semibold mt-4">Cash Flow Trends Over Time</h3>
            <ResponsiveContainer width="100%" height={300}>
                <LineChart data={cashFlowTrend}>
                    <XAxis dataKey="fiscalDateEnding" />
                    <YAxis />
                    <Tooltip formatter={(value) => `${value}M`} />
                    <Line type="monotone" dataKey="operatingCashFlow" stroke="#00C49F" name="Operating Cash Flow" dot={{ r: 0.3 }} />
                    <Line type="monotone" dataKey="cashFlowFromFinancing" stroke="#FFBB28" name="Cash Flow From Financing" dot={{ r: 0.3 }} />
                    <Line type="monotone" dataKey="cashFlowFromInvestment" stroke="#FF8042" name="Cash Flow From Investment" dot={{ r: 0.3 }} />
                    <Line type="monotone" dataKey="capitalExpenditures" stroke="#8884d8" name="Capital Expenditures" dot={{ r: 0.3 }} />
                </LineChart>
            </ResponsiveContainer>

            {/* Combined Cash Flow and Net Income */}
            <h3 className="text-lg font-semibold mt-4">Cash Flow vs Net Income</h3>
            <ResponsiveContainer width="100%" height={300}>
                <BarChart data={combinedData}>
                    <XAxis dataKey="fiscalDateEnding" />
                    <YAxis />
                    <Tooltip formatter={(value) => `${value}M`} />
                    <Bar dataKey="operatingCashFlow" fill="#00C49F" name="Operating Cash Flow" />
                    <Bar dataKey="netIncome" fill="#FF8042" name="Net Income" />
                </BarChart>
            </ResponsiveContainer>
        </div>
    );
}
