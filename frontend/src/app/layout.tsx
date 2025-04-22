import Link from "next/link";
import "./globals.css";
import SidebarMenu from "./components/SidebarMenu";
import { UserProvider } from "./context/UserContext";
import Login from "./components/Login";

export const metadata = {
    title: "Swamplocks",
    icons: {
        icon: "/logo2.ico", 
    },
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
    return (
        <html lang="en">
        {/*<head>*/}
        {/*    <link rel="icon" type="image/png" href="/logo.png" />*/}
        {/*</head>*/}
        <body className="bg-background text-white min-h-screen flex ">
        <UserProvider>
            <SidebarMenu />
    
            {/* Main Content Area */}
            <main className="flex-1 p-6 ml-72">
                {children}
            </main>
        </UserProvider>
        </body>
        </html>
    );
}
