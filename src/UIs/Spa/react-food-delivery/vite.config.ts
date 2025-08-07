import {defineConfig} from "vite";
import react from "@vitejs/plugin-react";
import path from "path";
import fs from "fs";
import child_process from "child_process";
import {env} from "process";
import tsconfigPaths from "vite-tsconfig-paths";

const baseFolder =
    env.APPDATA !== undefined && env.APPDATA !== ""
        ? path.join(env.APPDATA, "ASP.NET", "https")
        : path.join(env.HOME || "", ".aspnet", "https");

if (!fs.existsSync(baseFolder)) {
    fs.mkdirSync(baseFolder, {recursive: true}); // `recursive: true` creates parent folders if needed
}

const certificateName = "food-delivery.client"; // or extract from args as before
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

// Generate certificate if missing
if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    const result = child_process.spawnSync(
        "dotnet",
        [
            "dev-certs",
            "https",
            "--export-path",
            certFilePath,
            "--format",
            "Pem",
            "--no-password",
        ],
        {stdio: "inherit"}
    );

    if (result.status !== 0) {
        throw new Error("Could not create certificate.");
    }
}

 let gatewayTarget= process.env.services__gateway__https__0 || process.env.services__gateway__http__0 || env.VITE_GATEWAY_API_BASE_URL || "https://localhost:3001";
 let spaBffTarget= `${gatewayTarget}/spa-bff`

// https://github.com/dotnet/aspire-samples/blob/main/samples/AspireWithJavaScript/AspireJavaScript.Vite/vite.config.ts
// https://vitejs.dev/config/
export default defineConfig({
    plugins: [
        react(),
        // This will automatically use paths from your tsconfig.json
        tsconfigPaths(),
    ],
    css: {
        postcss: "./postcss.config.cjs",
    },
    server: {
        port: parseInt(env.VITE_PORT ?? "5173"),
        https: {
            key: fs.readFileSync(keyFilePath),
            cert: fs.readFileSync(certFilePath),
        },
        // https://github.com/DuendeSoftware/samples/blob/main/BFF/v3/React/react.client/vite.config.js
        proxy: {
            '^/gateway': {
               target: gatewayTarget,
                rewrite: (path) => path.replace(/^\/gateway/, ''),
                secure: false,
            },
            '^/bff': {
              target:  spaBffTarget,
                secure: false
            },
            '^/signin-oidc': {
                target:  spaBffTarget,
                secure: false
            },
            '^/signout-callback-oidc': {
                target: spaBffTarget,
                secure: false
            }
        }
    }
});
