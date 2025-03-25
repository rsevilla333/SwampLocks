"use client";
import Link from "next/link";
import StockSearch from "./components/StockSearch";

export default function Home() {
    const sectors = [
        { name: "Technology", path: "technology" },
        { name: "Energy", path: "energy" },
        { name: "Healthcare", path: "healthcare" },
        { name: "Financials", path: "financials" },
        { name: "Consumer Discretionary", path: "consumer-discretionary" },
        { name: "Consumer Staples", path: "consumer-staples" },
        { name: "Industrials", path: "industrials" },
        { name: "Materials", path: "materials" },
        { name: "Real Estate", path: "real-estate" },
        { name: "Utilities", path: "utilities" },
        { name: "Communication Services", path: "communication-services" }
    ];

    return (
        <div className="w-full flex justify-between h-full flex-col items-center p-6 ">
            {/* Header */}
            <header className="w-full flex flex-col items-center ">
                <p className="text-center text-lg text-gray-500 max-w-4xl mb-8 font-medium">
                    Your personalized portfolio optimization tool powered by machine learning, sentiment analysis, and sector performance insights. Strategically allocate investments based on data-driven recommendations.
                </p>
            </header>

            {/* Main Content */}
            <div className="flex flex-row w-full max-w-7xl gap-6  ">
                {/* Sectors Sidebar */}
                <div className="w-1/3 p-8 bg-white border-2 border-accent rounded-lg shadow-lg">
                    <h2 className="text-3xl font-extrabold mb-10 text-black tracking-wide">Sectors</h2>
                    <ol className="list-decimal pl-8 space-y-6 text-2xl font-medium">
                        {sectors.map((sector, index) => (
                            <li key={index} className="hover:bg-secondary hover:scale-110 px-4 py-3 rounded-lg transition-all duration-200">
                                <Link href={`/sector/${sector.path}`} className="text-black font-semibold">
                                    {sector.name}
                                </Link>
                            </li>
                        ))}
                    </ol>
                </div>


                {/* Heatmap and Stock Search */}
                <div className="flex-1 p-6 flex flex-col w-full items-center justify-center bg-white border-2 border-accent">
                    <p className="text-xl font-semibold text-black mb-6">Today's Market</p>
                    <img
                        src="heatmap.png"
                        alt="Stock Market Heatmap"
                        className="rounded-lg border border-gray-300 shadow-lg mb-6"
                        width={900}
                        height={600}
                    />
                    {/*<div className="w-full max-w-xl">*/}
                        <StockSearch ticker="" />
                    {/*</div>*/}
                </div>
            </div>

            {/* Footer */}
            <footer className="mt-12 text-center text-secondary">
                <p>Powered by Rafael, Andres, Deep, Chandler, and Mathew</p>
                <p className="mt-2 text-sm">
                    © {new Date().getFullYear()} SwampLocks. All rights reserved.
                </p>
            </footer>
        </div>
    );
}
