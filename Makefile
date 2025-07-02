# Makefile for food-delivery-microservices

## Variables
DOTNET := dotnet

## --- Setup & Tools ---
.PHONY: prepare
prepare: ## Install Husky and .NET tools
	husky install
	$(DOTNET) tool restore

.PHONY: install-dev-cert
install-dev-cert: ## Install dev cert (Bash)
	curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v vs2019 -l ~/vsdbg

.PHONY: upgrade-packages
upgrade-packages: ## Upgrade .NET packages
	$(DOTNET) outdated -u

## --- Dry-Run Checks (for CI/pre-commit) ---
.PHONY: check-format
check-format: ## Check C# formatting (CSharpier)
	$(DOTNET) csharpier $(PROJECT_PATH) --check

.PHONY: check-style
check-style: ## Check C# style rules (no fixes)
	find $(PROJECT_PATH) -name "*.csproj" -exec $(DOTNET) format style {} --verify-no-changes --severity error --verbosity diagnostic \;

.PHONY: check-analyzers
check-analyzers: ## Check C# analyzer rules (no fixes)
	find $(PROJECT_PATH) -name "*.csproj" -exec $(DOTNET) format analyzers {} --verify-no-changes --severity error --verbosity diagnostic \;

## --- Auto-Fix Commands (with Git staging) ---
.PHONY: fix-format
fix-format: ## Fix formatting with CSharpier and stage changes
	$(DOTNET) csharpier .
	git add -A .

.PHONY: fix-style
fix-style: ## Fix style rules (error level) and stage changes
	$(DOTNET) format style --severity error --verbosity diagnostic
	git add -A .

.PHONY: fix-style-warn
fix-style-warn: ## Fix style rules (warn level)
	$(DOTNET) format style --severity warn --verbosity diagnostic

.PHONY: fix-style-info
fix-style-info: ## Fix style rules (info level)
	$(DOTNET) format style --severity info --verbosity diagnostic

.PHONY: fix-analyzers
fix-analyzers: ## Fix analyzer rules (error level) and stage changes
	$(DOTNET) format analyzers --severity error --verbosity diagnostic
	git add -A .

.PHONY: fix-analyzers-warn
fix-analyzers-warn: ## Fix analyzer rules (warn level)
	$(DOTNET) format analyzers --severity warn --verbosity diagnostic

.PHONY: fix-analyzers-info
fix-analyzers-info: ## Fix analyzer rules (info level)
	$(DOTNET) format analyzers --severity info --verbosity diagnostic

.PHONY: check-all
check-all: check-format check-style check-analyzers ## Run all validation checks

.PHONY: fix-all
fix-all: fix-format fix-style fix-analyzers # Run all fixes

.PHONY: pre-commit
pre-commit: fix-all check-all

## Help
.PHONY: help
help:
	@echo "Available commands:"
	@echo "  make prepare            Install tools"
	@echo "  make check-all          Run all checks"
	@echo "  make pre-commit        Run all fixes"
	@echo "  npm run fix-format     Fix formatting"
	@echo "  npm run check-style    Validate style"