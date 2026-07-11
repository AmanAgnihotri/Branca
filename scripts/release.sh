#!/usr/bin/env bash
# Cut a release: tag main with v<version> and push the tag. The Release
# workflow then builds, tests, packs and publishes both packages to NuGet.
set -euo pipefail

script_dir=$(CDPATH= cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
repo_root=$(CDPATH= cd -- "$script_dir/.." && pwd)

cd "$repo_root"

if [[ $# -ne 1 ]]; then
  printf 'usage: scripts/release.sh <version>  e.g. scripts/release.sh 0.8.0\n' >&2
  exit 1
fi

version="${1#v}"

if [[ ! "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$ ]]; then
  printf 'error: %s is not a version like 0.8.0 or 1.0.0-rc.1\n' "$version" >&2
  exit 1
fi

tag="v$version"

if [[ "$(git branch --show-current)" != "main" ]]; then
  printf 'error: releases are tagged from main\n' >&2
  exit 1
fi

if [[ -n "$(git status --porcelain)" ]]; then
  printf 'error: the working tree is not clean; commit or stash first\n' >&2
  exit 1
fi

if git rev-parse -q --verify "refs/tags/$tag" >/dev/null; then
  printf 'error: tag %s already exists\n' "$tag" >&2
  exit 1
fi

git fetch origin --quiet

if [[ "$(git rev-parse HEAD)" != "$(git rev-parse origin/main)" ]]; then
  printf 'error: main is not in sync with origin/main; push or pull first\n' >&2
  exit 1
fi

printf 'Releasing %s from: %s\n' "$tag" "$(git log -1 --format='%h %s')"
read -r -p "Tag and push? [y/N] " answer

if [[ "$answer" != [yY] ]]; then
  printf 'aborted\n'
  exit 1
fi

git tag -m "Branca $version" "$tag"
git push origin "$tag"

printf 'Pushed %s.\n' "$tag"
printf 'Watch: https://github.com/AmanAgnihotri/Branca/actions/workflows/release.yml\n'
