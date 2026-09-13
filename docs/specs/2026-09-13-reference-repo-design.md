# Design: .NET service reference repo

Date: 2026-09-13
Status: approved in conversation, awaiting written review
Source project: `project/config-service` on branch `monomaniacal` of the Module 1 course repo

## 1. Goal

Turn the Module 1 config service into a public reference repo that other engineers clone, read,
and use as a `dotnet new` template for new .NET services. The repo demonstrates, and enforces
with tooling, a set of engineering practices chosen for engineer efficiency, security, and honest
test measurement. Every practice ships with a written rationale and a way to opt out or go deeper.

The repo does three jobs and the README routes readers to the right one:

1. Run and read the service (the exemplar).
2. Adopt a practice (the docs).
3. Start a new service (the template).

## 2. Principles

These are the rules every backlog item is checked against. They live in `docs/principles.md`.

- **YAGNI with a trigger.** Nothing is added because it might be useful later. Every item and
  every dependency names the condition that makes it necessary now. Gated items in the backlog
  state their trigger explicitly and stay unscheduled until it fires.
- **Microsoft-first tooling.** Prefer what ships with the .NET SDK, ASP.NET Core, or an official
  Microsoft package, and cite the Microsoft Learn page or official sample that backs it. Reach
  for a third-party tool only where Microsoft has no equivalent, and say so in the practice doc.
- **Rationale lives in docs, never in code.** Code carries no explanatory comments. The "why"
  for a practice is its practice page; the "why" for a decision is an ADR. The migration strips
  the "Task N report" comments the current source carries.
- **Measure tests by whether they catch bugs.** Mutation score is the quality signal. Coverage is
  checked only on changed lines in a pull request and never as a repo-wide number.
- **TDD by making it cheap and visible.** Tooling gives a fast red-green loop, assistant rules
  require the failing test first, and the PR template asks which test drove the change.
- **Correct use of environment variables.** Environment variables are the deployment-time
  override mechanism of the standard ASP.NET Core configuration pipeline. Settings bind to typed
  options validated at startup. One reference doc lists every setting. No secrets in committed
  files. No raw environment reads outside the composition root.

## 3. Decisions already made

| Decision | Choice |
| --- | --- |
| Location | New public repo owned by Daniel, seeded from a copy of `project/config-service`, fresh history. This course repo is untouched. Proposed name: `dotnet-service-reference`; change before bootstrap. |
| Audience | Team standard, personal reference, and teaching material at once. Rationale is mandatory; depth is dialled with tiers. |
| Template mechanism | A `dotnet new` template package, sourced from the repo's own tree. Generated services ship with the sample domain. |
| Test measurement | Stryker.NET mutation score plus diff coverage on changed lines. No repo-wide coverage threshold. |
| API scope | The service may evolve, but only when a change demonstrates a practice. Auth, DELETE, and pagination are unscheduled candidates with named triggers. |
| Backlog home | Stable, strategic content (principles, tiers, roadmap themes) in markdown. Volatile items as labelled GitHub issues seeded once from this spec. The roadmap links to label queries; it never lists items. |
| Repo visibility | Public now. Tooling is chosen so that going private later only loses CodeQL and GitHub secret scanning, both of which have in-repo backups. |
| Deploy target | Undecided. No Dockerfile now. DAST runs against `dotnet run` in CI. Container items are gated on the decision. |
| Developer OS | Windows, macOS, and Linux. Local tooling is dotnet-based only: tool manifest, Husky.Net hooks, no Makefile. CI scripts may use PowerShell 7 because GitHub runners ship it on every OS. |
| CI platform | GitHub Actions. |

## 4. Repo layout

