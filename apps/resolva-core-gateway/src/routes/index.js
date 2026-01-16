import { Router } from "express";
import { healthRouter } from "./health.routes.js";
import { internalRouter } from "./internal.routes.js";
import { whatsappRouter } from "./whatsapp.routes.js";
import { whatsappFlowsRouter } from "./whatsapp.flows.routes.js";

export const routes = Router();

routes.use(healthRouter);
routes.use(internalRouter);
routes.use(whatsappRouter);
routes.use(whatsappFlowsRouter);
