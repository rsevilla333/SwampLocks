"use client";

import React from "react";
import { useUser } from "../context/UserContext";
import { useRouter } from "next/navigation"; // Import useRouter to handle redirection
import { signOut } from "next-auth/react";
import UserPortfolio from "../components/UserPortfolio";

const UserInfoPage = () => {
    const { user } = useUser();
    const router = useRouter();

    const handleSignOut = async () => {
        await signOut({ redirect: false }); 
        router.push("/"); 
    };

    return (
        <div className="max-w mx-auto p-2 flex flex-col items-center ">
            {user ? (
                <div className="w-1/2  bg-white rounded-xl shadow-md p-6 mb-8 border border-gray-200">
                    <p className="text-gray-600">
                        <span className="font-medium">Name:</span> {user.name}
                    </p>
                    <p className="text-gray-600 mb-4">
                        <span className="font-medium">Email:</span> {user.email}
                    </p>
                    <button
                        onClick={handleSignOut}
                        className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600 transition"
                    >
                        Sign Out
                    </button>
                </div>
            ) : (
                <div className="text-gray-500 mb-8">No user information available</div>
            )}

            <UserPortfolio userId={user?.userId ? user.userId : " "} />
        </div>
    );
};

export default UserInfoPage;