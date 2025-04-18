import NextAuth from "next-auth";
import AzureADProvider from "next-auth/providers/azure-ad";
import type { NextAuthOptions } from "next-auth";

const authOptions: NextAuthOptions = {
    providers: [
        AzureADProvider({
            clientId: process.env.AZURE_AD_CLIENT_ID!,
            clientSecret: process.env.AZURE_AD_CLIENT_SECRET!,
            tenantId: "common",
        }),
    ],
    callbacks: {
        async signIn({ user, account, profile }) {
            console.log("Sign-in callback:", user, account, profile);
            return true;
        },
        async redirect({ url, baseUrl }) {
            console.log("Redirect callback:", url, baseUrl);
            return url;
        },
        async session({ session, user }) {
            console.log("Session callback:", session, user);
            return session;
        },
    },
};

const handler = NextAuth(authOptions);

export { handler as GET, handler as POST };
