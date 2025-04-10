"use client";
import { useState } from "react";
import Link from "next/link";
import Image from "next/image";
import Footer from "../components/Footer"

export default function Dashboard() {
    const [searchOpen, setSearchOpen] = useState(false);

    
    return (
        <div className="min-h-screen flex flex-col items-center p-6 bg-white text-black">
            {/* 🔹 Expanding Search Bar */}
            {searchOpen && (
                <div className="fixed top-16 w-full flex justify-center">
                    <input
                        type="text"
                        placeholder="Search stocks or sectors..."
                        className="w-1/2 p-2 border border-gray-400 rounded-lg shadow-md text-lg"
                    />
                </div>
            )}

            {/* 🔹 Dashboard Sections (Keep Layout but Change Background) */}
            <div className="w-full max-w-6xl mt-20 p-6 space-y-6">
                <div className="bg-gray-100 p-4 shadow-md rounded-lg">
                    <h2 className="text-2xl font-semibold mb-2">📈 Market Overview</h2>
                    <p>Live market indices, sector performance, and stock highlights.</p>
                </div>

                <div className="bg-gray-100 p-4 shadow-md rounded-lg">
                    <h2 className="text-2xl font-semibold mb-2">🔥 Trending Stocks</h2>
                    <p>Most searched and most traded stocks today.</p>
                </div>

                <div className="bg-gray-100 p-4 shadow-md rounded-lg flex flex-col items-center">
                    <h2 className="text-2xl font-semibold mb-2">🌡️ Market Heatmap</h2>
                    <Image src="/heatmap.png" alt="Market Heatmap" width={600} height={400} className="rounded-lg border border-gray-300 shadow-md" />
                </div>

                <div className="bg-gray-100 p-4 shadow-md rounded-lg">
                    <h2 className="text-2xl font-semibold mb-2">📰 Latest News</h2>
                    <p>Financial news and updates.</p>
                </div>

                <div className="bg-gray-100 p-4 shadow-md rounded-lg">
                    <h2 className="text-2xl font-semibold mb-2">⭐ Watchlist</h2>
                    <p>Your saved stocks and market updates.</p>
                </div>
            </div>
            <Footer/>
        </div>
    );
}
