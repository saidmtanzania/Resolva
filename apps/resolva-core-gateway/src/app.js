import express from "express";
import { routes } from "./routes/index.js";
import { errorHandler } from "./middleware/errorHandler.js";
import { requestLogger } from "./middleware/requestLogger.js";
import { rawBodyCapture } from "./middleware/rawBody.js";

export function createApp() {
    const app = express();

    app.use(rawBodyCapture);
    app.use(requestLogger);
    app.use(routes);
    app.use(errorHandler);
    
    return app;
}
