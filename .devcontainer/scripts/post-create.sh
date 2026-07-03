#!/usr/bin/env bash

set -eax

# Run the setup_fonts.sh script
./setup-fonts.sh

# Trust the development certificates
dotnet dev-certs https --trust

# Install .NET Aspire project templates
dotnet new install Aspire.ProjectTemplates --force

# Install aspirate
# https://github.com/prom3theu5/aspirational-manifests
dotnet tool install -g aspirate --prerelease

# https://learn.microsoft.com/en-us/dotnet/aspire/whats-new/dotnet-aspire-9.4#-aspire-cli-is-generally-available
dotnet tool install -g Aspire.Cli

# https://xunit.net/docs/getting-started/v3/getting-started#install-the-net-sdk-templates
dotnet new install xunit.v3.templates

npm install -g @devcontainers/cli

# Update npm to latest version
echo "📦 Updating npm to latest version..."
if command -v npm >/dev/null 2>&1; then
    npm install -g npm@latest
fi

# Install Just task runner
echo "🔧 Installing Just task runner..."
if command -v npm >/dev/null 2>&1; then
    npm install -g rust-just
    echo "✅ Just installed via npm"
    
    # Verify Just installation
    if command -v just >/dev/null 2>&1; then
        echo "✅ Just version: $(just --version)"
    else
        echo "⚠️ Just installed but not found in PATH. You may need to restart your terminal."
    fi
else
    echo "❌ npm not available, skipping Just installation"
fi
