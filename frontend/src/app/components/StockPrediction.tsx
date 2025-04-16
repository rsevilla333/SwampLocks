import React, { useEffect, useState } from "react";
import axios from "axios";
import { ArrowUpRight, ArrowDownRight, Minus } from "lucide-react";
import { Switch } from "@headlessui/react";

interface StockPredictionProps {
    ticker: string;
}

const StockPrediction: React.FC<StockPredictionProps> = ({ ticker }) => {
    const ML_API_BASE_URL = process.env.NEXT_PUBLIC_ML_MODEL_BASE_URL;
    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

    const [predictedPrice, setPredictedPrice] = useState<number | null>(null);
    const [currentPrice, setCurrentPrice] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchPrediction = async () => {
            try {
                const responsePresent = await axios.get(`${API_BASE_URL}/api/financials/stocks/${ticker}/latest-price`);
                const responseFuture = await axios.get(`${ML_API_BASE_URL}/api/MLModel?ticker=${ticker}`);
                setPredictedPrice(responseFuture.data.price);
                setCurrentPrice(responsePresent.data);
            } catch (err) {
                setError("Error fetching prediction");
            }
        };

        if (ticker) fetchPrediction();
    }, [ticker]);

    const getIndicator = () => {
        if (currentPrice === null || predictedPrice === null) return null;

        const diff = predictedPrice - currentPrice;
        const percentChange = (diff / currentPrice) * 100;

        if (percentChange > 1) {
            return (
                <div className="flex items-center text-green-600">
                    <ArrowUpRight className="mr-1" />
                    <span>Up {percentChange.toFixed(2)}%</span>
                </div>
            );
        } else if (percentChange < -1) {
            return (
                <div className="flex items-center text-red-600">
                    <ArrowDownRight className="mr-1" />
                    <span>Down {Math.abs(percentChange).toFixed(2)}%</span>
                </div>
            );
        } else {
            return (
                <div className="flex items-center text-gray-500">
                    <Minus className="mr-1" />
                    <span>Stable</span>
                </div>
            );
        }
    };

    return (
        <div className="p-4 rounded-xl shadow-md bg-white dark:bg-gray-800">
            <h2 className="text-xl font-semibold mb-2">Stock Forecast: {ticker}</h2>
            {error && <p className="text-red-500">{error}</p>}
            {currentPrice !== null && predictedPrice !== null ? (
                <>
                    <p className="text-md mb-2">
                        Next Quarter's Predicted Price: <strong>${predictedPrice.toFixed(2)}</strong>
                    </p>
                    {getIndicator()}
                </>
            ) : (
                <div>
                    <Switch/>
                </div>
            )}
        </div>
    );
};

export default StockPrediction;
