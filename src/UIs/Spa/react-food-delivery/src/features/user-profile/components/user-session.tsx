import { useState, useEffect } from "react";
import { authService } from "@shared/services/auth-service";
import type { UserClaim } from "@shared/models/user-claim";
import { Spinner } from "@shared/components/spinner";

export const UserSession = () => {
  const [userSessionInfo, setUserSessionInfo] = useState<UserClaim[] | null>(
    []
  );
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchUserSessionInfo();
  }, []);

  const fetchUserSessionInfo = async () => {
    try {
      setLoading(true);
      setError(null);
      const claims = await authService.getUserClaims();
      setUserSessionInfo(claims);
    } catch (err) {
      console.error("Failed to fetch user session:", err);
      setError("Failed to load user session. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const renderUserSessionTable = () => (
    <div className="overflow-x-auto">
      <table
        className="min-w-full divide-y divide-gray-200"
        aria-labelledby="tableLabel"
      >
        <thead className="bg-gray-50">
          <tr>
            <th
              scope="col"
              className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
            >
              Claim Type
            </th>
            <th
              scope="col"
              className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
            >
              Claim Value
            </th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {userSessionInfo &&
            userSessionInfo.map((claim) => (
              <tr key={claim.type}>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                  {claim.type}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 break-all">
                  {claim.value}
                </td>
              </tr>
            ))}
        </tbody>
      </table>
    </div>
  );

  const renderContent = () => {
    if (loading) {
      return <Spinner className="my-8" />;
    }

    if (error) {
      return (
        <div className="bg-red-50 border-l-4 border-red-400 p-4 my-4">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg
                className="h-5 w-5 text-red-400"
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 20 20"
                fill="currentColor"
              >
                <path
                  fillRule="evenodd"
                  d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                  clipRule="evenodd"
                />
              </svg>
            </div>
            <div className="ml-3">
              <p className="text-sm text-red-700">{error}</p>
            </div>
          </div>
        </div>
      );
    }

    return renderUserSessionTable();
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-8">
        <h1 id="tableLabel" className="text-2xl font-bold text-gray-900">
          User Session
        </h1>
        <p className="mt-2 text-gray-600">
          This page shows the current user's session information.
        </p>
      </div>

      {renderContent()}

      <div className="mt-6">
        <button
          onClick={fetchUserSessionInfo}
          disabled={loading}
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {loading ? "Refreshing..." : "Refresh Data"}
        </button>
      </div>
    </div>
  );
};
