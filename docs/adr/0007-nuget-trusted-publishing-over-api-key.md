# ADR-0007: NuGet Trusted Publishing Over Long-Lived API Key

**Date:** 2026-07-14
**Status:** Accepted

---

## Context

The `publish.yml` GitHub Actions workflow (introduced to pack and push the `HL7Parser` NuGet package on a `v*` tag push) originally authenticated to nuget.org with a long-lived API key: a `NUGET_API_KEY` repository secret, generated once on nuget.org and pasted into GitHub Actions secrets, used unchanged for every future publish until manually rotated or revoked.

A long-lived API key stored as a repository secret is a durable credential with a broad blast radius if it ever leaks — via a compromised workflow run, a misconfigured third-party action, a logging mistake, or a workflow file edited on a branch that gains write access to secrets. It grants push rights to the `HL7Parser` package indefinitely, with no built-in expiry, and its compromise is only detectable if nuget.org's own anomaly detection or the maintainer's own auditing catches it.

nuget.org supports [Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing): an OIDC-based federation between a specific GitHub repository + workflow file and a nuget.org publishing policy. Instead of a stored secret, the workflow requests a short-lived GitHub-issued OIDC token at run time, exchanges it for a nuget.org API key valid for roughly one hour via the `NuGet/login@v1` action, and uses that temporary key for the push. No nuget.org credential is stored in the repository at all.

This is nuget.org's own recommended approach going forward, and this decision was made deliberately as part of the v1.0.0 release preparation — the pipeline had not yet been used for a real publish, making this the natural point to adopt the stronger mechanism rather than migrate later under time pressure.

## Decision

Replace `NUGET_API_KEY`-based authentication in `publish.yml` with NuGet Trusted Publishing. The workflow's `publish` job declares `permissions: id-token: write` (plus `contents: write`, needed for GitHub Release creation once an explicit `permissions:` block is present), requests an OIDC token via `NuGet/login@v1` immediately before the push step, and uses the resulting temporary key with `dotnet nuget push`. The `NUGET_API_KEY` secret and its associated fail-fast check are removed entirely — not retained as a fallback path.

A `NUGET_USER` repository secret (the nuget.org account's profile username, not an email address) is still required, to identify which account's Trusted Publishing policy to exchange against. This secret only identifies an account; it cannot itself be used to authenticate or push a package, so it does not carry the same risk profile as the API key it replaces.

## Reasoning

**Reduced blast radius.** A leaked OIDC-exchanged key is valid for about an hour and scoped to the run that requested it. A leaked long-lived API key is valid indefinitely until someone notices and manually revokes it. For a public, actively-installed package, the cost of a missed or delayed revocation is significant — arbitrary code could be pushed as a trusted update to every consumer.

**No stored secret to rotate, audit, or accidentally expose.** The `NUGET_API_KEY` secret required manual generation on nuget.org and manual entry into GitHub Actions secrets, with no reminder mechanism if it needed rotation. Trusted Publishing removes that operational burden and the associated risk of the secret being pasted somewhere it shouldn't (logs, a fork's workflow run, a debugging step) since it never exists as a long-lived value in the first place.

**Matches the platform's own recommended direction.** nuget.org documents Trusted Publishing as the preferred mechanism for CI-driven publishing as of this decision, with API keys positioned as the legacy path. Adopting it now, before the package has ever been live-published, avoids a future migration under the added complexity of an existing user base depending on the pipeline's current behavior.

**Alternative considered — keep `NUGET_API_KEY` as a fallback alongside Trusted Publishing.** Rejected: maintaining two authentication paths doubles the surface area to secure and test, and defeats much of Trusted Publishing's benefit if the long-lived key remains present as a secret regardless of whether it's the primary path. The user explicitly requested full replacement, not a fallback, for this reason.

**Alternative considered — do nothing, keep `NUGET_API_KEY`.** Rejected because this release is the first real publish the pipeline will ever perform; there is no existing automation depending on the current mechanism, making this the lowest-cost point at which to switch.

## Consequences

**Positive:**
- No long-lived nuget.org credential exists anywhere in the repository or its secrets at rest.
- A compromised workflow run or leaked log at worst exposes a credential valid for about an hour, scoped to that run.
- No manual key rotation process is needed going forward.
- Aligns with nuget.org's own recommended practice, reducing future migration pressure.

**Negative:**
- Adds an external dependency on the `NuGet/login@v1` action and nuget.org's Trusted Publishing infrastructure being available at publish time; an outage there blocks publishing even if nuget.org's push API itself is healthy.
- Requires a one-time, user-only setup step (the nuget.org Trusted Publishing policy) that cannot be automated or verified by CI ahead of time — a misconfigured policy only surfaces as a failure during an actual tag-triggered run.
- The `permissions:` block is now explicit and job-scoped; any future step requiring a permission beyond `contents: write` / `id-token: write` (e.g. `packages: write`) must be added deliberately, since omitting it silently drops that permission rather than inheriting a broader repository default.

**Revisit if:** nuget.org changes Trusted Publishing's token lifetime, scoping, or availability guarantees in a way that materially changes this tradeoff; or if a second publish target (a different CI provider, a different package registry) is added that doesn't support an equivalent OIDC federation, which would reintroduce the question of whether a stored-secret fallback is warranted for that target specifically.

## References

- [NuGet Trusted Publishing (Microsoft Learn)](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
- `.github/workflows/publish.yml` — the `NuGet login (OIDC → temporary API key)` and `Publish to NuGet` steps
- `docs/RELEASING.md` — the Trusted Publishing policy setup and `NUGET_USER` secret prerequisite
- Superseded: `NUGET_API_KEY`-based authentication, as originally shipped by the CI/CD pack-and-publish pipeline spec
