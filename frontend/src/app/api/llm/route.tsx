// /app/api/ask/route.ts (or .js if not using TypeScript)
import { NextRequest, NextResponse } from "next/server";
import OpenAI from "openai";

const openai = new OpenAI({
    baseURL: "https://models.inference.ai.azure.com", 
    apiKey: process.env.OPENAI_API_KEY || "", 
});

export async function POST(req: NextRequest) {
    try {
        const body = await req.json();
        const userMessage = body.message;

        if (!userMessage) {
            return NextResponse.json({ error: "Message is required" }, { status: 400 });
        }

        const chat = await openai.chat.completions.create({
            model: "gpt-4o",
            messages: [
                { role: "system", content: "" },
                { role: "user", content: userMessage },
            ],
            temperature: 1,
            max_tokens: 4096,
            top_p: 1,
        });

        return NextResponse.json({ reply: chat.choices[0].message.content });
    } catch (error: any) {
        console.error("Error in /api/ask:", error);
        return NextResponse.json({ error: error.message || "Internal Server Error" }, { status: 500 });
    }
}
