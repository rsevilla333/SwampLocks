"use client";

import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import StockChart from "../../components/StockChart";
import BalanceSheet from "../../components/BalanceSheet";
import IncomeStatement from "../../components/IncomeStatement";
import FinancialStatements from "../../components/FinancialStatements";
import Articles from "../../components/Articles";
import axios from "axios";

const StockPage = () => {
    const { ticker } = useParams(); 
    
    return (
        <div className="w-full flex flex-col items-center gap-11">
            <StockChart ticker={ticker?.toString() || ""} />

            {/*/!* Articles Section *!/*/}
            <div className=" w-full border-2 border-gray-700 p-4">
                <Articles ticker={ticker?.toString() || ""} />
            </div>
            
            {/* Balance Sheet Section */}
            <div className= "w-full border-2 border-gray-700 p-4">
                <BalanceSheet ticker={ticker?.toString() || ""} />
            </div>
            
            {/* Cash Flow Statement Section */}
        <div className=" w-full border-2 border-gray-700 p-4">
            <FinancialStatements ticker={ticker?.toString() || ""} />
        </div>
            
           
        </div>
    );
};

export default StockPage;
