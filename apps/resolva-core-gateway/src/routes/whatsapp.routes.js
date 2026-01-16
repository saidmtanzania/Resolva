import { Router } from "express";
import { env } from "../config/env.js";

export const whatsappRouter = Router();

/**
 * Meta verification challenge:
 * GET /webhooks/whatsapp?hub.mode=subscribe&hub.verify_token=...&hub.challenge=...
 */
whatsappRouter.get("/webhooks/whatsapp", (req, res) => {
    const mode = req.query["hub.mode"];
    const token = req.query["hub.verify_token"];
    const challenge = req.query["hub.challenge"];
    
    if (mode === "subscribe" && token === process.env.WHATSAPP_VERIFY_TOKEN) {
        return res.status(200).send(challenge);
    }
    return res.sendStatus(403);
});

/**
 * WhatsApp sends messages + status updates here
 * We forward the raw payload to n8n webhook.
 */
whatsappRouter.post("/webhooks/whatsapp", async (req, res, next) => {
    try {
        const n8nUrl = process.env.N8N_WHATSAPP_WEBHOOK_URL; // full URL
        if (!n8nUrl) return res.status(500).json({ message: "N8N webhook URL not configured" });
        
        // Forward to n8n
        const r = await fetch(n8nUrl, {
            method: "POST",
            headers: { "content-type": "application/json" },
            body: req.rawBody
        });
        
        // WhatsApp requires a quick 200
        res.sendStatus(200);
    } catch (e) {
        next(e);
    }
});
