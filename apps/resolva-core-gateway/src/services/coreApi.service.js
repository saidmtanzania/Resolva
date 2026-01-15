import { env } from "../config/env.js";
import { fetchWithTimeout } from "../utils/http.js";

export async function forwardToCore({ path, method = "POST", headers = {}, rawBody = null }) {
    const url = env.CORE_API_BASE_URL.replace(/\/$/, "") + path;
    
    const res = await fetchWithTimeout(url, {
        method,
        headers,
        body: rawBody
    });
    
    const text = await res.text();
    
    // Return raw response to caller
    return { status: res.status, contentType: res.headers.get("content-type"), bodyText: text };
}
