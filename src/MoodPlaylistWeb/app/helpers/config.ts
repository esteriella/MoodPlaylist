// I am not sure if this file is necessary considering next js has its own next.config.ts file
import { ConfigModel } from "../models/config.model";
// do we need to import dotenv as this is a next js app and it next-env.d.ts already has the types for process.env
// seems it is not initialized at app startup but reinitialized on every call, I may be wrong though
export const config: ConfigModel = {
    apiBaseUrl: process.env.NEXT_PUBLIC_API_BASE_URL || "https://localhost:44302",
};  