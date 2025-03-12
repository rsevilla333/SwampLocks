import Link from "next/link";
import "./globals.css";

export default function RootLayout({ children }: { children: React.ReactNode }) {
    return (
        <html lang="en">
        <body>
        {/* 🔹 Global Navigation Bar */}
        <nav className="w-full flex justify-between items-center bg-blue-600 text-white p-4 fixed top-0 left-0 right-0 shadow-md z-50">
            {/* Left Side: SwampLocks Logo */}
            <Link href="/" className="text-2xl font-bold">SwampLocks</Link>

            {/* 🔹 Navigation Links for Dashboard Sections */}
            <div className="flex space-x-6">
                <Link href="/dashboard#market-overview" className="hover:underline">Market Overview</Link>
                <Link href="/sector/technology" className="hover:underline">Sectors</Link>
                <Link href="/dashboard#trending" className="hover:underline">Trending Stocks</Link>
                <Link href="/dashboard#news" className="hover:underline">News</Link>
                <Link href="/dashboard#heatmap" className="hover:underline">Heatmap</Link>
                
                
            </div>
        </nav>

        {/* 🔹 Page Content (Ensures Content Doesn't Get Covered by Fixed Navbar) */}
        <main className="mt-16 p-4">{children}</main>
        </body>
        </html>
    );
}
