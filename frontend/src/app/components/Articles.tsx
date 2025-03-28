import { useEffect, useState } from "react";
import axios from "axios";

interface Article {
    id: string;
    articleName: string;
    url: string;
    sentimentScore: number;
    date: string;
}

export default function Articles({ ticker }: { ticker: string }) {
    const [articles, setArticles] = useState<Article[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);;

   const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;


    useEffect(() => {
        const fetchArticles = async () => {
            try {
                const response = await axios.get(`${API_BASE_URL}/api/financials/stocks/${ticker}/articles`);
                console.log("Articles: ", response);
                setArticles(response.data);
            } catch (err) {
                setError("Failed to load articles.");
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        fetchArticles();
    }, [ticker]);
    
    if (loading) return <p>Loading articles...</p>;
    if (error) return <p className="text-red-500">{error}</p>;

    // Function to categorize sentiment based on score
    const getSentimentLabel = (score: number) => {
        if (score > 0.35) return "Bullish";
        if (score < -0.15) return "Bearish";
        return "Neutral";
    };
    
    const sortedArticles = articles.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());


    return (
        <section className="w-full">
            <h2 className="text-xl font-bold mb-4 text-black">Latest News</h2>
            <div
                className="overflow-y-scroll max-h-80 border p-4 rounded-lg shadow-md"
                style={{ height: '300px' }} // Adjust height as per your design needs
            >
                {sortedArticles.length > 0 ? (
                    articles.map((article) => (
                        <div key={article.id} className="p-4 border-b last:border-0">
                            <a href={article.url} target="_blank" rel="noopener noreferrer" className="text-blue-500">
                                <h3 className="text-lg font-bold">{article.articleName}</h3>
                            </a>
                            <p className="mt-1 text-black">
                            <span
                                className={`font-semibold ${
                                    article.sentimentScore > 0.1
                                        ? "text-green-500"
                                        : article.sentimentScore < -0.1
                                            ? "text-red-500"
                                            : "text-yellow-500"
                                }`}
                            >
                                {getSentimentLabel(article.sentimentScore)}
                            </span>
                                {" "} - Sentiment Score: {article.sentimentScore}
                            </p>
                            <p className="text-gray-500 text-sm">
                                Published on: {new Date(article.date).toLocaleDateString()}
                            </p>
                        </div>
                    ))
                ) : (
                    <p>No recent articles available.</p>
                )}
            </div>
        </section>
    );
}
