import { useEffect, useState } from "react";
import axios from "axios";

interface IncomeStatement {
    totalRevenue: number;
    netIncome: number;
    operatingIncome: number;
}

export default function IncomeStatement({ ticker }: { ticker: string }) {
    const [incomeStatement, setIncomeStatement] = useState<IncomeStatement | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

    useEffect(() => {
        const fetchIncomeStatement = async () => {
            try {
                const response = await axios.get(`${API_BASE_URL}/api/financials/incomestatements/${ticker}`);
                setIncomeStatement(response.data);
            } catch (err) {
                setError("Failed to load income statement.");
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        fetchIncomeStatement();
    }, [ticker]);

    if (loading) return <p>Loading income statement...</p>;
    if (error) return <p className="text-red-500">{error}</p>;

    return (
        <section className="mb-8">
            <h2 className="text-xl font-bold mb-2">Income Statement</h2>
            <div className="grid grid-cols-2 gap-4">
                <p><strong>Total Revenue:</strong> ${incomeStatement?.totalRevenue.toLocaleString()}</p>
                <p><strong>Net Income:</strong> ${incomeStatement?.netIncome.toLocaleString()}</p>
                <p><strong>Operating Income:</strong> ${incomeStatement?.operatingIncome.toLocaleString()}</p>
            </div>
        </section>
    );
}
