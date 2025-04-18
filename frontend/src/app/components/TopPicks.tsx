import React from 'react';


interface Stock {
  symbol: string;
  mse: number;
  R: number;
}

// Hard‑coded data requires the mse and return of next tp
const stocks: Stock[] = [
  { symbol: 'AAPL', mse: 0.02, R: 0.18 },
  { symbol: 'MSFT', mse: 0.03, R: 0.15 },
  { symbol: 'GOOGL', mse: 0.015, R: 0.20 },
  { symbol: 'AMZN', mse: 0.025, R: 0.22 },
  { symbol: 'TSLA', mse: 0.05, R: 0.30 },
  { symbol: 'FB', mse: 0.04, R: 0.17 },
  { symbol: 'NVDA', mse: 0.06, R: 0.28 },
  { symbol: 'JPM', mse: 0.07, R: 0.10 },
  { symbol: 'V', mse: 0.03, R: 0.12 },
  { symbol: 'JNJ', mse: 0.05, R: 0.08 },
  { symbol: 'WMT', mse: 0.06, R: 0.09 },
  { symbol: 'PG', mse: 0.04, R: 0.07 },
  { symbol: 'MA', mse: 0.025, R: 0.13 },
  { symbol: 'DIS', mse: 0.03, R: 0.11 },
  { symbol: 'HD', mse: 0.05, R: 0.09 },
  { symbol: 'BAC', mse: 0.07, R: 0.08 },
  { symbol: 'NFLX', mse: 0.045, R: 0.25 },
  { symbol: 'XOM', mse: 0.08, R: 0.06 },
  { symbol: 'PFE', mse: 0.09, R: 0.05 },
  { symbol: 'VZ', mse: 0.1, R: 0.04 },
  { symbol: 'KO', mse: 0.03, R: 0.06 },
  { symbol: 'MRK', mse: 0.04, R: 0.07 },
  { symbol: 'NKE', mse: 0.05, R: 0.08 },
  { symbol: 'ORCL', mse: 0.06, R: 0.09 },
  { symbol: 'T', mse: 0.08, R: 0.05 },
  { symbol: 'INTC', mse: 0.07, R: 0.06 },
  { symbol: 'CSCO', mse: 0.065, R: 0.07 },
  { symbol: 'CVX', mse: 0.09, R: 0.05 },
  { symbol: 'ABT', mse: 0.045, R: 0.08 },
  { symbol: 'CRM', mse: 0.055, R: 0.14 },
];

// Convert MSE into an accuracy‑like metric via inverse transform
const accuracy = (mse: number): number => 1 / (1 + mse);

// Compute score using Kelly edge formula: acc*(1+R) - 1
const score = (mse: number, R: number): number => accuracy(mse) * (1 + R) - 1;

const TopPicks: React.FC = () => {
  const ranked = stocks
    .map(s => ({
      ...s,
      accuracy: accuracy(s.mse),
      score: score(s.mse, s.R)
    }))
    .sort((a, b) => b.score - a.score)
    .slice(0, 10);

  return (
    <div className="p-6 bg-white rounded-2xl shadow-lg max-w-4xl mx-auto text-black">
      <h2 className="text-2xl text-center font-bold mb-4">Top Locks 🔒</h2>
      <div className="overflow-x-auto">
        <table className="min-w-full bg-white border border-gray-200">
          <thead>
            <tr className="bg-gray-100">
              <th className="px-6 py-3 border-b text-left text-sm font-semibold">Rank</th>
              <th className="px-6 py-3 border-b text-left text-sm font-semibold">Symbol</th>
              <th className="px-6 py-3 border-b text-left text-sm font-semibold">MSE</th>
              <th className="px-6 py-3 border-b text-left text-sm font-semibold">Return</th>
              <th className="px-6 py-3 border-b text-left text-sm font-semibold">Accuracy</th>
              <th className="px-6 py-3 border-b text-left text-sm font-semibold">Score</th>
            </tr>
          </thead>
          <tbody>
            {ranked.map((s, i) => (
              <tr
                key={s.symbol}
                className={`transition-colors hover:bg-gray-50 ${i % 2 === 0 ? '' : 'bg-gray-50'}`}
              >
                <td className="px-6 py-4 border-b text-sm">{i + 1}</td>
                <td className="px-6 py-4 border-b text-sm">{s.symbol}</td>
                <td className="px-6 py-4 border-b text-sm">{s.mse.toFixed(3)}</td>
                <td className="px-6 py-4 border-b text-sm">{(s.R * 100).toFixed(1)}%</td>
                <td className="px-6 py-4 border-b text-sm">{s.accuracy.toFixed(3)}</td>
                <td className="px-6 py-4 border-b text-sm">{s.score.toFixed(3)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default TopPicks;