```
README.md                      routes the three readers
AGENTS.md                      rules for AI assistants (TDD first, no comments, cite sources)
global.json                    SDK pin, Microsoft.Testing.Platform runner
Directory.Build.props          analyzers, warnings as errors, nullable, code style in build
Directory.Packages.props       central package versions (the only place a version appears)
.editorconfig                  formatting and analyzer severities
.config/dotnet-tools.json      dotnet-ef, husky, dotnet-stryker
.husky/                        pre-commit: format check, gitleaks
docker-compose.yml             Postgres only
src/ConfigService.Api/         the service
tests/ConfigService.Api.Tests/             unit tests (mocked repositories)
tests/ConfigService.Api.IntegrationTests/  Testcontainers Postgres, database-only concerns
tests/ConfigService.Architecture.Tests/    Tier 2, layering rules
docs/
  principles.md
  roadmap.md                   tiers, entry criteria, themes, label queries
  configuration.md             every setting: key, source, default, required, env var name
  dependencies.md              every package: why, what it replaces, tier
  practices/<name>.md          one page per practice, fixed six-part format
  decisions/NNNN-<slug>.md     ADRs
  openapi/v1.json              build-time generated, committed, drift-checked
template/
  .template.config/template.json
  Templates.csproj             packs src/, tests/, root config; sourceName ConfigService
.github/
  workflows/ci.yml             tier-named jobs
  workflows/codeql.yml
  workflows/mutation.yml
  workflows/template.yml       pack, install, instantiate, build, test
  ISSUE_TEMPLATE/backlog-item.yml
  PULL_REQUEST_TEMPLATE.md
  dependabot.yml
```

## 5. Tiers

Tiers are the plan levels. Each tier is a CI job group, an issue label, and a section of the
roadmap with an entry criterion.

| Tier | Label | Entry criterion | Required on PR |
| --- | --- | --- | --- |
| 0 Always on | `tier-0` | The repo exists. | Yes, branch protection |
| 1 Before first deploy | `tier-1` | Something will consume the service outside the dev loop. | Yes, once adopted |
| 2 Operating in production | `tier-2` | The service is deployed and someone is on call for it. | Per item |

Cross-cutting themes, used as labels: `theme:build`, `theme:testing`, `theme:security`,
`theme:api`, `theme:config`, `theme:governance`, `theme:template`, `theme:dx`.

## 6. Practice page format

Every file in `docs/practices/` has exactly these parts, in this order:

1. **What** – one paragraph.
2. **Why** – one paragraph, in terms of efficiency, security, or correctness.
3. **Source** – a Microsoft Learn link or official sample. If Microsoft has no equivalent, say
   "No Microsoft equivalent" and cite the tool's own documentation.
4. **Enforced by** – the analyzer, hook, or CI job, by name.
5. **Opt out** – the exact change that turns it off and what is lost.
6. **Go deeper** – the next-tier version, or "none".

## 7. Design by area

### 7.1 Build and lint (Tier 0)

- `Directory.Build.props`: `Nullable`, `ImplicitUsings`, `TreatWarningsAsErrors`,
  `AnalysisLevel=latest-recommended`, `EnforceCodeStyleInBuild`, `RestorePackagesWithLockFile`,
  `NuGetAuditMode=all`, `NuGetAuditLevel=low`. Warnings as errors turn audit warnings into
  restore failures, so vulnerable packages, transitive included, fail the build with no extra
  tool.
- `Directory.Packages.props` with `ManagePackageVersionsCentrally`. Pins EF Core once, which
  removes the explicit EF references and the version-unification warning (MSB3277) the
  integration-test project carries today.
- `.editorconfig` holds formatting and analyzer severities. `dotnet format --verify-no-changes`
  runs in the pre-commit hook and in CI. No StyleCop or Roslynator unless a gap is found and
  recorded in an ADR.
- `global.json` pins the SDK with `rollForward: latestPatch` and keeps the
  Microsoft.Testing.Platform runner opt-in.
- Tool manifest so `dotnet tool restore` on a fresh clone installs everything.
- Husky.Net pre-commit runs the format check on staged files and gitleaks protect.

### 7.2 Testing and TDD

- The unit and integration split stays as specified in the course spec. Integration tests cover
  only what needs the database.
- Coverage comes from `Microsoft.Testing.Extensions.CodeCoverage` in Cobertura format. The only
  coverage gate is diff-cover on changed lines in a PR. Migrations, the design-time factory, and
  `Program.cs` are excluded. The threshold is set from the first run and recorded in the
  practice page.
