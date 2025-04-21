"use client";
import React, { useState } from "react";

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
  const [rows] = useState([
    { sector: "Tech", performance: 0.15 },
    { sector: "Healthcare", performance: 0.10 },
    { sector: "Energy", performance: 0.05 },
    { sector: "Utilities", performance: 0.03 },
    { sector: "Financials", performance: 0.09 },
    { sector: "Consumer Discretionary", performance: 0.12 },
    { sector: "Industrials", performance: 0.06 },
    { sector: "Materials", performance: 0.04 },
    { sector: "Real Estate", performance: 0.02 },
    { sector: "Communication", performance: 0.13 },
    { sector: "Automotive", performance: 0.07 },
  ]);

  const [alpha, setAlpha] = useState(0.5);

  // Compute and then sort by weight descending
  const optimized = optimizePortfolio(rows, alpha)
    .slice()
    .sort((a, b) => b.weight - a.weight);

  return (
    <div className="min-h-screen flex items-center justify-center p-8 text-black">
      <div className="bg-white shadow-2xl rounded-xl p-8 w-full max-w-4xl">
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
