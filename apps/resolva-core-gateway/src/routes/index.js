import { Router } from "express";
import { healthRouter } from "./health.routes.js";
import { internalRouter } from "./internal.routes.js";

export const routes = Router();

routes.use(healthRouter);
routes.use(internalRouter);
