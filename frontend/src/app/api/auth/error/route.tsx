export async function GET() {
    return new Response("An error occurred during authentication", {
        status: 500,
    });
}