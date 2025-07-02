import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";
import fs from "fs";
import child_process from "child_process";
import { env } from "process";
import tsconfigPaths from "vite-tsconfig-paths";

const baseFolder =
  env.APPDATA !== undefined && env.APPDATA !== ""
    ? path.join(env.APPDATA, "ASP.NET", "https")
    : path.join(env.HOME || "", ".aspnet", "https");

if (!fs.existsSync(baseFolder)) {
  fs.mkdirSync(baseFolder, { recursive: true }); // `recursive: true` creates parent folders if needed
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
    { stdio: "inherit" }
  );

  if (result.status !== 0) {
    throw new Error("Could not create certificate.");
  }
}

// const target = env.ASPNETCORE_HTTPS_PORT
//   ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
//   : env.ASPNETCORE_URLS
//   ? env.ASPNETCORE_URLS.split(";")[0]
//   : "https://localhost:5001";

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
    port: 5173,
    https: {
      key: fs.readFileSync(keyFilePath),
      cert: fs.readFileSync(certFilePath),
    },

    // // these are the proxy routes that will be forwarded to your **BFF**, we can use yarp gateway as well
    // proxy: {
    //   "/bff": {
    //     target,
    //     secure: false,
    //   },
    //   "/signin-oidc": {
    //     target,
    //     secure: false,
    //   },
    //   "/signout-callback-oidc": {
    //     target,
    //     secure: false,
    //   },
    //   "/api": {
    //     target,
    //     secure: false,
    //   },
    // },
  },
});
