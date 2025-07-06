import { Outlet } from "react-router-dom";
import { NavMenu } from "./nav-menu";
import { ToastContainer } from "react-toastify";

export const Layout = () => {
  return (
    <div className="min-h-screen flex flex-col">
      <NavMenu />
      <main className="flex-grow container mx-auto p-4">
        <Outlet />
      </main>
      <ToastContainer
        position="top-right"
        autoClose={5000}
        hideProgressBar={false}
        newestOnTop={false}
        closeOnClick
        rtl={false}
        pauseOnFocusLoss
        draggable
        pauseOnHover
      />
    </div>
  );
};
