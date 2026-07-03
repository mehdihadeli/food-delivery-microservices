#!/usr/bin/env sh

set -eu

repo_root="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"

if ! command -v docker >/dev/null 2>&1; then
  echo "Docker is required to lint Dockerfiles with hadolint."
  exit 1
fi

cd "$repo_root"

find . \
  \( -path './.git' -o -path './bin' -o -path './obj' -o -path './node_modules' \) -prune -o \
  \( -name 'Dockerfile' -o -name 'Dockerfile.*' \) -type f -print |
while IFS= read -r dockerfile; do
  echo "Linting $dockerfile"
  docker run --rm -i hadolint/hadolint < "$dockerfile"
done