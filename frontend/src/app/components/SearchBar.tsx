import { useState } from "react";
import { Search } from "lucide-react";

interface SearchBarProps {
    onSearch: (query: string) => void;
}

export default function SearchBar({ onSearch }: SearchBarProps) {
    const [query, setQuery] = useState("");

    const handleSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
        setQuery(event.target.value);
    };

    const handleSubmit = (event: React.FormEvent) => {
        event.preventDefault();
        onSearch(query.trim().toUpperCase()); 
    };

    return (
        <div className="w-full bg-gray-300 rounded-md flex items-center p-2 hover:bg-gray-400 active:bg-gray-400 cursor-pointer">
            <Search className="text-gray-600" size={20} /> 
            <form onSubmit={handleSubmit} className="flex w-full ">
                <input
                    type="text"
                    placeholder="Enter stock ticker..."
                    value={query}
                    onChange={handleSearch}
                    className="w-full bg-transparent text-white px-2 outline-none focus:ring-0 focus:outline-none focus:ring-0 focus:outline-none focus:placeholder-white hover:placeholder-white"
                />
            </form>
        </div>
    );
}
