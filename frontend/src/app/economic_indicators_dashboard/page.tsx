import EconomicIndicatorDashboard from '../components/EconomicIndicatorDashboard'; 

export default function Page() {
    return (
        <div className="min-h-screen bg-gray-100 py-8">
            <div className="max-w-7xl mx-auto px-4">
                <h1 className="text-4xl font-extrabold text-center text-gray-800 mb-6">Economic Data Dashboard</h1>
                <EconomicIndicatorDashboard />
            </div>
        </div>
    );
}
