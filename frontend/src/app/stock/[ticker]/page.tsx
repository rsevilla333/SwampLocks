"use client";

import { useParams } from "next/navigation";
import StockChart from "../../components/StockChart";
import FinancialStatements from "../../components/FinancialStatements";
import Articles from "../../components/Articles";
import axios from "axios";
import StockSearchBar from "../../components/StockSearchBar";
import Footer from "../../components/Footer";

const StockPage = () => {
    const { ticker } = useParams();
    
    return (
        <div className="w-full h-full flex flex-row">
            <div className="flex flex-col items-center gap-11 w-3/4">
                <StockSearchBar/>
                <StockChart ticker={ticker?.toString() || ""} />
                {/* Cash Flow Statement Section */}
                <div className=" w-full p-4">
                    <FinancialStatements ticker={ticker?.toString() || ""} />
                </div>
                <Footer/>
            </div>
            <div className=" w-1/4 p-4 h-full">
                <Articles ticker={ticker?.toString() || ""} />
            </div>
        </div>
    );
};

export default StockPage;

{/*/!* Articles Section *!/*/}
