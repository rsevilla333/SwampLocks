import React from "react";

interface Stock {
    symbol: string;
    marketCap: number;
    change: number;
}

interface TreemapNode extends Stock {
    x: number;
    y: number;
    width: number;
    height: number;
}

// Compute Treemap Layout
function computeTreemap(
    items: Stock[],
    x: number,
    y: number,
    width: number,
    height: number,
    vertical: boolean
): TreemapNode[] {
    if (items.length === 0) return [];
    if (items.length === 1) {
        return [
            {
                ...items[0],
                x,
                y,
                width,
                height,
            },
        ];
    }

    const totalCap = items.reduce((sum, item) => sum + item.marketCap, 0);
    let sum = 0,
        splitIndex = 0;
    for (let i = 0; i < items.length; i++) {
        sum += items[i].marketCap;
        if (sum >= totalCap / 2) {
            splitIndex = i;
            break;
        }
    }

    const group1 = items.slice(0, splitIndex + 1);
    const group2 = items.slice(splitIndex + 1);
    const group1Cap = group1.reduce((sum, item) => sum + item.marketCap, 0);
    const group2Cap = group2.reduce((sum, item) => sum + item.marketCap, 0);

    let layout: TreemapNode[] = [];

    if (vertical) {
        const width1 = width * (group1Cap / totalCap);
        const layout1 = computeTreemap(group1, x, y, width1, height, !vertical);
        const layout2 = computeTreemap(group2, x + width1, y, width - width1, height, !vertical);
        layout = [...layout1, ...layout2];
    } else {
        const height1 = height * (group1Cap / totalCap);
        const layout1 = computeTreemap(group1, x, y, width, height1, !vertical);
        const layout2 = computeTreemap(group2, x, y + height1, width, height - height1, !vertical);
        layout = [...layout1, ...layout2];
    }

    return layout;
}

// Adjust font size based on square size
const getAdjustedFontSize = (squareWidth: number, squareHeight: number, text: string) => {
    const baseSize = Math.max(8, Math.min(squareWidth, squareHeight) / 4);
    const maxByWidth = squareWidth / (text.length * 0.6);
    const maxByHeight = squareHeight / 2;
    return Math.min(baseSize, maxByWidth, maxByHeight);
};

interface TreemapProps {
    stocks: Stock[];
    width?: number;
    height?: number;
}

const Treemap: React.FC<TreemapProps> = ({ stocks, width = 600, height = 480 }) => {

    const sortedStocks = [...stocks].sort((a, b) => b.marketCap - a.marketCap);
    
    const layout = computeTreemap(sortedStocks, 0, 0, width, height, width >= height);


    const getGradientColor = (change: number) => {
        // Cap the percentage change at 5% to ensure we are within the range of 5 colors
        const cappedChange = Math.min(5, Math.max(-5, change));
        
        let color: string;
        
        if (cappedChange >= 0) {
            // Assign color based on the percentage change (brighter green shades)
            if (cappedChange >= 2.5) color = "#008000"; // Bright green (strong positive)
            else if (cappedChange >= 2) color = "#43a047"; // Bright medium green (good positive)
            else if (cappedChange >= 1.5) color = "#66bb6a"; // Lively green (moderate positive)
            else if (cappedChange >= 0.5) color = "#81c784"; // Fresh light green (small positive)
            else color = "#a5d6a7"; // Very light green (minimal positive)
        } else {
            // Handle negative (red) change values
            if (cappedChange <= -2.5) color = "#e53935"; // Bright red (strong negative)
            else if (cappedChange <= -2) color = "#f44336"; // Vivid red (bad negative)
            else if (cappedChange <= -1.5) color = "#ef5350"; // Strong red (moderate negative)
            else if (cappedChange <= -0.5) color = "#ff7043"; // Lively red (small negative)
            else color = "#ff8a65"; // Light red (very small negative)
        }

        return color;
    };

    return (
        <div
            className="relative bg-transparent rounded-lg mx-auto h-full w-full"
            style={{ width: `${width}px`, height: `${height}px` }}
        >
            {layout.map((item, index) => {
                const symbolFontSize = getAdjustedFontSize(item.width, item.height, item.symbol);
                const percentText = `${item.change}%`;
                const percentFontSize = getAdjustedFontSize(item.width, item.height, percentText);

                return (
                    <div
                        key={index}
                        className="absolute flex flex-col items-center justify-center text-black font-mono border border-gray-600 p-1 text-gray-800 "
                        style={{
                            width: `${item.width}px`,
                            height: `${item.height}px`,
                            left: `${item.x}px`,
                            top: `${item.y}px`,
                            // Apply the gradient based on the percentage change
                            background: getGradientColor(item.change),
                        }}
                    >
                        <span className="w-full text-center" style={{ fontSize: `${symbolFontSize}px`, lineHeight: 1.2 }}>
                            {item.symbol}
                        </span>
                        <span className="w-full text-center" style={{ fontSize: `${percentFontSize}px`, lineHeight: 1.2 }}>
                            {percentText}
                        </span>
                    </div>
                );
            })}
        </div>
    );
};

export default Treemap;