- Mutation testing uses Stryker.NET's Microsoft Testing Platform runner (`--test-runner mtp`,
  preview from 4.13). A reported issue with xunit.v3 and mutation activation means the first
  Tier 1 item is a spike: confirm mutants are killed, record the baseline score in an ADR. The
  break threshold starts just under the baseline and ratchets up. Runs on PRs that touch `src/`
  and weekly.
- TDD support: `dotnet watch test` documented as the inner loop, an AGENTS.md rule requiring the
  failing test first, a CONTRIBUTING.md workflow, and a PR template line asking which test drove
  the change. Test names follow `Method_Scenario_Expectation` by convention, not tooling.

### 7.3 Security

- CodeQL with `build-mode: none` on PRs and weekly.
- GitHub secret scanning with push protection as a repo setting. gitleaks in the pre-commit hook
  and as a CI job, so secret detection keeps working if the repo goes private.
- Dependency updates by Dependabot for NuGet and GitHub Actions, grouped weekly. Vulnerability
  detection is NuGet audit at build time. All Actions pinned to commit SHAs.
- DAST (Tier 1): ZAP API scan action, fed `docs/openapi/v1.json`, against the service started in
  CI with a Postgres service container. Fails on High alerts. A rules file records every tuned
  alert with its reason.
- Container scanning and SBOM are Tier 2 items gated on choosing container deployment.

### 7.4 OpenAPI and architecture in tooling

- `Microsoft.Extensions.ApiDescription.Server` generates `docs/openapi/v1.json` at build time.
  CI fails if the committed document differs from the build output.
- oasdiff on PRs fails on breaking changes to the committed document.
- Scalar interactive UI in Development only, as an opt-out Tier 0 item. Microsoft's OpenAPI
  documentation covers it as the recommended UI option for .NET 9 and later.
- Configuration: settings bind with `AddOptions<T>().BindConfiguration().ValidateDataAnnotations()
  .ValidateOnStart()`. The connection string moves out of `appsettings.Development.json` into
  user-secrets locally and environment variables elsewhere. `docs/configuration.md` is the single
  list.
- Architecture tests (Tier 2): endpoints never reference the DbContext, contracts never reference
  entities, no environment reads outside `Program.cs`. Library choice (ArchUnitNET or
  NetArchTest.Rules) is an ADR when the item is picked up.

### 7.5 Governance

- `docs/dependencies.md` has one row per package in `Directory.Packages.props`. A CI step fails
  when a package has no row.
- ADRs for non-obvious choices, starting with: central package management, the mutation runner,
  no Dockerfile yet, and each third-party tool.
- PR template asks: what breaks without this, which tier, which practice page changed, which
  test was written first.
- Issue template fields: why now, how verified, tier, theme.
- Branch protection on `main` requires every Tier 0 job. Tier 1 jobs are added to the required
  set when the tier is adopted.

### 7.6 CI shape

`ci.yml` jobs, named with their tier prefix so the required-check list reads as the roadmap:

| Job | Does |
| --- | --- |
| `tier0-format` | `dotnet format --verify-no-changes` |
| `tier0-build` | locked restore, build with warnings as errors (includes NuGet audit), OpenAPI drift check, dependency register check |
| `tier0-unit` | unit tests, Cobertura artifact |
| `tier0-diff-coverage` | diff-cover against the PR base |
| `tier0-secrets` | gitleaks |
| `tier1-integration` | integration tests with Testcontainers |
| `tier1-openapi-diff` | oasdiff breaking-change check |
| `tier1-dast` | start service with Postgres service container, ZAP API scan |

Separate workflows: `codeql.yml`, `mutation.yml`, `template.yml`. Concurrency cancels superseded
runs per branch. Budget: all Tier 0 jobs finish in under five minutes.

### 7.7 Template package

`template/Templates.csproj` packs the repo's `src/`, `tests/`, and root configuration files with
`ConfigService` as the `sourceName`, excluding `docs/roadmap.md`, `template/`, `.git`, and
workflow files that only make sense for the reference repo itself. `template.yml` packs it,
installs it from the local package, runs `dotnet new`, then builds and tests the result. A
bootstrap checklist in the template README covers renaming, repo settings, and replacing the
sample domain. Conditional stripping of the sample domain is gated on renaming proving painful.

