"use client";

import Image from "next/image";
import Link from "next/link";
import { useState, useEffect } from "react";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import SectorAnalysis from "./SectorAnalysis";

interface SectorPageProps {
    sectorName: string;
    imageUrl: string;
}

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
            <div className="w-max">
                <SectorAnalysis sectorName={sectorName} />
            </div>
            {/* Heatmap Section */}
            <div className="w-full">
                <h2 className="text-2xl font-semibold text-gray-700 mb-4">Sector Heatmap</h2>
                <Image
                    src={imageUrl}
                    alt={`${sectorName} Heatmap`}
                    width={900}
                    height={600}
                    className="rounded-lg border border-gray-300 shadow-lg"
                />
            </div>
        </div>
    );
}
