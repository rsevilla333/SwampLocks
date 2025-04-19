"use client";

import React, { useState, useEffect, useMemo } from "react";
import {
  ResponsiveContainer,
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip
} from "recharts";

const ALL_TICKERS = [
  "AAPL", "GOOGL", "MSFT", "TSLA", "AMZN",
  "META", "NVDA", "IBM", "ORCL", "NFLX",
];

const LOCAL_DATA: Record<string, Array<{ date: string; closingPrice: number }>> = {
  AAPL: [
    { date: "2025-03-01", closingPrice: 150 },
    /* … */
    { date: "2025-03-10", closingPrice: 160 },
  ],
  GOOGL: [
    { date: "2025-03-01", closingPrice: 2800 },
    /* … */
    { date: "2025-03-10", closingPrice: 2880 },
  ],
  MSFT: [
    { date: "2025-03-01", closingPrice: 300 },
    /* … */
    { date: "2025-03-10", closingPrice: 310 },
  ],
  // Add other tickers as needed…
};

const initialColors: Record<string, string> = {
  AAPL: "#8884d8",
  GOOGL: "#82ca9d",
  MSFT: "#ff7300",
};

function generateRandomColor(): string {
  const letters = "0123456789ABCDEF";
  let color = "#";
  for (let i = 0; i < 6; i++) {
    color += letters[Math.floor(Math.random() * 16)];
  }
  return color;
}

const timeframes = [
  { label: "Today", value: "1d" },
  { label: "1W", value: "1w" },
  { label: "1M", value: "1mo" },
  { label: "YTD", value: "ytd" },
  { label: "6M", value: "6mo" },
  { label: "1Y", value: "1y" },
  { label: "5Y", value: "5y" },
  { label: "Max", value: "max" }
];

interface MergedRow {
  date: string;
  [ticker: string]: string | number;
}

