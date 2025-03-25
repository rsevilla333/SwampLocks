"use client";
import { useState } from "react";
import Link from "next/link";
import Image from "next/image";

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
    { symbol: "UBER", marketCap: 50000, change: 1.0 },
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
  
  // Utility function to compute adjusted font size so that text fits the square.
  // We assume an average character width of ~0.6 * fontSize in pixels,
  // and reserve half of the square's height for one line of text.
  const getAdjustedFontSize = (squareWidth: number, squareHeight: number, text: string) => {
    const baseSize = Math.max(8, Math.min(squareWidth, squareHeight) / 4);
    const maxByWidth = squareWidth / (text.length * 0.6);
    const maxByHeight = squareHeight / 2;
    return Math.min(baseSize, maxByWidth, maxByHeight);
  };

export default function Dashboard() {
    const [searchOpen, setSearchOpen] = useState(false);

      // Fixed container dimensions for the heatmap
      const containerWidth = 600;
      const containerHeight = 400;
  
      // Recursive slice-and-dice treemap algorithm
      function computeTreemap(
          items: typeof stocks,
          x: number,
          y: number,
          width: number,
          height: number,
          vertical: boolean
      ) {
          if (items.length === 0) return [];
          if (items.length === 1) {
          return [
              {
              ...items[0],
              x,
              y,
              width,
              height,
              },
          ];
          }
          const totalCap = items.reduce((sum, item) => sum + item.marketCap, 0);
          let sum = 0,
          splitIndex = 0;
          for (let i = 0; i < items.length; i++) {
          sum += items[i].marketCap;
          if (sum >= totalCap / 2) {
              splitIndex = i;
              break;
          }
          }
          const group1 = items.slice(0, splitIndex + 1);
          const group2 = items.slice(splitIndex + 1);
          const group1Cap = group1.reduce((sum, item) => sum + item.marketCap, 0);
          const group2Cap = group2.reduce((sum, item) => sum + item.marketCap, 0);
  
          let layout = [];
          if (vertical) {
          const width1 = width * (group1Cap / totalCap);
          const layout1 = computeTreemap(group1, x, y, width1, height, !vertical);
          const layout2 = computeTreemap(group2, x + width1, y, width - width1, height, !vertical);
          layout = [...layout1, ...layout2];
          } else {
          const height1 = height * (group1Cap / totalCap);
          const layout1 = computeTreemap(group1, x, y, width, height1, !vertical);
          const layout2 = computeTreemap(group2, x, y + height1, width, height - height1, !vertical);
          layout = [...layout1, ...layout2];
          }
          return layout;
      }
  
      // Sort stocks in descending order (largest first)
      const sortedStocks = [...stocks].sort((a, b) => b.marketCap - a.marketCap);
      // Compute treemap layout
      const layout = computeTreemap(
          sortedStocks,
          0,
          0,
          containerWidth,
          containerHeight,
          containerWidth >= containerHeight
      );

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
                    <div
                        className="relative bg-gray-700 rounded-lg overflow-hidden"
                        style={{ width: `${containerWidth}px`, height: `${containerHeight}px` }}
                    >
                        {layout.map((item, index) => {
                        // Compute adjusted font sizes for the symbol and percentage.
                        const symbolFontSize = getAdjustedFontSize(item.width, item.height, item.symbol);
                        const percentText = `${item.change}%`;
                        const percentFontSize = getAdjustedFontSize(item.width, item.height, percentText);
                        return (
                            <div
                            key={index}
                            className="absolute flex flex-col items-center justify-center text-black font-bold border border-gray-600 p-1"
                            style={{
                                width: `${item.width}px`,
                                height: `${item.height}px`,
                                left: `${item.x}px`,
                                top: `${item.y}px`,
                                backgroundColor: item.change >= 0 ? "#13d62a" : "#ef4444",
                            }}
                            >
                            <span
                                className="w-full text-center"
                                style={{ fontSize: `${symbolFontSize}px`, lineHeight: 1.2 }}
                            >
                                {item.symbol}
                            </span>
                            <span
                                className="w-full text-center"
                                style={{ fontSize: `${percentFontSize}px`, lineHeight: 1.2 }}
                            >
                                {percentText}
                            </span>
                            </div>
                        );
                        })}
                    </div>
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
        </div>
    );
}