### 7.8 Migration from the course repo

1. Create the new repo (public, empty).
2. Copy `project/config-service` to the new repo root. Do not copy `.vs/`, `bin/`, `obj/`, or
   `*.csproj.user`.
3. Strip explanatory comments from source. Keep XML doc comments only where they feed the
   OpenAPI document.
4. Remove the explicit EF Core references from the integration-test project once central package
   management is in place.
5. Initial commit. Course artifacts (prompts, journal, TODO) stay here.

## 8. Backlog

Ordered. Tier first, then by what unblocks the most. Each line becomes one issue with the
listed labels. "Gated" items are created as issues with the trigger in the body and stay
unscheduled.

### Tier 0

| # | Item | Theme | Why now | Verified by | Depends on |
| --- | --- | --- | --- | --- | --- |
| 1 | Bootstrap repo: copy service to root, strip comments, fresh history, README routing skeleton, `docs/principles.md` | governance | Everything else lands here | Fresh clone builds and unit tests pass | none |
| 2 | `Directory.Build.props` and `.editorconfig`: analyzers latest-recommended, code style in build, warnings as errors | build | Cheapest lint with zero dependencies | Introduce a style violation, build fails | 1 |
| 3 | Central package management, lock files, locked restore | build | One place for versions; fixes the EF unification warning | `dotnet build -warnaserror` clean solution-wide; explicit EF references removed | 2 |
| 4 | NuGet audit fail-on-vulnerable (mode all, level low) | security | Built-in dependency scanning, no tool | Pin a known-vulnerable package on a throwaway branch, restore fails | 3 |
| 5 | Tool manifest: dotnet-ef, husky | dx | One command installs every tool | `dotnet tool restore` on fresh clone | 1 |
| 6 | Husky.Net pre-commit: format check, gitleaks protect | dx | Feedback before CI, cross-platform | Commit with a formatting error is rejected; commit with a fake secret is rejected | 5 |
| 7 | `ci.yml` Tier 0 jobs: format, build, unit tests with coverage artifact, secrets; SHA-pinned actions; concurrency | build | First green check; required for branch protection | Green on a PR; under five minutes | 3, 6 |
| 8 | Diff coverage gate with diff-cover; exclusions documented | testing | Only coverage gate that measures the change, not the repo | PR adding an untested branch fails | 7 |
| 9 | Typed options with ValidateOnStart; connection string to user-secrets; `docs/configuration.md` | config | Removes a committed password; fails fast on misconfiguration | Start with no connection string, startup fails with a clear message; doc lists every key | 1 |
| 10 | Build-time OpenAPI document committed; drift check in CI | api | Makes the contract reviewable and feeds DAST and diffing | Change an endpoint without regenerating, CI fails | 7 |
| 11 | Scalar UI in Development (opt-out) | dx | Explore the API without a client | UI loads in Development; not mapped otherwise | 10 |
| 12 | Dependency register and CI check | governance | Every package justifies itself | Add a package without a row, CI fails | 3, 7 |
| 13 | Repo settings: branch protection with Tier 0 checks, secret scanning, push protection, Dependabot alerts | security | Enforcement instead of convention | Direct push to `main` rejected | 7 |
| 14 | `dependabot.yml`: nuget and github-actions, grouped weekly | security | Keeps pins current without noise | First grouped PR appears | 3 |
| 15 | Issue template, PR template, CONTRIBUTING.md with the TDD loop, AGENTS.md rules | governance | Makes why-now and test-first visible on every change | New issue and PR show the fields | 1 |
| 16 | Practice pages for every Tier 0 practice, six-part format with Microsoft sources | governance | The rationale is the product | Every page has all six parts; sources resolve | 2 to 15 |
| 17 | `docs/roadmap.md` with tiers, entry criteria, label queries | governance | Plan levels visible in the repo | Links resolve to the seeded issues | 18 |
| 18 | Seed issues from this backlog with labels | governance | Backlog lives in issues from here on | Issue count matches this table | 13 |

### Tier 1

