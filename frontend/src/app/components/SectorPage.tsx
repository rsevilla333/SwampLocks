"use client";

import Image from "next/image";
import Link from "next/link";
import { useState, useEffect } from "react";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import SectorAnalysis from "./SectorAnalysis";
import Treemap from "./TreeMap";
import Footer from "./Footer";
import { ResponsiveContainer, PieChart, Pie, Cell, Tooltip, Legend } from "recharts";

interface SectorPageProps {
    sectorName: string;
    imageUrl: string;
}

// Mock Market Cap Data
const mockMarketCapData = [
    { name: "AAPL", value: 2500 },
    { name: "MSFT", value: 2300 },
    { name: "GOOGL", value: 1800 },
    { name: "AMZN", value: 1700 },
    { name: "TSLA", value: 900 },
];

// Mock Data
const stocks = [
    { symbol: "AAPL", marketCap: 250000, change: 1.5 },
    { symbol: "GOOGL", marketCap: 180000, change: -0.7 },
    { symbol: "AMZN", marketCap: 200000, change: 2.1 },
    { symbol: "TSLA", marketCap: 150000, change: -1.2 },
    { symbol: "MSFT", marketCap: 220000, change: 0.9 },
    { symbol: "FB", marketCap: 130000, change: -0.4 },
    { symbol: "NFLX", marketCap: 120000, change: 1.3 },
    { symbol: "NVDA", marketCap: 140000, change: 2.5 },
  

    // Second set (slightly modified values)
    { symbol: "AAPL2", marketCap: 245000, change: 1.6 },
    { symbol: "GOOGL2", marketCap: 175000, change: -0.8 },
    { symbol: "AMZN2", marketCap: 195000, change: 2.0 },


    // Third set (again with slight modifications)
    { symbol: "AAPL3", marketCap: 255000, change: 1.4 },
    { symbol: "GOOGL3", marketCap: 182000, change: -0.6 },
    { symbol: "AMZN3", marketCap: 205000, change: 2.2 },
    { symbol: "TSLA3", marketCap: 148000, change: -1.0 },
    { symbol: "MSFT3", marketCap: 225000, change: 0.8 },
    { symbol: "FB3", marketCap: 132000, change: -0.5 },
    { symbol: "NFLX3", marketCap: 118000, change: 1.2 },
   
];


const COLORS = ["#FF5733", "#33A1FF", "#33FF57", "#FFD700", "#8B4513"];

export default function SectorPage({ sectorName, imageUrl }: SectorPageProps) {
    // Set the default selected date to today's date
    const [selectedDate, setSelectedDate] = useState<Date>(new Date());
    const [isCalendarOpen, setIsCalendarOpen] = useState(false);

    // Handle the date change
    const handleDateChange = (date: Date | null) => {
        if (date) {
            setSelectedDate(date);
            setIsCalendarOpen(false); // Close calendar after selection
        }
    };

    // Toggle the calendar popup
    const toggleCalendar = () => {
        setIsCalendarOpen(!isCalendarOpen);
    };

    return (
        <div className="min-h-screen flex flex-col items-center p-8 bg-gray-50">
            {/* Back to Home Link */}
            <Link
                href="/"
                className="text-lg text-blue-600 font-medium hover:underline mb-6"
            >
                ← Back to Home
            </Link>

            {/* Sector Title */}
            <h1 className="text-4xl font-semibold text-gray-800 mb-8">{sectorName}</h1>

            {/* Date Picker Button */}
            <div className="mb-8 w-full">
                <button
                    onClick={toggleCalendar}
                    className="px-4 py-2 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition"
                >
                    Date: {selectedDate.toLocaleDateString()}
                </button>

                {/* Date Picker Popup */}
                {isCalendarOpen && (
                    <div className="mt-4 z-10 absolute top-16 bg-white shadow-lg rounded-lg border p-4">
                        <DatePicker
                            selected={selectedDate}
                            onChange={handleDateChange}
                            inline
                            dateFormat="MMMM d, yyyy"
                            maxDate={new Date()}
                        />
                    </div>
                )}
            </div>

            {/* Main Content */}
            <div className="w-full grid grid-cols-1 md:grid-cols-2 gap-8">
                {/* Top Movers Section */}
                <div className="p-6 bg-white shadow-md rounded-lg">
                    <h2 className="text-2xl font-semibold text-gray-700 mb-4">Top Movers</h2>
                    <p>(Data will be fetched for {selectedDate ? selectedDate.getFullYear() : "the current year"})</p>
                </div>

                {/* Top Market Cap Stocks Section */}
                <div className="p-6 bg-white shadow-md rounded-lg">
                    <h2 className="text-2xl font-semibold text-gray-700 mb-4">Top Market Cap Stocks</h2>
                    <p>(Data will be fetched for {selectedDate ? selectedDate.getFullYear() : "the current year"})</p>
                </div>
            </div>
            {/* Pie Chart for Market Cap */}
            <div className="w-full flex justify-center mt-6">
                <ResponsiveContainer width={800} height={600}>
                    <PieChart>
                        <Pie
                            data={mockMarketCapData}
                            cx="50%"
                            cy="50%"
                            labelLine={false}
                            outerRadius={180}
                            fill="#8884d8"
                            dataKey="value"
                        >
                            {mockMarketCapData.map((entry, index) => (
                                <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                            ))}
                        </Pie>
                        <Tooltip />
                        <Legend />
                    </PieChart>
                </ResponsiveContainer>
            </div>
            <div className="w-max">
                <SectorAnalysis sectorName={sectorName} />
            </div>
            {/* Heatmap Section */}
            <div className="w-full flex flex-col items-center">
                <h2 className="text-2xl font-semibold text-gray-700 mb-4 text-center">Heatmap</h2>
                <Treemap stocks={stocks} />
            </div>
            <Footer />
        </div>
    );
}