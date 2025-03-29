#!/usr/bin/env bash

set -eax

# Run the setup_fonts.sh script
./setup-fonts.sh

# Trust the development certificates
dotnet dev-certs https --trust

# Install .NET Aspire project templates
dotnet new install Aspire.ProjectTemplates
