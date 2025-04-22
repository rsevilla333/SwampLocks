"use client";
import React, {useEffect, useState} from "react";

export function optimizePortfolio(
  items: { sector: string; performance: number }[],
  alpha: number
): Array<{ sector: string; performance: number; weight: number }> {
  const n = items.length;
  if (n === 0) return [];

  if (alpha === 1) {
    let maxIdx = 0;
    let maxPerf = items[0].performance;
    for (let i = 1; i < n; i++) {
      if (items[i].performance > maxPerf) {
        maxPerf = items[i].performance;
        maxIdx = i;
      }
    }
    return items.map((x, i) => ({
      sector: x.sector,
      performance: x.performance,
      weight: i === maxIdx ? 1 : 0,
    }));
  }

  const r = items.map((x) => x.performance);
  let weights = new Array(n).fill(0);
  let active = Array.from({ length: n }, (_, i) => i);

  while (true) {
    const freeCount = active.length;
    if (freeCount === 0) break;

    const sumR = active.reduce((acc, i) => acc + r[i], 0);
    const lambda = (alpha * sumR - 2 * (1 - alpha)) / freeCount;

    let anyNegative = false;
    for (const i of active) {
      const w_i = (alpha * r[i] - lambda) / (2 * (1 - alpha));
      weights[i] = w_i;
      if (w_i < 0) anyNegative = true;
    }

    if (!anyNegative) {
      let sumW = active.reduce((acc, i) => acc + weights[i], 0);
      if (sumW <= 0) {
        active.forEach(i => (weights[i] = 1 / freeCount));
      } else {
        active.forEach(i => (weights[i] /= sumW));
      }
      break;
    }

    const newActive: number[] = [];
    for (const i of active) {
      if (weights[i] > 0) newActive.push(i);
      else weights[i] = 0;
    }
    if (newActive.length === active.length) break;
    active = newActive;
  }

  return items.map((x, i) => ({
    sector: x.sector,
    performance: x.performance,
    weight: weights[i],
  }));
}

export default function DiversityOptimizer() {
  const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

  const sectors = [
    { name: "Information Technology"},
    { name: "Energy"},
    { name: "Healthcare" },
    { name: "Financials" },
    { name: "Consumer Discretionary" },
    { name: "Consumer Staples" },
    { name: "Industrials" },
    { name: "Materials" },
    { name: "Real Estate" },
    { name: "Utilities" },
    { name: "Communication Services" }
  ];

  const [rows, setRows] = useState<{ sector: string; performance: number }[]>([]);
  const [alpha, setAlpha] = useState(0.5);

  useEffect(() => {
    const fetchSectorScores = async () => {
      try {
        const results: { sector: string; performance: number }[] = await Promise.all(
            sectors.map(async (sector) => {
              try {
                const res = await fetch(`${API_BASE_URL}/api/financials/sector-growth?sectorName=${encodeURIComponent(sector.name)}`);
                const score = await res.json();
                console.log(score);
                return { sector: sector.name, performance: score * 0.01};
              } catch (err) {
                console.error(`Failed to fetch score for ${sector.name}`, err);
                return { sector: sector.name, performance: 0 };
              }
            })
        );

        results.sort((a, b) => b.performance - a.performance);
        setRows(results);
      } catch (err) {
        console.error("Error fetching sector scores:", err);
      }
    };

    fetchSectorScores();
  }, []);

  // Compute and then sort by weight descending
  const optimized = optimizePortfolio(rows, alpha)
    .slice()
    .sort((a, b) => b.weight - a.weight);

  return (
    <div className="  min-h-screen flex items-center justify-center p-8 text-black">
      <div className=" shadow-2xl rounded-xl p-8 w-full max-w-4xl">
        <h1 className="text-3xl font-extrabold mb-6 text-center">
          Portfolio Optimizer
        </h1>
        <p className="mb-4 text-center text-lg">
          Optimize your portfolio by balancing performance against diversification.
          Adjust the slider to change the trade-off parameter <strong>α</strong>.
        </p>

        {/* Slider for alpha */}
        <div className="mb-6">
          <label htmlFor="alphaRange" className="block font-semibold mb-2">
            Alpha = {alpha.toFixed(2)}
          </label>
          <input
            id="alphaRange"
            type="range"
            min="0"
            max="1"
            step="0.01"
            value={alpha}
            onChange={(e) => setAlpha(Number(e.target.value))}
            className="w-full accent-blue-500"
          />
          <p className="text-sm text-gray-600 mt-1">
            <strong>α=1</strong>: Focus solely on performance. <br />
            <strong>α=0</strong>: Focus solely on diversification.
          </p>
        </div>

        {/* Results Table */}
        <table className="min-w-full border-collapse border border-gray-300">
          <thead>
            <tr className="bg-gray-200">
              <th className="border p-2 text-left">Sector</th>
              <th className="border p-2 text-left">Weight %</th>
            </tr>
          </thead>
          <tbody>
            {optimized.map((row, i) => (
              <tr key={i} className="hover:bg-gray-100 transition-colors">
                <td className="border p-2">{row.sector}</td>
                <td className="border p-2">
                  {(row.weight * 100).toFixed(2)}%
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
