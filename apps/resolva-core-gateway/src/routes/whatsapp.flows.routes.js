import { Router } from "express";
import FormData from "form-data";

export const whatsappFlowsRouter = Router();

const GRAPH_VER = process.env.GRAPH_API_VERSION || "v18.0";
const TOKEN = process.env.WHATSAPP_ACCESS_TOKEN;
const WABA_ID = process.env.WHATSAPP_WABA_ID;

function graphUrl(path) {
    return `https://graph.facebook.com/${GRAPH_VER}${path}`;
}

function authHeaders() {
    if (!TOKEN) throw new Error("WHATSAPP_ACCESS_TOKEN missing");
    return { Authorization: `Bearer ${TOKEN}` };
}

// POST /integrations/whatsapp/flows/create
whatsappFlowsRouter.post("/integrations/whatsapp/flows/create", async (req, res, next) => {
    try {
        if (!WABA_ID) return res.status(500).json({ message: "WHATSAPP_WABA_ID missing" });
        
        const { name, categories } = req.body || {};
        if (!name) return res.status(400).json({ message: "name required" });
        
        const form = new FormData();
        form.append("name", name);
        // Meta expects categories as JSON string array
        form.append("categories", JSON.stringify(categories || ["SURVEY"]));
        
        const r = await fetch(graphUrl(`/${WABA_ID}/flows`), {
            method: "POST",
            headers: { ...authHeaders(), ...form.getHeaders() },
            body: form
        });
        
        const data = await r.text();
        if (!r.ok) return res.status(r.status).send(data);
        
        const parsed = JSON.parse(data);
        return res.json({ flowId: parsed.id });
    } catch (e) { next(e); }
});

// POST /integrations/whatsapp/flows/:flowId/assets
// body: { filename: "flow.json", content: <json> }
whatsappFlowsRouter.post("/integrations/whatsapp/flows/:flowId/assets", async (req, res, next) => {
    try {
        const { flowId } = req.params;
        const { filename, content } = req.body || {};
        if (!flowId) return res.status(400).json({ message: "flowId required" });
        if (!content) return res.status(400).json({ message: "content required" });
        
        const form = new FormData();
        form.append("name", filename || "flow.json");
        form.append("asset_type", "FLOW_JSON");
        
        // file must be uploaded as binary; we attach string as buffer
        const flowJsonString = typeof content === "string" ? content : JSON.stringify(content);
        form.append("file", Buffer.from(flowJsonString, "utf8"), {
            filename: filename || "flow.json",
            contentType: "application/json"
        });
        
        const r = await fetch(graphUrl(`/${flowId}/assets`), {
            method: "POST",
            headers: { ...authHeaders(), ...form.getHeaders() },
            body: form
        });
        
        const data = await r.text();
        if (!r.ok) return res.status(r.status).send(data);
        
        return res.status(200).send(data);
    } catch (e) { next(e); }
});

// POST /integrations/whatsapp/flows/:flowId/publish
whatsappFlowsRouter.post("/integrations/whatsapp/flows/:flowId/publish", async (req, res, next) => {
    try {
        const { flowId } = req.params;
        const r = await fetch(graphUrl(`/${flowId}/publish`), {
            method: "POST",
            headers: { ...authHeaders() }
        });

        const data = await r.text();
        if (!r.ok) return res.status(r.status).send(data);

        return res.status(200).send(data);
    } catch (e) { next(e); }
});
