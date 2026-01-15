import "dotenv/config";

function requireEnv(name) {
    const v = process.env[name];
    if (!v) throw new Error(`Missing required env var: ${name}`);
    return v;
}

export const env = {
    PORT: Number(process.env.PORT || 3005),
    NODE_ENV: process.env.NODE_ENV || "development",

    CORE_API_BASE_URL: requireEnv("CORE_API_BASE_URL"),
    RESOLVA_INTERNAL_SECRET: requireEnv("RESOLVA_INTERNAL_SECRET")
};
