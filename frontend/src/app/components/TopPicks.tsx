import React from 'react';

interface Stock {
  symbol: string;
  mse: number;
  R: number;
}

// Hard‑coded data: MSE and next‑period return
const stocks: Stock[] = [
  { symbol: 'AAPL', mse: 200, R: 0.18 },
  { symbol: 'MSFT', mse: 300, R: 0.15 },
  { symbol: 'GOOGL', mse: 150, R: 0.20 },
  { symbol: 'AMZN', mse: 250, R: 0.22 },
  { symbol: 'TSLA', mse: 500, R: 0.30 },
  { symbol: 'FB', mse: 400, R: 0.17 },
  { symbol: 'NVDA', mse: 600, R: 0.28 },
  { symbol: 'JPM', mse: 700, R: 0.10 },
  { symbol: 'V', mse: 300, R: 0.12 },
  { symbol: 'JNJ', mse: 5000, R: 0.08 },
  { symbol: 'WMT', mse: 600, R: 0.09 },
  { symbol: 'PG', mse: 400, R: 0.07 },
  { symbol: 'MA', mse: 2500, R: 0.13 },
  { symbol: 'DIS', mse: 300, R: 0.11 },
  { symbol: 'HD', mse: 500, R: 0.09 },
  { symbol: 'BAC', mse: 70, R: 0.08 },
];

// Convert MSE into RMSE
const rmse = (mse: number): number => Math.sqrt(mse);

// Convert RMSE into an accuracy‑like metric
const accuracy = (mse: number): number => 1 / (1 + rmse(mse));

// Compute score: net expected return per unit stake
const score = (mse: number, R: number): number =>
  accuracy(mse) * (1 + R) - 1;

const TopPicks: React.FC = () => {
  const ranked = stocks
    .map(s => ({
      ...s,
      rmse: rmse(s.mse),
      accuracy: accuracy(s.mse),
      score: score(s.mse, s.R),
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
              <th className="px-6 py-3 border-b text-left text-sm font-semibold">RMSE</th>
              <th className="px-6 py-3 border-b text-left text-sm font-semibold">Return</th>
              {/* <th className="px-6 py-3 border-b text-left text-sm font-semibold">Accuracy</th> */}
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
                <td className="px-6 py-4 border-b text-sm">{s.rmse.toFixed(3)}</td>
                <td className="px-6 py-4 border-b text-sm">{(s.R * 100).toFixed(1)}%</td>
                {/* <td className="px-6 py-4 border-b text-sm">{s.accuracy.toFixed(3)}</td> */}
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
