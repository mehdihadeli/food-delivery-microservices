#!/usr/bin/env bash

set -eax

# Run the setup_fonts.sh script
./setup-fonts.sh

# Trust the development certificates
dotnet dev-certs https --trust

# Install .NET Aspire project templates
dotnet new install Aspire.ProjectTemplates

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