| # | Item | Theme | Why now | Verified by | Depends on |
| --- | --- | --- | --- | --- | --- |
| 19 | Spike: Stryker.NET MTP runner with xunit.v3 4.0; record baseline and decision in an ADR | testing | Preview runner has a reported activation issue; nothing else in the tier makes sense until this is known | Deliberately broken assertion produces surviving mutants; ADR written | 7 |
| 20 | `mutation.yml`: threshold from baseline, PRs touching `src/` plus weekly, HTML report artifact | testing | The actual test-quality gate | Delete an assertion, score drops below break | 19 |
| 21 | `codeql.yml` with build-mode none, PR and weekly | security | Free SAST on a public repo | Alerts tab populates; status check present | 7 |
| 22 | Integration tests in CI (`tier1-integration`) | testing | Database behaviour verified on every PR | Green on ubuntu runner | 7 |
| 23 | oasdiff breaking-change check | api | Contract changes are caught before consumers see them | Remove a response field, PR fails | 10 |
| 24 | DAST: ZAP API scan against the running service with Postgres service container; rules file with reasons | security | Runtime checks the analyzers cannot do | Report artifact produced; job fails on an injected High alert | 10, 22 |
| 25 | Readiness endpoint with database check, separate from liveness | config | A deployer needs to know when the service can take traffic | Readiness returns 503 with the database stopped | 9 |
| 26 | Template package and `template.yml` smoke test | template | Tier 0 is stable enough to package | `dotnet new` result builds and passes tests in CI | 16 |
| 27 | Template bootstrap checklist | template | Generated services need a first-day guide | Checklist walked once end to end | 26 |
| 28 | Practice pages for Tier 1 items | governance | Same rule as Tier 0 | Six parts, sources resolve | 19 to 27 |

### Tier 2 and gated

| # | Item | Theme | Trigger | Verified by |
| --- | --- | --- | --- | --- |
| 29 | Architecture tests project; library chosen in an ADR | api | Service is deployed | Forbidden reference fails the test |
| 30 | HTTP logging and OpenTelemetry via Microsoft packages | config | Service is deployed and someone is on call | Traces visible in a local collector |
| 31 | Dockerfile, Trivy scan, SBOM | security | Container deployment chosen | Image builds; scan and SBOM artifacts produced |
| 32 | Reusable workflows and shared build props package | build | A second service exists | Second service consumes them with no copy |
| 33 | OpenAPI style lint (Spectral) | api | An external consumer depends on the contract | Lint passes on the committed document |
| 34 | Auth scheme | security | A consumer needs identity, or a security practice needs demonstrating | Integration test proves unauthenticated requests are rejected |
| 35 | DELETE and pagination | api | A consumer asks for them | Endpoint tests |
| 36 | Conditional strip of the sample domain in the template | template | Renaming by hand proves painful twice | `dotnet new --sample false` builds |
| 37 | Devcontainer | dx | Onboarding a machine takes more than an hour | Fresh container builds and tests |
| 38 | Second persistence adapter behind the repository interfaces, with one contract test suite run against both adapters | api | Deferred. Trigger: the template package lands (item 26) or someone needs to run the service without Docker. Options and trade-offs in ADR 0006. | Contract suite passes against both adapters; database-only tests unchanged |

## 9. Out of scope

- Any change to this course repo beyond this spec.
- Multi-service or platform tooling before a second service exists.
- Deployment pipelines of any kind before a target is chosen.
- A UI or client for the service.

## 10. Sources consulted

- Stryker.NET MTP runner announcement: https://stryker-mutator.io/blog/stryker-net-mtp-runner/
- Stryker.NET xunit.v3 issue: https://github.com/stryker-mutator/stryker-net/issues/3117
- Microsoft.Testing.Platform code coverage: https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-extensions-code-coverage
- xUnit.net v3 with MTP: https://xunit.net/docs/getting-started/v3/microsoft-testing-platform
- CodeQL C# without a build: https://github.blog/changelog/2024-08-28-codeql-code-scanning-can-analyze-java-and-c-codebases-without-needing-a-build-ga/
- Build-time OpenAPI generation: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-10.0
- ZAP API scan action: https://github.com/zaproxy/action-api-scan
