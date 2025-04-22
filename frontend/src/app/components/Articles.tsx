import { useEffect, useState } from "react";
import CircularProgress from '@mui/material/CircularProgress';
import axios from "axios";
import { cache } from 'react';

interface Article {
    id: string;
    articleName: string;
    url: string;
    sentimentScore: number;
    date: string;
    ticker: string;
}

export default function Articles({ ticker }: { ticker?: string }) {
    const [articles, setArticles] = useState<Article[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);;

   const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
   
    useEffect(() => {
        const fetchArticles = cache(async () => {
            try {
                let url;
                if(ticker) {
                    url = `${API_BASE_URL}/api/financials/stocks/${ticker}/articles`;
                } else{
                    url = `${API_BASE_URL}/api/financials/stocks/articles/all`;
                }
                const response = await axios.get(url);
                console.log("Articles: ", response);
                setArticles(response.data);
            } catch (err) {
                setError("Failed to load articles.");
                console.error(err);
            } finally {
                setLoading(false);
            }
        });

        fetchArticles();
    }, [ticker]);
    
    if (loading) return <p><CircularProgress/></p>;
    if (error) return <p className="text-red-500">{error}</p>;

    // Function to categorize sentiment based on score
    const getSentimentLabel = (score: number) => {
        if (score >= 0.35) return "Bullish";
        if (score <= -0.35) return "Bearish";
        if(score >= 0.15 && score < 0.35) return "Somewhat Bullish";
        return "Somewhat Bearish";
    };
    
    const sortedArticles = articles.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());


    return (
        <section className="min-w-full min-h-max">
            <div
                className="overflow-y-scroll h-full bg-gray-100 p-4 rounded-lg shadow-md mb-6"
                style={{ height: '2500px' }} // Adjust height as per your design needs
            >
                {sortedArticles.length > 0 ? (
                    articles.map((article) => (
                        <div key={article.id} className="p-4 ">
                            <a href={article.url} target="_blank" rel="noopener noreferrer" className="text-blue-500">
                                <h3 className="text-lg font-bold">{article.articleName}</h3>
                                <h4 className="text-gray-800">{ticker ? "" : article.ticker}</h4>
                            </a>
                            
                            <p className="mt-1 text-black">
                            <span
                                className={`font-semibold ${
                                    article.sentimentScore >= 0.35
                                        ? "text-green-500"
                                        : article.sentimentScore >= -0.15
                                            ? "text-yellow-600"
                                            : "text-red-500"
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
