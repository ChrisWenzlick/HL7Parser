# Releasing HL7Parser

This document describes how to cut a release of the `HL7Parser` NuGet package. Publishing is automated by [`.github/workflows/publish.yml`](../.github/workflows/publish.yml), triggered by pushing a version tag.

---

## Prerequisite: `NUGET_API_KEY` Secret

The publish workflow pushes the package to nuget.org using an API key stored in the repository's GitHub Actions secrets as `NUGET_API_KEY`.

This must be configured by a repository admin before the first release:

1. Generate an API key on nuget.org scoped to the `HL7Parser` package (or to "push new packages and package versions" if the package doesn't exist yet).
2. Add it to the repository: **Settings → Secrets and variables → Actions → New repository secret**, named `NUGET_API_KEY` (or via `gh secret set NUGET_API_KEY`).

If this secret is missing, the workflow fails clearly with an `NUGET_API_KEY secret is not set` error rather than silently skipping the publish step.

---

## Release Runbook

1. Confirm all intended pull requests for this release have been merged to `main`.
2. In `CHANGELOG.md`, rename `## [Unreleased]` to `## [{version}] - {YYYY-MM-DD}` (e.g. `## [1.0.0] - 2026-08-01`), and add a fresh empty `## [Unreleased]` section above it, per the file's own documented convention.
3. Commit that `CHANGELOG.md` change to `main`.
4. Create and push a version tag matching the version used in the CHANGELOG heading:
   ```
   git tag v1.0.0
   git push origin v1.0.0
   ```
5. The `Publish` workflow picks up from there: it builds and tests the solution (Release configuration, net8.0/net9.0/net10.0), packs `HL7Parser.{version}.nupkg` and `.snupkg` with the version derived from the tag, pushes both to nuget.org, and creates a GitHub Release using the tag as the title and the matching `CHANGELOG.md` section as the release notes.

If step 2 is skipped or the version doesn't match, the release-notes extraction step fails clearly instead of publishing a release with an empty body.

---

## Dry Run

To validate the pipeline (build, test, pack, and version derivation) without publishing anything, trigger the workflow manually via **Actions → Publish → Run workflow** (`workflow_dispatch`). A manual run is not a tag push, so it uses a placeholder version and skips the NuGet push and GitHub Release steps.

---

## Versioning

Versioning is git-tag-driven: the package version is derived directly from the pushed tag (stripping the leading `v`), not from an automated versioning tool or a manually maintained `<Version>` in source. There is no automated version bumping — the next tag's version is whatever the maintainer chooses to tag next.

Pre-release tags (e.g. `v1.0.0-rc.1`) are supported and publish to the same nuget.org feed; NuGet's own pre-release-version handling is sufficient to distinguish them.
