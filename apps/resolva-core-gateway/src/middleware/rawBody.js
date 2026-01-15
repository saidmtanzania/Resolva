import express from "express";

// IMPORTANT: verify() lets us capture raw buffer
export const rawBodyCapture = express.json({
    verify: (req, res, buf) => {
        req.rawBody = buf; // Buffer
        }
});
