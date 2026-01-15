import { SIGNATURE } from "../config/constants.js";
import { env } from "../config/env.js";
import { hmacSha256Hex, timingSafeEqualHex } from "../utils/crypto.js";

export function requireInternalHmac(req, res, next) {
    const ts = req.header(SIGNATURE.TS_HEADER);
    const sig = req.header(SIGNATURE.SIG_HEADER);
    
    if (!ts || !sig) {
        return res.status(401).json({ message: "Unauthorized", reason: "Missing signature headers" });
    }
    
    const tsNum = Number(ts);
    if (!Number.isFinite(tsNum)) {
        return res.status(401).json({ message: "Unauthorized", reason: "Invalid timestamp" });
    }
    
    const now = Math.floor(Date.now() / 1000);
    if (Math.abs(now - tsNum) > SIGNATURE.MAX_SKEW_SECONDS) {
        return res.status(401).json({ message: "Unauthorized", reason: "Timestamp out of range" });
    }
    
    const body = req.rawBody ? req.rawBody.toString("utf8") : "";
    const signed = `${ts}.${body}`;
    const expected = hmacSha256Hex(env.RESOLVA_INTERNAL_SECRET, signed);
    
    if (!timingSafeEqualHex(expected, sig)) {
        return res.status(401).json({ message: "Unauthorized", reason: "Bad signature" });
    }
    
    next();
}
