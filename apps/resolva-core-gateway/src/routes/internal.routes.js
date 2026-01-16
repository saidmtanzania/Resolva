import { Router } from "express";
import { requireInternalHmac } from "../middleware/hmacAuth.js";
import { forwardToCore } from "../services/coreApi.service.js";

export const internalRouter = Router();

/**
 * POST mappings (existing)
 */
const MAP_POST = {
  "surveys-generated": "/internal/surveys/generated",
  "surveys-answer": "/internal/surveys/answer",
  "surveys-complete": "/internal/surveys/complete",
};

/**
 * GET mappings (new)
 * Note: some GET routes need query params; we’ll append req.originalUrl query.
 */
const MAP_GET = {
  "sessions-active": "/internal/sessions/active",
  // Add more GET keys here if needed
};

function buildForwardHeaders(req) {
  return {
    "content-type": "application/json",
    "x-resolva-timestamp": req.header("x-resolva-timestamp"),
    "x-resolva-signature": req.header("x-resolva-signature"),
  };
}

/**
 * Existing POST forwarder
 */
internalRouter.post(
  "/integrations/internal/:key",
  requireInternalHmac,
  async (req, res, next) => {
    try {
      const key = req.params.key;
      const targetPath = MAP_POST[key];
      if (!targetPath)
        return res
          .status(404)
          .json({ message: "Unknown internal POST route key" });

      const { status, contentType, bodyText } = await forwardToCore({
        path: targetPath,
        method: "POST",
        headers: buildForwardHeaders(req),
        rawBody: req.rawBody,
      });

      if (contentType) res.setHeader("content-type", contentType);
      return res.status(status).send(bodyText);
    } catch (e) {
      next(e);
    }
  }
);

/**
 * NEW: GET forwarder
 * Example:
 *  GET /integrations/internal/sessions-active?phone=2557...
 *  -> GET /internal/sessions/active?phone=2557...
 */
internalRouter.get(
  "/integrations/internal/:key",
  requireInternalHmac,
  async (req, res, next) => {
    try {
      const key = req.params.key;
      const targetBase = MAP_GET[key];
      if (!targetBase)
        return res
          .status(404)
          .json({ message: "Unknown internal GET route key" });

      // Preserve query string
      const query = req.originalUrl.includes("?")
        ? req.originalUrl.substring(req.originalUrl.indexOf("?"))
        : "";
      const targetPath = `${targetBase}${query}`;

      const { status, contentType, bodyText } = await forwardToCore({
        path: targetPath,
        method: "GET",
        headers: {
          "x-resolva-timestamp": req.header("x-resolva-timestamp"),
          "x-resolva-signature": req.header("x-resolva-signature"),
        },
        rawBody: null,
      });

      if (contentType) res.setHeader("content-type", contentType);
      return res.status(status).send(bodyText);
    } catch (e) {
      next(e);
    }
  }
);

/**
 * OPTIONAL (recommended): direct pass-through routes for session details
 * These are nicer than key-based routing because they preserve IDs cleanly.
 *
 * GET /integrations/internal/sessions/:id -> /internal/sessions/:id
 * GET /integrations/internal/sessions/:id/responses -> /internal/sessions/:id/responses
 */
internalRouter.get(
  "/integrations/internal/sessions/:id",
  requireInternalHmac,
  async (req, res, next) => {
    try {
      const targetPath = `/internal/sessions/${encodeURIComponent(
        req.params.id
      )}`;

      const { status, contentType, bodyText } = await forwardToCore({
        path: targetPath,
        method: "GET",
        headers: {
          "x-resolva-timestamp": req.header("x-resolva-timestamp"),
          "x-resolva-signature": req.header("x-resolva-signature"),
        },
        rawBody: null,
      });

      if (contentType) res.setHeader("content-type", contentType);
      return res.status(status).send(bodyText);
    } catch (e) {
      next(e);
    }
  }
);

internalRouter.get(
  "/integrations/internal/sessions/:id/responses",
  requireInternalHmac,
  async (req, res, next) => {
    try {
      const targetPath = `/internal/sessions/${encodeURIComponent(
        req.params.id
      )}/responses`;

      const { status, contentType, bodyText } = await forwardToCore({
        path: targetPath,
        method: "GET",
        headers: {
          "x-resolva-timestamp": req.header("x-resolva-timestamp"),
          "x-resolva-signature": req.header("x-resolva-signature"),
        },
        rawBody: null,
      });

      if (contentType) res.setHeader("content-type", contentType);
      return res.status(status).send(bodyText);
    } catch (e) {
      next(e);
    }
  }
);
