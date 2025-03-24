import Image from "next/image";
import Link from "next/link";

export default function RealEstatePage() {
    return (
        <div className="min-h-screen flex flex-col items-center p-6">
            <Link href="/" className="text-4xl font-extrabold mb-4 text-blue-600 hover:underline">
                ← Back to Home
            </Link>
            <h1 className="text-5xl font-bold mb-6">Real Estate</h1>

            <div className="flex w-full max-w-6xl">
                <div className="w-1/2 p-4">
                    <h2 className="text-2xl font-semibold mb-4">Top Movers</h2>
                    <p>(Data will be fetched here)</p>
                </div>

                <div className="w-1/2 p-4">
                    <h2 className="text-2xl font-semibold mb-4">Top Market Cap Stocks</h2>
                    <p>(Data will be fetched here)</p>
                </div>
            </div>

            <div className="mt-6">
                <h2 className="text-2xl font-semibold mb-4">Sector Heatmap</h2>
                <Image
                    src="/sector-images/technology.png"
                    alt="Technology Heatmap"
                    width={800}
                    height={500}
                    className="rounded-lg border border-gray-300 shadow-md"
                />
            </div>
        </div>
    );
}
