"use client";

import Link from "next/link";

export default function SidebarMenu() {
    
    return (
        <aside className="fixed top-0 left-0 h-full w-72 bg-gray-800 text-white p-8 flex flex-col shadow-lg">
            <Link href="/" className="mb-8">
                <h2 className="text-3xl font-extrabold uppercase tracking-wide cursor-pointer hover:text-accent transition-colors">
                    SwampLocks
                </h2>
            </Link>

            <nav className="flex flex-col justify-between space-y-6 text-lg font-medium ">
                <Link href="/dashboard#market-overview" className="hover:underline hover:text-accent">Market Overview</Link>
                <Link href="/dashboard#trending" className="hover:underline hover:text-accent">Trending Stocks</Link>
                <Link href="/dashboard#news" className="hover:underline hover:text-accent">News</Link>
                <Link href="/dashboard#watchlist" className="hover:underline hover:text-accent">Watchlist</Link>
                <Link href="/commodities" className="hover:underline hover:text-accent">Commodities</Link>
                <Link href="/economic_indicators_dashboard" className="hover:underline hover:text-accent">Economic Indicators</Link>
                <Link href="/ex_rates" className="hover:underline hover:text-accent">Ex Rates</Link>
            </nav>
        </aside>
    );
}
