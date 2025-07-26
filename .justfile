prepare:  # Install Husky and .NET tools
    husky
    dotnet tool restore

aspire-run:  # Run .NET Aspire application
    aspire run 

run PROJECT_PATH=".": # Run the current project
    dotnet run 

run-project PROJECT_PATH:  # Run the input .NET project
    dotnet run --project {{PROJECT_PATH}}

install-dev-cert:  # Install dev cert (Bash)
    curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v vs2019 -l ~/vsdbg

upgrade-packages:  # Upgrade .NET packages
    dotnet outdated -u

check-format PROJECT_PATH=".":  # Check C# formatting
    dotnet csharpier {{PROJECT_PATH}} --check

check-style PROJECT_PATH=".":  # Check C# style rules
   dotnet format style {{PROJECT_PATH}} --verify-no-changes --severity error --verbosity diagnostic

check-analyzers PROJECT_PATH=".":  # Check C# analyzer rules
   dotnet format analyzers {{PROJECT_PATH}} --verify-no-changes --severity error --verbosity diagnostic

fix-format PROJECT_PATH=".":  # Fix formatting and stage changes
    dotnet csharpier {{PROJECT_PATH}}

fix-style PROJECT_PATH=".":  # Fix style rules for all projects (error level)
   dotnet format style {{PROJECT_PATH}} --severity error --verbosity diagnostic

fix-style-warn PROJECT_PATH=".":  # Fix style rules (warn level)
    dotnet format style {{PROJECT_PATH}} --severity warn --verbosity diagnostic

fix-style-info PROJECT_PATH=".":  # Fix style rules (info level)
    dotnet format style {{PROJECT_PATH}} --severity info --verbosity diagnostic

fix-analyzers PROJECT_PATH=".":  # Fix analyzer rules (error level)
    dotnet format analyzers {{PROJECT_PATH}} --severity error --verbosity diagnostic

fix-analyzers-warn PROJECT_PATH=".":  # Fix analyzer rules (warn level)
    dotnet format analyzers {{PROJECT_PATH}} --severity warn --verbosity diagnostic

fix-analyzers-info PROJECT_PATH=".":  # Fix analyzer rules (info level)
    dotnet format analyzers {{PROJECT_PATH}} --severity info --verbosity diagnostic

check-all:  # Run all validation checks
    @just check-analyzers
    @just check-format
    @just check-style

fix-all:  # Run all fixes
    @just fix-analyzers
    @just fix-format
    @just fix-style

help:
    @just --list --unsorted