import { Router } from "express";
import { requireInternalHmac } from "../middleware/hmacAuth.js";
import { forwardToCore } from "../services/coreApi.service.js";

export const internalRouter = Router();

/**
 * n8n calls:
 *  POST /integrations/internal/surveys-generated  -> /internal/surveys/generated
 *  POST /integrations/internal/surveys-answer     -> /internal/surveys/answer
 *  POST /integrations/internal/surveys-complete   -> /internal/surveys/complete
 */
const MAP = {
    "surveys-generated": "/internal/surveys/generated",
    "surveys-answer": "/internal/surveys/answer",
    "surveys-complete": "/internal/surveys/complete"
};

internalRouter.post("/integrations/internal/:key", requireInternalHmac, async (req, res, next) => {
    try {
        const key = req.params.key;
        const targetPath = MAP[key];
        if (!targetPath) return res.status(404).json({ message: "Unknown internal route key" });
        
        // Forward signature headers as-is (ASP.NET Core also verifies)
        const fwdHeaders = {
            "content-type": "application/json",
            "x-resolva-timestamp": req.header("x-resolva-timestamp"),
            "x-resolva-signature": req.header("x-resolva-signature")
        };
        
        const { status, contentType, bodyText } = await forwardToCore({
            path: targetPath,
            method: "POST",
            headers: fwdHeaders,
            rawBody: req.rawBody
        });
        
        if (contentType) res.setHeader("content-type", contentType);
        res.status(status).send(bodyText);
    } catch (e) {
        next(e);
    }
});
