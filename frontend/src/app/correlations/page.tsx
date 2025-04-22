"use client";

import { useState } from "react";
import Footer from "../components/Footer"
import MultiStockSearchChart from "../components/MultiStockChart";

export default function CorrelationsPage() {
    
    return (
        <div className="min-h-screen flex flex-col items-center p-6 bg-white text-black">
            <MultiStockSearchChart />
            <Footer/>
        </div>
    );
}
