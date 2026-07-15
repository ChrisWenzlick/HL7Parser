# Releasing HL7Parser

This document describes how to cut a release of the `HL7Parser` NuGet package. Publishing is automated by [`.github/workflows/publish.yml`](../.github/workflows/publish.yml), triggered by pushing a version tag.

---

## Prerequisite: NuGet Trusted Publishing

The publish workflow authenticates to nuget.org via [Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing) — OIDC-issued, short-lived (1-hour) credentials exchanged for each run. There is no long-lived API key stored in the repository.

This requires two things configured by a repository admin before the first release:

1. **A Trusted Publishing policy on nuget.org**, added under the publishing account's nuget.org profile (**Username → Trusted Publishing → Add policy**) with these exact values:
   - **Repository Owner:** `ChrisWenzlick`
   - **Repository:** `HL7Parser`
   - **Workflow File:** `publish.yml` (file name only, not the `.github/workflows/` path)
   - **Environment:** leave empty — the workflow does not use a GitHub Actions `environment:`
2. **A `NUGET_USER` repository secret** containing the nuget.org account's profile *username* (not an email address, not hardcoded in the workflow) — the account the Trusted Publishing policy above was configured under. Add it via **Settings → Secrets and variables → Actions → New repository secret**, or `gh secret set NUGET_USER`.

At publish time, the workflow requests a GitHub OIDC token (`permissions: id-token: write`) and exchanges it via the `NuGet/login@v1` action for a temporary nuget.org API key scoped to that push, using the `NUGET_USER` secret to identify the account. If `NUGET_USER` is missing, the workflow fails clearly with a `NUGET_USER secret is not set` error rather than silently skipping the publish step. If the Trusted Publishing policy itself isn't configured (or doesn't match exactly), the `NuGet/login@v1` step fails instead.

---

## Release Runbook

1. Confirm all intended pull requests for this release have been merged to `main`.
2. In `CHANGELOG.md`, rename `## [Unreleased]` to `## [{version}] - {YYYY-MM-DD}` (e.g. `## [1.0.0] - 2026-08-01`), and add a fresh empty `## [Unreleased]` section above it, per the file's own documented convention.
3. Open a pull request with that `CHANGELOG.md` change and merge it to `main`. `main` is a protected branch — a direct push is rejected, so this step cannot be a plain `git push`, even for a maintainer.
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
