"use client";
import Link from "next/link";

export default function Home() {
    const sectors = [
        { name: "Technology", path: "technology" },
        { name: "Energy", path: "energy" },
        { name: "Healthcare", path: "healthcare" },
        { name: "Finance", path: "finance" },
        { name: "Consumer Discretionary", path: "consumer-discretionary" },
        { name: "Consumer Staples", path: "consumer-staples" },
        { name: "Industrials", path: "industrials" },
        { name: "Materials", path: "materials" },
        { name: "Real Estate", path: "real-estate" },
        { name: "Utilities", path: "utilities" },
        { name: "Communication Services", path: "communication-services" }
    ];

    return (
        <div className="min-h-screen flex flex-col items-center p-6">
            <h1 className="text-6xl font-extrabold mb-8 text-blue-600">SwampLocks</h1>

            <div className="flex w-full max-w-6xl">
                <div className="w-1/3 p-4 border-r border-gray-300">
                    <h2 className="text-2xl font-bold mb-4">Top Sectors Today:</h2>
                    <ol className="list-decimal pl-6 space-y-2 text-lg">
                        {sectors.map((sector, index) => (
                            <li key={index}>
                                <Link href={`/sector/${sector.path}`} className="text-blue-600 hover:underline font-semibold">
                                    {sector.name}
                                </Link>
                            </li>
                        ))}
                    </ol>
                </div>

                <div className="flex-1 flex flex-col items-center justify-center p-8">
                    <p className="text-lg font-semibold mb-4">Stock Market Heatmap</p>
                    <img
                        src="/heatmap.png"
                        alt="Stock Market Heatmap"
                        width={800}
                        height={500}
                        className="rounded-lg border border-gray-300 shadow-md"
                    />
                </div>
            </div>
        </div>
    );
}
