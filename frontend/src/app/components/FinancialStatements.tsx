import { useEffect, useState } from "react";
import axios from "axios";
import {
    LineChart,
    Line,
    XAxis,
    YAxis,
    Tooltip,
    ResponsiveContainer,
    BarChart,
    Bar,
    PieChart,
    Pie,
    Cell, Legend
} from "recharts";
import { motion } from "framer-motion";

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

interface Earnings {
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

interface EarningStatement {
    ticker: string;
    fiscalDateEnding: string; // ISO Date string
    reportedDate: string;     // ISO Date string
    reportedEPS: number;
    estimatedEPS: number;
    surprise: number;
    suprisePercentage: number; // Note: the model has "SuprisePercentage", fix typo if needed
    reportTime: string;       // e.g., "After Market Close"
}

function generateInsights({
    cashFlowStatements,
    incomeStatements,
     balanceSheets
                          }: {
    cashFlowStatements: CashFlowStatement[],
    incomeStatements: IncomeStatement[],
    balanceSheets: BalanceSheet[]
}): string[] {
    const insights: string[] = [];

    if (cashFlowStatements.length >= 2) {
        const latest = cashFlowStatements[0];
        const previous = cashFlowStatements[1];
        const change = ((Number(latest.operatingCashFlow) - Number(previous.operatingCashFlow)) / Number(previous.operatingCashFlow)) * 100;


        if (change > 10) {
            insights.push(`Operating cash flow increased by ${change.toFixed(1)}% QoQ — a strong liquidity sign.`);
        } else if (change < -10) {
            insights.push(`Operating cash flow dropped by ${Math.abs(change).toFixed(1)}% QoQ — keep an eye on cash generation.`);
        }
    }

    if (incomeStatements.length >= 2) {
        const netChange = incomeStatements[0].netIncome - incomeStatements[1].netIncome;
        if (netChange > 0) {
            insights.push(`Net income rose by $${(netChange / 1_000_000).toFixed(2)}M.`);
        } else {
            insights.push(`Net income declined by $${(Math.abs(netChange) / 1_000_000).toFixed(2)}M.`);
        }
    }

    if (balanceSheets.length > 0) {
        const latestBS = balanceSheets[0];
        const ratio = latestBS.totalLiabilities / latestBS.totalShareholderEquity;
        if (ratio > 2) {
            insights.push(`High debt-to-equity ratio (${ratio.toFixed(2)}). Company is highly leveraged.`);
        } else if (ratio < 1) {
            insights.push(`Strong equity position with debt-to-equity ratio at ${ratio.toFixed(2)}.`);
        }
    }

    return insights;
}

export default function FinancialStatements({ ticker }: { ticker: string }) {
    const [cashFlowStatements, setCashFlowStatements] = useState<CashFlowStatement[]>([]);
    const [earnings, setEarnings] = useState<CashFlowStatement[]>([]);
    const [balanceSheets, setBalanceSheets] = useState<BalanceSheet[]>([]);
    const [incomeStatements, setIncomeStatements] = useState<IncomeStatement[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [yAxisDomain, setYAxisDomain] = useState<[number, number]>([0, 5000]);
    const COLORS = ["#0088FE", "#00C49F", "#FFBB28", "#FF8042", "#8884d8"];
    const [insights, setInsights] = useState<string[]>([]);
    

    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

    useEffect(() => {
        if (cashFlowStatements.length && incomeStatements.length && balanceSheets.length) {
            const generated = generateInsights({ cashFlowStatements, incomeStatements, balanceSheets });
            setInsights(generated);
        }
    }, [cashFlowStatements, incomeStatements, balanceSheets]);


    useEffect(() => {
        const fetchAll = async () => {
            try {
                const [cf, bs, is, er] = await Promise.all([
                    axios.get(`${API_BASE_URL}/api/financials/cashflowstatements/${ticker}`),
                    axios.get(`${API_BASE_URL}/api/financials/balancesheets/${ticker}`),
                    axios.get(`${API_BASE_URL}/api/financials/incomestatements/${ticker}`),
                    axios.get(`${API_BASE_URL}/api/financials/earnings/${ticker}`)
                ]);
                setCashFlowStatements(cf.data);
                setBalanceSheets(bs.data);
                setIncomeStatements(is.data);
                setEarnings(er.data);
            } catch (err) {
                setError("Error loading financial data");
                console.error(err);
            } finally {
                setLoading(false);
            }
        };
        fetchAll();
    }, [ticker]);

    
    if (loading) return <p>Loading financial statements...</p>;
    if (error) return <p className="text-red-500">{error}</p>;

    const latestCashFlow = cashFlowStatements[0];
    const latestIncomeStatement = incomeStatements[0];
    const latestBalanceSheet = balanceSheets[0];

    const debtToEquity = (latestBalanceSheet.totalLiabilities / latestBalanceSheet.totalShareholderEquity).toFixed(2);
    const returnOnAssets = (latestIncomeStatement.netIncome / latestBalanceSheet.totalAssets * 100).toFixed(1);
    
    
    const formatMillions = (num: number) => (num / 1_000_000).toFixed(2) + "M";
    
    if (!latestIncomeStatement) {
        return <p className="text-red-500">No latest income statement available.</p>;
    }

    // Pie chart data for asset breakdown
    const assetBreakdown = [
        { name: "Cash & Equivalents", value: latestBalanceSheet.cashAndCashEquivalents / 1_000_000 },
        { name: "Short-Term Investments", value: latestBalanceSheet.shortTermInvestments / 1_000_000 },
        { name: "Inventory", value: latestBalanceSheet.inventory / 1_000_000 },
        { name: "Property & Equipment", value: latestBalanceSheet.propertyPlantEquipment / 1_000_000 },
        { name: "Intangible Assets", value: latestBalanceSheet.intangibleAssets / 1_000_000 }
    ];

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
        <div className="bg-gray-100 p-4 rounded-lg shadow-md mb-6">
            <div className="mb-8 text-black w-full p-6 flex flex-col gap-11 ">
                <div className="flex flex-col text-black">
                    {insights.length > 0 && (
                        <div className=" p-8">
                            <h3 className="font-bold mb-2 text-lg">📊 Key Insights</h3>
                            <ul className="list-disc pl-6 space-y-1 text-gray-700 text-sm">
                                {insights.map((text, idx) => (
                                    <li key={idx}>{text}</li>
                                ))}
                            </ul>
                        </div>
                    )}
                </div>
                {/* Key Financials */}
                <div className="flex flex-col gap-5 ">
                    <p><strong className={"text-green-800"}>Total Assets:</strong> ${formatMillions(latestBalanceSheet.totalAssets)}</p>
                    <p><strong className={"text-red-800"}>Total Liabilities:</strong> ${formatMillions(latestBalanceSheet.totalLiabilities)}</p>
                    <p><strong>Shareholder Equity:</strong> ${formatMillions(latestBalanceSheet.totalShareholderEquity)}</p>
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
                    <li><strong>Debt to Equity:</strong> {debtToEquity}</li>
                    <li><strong>Return on Assets (ROA):</strong> {returnOnAssets}%</li>
                </div>
                {/* Asset Breakdown */}
                <h3 className="text-lg font-semibold mt-4">Asset Breakdown</h3>
                <motion.div initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.6 }}>
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
                </motion.div>

                {/* Assets vs. Liabilities Over Time */}
                <h3 className="text-lg font-semibold mt-4">Assets/Liabilities Over Time</h3>
                <motion.div initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.6 }}>
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
                </motion.div>
            </div>
            <div className="mb-8 text-black w-full p-6 flex flex-col gap-11">
                
                {/* Cash Flow Trends */}
                <h3 className="text-lg font-semibold mt-4">Cash Flow Trends Over Time</h3>
                <motion.div initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.6 }}>
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
                </motion.div>
    
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
                {/* EPS Data */}
                <ResponsiveContainer width="100%" height={300}>
                    <BarChart data={earnings}>
                       {/*<XAxis dataKey="fiscalDateEnding" />*/}
                        <Tooltip />
                        <Legend />
                        <Bar dataKey="reportedEPS" fill="#4ade80" name="Reported EPS" />
                        <Bar dataKey="estimatedEPS" fill="#60a5fa" name="Estimated EPS" />
                    </BarChart>
                </ResponsiveContainer>
            </div>
        </div>
    );
}
