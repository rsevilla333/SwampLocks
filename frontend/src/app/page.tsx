"use client";

import Link from "next/link";
import Treemap from "./components/TreeMap";
import Footer from "./components/Footer";
import { useRouter } from "next/navigation";
import StockSearchBar from "./components/StockSearchBar";
import Articles from "./components/Articles";
import TopMoverDashBoard from "./components/TopMoversDashboard";
import CompactStockChart from "./components/CompactStockChart";
import { Switch } from "@headlessui/react";
import MountainMap from "./components/MountainMap";
import {useState} from "react";


export default function Home() {

    const [selectedMap, setSelectedMap] = useState<'mountain' | 'treemap'>('mountain');
    
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

    const stocks = [
        { symbol: "AAPL", marketCap: 250000, change: 1.5 },
        { symbol: "GOOGL", marketCap: 180000, change: -0.7 },
        { symbol: "AMZN", marketCap: 200000, change: 2.1 },
        { symbol: "TSLA", marketCap: 150000, change: -1.2 },
        { symbol: "MSFT", marketCap: 220000, change: 0.9 },
        { symbol: "FB", marketCap: 130000, change: -0.4 },
        { symbol: "NFLX", marketCap: 120000, change: 1.3 },
        { symbol: "NVDA", marketCap: 140000, change: 2.5 },
        { symbol: "ADBE", marketCap: 110000, change: -0.8 },
        { symbol: "ORCL", marketCap: 100000, change: 0.7 },
        { symbol: "INTC", marketCap: 90000, change: -1.0 },
        { symbol: "CSCO", marketCap: 80000, change: 0.4 },
        { symbol: "IBM", marketCap: 70000, change: -0.5 },
        { symbol: "SAP", marketCap: 60000, change: 0.8 },
        { symbol: "CRM", marketCap: 65000, change: 1.2 },
        { symbol: "TWTR", marketCap: 55000, change: -0.9 },
        { symbol: "UBER", marketCap: 50000, change: 4.0 },
        { symbol: "LYFT", marketCap: 45000, change: -1.1 },
        { symbol: "SNAP", marketCap: 40000, change: 2.0 },
        { symbol: "SHOP", marketCap: 48000, change: 0.6 },
        { symbol: "SQ", marketCap: 52000, change: 1.8 },
        { symbol: "BIDU", marketCap: 43000, change: -0.6 },
        { symbol: "JD", marketCap: 47000, change: 1.1 },
        { symbol: "PDD", marketCap: 39000, change: 0.5 },
        { symbol: "ZM", marketCap: 36000, change: -0.7 },
        { symbol: "DOCU", marketCap: 34000, change: 1.4 },
        { symbol: "WDAY", marketCap: 32000, change: -1.3 },

        // Second set (slightly modified values)
        { symbol: "AAPL2", marketCap: 245000, change: 1.6 },
        { symbol: "GOOGL2", marketCap: 175000, change: -0.8 },
        { symbol: "AMZN2", marketCap: 195000, change: 2.0 },
        { symbol: "TSLA2", marketCap: 155000, change: -1.3 },
        { symbol: "MSFT2", marketCap: 215000, change: 1.0 },
        { symbol: "FB2", marketCap: 135000, change: -0.3 },
        { symbol: "NFLX2", marketCap: 125000, change: 1.4 },
        { symbol: "NVDA2", marketCap: 138000, change: 2.4 },
        { symbol: "ADBE2", marketCap: 112000, change: -0.7 },
        { symbol: "ORCL2", marketCap: 102000, change: 0.8 },
        { symbol: "INTC2", marketCap: 88000, change: -1.1 },
        { symbol: "CSCO2", marketCap: 82000, change: 0.5 },
        { symbol: "IBM2", marketCap: 68000, change: -0.4 },
        { symbol: "SAP2", marketCap: 62000, change: 0.9 },
        { symbol: "CRM2", marketCap: 63000, change: 1.3 },
        { symbol: "TWTR2", marketCap: 54000, change: -1.0 },
        { symbol: "UBER2", marketCap: 51000, change: 1.1 },
        { symbol: "LYFT2", marketCap: 46000, change: -1.0 },
        { symbol: "SNAP2", marketCap: 41000, change: 2.1 },
        { symbol: "SHOP2", marketCap: 48500, change: 0.7 },
        { symbol: "SQ2", marketCap: 52500, change: 1.7 },
        { symbol: "BIDU2", marketCap: 43500, change: -0.5 },
        { symbol: "JD2", marketCap: 46500, change: 1.2 },
        { symbol: "PDD2", marketCap: 39500, change: 0.6 },
        { symbol: "ZM2", marketCap: 36500, change: -0.8 },
        { symbol: "DOCU2", marketCap: 34500, change: 1.5 },
        { symbol: "WDAY2", marketCap: 31500, change: -1.4 },

        // Third set (again with slight modifications)
        { symbol: "AAPL3", marketCap: 255000, change: 1.4 },
        { symbol: "GOOGL3", marketCap: 182000, change: -0.6 },
        { symbol: "AMZN3", marketCap: 205000, change: 2.2 },
        { symbol: "TSLA3", marketCap: 148000, change: -1.0 },
        { symbol: "MSFT3", marketCap: 225000, change: 0.8 },
        { symbol: "FB3", marketCap: 132000, change: -0.5 },
        { symbol: "NFLX3", marketCap: 118000, change: 1.2 },
        { symbol: "NVDA3", marketCap: 142000, change: 2.6 },
        { symbol: "ADBE3", marketCap: 108000, change: -0.9 },
        { symbol: "ORCL3", marketCap: 99000, change: 0.9 },
        { symbol: "INTC3", marketCap: 91000, change: -1.2 },
        { symbol: "CSCO3", marketCap: 79000, change: 0.3 },
        { symbol: "IBM3", marketCap: 71000, change: -0.6 },
        { symbol: "SAP3", marketCap: 61000, change: 0.7 },
        { symbol: "CRM3", marketCap: 66000, change: 1.1 },
        { symbol: "TWTR3", marketCap: 56000, change: -1.2 },
        { symbol: "UBER3", marketCap: 52000, change: 0.9 },
        { symbol: "LYFT3", marketCap: 44000, change: -1.3 },
        { symbol: "SNAP3", marketCap: 42000, change: 2.2 },
        { symbol: "SHOP3", marketCap: 49000, change: 0.5 },
        { symbol: "SQ3", marketCap: 53000, change: 1.9 },
        { symbol: "BIDU3", marketCap: 42500, change: -0.7 },
        { symbol: "JD3", marketCap: 47500, change: 1.0 },
        { symbol: "PDD3", marketCap: 38500, change: 0.4 },
        { symbol: "ZM3", marketCap: 35500, change: -0.9 },
        { symbol: "DOCU3", marketCap: 33500, change: 1.6 },
        { symbol: "WDAY3", marketCap: 32500, change: -1.5 },
    ];
    
        return (
            <div className="w-full h-full flex flex-col items-center gap-y-10">
                {/* Stock Search Bar */}
                <div className="p-6 w-full mx-auto">
                    <StockSearchBar />
                </div>

                {/* Header Text */}
                <header className="w-full flex flex-col items-center">
                    <p className="text-center text-lg text-gray-500 max-w-4xl mb-8 font-medium">
                        Your personalized portfolio optimization tool powered by machine learning, sentiment analysis, and sector performance insights. Strategically allocate investments based on data-driven recommendations.
                    </p>
                </header>

                {/* Mountain Map */}
                {/*<div className="mx-auto p-4 flex flex-col items-center">*/}
                {/*    <h1 className="text-black text-4xl">The Market</h1>*/}
                {/*    <MountainMap stocks={stocks} />*/}
                {/*    <Treemap stocks={stocks} />*/}
                {/*</div>*/}
                <div className="mb-4 flex flex-row items-center gap-2">
                    <h1 className="text-black text-4xl">The Market</h1>
                    <Switch
                        checked={selectedMap === 'treemap'}
                        onChange={() => setSelectedMap(selectedMap === 'mountain' ? 'treemap' : 'mountain')}
                        className={`${
                            selectedMap === 'treemap' ? 'bg-green-500' : 'bg-gray-300'
                        } relative inline-flex items-center h-6 rounded-full w-12 transition-colors duration-200 ease-in-out`}
                    >
                    <span
                        className={`${
                            selectedMap === 'treemap' ? 'translate-x-6' : 'translate-x-1'
                        } inline-block w-4 h-4 transform bg-white rounded-full transition-transform duration-200 ease-in-out`}
                    />
                    </Switch>
                </div>


                {/* Conditionally render the selected map */}
                {selectedMap === 'mountain' ? (
                    <MountainMap stocks={stocks} />
                ) : (
                    <Treemap stocks={stocks} />
                )}

                
                {/* Main Content Grid */}
                <div className="flex w-full max-w-7xl gap-8 max-h-full">
                    {/* Left Sidebar */}
                    <div className="w-3/4 flex flex-col gap-8">
                        {/* Sectors Section */}
                        <div className="bg-transparent border-2 border-accent p-8">
                            <h2 className="text-3xl font-extrabold mb-6 text-black">Sectors</h2>
                            <ol className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4 w-full list-decimal pl-8 text-2xl font-medium">
                                {sectors.map((sector, index) => (
                                    <li key={index} className="hover:bg-secondary hover:scale-110 px-4 py-3 rounded-lg transition-all duration-200">
                                        <Link href={`/sector/${sector.path}`} className="text-black font-semibold">
                                            {sector.name}
                                        </Link>
                                    </li>
                                ))}
                            </ol>
                        </div>
                        {/* Top Movers */}
                        <div className="min-w-full max-h-[550px] bg-transparent">
                            <h2 className="text-black text-4xl items-center">Top Movers</h2>
                            <TopMoverDashBoard />
                        </div>
                    </div>

                    {/* Right Sidebar for Articles */}
                    <div className="w-1/4 bg-transparent h-full">
                        <Articles /> {/* Your Articles component */}
                    </div>
                </div>
                <Footer></Footer>
            </div>
    );
    }
