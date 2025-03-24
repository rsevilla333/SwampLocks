import Link from "next/link";
import "./globals.css";

export default function RootLayout({ children }: { children: React.ReactNode }) {
    return (
        <html lang="en">
        <body className="bg-white text-black">
        {/* 🔹 Navigation Bar */}
        <nav className="w-full flex justify-between items-center bg-blue-600 text-white px-6 py-4 fixed top-0 left-0 right-0 shadow-md z-50">
            <Link href="/" className="text-2xl font-bold">SwampLocks</Link>

            <div className="flex space-x-6">
                <Link href="/dashboard" className="hover:underline">Dashboard</Link>
                <Link href="/dashboard#market-overview" className="hover:underline">Market Overview</Link>
                <Link href="/dashboard#trending" className="hover:underline">Trending Stocks</Link>
                <Link href="/dashboard#heatmap" className="hover:underline">Heatmap</Link>
                <Link href="/dashboard#news" className="hover:underline">News</Link>
                <Link href="/dashboard#watchlist" className="hover:underline">Watchlist</Link>
                <Link href="/sector/technology" className="hover:underline">Sectors</Link>
            </div>
        </nav>

        {/* 🔹 Page Content (Keeps Everything Centered) */}
        <main className="container mt-16">{children}</main>
        </body>
        </html>
    );
}
