// components/SectorAnalysis.tsx
"use client";

import { useState, useEffect } from "react";

type Sector = "Technology" | "Energy" | "Healthcare" | "Financials";

const mockedPerformanceData: Record<Sector, { performance: string }> = {
    Technology: { performance: "Positive growth over the last quarter. Strong performance in cloud and AI stocks." },
    Energy: { performance: "Declining performance due to global energy price fluctuations." },
    Healthcare: { performance: "Stable growth with minor fluctuations. Biotech stocks driving growth." },
    Financials: { performance: "Stable with small growth. Volatility due to interest rates." },
};

const mockedPortfolioRecommendations: Record<Sector, string[]> = {
    Technology: [
        "Buy 40% in technology ETFs",
        "Increase exposure to AI and Cloud stocks",
        "Reduce exposure to semiconductor stocks due to short-term instability"
    ],
    Energy: [
        "Focus on renewable energy stocks",
        "Reduce exposure to traditional oil and gas ETFs"
    ],
    Healthcare: [
        "Invest in biotech ETFs",
        "Hold positions in major pharmaceutical companies"
    ],
    Financials: [
        "Increase exposure to fintech ETFs",
        "Reduce exposure to banks due to potential rate hikes"
    ],
};

type SectorAnalysisProps = {
    sectorName: string;
};

export default function SectorAnalysis({ sectorName }: SectorAnalysisProps) {
    const [sectorData, setSectorData] = useState<{ performance: string } | null>(null);
    const [sectorRecommendations, setSectorRecommendations] = useState<string[] | null>(null);
    const [isClient, setIsClient] = useState(false);

    useEffect(() => {
        // Check for client-side render
        setIsClient(true);

        // Set the sector data based on the passed sectorName prop
        const sector = sectorName.toLowerCase() as Sector;
        setSectorData(mockedPerformanceData[sector]);
        setSectorRecommendations(mockedPortfolioRecommendations[sector]);
    }, [sectorName]);

    // Prevent rendering the component until it is client-side to avoid hydration issues
    if (!isClient) {
        return null;
    }

    return (
        <div className="w-full flex justify-between h-full flex-col items-center">
            <header className="w-full flex flex-col items-center">
                <p className="text-center text-lg text-gray-500 max-w-4xl mb-8 font-medium">
                    Mocked Sector Performance and Portfolio Recommendations
                </p>
            </header>

            {/* Main Content */}
                <div className="w-full p-8 bg-white border-2 border-accent rounded-lg shadow-lg">

                    {/* Display Sector Performance */}
                    <p className="text-xl font-semibold text-black mb-6">Sector Performance</p>
                    <p className="mb-6">{sectorData?.performance}</p>

                    {/* Display Portfolio Recommendations */}
                    <p className="text-xl font-semibold text-black mb-6">Portfolio Recommendations</p>
                    <ul>
                        {sectorRecommendations?.map((rec, index) => (
                            <li key={index}>{rec}</li>
                        ))}
                    </ul>
                </div>

            {/* Footer */}
        </div>
    );
}
