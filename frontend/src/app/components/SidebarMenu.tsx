"use client";

import Link from "next/link";
import Login from "./Login";
import { usePathname } from "next/navigation";
import { SessionProvider } from "next-auth/react";

export default function SidebarMenu() {
    const pathname = usePathname();

    return (
        <SessionProvider>
            <aside className="fixed top-0 left-0 h-full w-72 bg-gray-800 text-white p-8 flex flex-col shadow-lg">
                <div className="w-full h-15 pb-5">
                    <Login />
                </div>

                <Link
                    href="/"
                    className={`mb-8`}
                >
                    <h2 className="text-3xl font-extrabold uppercase tracking-wide cursor-pointer rounded transition-all duration-150 hover:text-gray-300">
                        SwampLocks
                    </h2>
                </Link>

                <nav className="flex flex-col justify-between space-y-6 text-lg font-medium">
                    {[
                        { href: "/commodities", label: "Commodities" },
                        { href: "/economic_indicators_dashboard", label: "Economic Indicators" },
                        { href: "/ex_rates", label: "Exchange Rates" },
                        { href: "/correlations", label: "Correlations" },
                    ].map(({ href, label }) => {
                        const isActive = pathname === href;

                        return (
                            <Link
                                key={href}
                                href={href}
                                className={`px-2 py-1 rounded transition-all duration-150
                                    ${isActive ? "bg-gray-700 text-white" : ""}
                                    hover:bg-gray-600 hover:text-lg hover:font-semibold`}
                            >
                                {label}
                            </Link>
                        );
                    })}
                </nav>
            </aside>
        </SessionProvider>
    );
}