export default function MultiStockSearchChart() {
  const [searchQuery, setSearchQuery] = useState("");
  const [suggestions, setSuggestions] = useState<string[]>([]);
  const [selectedTickers, setSelectedTickers] = useState<string[]>([]);
  const [tickerColors, setTickerColors] = useState<Record<string, string>>(initialColors);
  const [mergedData, setMergedData] = useState<MergedRow[]>([]);
  const [filteredData, setFilteredData] = useState<MergedRow[]>([]);
  const [timeframe, setTimeframe] = useState("max");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // 1) Autocomplete suggestions
  useEffect(() => {
    if (!searchQuery) {
      setSuggestions([]);
      return;
    }
    const q = searchQuery.toUpperCase();
    setSuggestions(ALL_TICKERS.filter((t) => t.includes(q)));
  }, [searchQuery]);

  // 2) Add ticker
  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    addTicker(searchQuery);
  }
  function addTicker(ticker: string) {
    const up = ticker.toUpperCase().trim();
    if (!up || selectedTickers.includes(up)) return;
    if (!LOCAL_DATA[up]) {
      setError(`No local data for ticker: ${up}`);
      return;
    }
    if (!tickerColors[up]) {
      setTickerColors((p) => ({ ...p, [up]: generateRandomColor() }));
    }
    setSelectedTickers((p) => [...p, up]);
    setSearchQuery("");
    setSuggestions([]);
    setError(null);
  }

  // 3) Remove ticker
  function removeTicker(t: string) {
    setSelectedTickers((p) => p.filter((x) => x !== t));
  }

  // 4) Merge data from LOCAL_DATA
  useEffect(() => {
    if (selectedTickers.length === 0) {
      setMergedData([]);
      return;
    }
    setLoading(true);
    const map: Record<string, any> = {};
    selectedTickers.forEach((tk) => {
      (LOCAL_DATA[tk] || []).forEach((pt) => {
        if (!map[pt.date]) map[pt.date] = { date: pt.date };
        map[pt.date][tk] = pt.closingPrice;
      });
    });
    const arr = Object.values(map).sort((a, b) => a.date.localeCompare(b.date));
    setMergedData(arr as MergedRow[]);
    setLoading(false);
  }, [selectedTickers]);

  // 5) Filter by timeframe
  useEffect(() => {
    if (timeframe === "max") {
      setFilteredData(mergedData);
      return;
    }
    const now = new Date();
    let f = [...mergedData];
    if (timeframe === "1d") {
      const t0 = new Date(); t0.setHours(0,0,0,0);
      f = f.filter((row) => new Date(row.date) >= t0);
    } else if (timeframe === "1w") {
      const w = new Date(now); w.setDate(now.getDate()-7);
      f = f.filter((row) => new Date(row.date) >= w);
    } else if (timeframe === "1mo") {
      const m = new Date(now); m.setMonth(now.getMonth()-1);
      f = f.filter((row) => new Date(row.date) >= m);
    } else if (timeframe === "6mo") {
      const m6 = new Date(now); m6.setMonth(now.getMonth()-6);
      f = f.filter((row) => new Date(row.date) >= m6);
    } else if (timeframe === "1y") {
      const y1 = new Date(now); y1.setFullYear(now.getFullYear()-1);
      f = f.filter((row) => new Date(row.date) >= y1);
    } else if (timeframe === "5y") {
      const y5 = new Date(now); y5.setFullYear(now.getFullYear()-5);
      f = f.filter((row) => new Date(row.date) >= y5);
    } else if (timeframe === "ytd") {
      const y0 = new Date(now.getFullYear(), 0, 1);
      f = f.filter((row) => new Date(row.date) >= y0);
    }
    setFilteredData(f);
  }, [timeframe, mergedData]);

  // 6) Standardize all series to percent-start-at-100
  const standardizedData = useMemo<MergedRow[]>(() => {
    if (!filteredData.length || !selectedTickers.length) return [];
    const base = filteredData[0];
    return filteredData.map((row) => {
      const out: any = { date: row.date };
      selectedTickers.forEach((tk) => {
        out[tk] = base[tk] ? (row[tk] as number) / (base[tk] as number) * 100 : null;
      });
      return out;
    });
  }, [filteredData, selectedTickers]);

  // 7) Compute daily returns
  const returnsData = useMemo(() => {
    if (filteredData.length < 2) return [];
    const R: any[] = [];
    for (let i = 1; i < filteredData.length; i++) {
      const p = filteredData[i-1], c = filteredData[i];
      const row: any = { date: c.date };
      selectedTickers.forEach((tk) => {
        const pv = p[tk] as number, cv = c[tk] as number;
        row[tk] = pv ? (cv - pv)/pv : 0;
      });
      R.push(row);
    }
    return R;
  }, [filteredData, selectedTickers]);

  // 8) Pairwise Pearson
  const pairwiseCorr = useMemo<Record<string, number>>(() => {
    const res: Record<string, number> = {};
    if (returnsData.length < 1 || selectedTickers.length<2) return res;
    const map: Record<string, number[]> = {};
    selectedTickers.forEach((tk) => map[tk]=[]);
    returnsData.forEach((r) => {
      selectedTickers.forEach((tk)=>map[tk].push(r[tk] as number));
    });
    for (let i=0; i<selectedTickers.length; i++) {
      for (let j=i+1; j<selectedTickers.length; j++){
        const a=selectedTickers[i], b=selectedTickers[j];
        const x=map[a], y=map[b];
        const n=x.length;
        const m1=x.reduce((s,v)=>(s+v),0)/n;
        const m2=y.reduce((s,v)=>(s+v),0)/n;
        let num=0, d1=0, d2=0;
        for (let k=0;k<n;k++){
          const dx=x[k]-m1, dy=y[k]-m2;
          num+=dx*dy; d1+=dx*dx; d2+=dy*dy;
        }
        res[`${a}-${b}`] = (d1 && d2) ? num/Math.sqrt(d1*d2) : 0;
      }
    }
    return res;
  }, [returnsData, selectedTickers]);

  // Chart bounds
  const allVals = standardizedData.flatMap((row)=>
    selectedTickers.map((tk)=> (row[tk] as number)||0)
  );
  const yMin =  allVals.length?Math.min(...allVals):0;
  const yMax =  allVals.length?Math.max(...allVals):100;

  // Percentage changes
  const pctChg: Record<string, number> = {};
  if (standardizedData.length>1){
    const f=standardizedData[0], l=standardizedData[standardizedData.length-1];
    selectedTickers.forEach((tk)=>{
      const fv = f[tk] as number, lv = l[tk] as number;
      pctChg[tk] = fv?((lv-fv)/fv)*100:0;
    });
  } else selectedTickers.forEach((tk)=>pctChg[tk]=0);

  return (
    <div className="w-full flex flex-col gap-4 relative text-black">

      {/* Title */}
      <h2 className="text-3xl font-bold text-center mt-4 mb-2">
        Correlation Stock Chart
      </h2>

      {/* Info icon */}
      <div className="absolute top-0 right-0 mt-2 mr-2 group cursor-pointer">
        <span className="border border-black rounded-full px-2 py-1 bg-gray-100 text-sm">
          i
        </span>
        <div className="absolute hidden group-hover:block bg-white border border-gray-300 rounded p-2 w-60 text-xs right-0 mt-1 z-10">
          <p className="font-semibold">Disclaimer</p>
          <p>This is a percent-change correlation tool. Not actual price data.</p>
        </div>
      </div>

      {/* Search bar */}
      <div className="relative w-full max-w-xl mx-auto">
        <form onSubmit={handleSubmit} className="flex bg-gray-100 rounded overflow-hidden">
          <input
            value={searchQuery}
            onChange={e=>{setSearchQuery(e.target.value); setError(null)}}
            type="text"
            placeholder="Enter ticker..."
            className="flex-grow px-4 py-2 outline-none"
          />
          <button type="submit" className="px-4 py-2 bg-gray-300">Add</button>
        </form>
        {suggestions.length>0 && (
          <div className="absolute bg-white border border-gray-300 mt-1 w-full z-10 max-h-40 overflow-y-auto">
            {suggestions.map(s=>(
              <div
                key={s}
                className="px-4 py-2 hover:bg-gray-200 cursor-pointer"
                onClick={()=>addTicker(s)}
              >{s}</div>
            ))}
          </div>
        )}
      </div>

      {/* Error */}
      {error && <div className="text-red-600 text-center">{error}</div>}

      {/* Selected tickers */}
      {!!selectedTickers.length && (
        <div className="flex flex-wrap gap-2 justify-center">
          {selectedTickers.map(t=>(
            <div key={t} className="flex items-center bg-gray-200 px-3 py-1 rounded-full">
              <span>{t}</span>
              <button onClick={()=>removeTicker(t)} className="ml-2 text-red-600">×</button>
            </div>
          ))}
        </div>
      )}

      {/* Timeframes */}
      <div className="flex gap-2 justify-center mb-2">
        {timeframes.map(tf=>(
          <button
            key={tf.value}
            onClick={()=>setTimeframe(tf.value)}
            className={`px-3 py-1 rounded-full ${
              timeframe===tf.value
                ? "bg-blue-600 text-white"
                : "bg-gray-200 text-black hover:bg-gray-300"
            }`}
          >{tf.label}</button>
        ))}
      </div>

      {/* Loading */}
      {loading && <p className="text-center">Loading...</p>}

      {/* Chart */}
      {!selectedTickers.length && <p className="text-gray-600 text-center">No tickers added yet.</p>}
      {!!selectedTickers.length && !!standardizedData.length && (
        <ResponsiveContainer width="100%" height={400}>
          <LineChart data={standardizedData} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
            <XAxis dataKey="date" tick={{ fill:"#000"}} tickFormatter={t=>{const d=new Date(t);return`${d.getMonth()+1}/${d.getDate()}`}}/>
            <YAxis hide domain={[yMin-5, yMax+5]}/>
            <Tooltip contentStyle={{background:"#fff"}}/>
            {selectedTickers.map(t=>(
              <Line key={t} type="monotone" dataKey={t} stroke={tickerColors[t]||"#000"} strokeWidth={2} dot={false}/>
            ))}
          </LineChart>
        </ResponsiveContainer>
      )}

      {/* Legend & correlations */}
      {!!selectedTickers.length && !!standardizedData.length && (
        <div className="mt-4 text-black space-y-4">
          <div>
            <h3 className="text-xl font-semibold">Stocks</h3>
            <ul className="flex flex-wrap gap-4 mt-2">
              {selectedTickers.map(t=>(
                <li key={t} className="flex items-center gap-2">
                  <div style={{ width:20, height:4, backgroundColor:tickerColors[t]}}/>
                  <span>{t} ({pctChg[t]?.toFixed(2)}%)</span>
                </li>
              ))}
            </ul>
          </div>
          {selectedTickers.length>1 && (
            <div>
              <h3 className="text-xl font-semibold">Pairwise Correlations</h3>
              {Object.keys(pairwiseCorr).length===0
                ? <p>No correlation data (need at least two dates).</p>
                : <ul className="list-disc list-inside mt-2 space-y-1">
                    {Object.entries(pairwiseCorr)
                      .sort(([a],[b])=>a.localeCompare(b))
                      .map(([p,c])=>(
                        <li key={p}>{p}: {c.toFixed(2)}</li>
                      ))}
                  </ul>
              }
            </div>
          )}
        </div>
      )}
    </div>
  );
}
