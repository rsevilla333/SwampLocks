"use client";

import React from "react";
import { useUser } from "../context/UserContext";
import { useRouter } from "next/navigation"; // Import useRouter to handle redirection
import { signOut } from "next-auth/react";

const UserInfoPage = () => {
    const { user } = useUser();
    const router = useRouter();

    const handleSignOut = async () => {
        await signOut({ redirect: false }); // Sign out the user without automatic redirect
        router.push("/"); // Redirect to the login page (or homepage)
    };

    return (
        <div>
            {/* Check if user data exists */}
            {user ? (
                <div className="text-black">
                    <p>Email: {user.email}</p>
                    <p>Name: {user.name}</p>
                    <button onClick={handleSignOut}>Sign Out</button>
                </div>
            ) : (
                <p>No user information available</p>
            )}
        </div>
    );
};

export default UserInfoPage;