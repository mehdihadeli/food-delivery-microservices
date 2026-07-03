#!/usr/bin/env bash

set -euo pipefail

repo_root="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"

solution_path="${TEST_SOLUTION_PATH:-food-delivery-microservices.slnx}"
configuration="${TEST_CONFIGURATION:-Release}"
results_directory="${TEST_RESULTS_DIRECTORY:-TestResults}"
coverage_output_format="${TEST_COVERAGE_OUTPUT_FORMAT:-cobertura}"
verbosity="${TEST_VERBOSITY:-minimal}"
no_build="${TEST_NO_BUILD:-true}"

dotnet_args=(
  test
  --solution "$solution_path"
  --configuration "$configuration"
  --results-directory "$results_directory"
  --report-trx
  --coverage
  --coverage-output-format "$coverage_output_format"
  --verbosity "$verbosity"
)

if [[ "$no_build" == "true" ]]; then
  dotnet_args+=(--no-build)
fi

cd "$repo_root"
mkdir -p "$results_directory"

dotnet "${dotnet_args[@]}"