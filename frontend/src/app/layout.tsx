import Link from "next/link";
import "./globals.css";
import SidebarMenu from "./components/SidebarMenu";

export default function RootLayout({ children }: { children: React.ReactNode }) {
    return (
        <html lang="en">
        <body className="bg-background text-white min-h-screen flex ">
        {/* Sidebar Menu (Always Visible) */}
        <SidebarMenu />

        {/* Main Content Area */}
        <main className="flex-1 p-6 ml-72">
            {children}
        </main>
        </body>
        </html>
    );
}
