import React from "react";
import ReactDOM from "react-dom/client";
import "./app/styles/globals.css";
import { BrowserRouter } from "react-router";
import App from "./app";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <BrowserRouter>
      <App />
    </BrowserRouter>
  </React.StrictMode>
);
