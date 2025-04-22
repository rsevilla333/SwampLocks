"use client";

import { useState, useEffect } from "react";
import { Search } from "lucide-react";
import { useRouter } from "next/navigation";
import axios from "axios";
import {COMPILER_INDEXES} from "next/constants";
import CompactStockChart from "./CompactStockChart";

interface SearchBarProps {
    onSearch: (query: string) => void;
}

export default function StockSearchBar() {
    const [query, setQuery] = useState("");
    const router = useRouter();
    const [suggestions, setSuggestions] = useState<string[]>([]);
    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

    const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setQuery(event.target.value);
    };

    const handleSuggestionClick = (ticker: string) => {
        setQuery(ticker); 
        router.push(`/stock/${ticker}`); 
    };

    useEffect(() => {
        if (query.length > 0) {
            const fetchSuggestions = async () => {
                try {
                    const response = await axios.get(`${API_BASE_URL}/api/financials/stocks/autocomplete?query=${query}`);
                    console.log("autocomplete", response);
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

    const handleSubmit = async (event: React.FormEvent) => {
        event.preventDefault();
        const ticker = query.trim().toUpperCase();
        if (!ticker) return;

        try {
            const response = await fetch(`${API_BASE_URL}/api/financials/stocks/${ticker}/exists`, {next: { revalidate: 3600 }});
            if (response.status === 200) {
                router.push(`/stock/${ticker}`);
            }
        } catch (error) {
            alert("Stock not found. Please try a different symbol.");
        }
    };

    return (
        <div className="relative w-full">
        <div className=" w-full bg-gray-300 rounded-md flex items-center p-2 hover:bg-gray-400 active:bg-gray-400 cursor-pointer">
            <Search className="text-gray-600" size={20} />
            <form onSubmit={handleSubmit} className="flex w-full">
                <input
                    type="text"
                    placeholder="Enter stock ticker..."
                    value={query}
                    onChange={handleInputChange}
                    className="w-full bg-transparent text-white px-2 outline-none focus:ring-0 focus:outline-none focus:placeholder-white hover:placeholder-white"
                />
            </form>
        </div>
            {suggestions.length > 0 && (
                <div className="absolute bg-background text-black border rounded-md mt-1 w-full max-h-60 overflow-y-auto z-10 ">
                    {suggestions.map((suggestion) => (
                        <div
                            key={suggestion}
                            className="px-4 py-2 cursor-pointer hover:bg-gray-200 flex flex-row max-h-full gap-x-5"
                            onClick={() => handleSuggestionClick(suggestion)}
                        >
                            {suggestion}
                            <div className="w-3/4 h-15">
                                <CompactStockChart ticker={suggestion} />
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}