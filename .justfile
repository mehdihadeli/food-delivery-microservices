# justfile for food-delivery-microservices

# Variables
dotnet := "dotnet"

# --- Setup & Tools ---
prepare:  # Install Husky and .NET tools
    husky install
    {{dotnet}} tool restore

install-dev-cert:  # Install dev cert (Bash)
    curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v vs2019 -l ~/vsdbg

upgrade-packages:  # Upgrade .NET packages
    {{dotnet}} outdated -u

# --- Dry-Run Checks (for CI/pre-commit) ---
check-format:  # Check C# formatting (CSharpier)
    {{dotnet}} csharpier {{PROJECT_PATH}} --check

check-style:  # Check C# style rules (no fixes)
    find {{PROJECT_PATH}} -name "*.csproj" -exec {{dotnet}} format style {} --verify-no-changes --severity error --verbosity diagnostic \;

check-analyzers:  # Check C# analyzer rules (no fixes)
    find {{PROJECT_PATH}} -name "*.csproj" -exec {{dotnet}} format analyzers {} --verify-no-changes --severity error --verbosity diagnostic \;

# --- Auto-Fix Commands (with Git staging) ---
fix-format:  # Fix formatting with CSharpier and stage changes
    {{dotnet}} csharpier .
    git add -A .

fix-style:  # Fix style rules (error level) and stage changes
    {{dotnet}} format style --severity error --verbosity diagnostic
    git add -A .

fix-style-warn:  # Fix style rules (warn level)
    {{dotnet}} format style --severity warn --verbosity diagnostic

fix-style-info:  # Fix style rules (info level)
    {{dotnet}} format style --severity info --verbosity diagnostic

fix-analyzers:  # Fix analyzer rules (error level) and stage changes
    {{dotnet}} format analyzers --severity error --verbosity diagnostic
    git add -A .

fix-analyzers-warn:  # Fix analyzer rules (warn level)
    {{dotnet}} format analyzers --severity warn --verbosity diagnostic

fix-analyzers-info:  # Fix analyzer rules (info level)
    {{dotnet}} format analyzers --severity info --verbosity diagnostic

check-all: check-format check-style check-analyzers  # Run all validation checks

fix-all: fix-format fix-style fix-analyzers  # Run all fixes

pre-commit: fix-all check-all  # Run all fixes and checks

# Help
help:
    @echo "Available commands:"
    @echo "  just prepare            Install tools"
    @echo "  just check-all          Run all checks"
    @echo "  just pre-commit        Run all fixes"
    @echo "  npm run fix-format     Fix formatting"
    @echo "  npm run check-style    Validate style"