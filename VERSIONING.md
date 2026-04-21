# Versioning Strategy

This repository follows Semantic Versioning (SemVer): `MAJOR.MINOR.PATCH`.

## Rules

1. `MAJOR`: increment for breaking API changes (signature removals/renames, behavior changes that require consumer code changes).
2. `MINOR`: increment for backward-compatible additions (new modules, new methods, optional parameters, new capabilities).
3. `PATCH`: increment for backward-compatible fixes only (bug fixes, docs/source packaging fixes, non-breaking internal improvements).

## Package Cohesion

1. All publishable packages in this repository are versioned together.
2. A release tag `vX.Y.Z` maps to package version `X.Y.Z` for every NuGet package produced from this repo.
3. If only one module changes, all packages still receive the same version to keep the SDK surface coherent.

## Pre-release

1. Pre-release builds use SemVer prerelease suffixes, for example: `1.4.0-preview.1`.
2. Stable release is published only when the prerelease suffix is removed.

## Branch and Release Flow

1. Development occurs on regular feature branches.
2. Release is triggered by creating a Git tag in the form `v*.*.*`.
3. CI packs and publishes NuGet artifacts from that tag.

## Required Metadata Before Public Release

1. Replace `https://TODO-REPLACE.invalid/abacus-dotnet-sdk` in `Directory.Build.props` with the real repository URL.
2. Keep `LICENSE` and `README.md` included in every package.
3. Ensure SourceLink is valid for the target Git hosting provider (currently configured for GitHub).
