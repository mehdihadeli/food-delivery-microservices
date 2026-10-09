#!/usr/bin/env bash

set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$script_dir/.."

nbgv="${NBGV:-nbgv}"
output_file="${GITHUB_OUTPUT:?GITHUB_OUTPUT must be set by GitHub Actions}"

nbgv_version="$($nbgv get-version -v SemVer2)"
version="$nbgv_version"
assembly_version="$($nbgv get-version -v AssemblyVersion)"
commit="$(git rev-parse --short HEAD)"

if [[ "${GITHUB_REF}" =~ ^refs/tags/v[0-9]+\.[0-9]+\.[0-9]+(-rc\.[0-9]+)?$ ]]; then
  version="${GITHUB_REF_NAME#v}"
  nbgv_tag_version="${nbgv_version%%.g*}"
  if [[ "${version}" != "${nbgv_tag_version}" ]]; then
    echo "Release tag ${version} does not match NBGV version ${nbgv_tag_version} (calculated: ${nbgv_version})." >&2
    exit 1
  fi

  if [[ "${version}" == *-rc.* ]]; then
    date_part="$(date -u +%y%j)"
    revision="${GITHUB_RUN_NUMBER}"
    version="${version}.${date_part}.${revision}"
    environment="staging"
  else
    environment="production"
  fi
elif [[ "${GITHUB_REF}" == "refs/heads/main" ]]; then
  if [[ "${nbgv_version}" == *-preview.* ]]; then
    if [[ "${nbgv_version}" =~ -preview\.0(\.|$) ]]; then
      environment="none"
    else
      if [[ ! "${nbgv_version}" =~ ^([0-9]+\.[0-9]+\.[0-9]+-preview\.[0-9]+) ]]; then
        echo "Unable to parse NBGV preview version: ${nbgv_version}." >&2
        exit 1
      fi

      date_part="$(date -u +%y%j)"
      revision="${GITHUB_RUN_NUMBER}"
      version="${BASH_REMATCH[1]}.${date_part}.${revision}"
      environment="dev"
    fi
  else
    environment="none"
  fi
else
  echo "Unsupported release ref: ${GITHUB_REF_NAME}." >&2
  exit 1
fi

informational_version="${version}+${commit}"
docker_tag="${version/+/-}"

{
  echo "version=${version}"
  echo "assembly_version=${assembly_version}"
  echo "informational_version=${informational_version}"
  echo "environment=${environment}"
  echo "docker_tag=${docker_tag}"
  echo "commit=${commit}"
} >> "$output_file"

echo "Calculated SemVer2 version: ${version}"
echo "Target environment: ${environment}"