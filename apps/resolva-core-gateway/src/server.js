import { createApp } from "./app.js";
import { env } from "./config/env.js";

const app = createApp();

app.listen(env.PORT, () => {
    console.log(`Resolva Gateway listening on http://localhost:${env.PORT}`);
});
