#!/usr/bin/env sh

set -eu

repo_root="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
results_dir="$repo_root/TestResults"

cd "$repo_root"
mkdir -p "$results_dir"

dotnet test food-delivery-microservices.slnx \
  --configuration Release \
  --no-build \
  --results-directory "$results_dir" \
  -- \
  --coverage \
  --coverage-output-format cobertura \
  --report-trx