.PHONY: prepare install-dev-cert upgrade-packages check-format check-style check-analyzers \
        fix-format fix-style fix-style-warn fix-style-info fix-analyzers fix-analyzers-warn \
        fix-analyzers-info check-all fix-all help

PROJECT_PATH ?= "."

# Install Husky and .NET tools
prepare:
	husky
	dotnet tool restore

# Install dev cert (Bash)
install-dev-cert:
	curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v vs2019 -l ~/vsdbg

# Run the current .NET project
run: 
	dotnet run

# Run the input .NET project
run-project: 
	dotnet run --project $(PROJECT_PATH)

# Run .NET Aspire application
aspire-run: 
	aspire run

# Upgrade .NET packages
upgrade-packages:
	dotnet outdated -u

# Check C# formatting
check-format:
	dotnet csharpier $(PROJECT_PATH) --check

# Check C# style rules
check-style:
	dotnet format style $(PROJECT_PATH) --verify-no-changes --severity error --verbosity diagnostic

# Check C# analyzer rules
check-analyzers:
	dotnet format analyzers $(PROJECT_PATH) --verify-no-changes --severity error --verbosity diagnostic

# Fix formatting and stage changes
fix-format:
	dotnet csharpier $(PROJECT_PATH)

# Fix style rules for all projects (error level)
fix-style:
	dotnet format style $(PROJECT_PATH) --severity error --verbosity diagnostic

# Fix style rules (warn level)
fix-style-warn:
	dotnet format style $(PROJECT_PATH) --severity warn --verbosity diagnostic

# Fix style rules (info level)
fix-style-info:
	dotnet format style $(PROJECT_PATH) --severity info --verbosity diagnostic

# Fix analyzer rules (error level)
fix-analyzers:
	dotnet format analyzers $(PROJECT_PATH) --severity error --verbosity diagnostic

# Fix analyzer rules (warn level)
fix-analyzers-warn:
	dotnet format analyzers $(PROJECT_PATH) --severity warn --verbosity diagnostic

# Fix analyzer rules (info level)
fix-analyzers-info:
	dotnet format analyzers $(PROJECT_PATH) --severity info --verbosity diagnostic

# Run all validation checks
check-all: check-analyzers check-format check-style

# Run all fixes
fix-all: fix-analyzers fix-format fix-style

# Show help
help:
	@echo "Available targets:"
	@echo "  prepare            - Install Husky and .NET tools"
	@echo "  install-dev-cert   - Install dev cert (Bash)"
	@echo "  upgrade-packages   - Upgrade .NET packages"
	@echo "  check-format       - Check C# formatting"
	@echo "  check-style        - Check C# style rules"
	@echo "  check-analyzers    - Check C# analyzer rules"
	@echo "  fix-format         - Fix formatting and stage changes"
	@echo "  fix-style          - Fix style rules for all projects (error level)"
	@echo "  fix-style-warn     - Fix style rules (warn level)"
	@echo "  fix-style-info     - Fix style rules (info level)"
	@echo "  fix-analyzers      - Fix analyzer rules (error level)"
	@echo "  fix-analyzers-warn - Fix analyzer rules (warn level)"
	@echo "  fix-analyzers-info - Fix analyzer rules (info level)"
	@echo "  check-all          - Run all validation checks"
	@echo "  fix-all            - Run all fixes"
	@echo ""
	@echo "Variables:"
	@echo "  PROJECT_PATH - Path to project (default: \".\")"