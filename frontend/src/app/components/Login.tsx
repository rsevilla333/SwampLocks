"use client";

import { useEffect, useState } from "react";
import { useSession, signIn, signOut } from "next-auth/react";
import { useRouter } from "next/navigation";
import { FaRegUser,  FaRegUserCircle } from "react-icons/fa";
import { useUser } from "../context/UserContext";

const Login = () => {
    const { data: session, status } = useSession();
    const { setUser } = useUser();
    const router = useRouter();
    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

    useEffect(() => {
        if (session?.user) {
            setUser({
                email: session.user?.email || null,
                name: session.user?.name || null,
            });
            sendWelcomeEmail(session.user?.email ?? "NONE", session.user?.name ?? "NONE");

        }
    }, [session, setUser])

    const sendWelcomeEmail = async (userEmail: string, name: string) => {
        if (userEmail) {
            try {
                const response = await fetch(`${API_BASE_URL}/api/financials/login/${userEmail}/${name}`);

                if (response.ok) {
                    console.log("Welcome email sent successfully.");
                } else {
                    console.error("Failed to send welcome email.");
                }
            } catch (error) {
                console.error("Error sending welcome email:", error);
            }
        }
    };

    const handleLogout = async () => {
        signOut();
    };

    const handleLogin = () => {
        signIn();
    };

    const redirectToUserInfo = () => {
        router.push("/user_info"); 
    };

    return (
        <div>
            {status === "loading" ? (
                <p>Loading...</p>
            ) : session ? (
                <div>
                    {/*<button onClick={handleLogout}>Sign Out</button>*/}
                    <FaRegUserCircle className="cursor-pointer hover:text-blue-500 h-12 w-12" onClick={redirectToUserInfo}/>
                    <p>{session.user?.email}</p>
                    
                </div>
            ) : (
                <div className="w-full, h-full">
                    <button 
                        onClick={handleLogin}
                    >
                        <FaRegUser className=" cursor-pointer hover:text-blue-500 h-12 w-12" />
                    </button>
                </div>
            )}
        </div>
    );
};

export default Login;
