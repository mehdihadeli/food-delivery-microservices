module.exports = {
  rules: {
    "schema/required-fields": "error",
    "schema/valid-semver": "error",
    "schema/valid-email": "warn",
    "refs/owner-exists": "error",
    "refs/valid-version-range": "error",
    "best-practices/summary-required": "warn",
    "best-practices/owner-required": "error",
  },
  ignorePatterns: ["**/archived/**", "**/drafts/**"],
  overrides: [
    {
      files: ["**/experimental/**"],
      rules: { "best-practices/owner-required": "off" },
    },
  ],
};
