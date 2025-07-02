import { authService } from "@shared/services/auth-service";
import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faUser } from "@fortawesome/free-solid-svg-icons";

export const NavMenu = () => {
  const [isCollapsed, setIsCollapsed] = useState(true);
  const [authState, setAuthState] = useState(authService.currentState);
  const [username, setUsername] = useState<string | null>(null);

  useEffect(() => {
    // Subscribe to auth state changes
    const unsubscribe = authService.subscribe(async (newState) => {
      setAuthState(newState);
    });

    (async () => {
      const claims = await authService.getUserClaims();
      if (claims) {
        const nameClaim = claims.find((claim) => claim.type === "name");
        setUsername(nameClaim?.value || null);
      }
    })();

    return unsubscribe;
  }, []);

  const toggleNavbar = () => setIsCollapsed(!isCollapsed);

  const handleLogout = () => {
    authService.logout("/");
  };

  const handleLogin = () => {
    authService.login(window.location.pathname);
  };

  return (
    <header className="bg-white shadow-sm mb-6 border-b border-gray-100">
      <nav className="container mx-auto px-4 py-4">
        <div className="flex justify-between items-center">
          <Link
            to="/"
            className="text-2xl font-bold text-indigo-600 hover:text-indigo-700 transition-colors"
          >
            Food Delivery
          </Link>

          <div
            className={`${
              isCollapsed ? "hidden" : "block"
            } md:flex md:items-center md:space-x-6`}
          >
            <div className="flex flex-col md:flex-row md:items-center space-y-2 md:space-y-0">
              <Link
                to="/"
                className="px-4 py-2 text-gray-700 hover:text-indigo-600 rounded-lg text-sm font-medium transition-colors"
              >
                Home
              </Link>

              {/* Products Navigation */}
              {authState.isAuthenticated && (
                <div className="flex items-center">
                  <Link
                    to="/products"
                    className="px-4 py-2 text-gray-700 hover:text-indigo-600 rounded-lg text-sm font-medium transition-colors flex items-center"
                  >
                    Products
                  </Link>
                </div>
              )}

              {/* User Session Link */}
              {authState.isAuthenticated && (
                <Link
                  to="/user-session"
                  className="px-4 py-2 text-gray-700 hover:text-indigo-600 rounded-lg text-sm font-medium transition-colors"
                >
                  Account
                </Link>
              )}

              {/* Separator */}
              <div className="hidden md:block h-6 w-px bg-gray-200 mx-2"></div>

              {/* User Actions */}
              {authState.isAuthenticated ? (
                <div className="flex items-center space-x-4">
                  <div className="flex items-center">
                    <FontAwesomeIcon
                      icon={faUser}
                      className="w-4 h-4 text-gray-500 mr-2"
                    />
                    {username && (
                      <span className="text-gray-700 font-medium">
                        {username}
                      </span>
                    )}
                  </div>
                  <button
                    onClick={handleLogout}
                    className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-medium transition-colors"
                  >
                    Logout
                  </button>
                </div>
              ) : (
                <button
                  onClick={handleLogin}
                  className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-medium transition-colors"
                >
                  Login
                </button>
              )}
            </div>
          </div>
        </div>
      </nav>
    </header>
  );
};
