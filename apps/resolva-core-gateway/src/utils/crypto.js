import crypto from "crypto";

export function hmacSha256Hex(secret, data) {
    return crypto.createHmac("sha256", secret).update(data).digest("hex");
}

export function timingSafeEqualHex(a, b) {
    try {
        const ab = Buffer.from(a, "hex");
        const bb = Buffer.from(b, "hex");
        if (ab.length !== bb.length) return false;
        return crypto.timingSafeEqual(ab, bb);
    } catch {
        return false;
    }
}
