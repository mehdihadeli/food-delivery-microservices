#!/usr/bin/env bash

set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$script_dir/.."

usage() {
  cat <<'EOF'
Usage:
  ./scripts/release-version.sh prepare-train <major.minor.patch>
  ./scripts/release-version.sh prepare-rc <major.minor.patch>
  ./scripts/release-version.sh prepare-stable <major.minor.patch>
  ./scripts/release-version.sh tag

Examples:
  ./scripts/release-version.sh prepare-train 1.1.0
  ./scripts/release-version.sh prepare-rc 1.0.0
  ./scripts/release-version.sh prepare-stable 1.0.0
  ./scripts/release-version.sh tag
EOF
}

if [[ $# -lt 1 ]]; then
  usage
  exit 1
fi

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet is required. Restore the repository tools with: dotnet tool restore" >&2
  exit 1
fi

clear_version_height_offset() {
  local temporary_file

  temporary_file="$(mktemp)"
  awk '
    /"versionHeightOffset":/ { next }
    /"versionHeightOffsetAppliesTo":/ { next }
    { print }
  ' version.json > "$temporary_file"
  mv "$temporary_file" version.json
}

prepare_train() {
  local base_version="$1"

  dotnet nbgv set-version "${base_version}-preview.{height}"
  clear_version_height_offset
  echo "Updated version.json to ${base_version}-preview.{height}. Commit and push this release-train change."
}

prepare_rc() {
  local base_version="$1"

  dotnet nbgv set-version "${base_version}-rc.{height}"
  echo "Updated version.json to ${base_version}-rc.{height}. Commit and push this RC change."
}

prepare_stable() {
  local base_version="$1"

  dotnet nbgv set-version "$base_version"
  clear_version_height_offset
  echo "Updated version.json to ${base_version}. Commit and push this stable release change."
}

tag_release() {
  dotnet nbgv tag
  echo "Created the NBGV tag. Push it with: git push origin <tag-name>"
}

case "$1" in
  prepare-train)
    [[ $# -eq 2 ]] || { usage; exit 1; }
    prepare_train "$2"
    ;;
  prepare-rc)
    [[ $# -eq 2 ]] || { usage; exit 1; }
    prepare_rc "$2"
    ;;
  prepare-stable)
    [[ $# -eq 2 ]] || { usage; exit 1; }
    prepare_stable "$2"
    ;;
  tag)
    [[ $# -eq 1 ]] || { usage; exit 1; }
    tag_release
    ;;
  *)
    usage
    exit 1
    ;;
esac