# Tier 0 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Land every Tier 0 "always on" practice from the spec on the reference repo, each enforced by tooling, each explained by a practice page, so that branch protection can require the Tier 0 checks on every pull request.

**Architecture:** One ASP.NET Core 10 Minimal API service at the repo root is the exemplar. Practices land as root-level build configuration (`Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`), a tool manifest with Husky.Net hooks, and one GitHub Actions workflow whose jobs are named by tier. Every practice ships with a page in `docs/practices/` in a fixed six-part format and, where a choice was non-obvious, an ADR in `docs/decisions/`.

**Tech Stack:** .NET SDK 10.0.401, ASP.NET Core 10 Minimal APIs, EF Core 10 with Npgsql 10.0.3, xunit.v3 4.0.0 on Microsoft.Testing.Platform, NSubstitute 6.2.0, Testcontainers.PostgreSql 4.15.0, Husky.Net 0.9.1, Microsoft.Testing.Extensions.CodeCoverage 18.11.2, diff-cover 10.5.1, Microsoft.Extensions.ApiDescription.Server 10.0.11, Scalar.AspNetCore 2.17.3, GitHub Actions, GitHub CLI.

**Spec:** `docs/specs/2026-09-13-reference-repo-design.md`. Sections 2 (principles), 5 (tiers), 6 (practice page format), 7.1 to 7.6 (design), 8 (backlog items 2 to 18) drive this plan. Item 1 is done (commit `aaa238a`).

## Global Constraints

- Target framework `net10.0`; SDK pinned to `10.0.401` with `rollForward: latestPatch`.
- Keep the package versions already in use unless a task says otherwise: `Microsoft.AspNetCore.OpenApi` 10.0.11, `Microsoft.EntityFrameworkCore.Design` 10.0.11, `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3, `Microsoft.AspNetCore.Mvc.Testing` 10.0.11, `Microsoft.NET.Test.Sdk` 18.9.0, `NSubstitute` 6.2.0, `Testcontainers.PostgreSql` 4.15.0, `xunit.v3` 4.0.0, `xunit.runner.visualstudio` 4.0.0.
- No explanatory comments in code. Rationale goes in `docs/practices/` or `docs/decisions/`. XML doc comments only where they feed the OpenAPI document.
- Microsoft-first: prefer SDK, ASP.NET Core, or official Microsoft packages; cite the Microsoft Learn page in the practice page. Third-party only where Microsoft has no equivalent, and the page says so.
- Every new package gets a row in `docs/dependencies.md` (from Task 12 on) and a version in `Directory.Packages.props` only (from Task 3 on). Never a `Version` attribute in a `.csproj` after Task 3.
- Every practice page has exactly six parts in this order: What, Why, Source, Enforced by, Opt out, Go deeper.
- All GitHub Actions pinned to a full commit SHA with the version as a trailing comment.
- Local tooling is dotnet-based only (tool manifest, Husky.Net). No Makefile. CI scripts may use bash on `ubuntu-latest`.
- Warnings are errors everywhere. A build with any warning is a failed task.
- Failing test first for every behaviour change. Configuration-only tasks verify by proving the gate fails on a bad input, then passes on a good one.
- Line endings are LF (`.gitattributes` from PR #1). Files you create must be LF.
- Git: one branch per task named `tier0/NN-<slug>`, branched from `main` (or from the previous task's branch if that PR is not merged yet). Commit small. Open a PR with `gh pr create` at the end of each task; Daniel merges. Commit messages end with `Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>`.
- Working directory for every command: the repo root, unless a step says otherwise.

---

## File structure

Created or modified across the plan. Each file has one job.

| Path | Responsibility |
| --- | --- |
| `docs/principles.md` | The rules every backlog item is checked against (spec section 2) |
| `global.json` | SDK pin and test runner selection |
| `Directory.Build.props` | Compiler, analyzer, and restore settings shared by every project |
| `Directory.Packages.props` | The only place a package version appears |
| `.editorconfig` | Formatting and analyzer severities |
| `src/**/packages.lock.json`, `tests/**/packages.lock.json` | Locked restore graphs |
| `.config/dotnet-tools.json` | Tool manifest: dotnet-ef, husky |
| `.husky/task-runner.json`, `.husky/pre-commit`, `.husky/csx/gitleaks.csx` | Pre-commit tasks |
| `.github/workflows/ci.yml` | Tier 0 jobs |
| `src/ConfigService.Api/Options/DatabaseOptions.cs` | Typed, validated database settings |
| `docs/configuration.md` | Every setting: key, env var, source, default, required |
| `docs/openapi/v1.json` | Build-time generated OpenAPI document, committed |
| `docs/dependencies.md` | One row per package |
| `scripts/CheckDependencyRegister.cs` | File-based .NET app that fails when a package has no row |
| `.github/dependabot.yml` | Grouped weekly updates for NuGet and Actions |
| `.github/ISSUE_TEMPLATE/backlog-item.yml`, `.github/PULL_REQUEST_TEMPLATE.md`, `CONTRIBUTING.md` | Governance templates |
| `docs/roadmap.md` | Tiers, entry criteria, label queries |
| `docs/practices/*.md` | One page per practice, six parts |

---

### Task 1: Principles page

**Files:**
- Create: `docs/principles.md`
- Modify: `AGENTS.md` (first paragraph)

**Interfaces:**
- Produces: `docs/principles.md`, linked from every practice page's Why section where relevant.

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/01-principles
```

- [ ] **Step 2: Write the page**

Create `docs/principles.md`:

```markdown
# Principles

Every backlog item, pull request, and dependency is checked against these. They are the
"why" behind the practice pages in `docs/practices/`.

## YAGNI with a trigger

Nothing is added because it might be useful later. Every item and every dependency names the
condition that makes it necessary now. Gated backlog items state their trigger and stay
unscheduled until it fires.

## Microsoft-first tooling

Prefer what ships with the .NET SDK, ASP.NET Core, or an official Microsoft package, and cite
the Microsoft Learn page or official sample that backs it. Reach for a third-party tool only
where Microsoft has no equivalent, and say so in the practice page.

## Rationale lives in docs, never in code

Code carries no explanatory comments. The "why" for a practice is its practice page. The "why"
for a decision is an ADR in `docs/decisions/`. XML doc comments are allowed only where they feed
the OpenAPI document.

## Measure tests by whether they catch bugs

Mutation score is the quality signal. Coverage is checked only on changed lines in a pull
request and never as a repo-wide number.

## TDD by making it cheap and visible

Tooling gives a fast red-green loop, assistant rules require the failing test first, and the
pull request template asks which test drove the change.

## Correct use of environment variables

Environment variables are the deployment-time override mechanism of the standard ASP.NET Core
configuration pipeline. Settings bind to typed options validated at startup. One reference
document, `docs/configuration.md`, lists every setting. No secrets in committed files. No raw
environment reads outside the composition root.

## Tiers are the plan levels

Tier 0 is always on and required on every pull request. Tier 1 is adopted before the first
deploy. Tier 2 is adopted when the service is operated in production. See `docs/roadmap.md`.
```

- [ ] **Step 3: Point AGENTS.md at it**

In `AGENTS.md`, replace the first paragraph:

```markdown
Working agreements for AI assistants in this repo. Read `docs/principles.md` first once it
exists; until then this file is the rule set.
```

with:

```markdown
Working agreements for AI assistants in this repo. `docs/principles.md` holds the rules every
change is checked against; this file holds the working agreements that follow from them.
```

- [ ] **Step 4: Commit and open PR**

```bash
git add docs/principles.md AGENTS.md
git commit -m "Add principles page and point AGENTS.md at it

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/01-principles
gh pr create --base main --title "Add principles page" --body "Spec section 2 as docs/principles.md. Closes the leftover from backlog item 1.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
```

---

### Task 2: Shared build settings, analyzers, SDK pin

**Files:**
- Create: `Directory.Build.props`, `.editorconfig`, `docs/practices/build-analyzers.md`
- Modify: `global.json`, `src/ConfigService.Api/ConfigService.Api.csproj`, `tests/ConfigService.Api.Tests/ConfigService.Api.Tests.csproj`, `tests/ConfigService.Api.IntegrationTests/ConfigService.Api.IntegrationTests.csproj`

**Interfaces:**
- Produces: `Directory.Build.props` properties that every later task relies on: `TreatWarningsAsErrors`, `AnalysisLevel`, `EnforceCodeStyleInBuild`. Task 3 adds restore properties to the same file.

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/02-build-settings
```

- [ ] **Step 2: Prove the gate is absent**

Add a style violation and confirm the build accepts it. In `src/ConfigService.Api/Entities/IAuditable.cs` temporarily wrap the interface in a block-scoped namespace:

```csharp
namespace ConfigService.Api.Entities
{
    public interface IAuditable
    {
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }
}
```

Run: `dotnet build ConfigService.sln --nologo -v q`
Expected: `0 Error(s)` (the violation is not caught). Revert the file with `git checkout -- src/ConfigService.Api/Entities/IAuditable.cs`.

- [ ] **Step 3: Pin the SDK**

Replace `global.json` with:

```json
{
  "sdk": {
    "version": "10.0.401",
    "rollForward": "latestPatch"
  },
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

- [ ] **Step 4: Create Directory.Build.props**

```xml
<Project>

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>

</Project>
```

- [ ] **Step 5: Create .editorconfig**

```ini
root = true

[*]
charset = utf-8
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true
indent_style = space
indent_size = 4

[*.{json,yml,yaml,props,targets,csproj,sln,md,csx}]
indent_size = 2

[*.md]
trim_trailing_whitespace = false

[*.cs]
csharp_style_namespace_declarations = file_scoped:warning
csharp_using_directive_placement = outside_namespace:warning
csharp_prefer_braces = true:warning
csharp_style_var_for_built_in_types = true:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_elsewhere = true:suggestion
dotnet_sort_system_directives_first = true
dotnet_style_qualification_for_field = false:warning
dotnet_style_qualification_for_property = false:warning
dotnet_style_qualification_for_method = false:warning
dotnet_style_require_accessibility_modifiers = for_non_interface_members:warning
dotnet_diagnostic.IDE0055.severity = warning

[**/Migrations/*.cs]
generated_code = true
dotnet_analyzer_diagnostic.severity = none

[tests/**/*.cs]
dotnet_diagnostic.CA1707.severity = none
```

- [ ] **Step 6: Remove the duplicated properties from each csproj**

In all three `.csproj` files delete these lines from the first `<PropertyGroup>`: `<TargetFramework>`, `<Nullable>`, `<ImplicitUsings>`, and (API project only) `<TreatWarningsAsErrors>`. The API project's first `PropertyGroup` becomes empty; delete the empty element. The test projects keep nothing in that group either; delete it.

- [ ] **Step 7: Build and fix what the analyzers report**

Run: `dotnet build ConfigService.sln --nologo`
Expected on first run: errors from new analyzer rules. For each diagnostic:

1. If the fix improves the code, fix the code.
2. If the rule does not fit this codebase, add `dotnet_diagnostic.<ID>.severity = none` under the matching section in `.editorconfig` and list the ID with a one-line reason in the practice page's Opt out section (Step 10).

Do not use `#pragma` or `[SuppressMessage]` in code.

Repeat until: `0 Warning(s)`, `0 Error(s)`.

- [ ] **Step 8: Prove the gate now works**

Reapply the block-scoped namespace from Step 2.
Run: `dotnet build ConfigService.sln --nologo -v q`
Expected: `error IDE0161: Convert to file-scoped namespace`. Revert the file.

- [ ] **Step 9: Run the tests**

Run: `dotnet test tests/ConfigService.Api.Tests`
Expected: `total: 16`, `failed: 0`.

- [ ] **Step 10: Write the practice page**

Create `docs/practices/build-analyzers.md`:

```markdown
# Build settings and analyzers

## What

`Directory.Build.props` sets the compiler and analyzer options once for every project: nullable
reference types, implicit usings, warnings as errors, the `latest-recommended` analysis level,
and code-style enforcement in the build. `.editorconfig` holds the formatting and style rules
and the per-rule severities. `global.json` pins the SDK so every machine and CI runner compiles
with the same toolchain.

## Why

Lint that runs in the compiler costs nothing to install and cannot be skipped. One props file
means a new project inherits the rules by existing. Warnings as errors turns every analyzer
finding into a failed build, so nothing is triaged by hand later.

## Source

- Directory.Build.props: https://learn.microsoft.com/visualstudio/msbuild/customize-by-directory
- Code analysis in .NET: https://learn.microsoft.com/dotnet/fundamentals/code-analysis/overview
- Code-style rules and EditorConfig: https://learn.microsoft.com/dotnet/fundamentals/code-analysis/code-style-rule-options
- global.json: https://learn.microsoft.com/dotnet/core/tools/global-json

## Enforced by

`dotnet build` (every project, every CI job that builds). `dotnet format --verify-no-changes` in
the pre-commit hook and the `tier0-format` job.

## Opt out

Lower `AnalysisLevel` to `latest-minimum` in `Directory.Build.props` to keep only the rules
Microsoft enables by default. Remove `EnforceCodeStyleInBuild` to stop style rules failing the
build. Either change loses the guarantee that CI and the editor agree.

Rules disabled in `.editorconfig` and why:

- `CA1707` in tests: underscores in test names are the naming convention.
- (add any rule disabled in Step 7 here, one line each)

## Go deeper

`AnalysisLevel=latest-all` enables every rule and needs a triage pass. Roslynator or StyleCop
add rules Microsoft does not ship; neither is planned.
```

Replace the placeholder line in the Opt out list with the actual rules from Step 7, or delete it if none were disabled.

- [ ] **Step 11: Commit and open PR**

```bash
git add global.json Directory.Build.props .editorconfig src tests docs/practices/build-analyzers.md
git commit -m "Share build settings, enable analyzers, pin the SDK

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/02-build-settings
gh pr create --base main --title "Tier 0: shared build settings and analyzers" --body "Backlog item 2. Directory.Build.props, .editorconfig, SDK pin. Verified: a block-scoped namespace fails the build with IDE0161.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
```

---

### Task 3: Central package management and lock files

**Files:**
- Create: `Directory.Packages.props`, `docs/practices/central-package-management.md`, `src/ConfigService.Api/packages.lock.json`, `tests/ConfigService.Api.Tests/packages.lock.json`, `tests/ConfigService.Api.IntegrationTests/packages.lock.json`
- Modify: `Directory.Build.props`, all three `.csproj`, `docs/decisions/0005-explicit-ef-core-references-in-integration-tests.md`

**Interfaces:**
- Produces: `Directory.Packages.props` with one `<PackageVersion>` per package. Tasks 8, 10, 11 add entries here and reference the package without a version.

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/03-central-packages
```

- [ ] **Step 2: Record the current warning**

Run: `dotnet build ConfigService.sln --nologo 2>&1 | grep -c MSB3277`
Expected: a number greater than 0 (the EF Core unification warning). With warnings as errors from Task 2 this line may already appear as an error; either way it is present.

- [ ] **Step 3: Create Directory.Packages.props**

```xml
<Project>

  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>

  <ItemGroup>
    <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.11" />
    <PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="10.0.11" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.11" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.11" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Relational" Version="10.0.11" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="18.9.0" />
    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.3" />
    <PackageVersion Include="NSubstitute" Version="6.2.0" />
    <PackageVersion Include="Testcontainers.PostgreSql" Version="4.15.0" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="4.0.0" />
    <PackageVersion Include="xunit.v3" Version="4.0.0" />
  </ItemGroup>

</Project>
```

- [ ] **Step 4: Strip versions from the csproj files and remove the explicit EF references**

In every `<PackageReference>` in all three `.csproj` files delete the `Version="..."` attribute. In `tests/ConfigService.Api.IntegrationTests/ConfigService.Api.IntegrationTests.csproj` delete the whole `<ItemGroup>` that contains `Microsoft.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.Relational`.

The API project's package group becomes:

```xml
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" />
  </ItemGroup>
```

- [ ] **Step 5: Turn on lock files**

Add to the `<PropertyGroup>` in `Directory.Build.props`:

```xml
    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
```

- [ ] **Step 6: Restore, build, confirm the warning is gone**

```bash
dotnet restore ConfigService.sln
dotnet build ConfigService.sln --nologo 2>&1 | grep -c MSB3277
```

Expected: `0`. Then `dotnet build ConfigService.sln --nologo -v q` shows `0 Warning(s)`, `0 Error(s)`. Three `packages.lock.json` files now exist.

- [ ] **Step 7: Prove locked mode catches drift**

```bash
dotnet restore ConfigService.sln --locked-mode
```

Expected: succeeds. Now change `NSubstitute` to `6.1.0` in `Directory.Packages.props` and rerun the same command.
Expected: `error NU1004` (lock file out of date). Revert the version.

- [ ] **Step 8: Run the tests**

Run: `dotnet test tests/ConfigService.Api.Tests`
Expected: `total: 16`, `failed: 0`.

- [ ] **Step 9: Mark ADR 0005 superseded**

In `docs/decisions/0005-explicit-ef-core-references-in-integration-tests.md` change the status line to:

```markdown
Status: superseded on <today's date> by central package management. See `docs/practices/central-package-management.md`.
```

- [ ] **Step 10: Write the practice page**

Create `docs/practices/central-package-management.md`:

```markdown
# Central package management and lock files

## What

`Directory.Packages.props` is the only file that states a package version. Project files
reference packages by name. Transitive pinning makes every project resolve the same version of
a package that appears in the props file, even when it arrives indirectly. Each project commits
a `packages.lock.json`, and CI restores with `--locked-mode`.

## Why

One place to read, review, and bump versions. Transitive pinning removed the EF Core version
conflict that used to need an explicit reference in the integration-test project (ADR 0005).
Lock files make a restore reproducible: the same inputs give the same graph on every machine
and every CI run, and a pull request that changes the graph shows it in the diff.

## Source

- Central package management: https://learn.microsoft.com/nuget/consume-packages/central-package-management
- Lock files: https://learn.microsoft.com/nuget/consume-packages/package-references-in-project-files#locking-dependencies

## Enforced by

Restore fails on a `Version` attribute in a project file (`NU1008`) and on a lock file that no
longer matches (`NU1004`) when the `tier0-build` job runs `dotnet restore --locked-mode`.

## Opt out

Remove `RestorePackagesWithLockFile` and delete the lock files to lose reproducible restores.
Set `CentralPackageTransitivePinningEnabled` to `false` to let transitive versions float; the
EF Core conflict returns.

## Go deeper

Dependabot (`docs/practices/dependabot.md`) updates the props file and the lock files together.
```

- [ ] **Step 11: Commit and open PR**

```bash
git add Directory.Build.props Directory.Packages.props src tests docs
git commit -m "Manage package versions centrally and lock restores

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/03-central-packages
gh pr create --base main --title "Tier 0: central package management and lock files" --body "Backlog item 3. Removes the explicit EF Core pins in the integration-test project and the MSB3277 warning. Verified: locked-mode restore fails with NU1004 on a version change.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
```

---

### Task 4: NuGet audit fails the build on vulnerable packages

**Files:**
- Modify: `Directory.Build.props`
- Create: `docs/practices/nuget-audit.md`

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/04-nuget-audit
```

- [ ] **Step 2: Enable audit at the strictest level**

Add to the `<PropertyGroup>` in `Directory.Build.props`:

```xml
    <NuGetAudit>true</NuGetAudit>
    <NuGetAuditMode>all</NuGetAuditMode>
    <NuGetAuditLevel>low</NuGetAuditLevel>
```

- [ ] **Step 3: Prove it fails on a known-vulnerable package**

Add to `Directory.Packages.props`:

```xml
    <PackageVersion Include="Newtonsoft.Json" Version="12.0.1" />
```

and to `tests/ConfigService.Api.Tests/ConfigService.Api.Tests.csproj`:

```xml
    <PackageReference Include="Newtonsoft.Json" />
```

Run: `dotnet restore ConfigService.sln --force-evaluate`
Expected: `error NU1903` naming `Newtonsoft.Json 12.0.1` with a high-severity advisory. Revert both files with `git checkout -- Directory.Packages.props tests/ConfigService.Api.Tests/ConfigService.Api.Tests.csproj`.

- [ ] **Step 4: Confirm the real graph is clean**

Run: `dotnet restore ConfigService.sln --force-evaluate && dotnet build ConfigService.sln --nologo -v q`
Expected: `0 Warning(s)`, `0 Error(s)`. If a real advisory appears, bump the affected package in `Directory.Packages.props`, run `dotnet restore --force-evaluate` to refresh the lock files, and record the bump in the commit message.

- [ ] **Step 5: Write the practice page**

Create `docs/practices/nuget-audit.md`:

```markdown
# NuGet audit

## What

Restore checks every package, direct and transitive, against the GitHub Advisory Database and
reports advisories of any severity as warnings. Warnings are errors, so a vulnerable package
fails the restore.

## Why

Dependency scanning with no tool to install, no service to sign up for, and no way to skip it:
it runs inside `dotnet restore`. Transitive mode matters because most vulnerable packages
arrive indirectly.

## Source

- NuGet audit: https://learn.microsoft.com/nuget/concepts/auditing-packages

## Enforced by

`dotnet restore` in every build, locally and in `tier0-build`.

## Opt out

Raise `NuGetAuditLevel` to `moderate`, `high`, or `critical` to ignore lower severities. Set
`NuGetAuditMode` to `direct` to skip transitive packages. To accept one advisory knowingly, add
a `<NuGetAuditSuppress Include="<advisory url>" />` item to `Directory.Packages.props` with the
reason in `docs/dependencies.md`.

## Go deeper

Dependabot security updates (`docs/practices/repo-settings.md`) open the fixing pull request
automatically.
```

- [ ] **Step 6: Commit and open PR**

```bash
git add Directory.Build.props docs/practices/nuget-audit.md
git commit -m "Fail restore on any vulnerable package, transitive included

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/04-nuget-audit
gh pr create --base main --title "Tier 0: NuGet audit" --body "Backlog item 4. Verified: Newtonsoft.Json 12.0.1 fails restore with NU1903.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
```

---

### Task 5: Tool manifest

**Files:**
- Create: `.config/dotnet-tools.json`, `docs/practices/tool-manifest.md`
- Modify: `README.md` (prerequisites section)

**Interfaces:**
- Produces: `dotnet ef` and `dotnet husky` available after `dotnet tool restore`. Task 6 relies on `husky`.

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/05-tool-manifest
```

- [ ] **Step 2: Create the manifest and install the tools**

```bash
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 10.0.11
dotnet tool install husky --version 0.9.1
```

Expected `.config/dotnet-tools.json`:

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "dotnet-ef": {
      "version": "10.0.11",
      "commands": [
        "dotnet-ef"
      ],
      "rollForward": false
    },
    "husky": {
      "version": "0.9.1",
      "commands": [
        "husky"
      ],
      "rollForward": false
    }
  }
}
```

- [ ] **Step 3: Prove a fresh clone gets the tools**

```bash
dotnet tool restore
dotnet ef --version
dotnet husky --version
```

Expected: `Entity Framework Core .NET Command-line Tools 10.0.11` and `0.9.1`.

- [ ] **Step 4: Update the README prerequisites**

In `README.md`, replace the `dotnet-ef` global-tool instructions (the bullet starting "The **`dotnet-ef`** global tool" including its code block and the "(or `dotnet tool update`...)" line) with:

```markdown
- Repo-local tools (`dotnet-ef`, `husky`) come from the tool manifest. After cloning, run:

  ```bash
  dotnet tool restore
  ```
```

Also update the migration command later in the README from `dotnet ef database update --project ...` to keep the same text; `dotnet ef` now resolves to the manifest tool without a global install.

- [ ] **Step 5: Write the practice page**

Create `docs/practices/tool-manifest.md`:

```markdown
# Tool manifest

## What

`.config/dotnet-tools.json` lists the .NET tools the repo needs and their exact versions.
`dotnet tool restore` installs them for the repo only.

## Why

One command after cloning, on any OS, with no global installs to drift between machines. The
manifest is versioned with the code, so the tool version matches the code it was used with.

## Source

- Local tools: https://learn.microsoft.com/dotnet/core/tools/local-tools-how-to-use

## Enforced by

Commands run through `dotnet <tool>` fail with a clear message if the manifest has not been
restored. The Husky.Net target in Task 6 restores the manifest during `dotnet restore`.

## Opt out

Install tools globally with `dotnet tool install -g`. You lose version pinning per repo.

## Go deeper

Tier 1 adds `dotnet-stryker` to the same manifest.
```

- [ ] **Step 6: Commit and open PR**

```bash
git add .config/dotnet-tools.json README.md docs/practices/tool-manifest.md
git commit -m "Add tool manifest with dotnet-ef and husky

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/05-tool-manifest
gh pr create --base main --title "Tier 0: tool manifest" --body "Backlog item 5. Verified: dotnet tool restore on a fresh clone yields dotnet-ef 10.0.11 and husky 0.9.1.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
```

---

### Task 6: Pre-commit hooks with Husky.Net

**Files:**
- Create: `.husky/task-runner.json`, `.husky/pre-commit`, `.husky/csx/gitleaks.csx`, `docs/practices/pre-commit-hooks.md`
- Modify: `src/ConfigService.Api/ConfigService.Api.csproj`, `.gitignore`

**Interfaces:**
- Consumes: `husky` from the tool manifest (Task 5).
- Produces: the `HUSKY=0` environment switch that Task 7's workflow sets to skip hook installation in CI.

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/06-pre-commit
```

- [ ] **Step 2: Install Husky and create the hook**

```bash
dotnet husky install
dotnet husky add pre-commit -c "dotnet husky run --group pre-commit"
```

- [ ] **Step 3: Define the tasks**

Replace `.husky/task-runner.json` with:

```json
{
  "$schema": "https://alirezanet.github.io/Husky.Net/schema.json",
  "tasks": [
    {
      "name": "format-staged",
      "group": "pre-commit",
      "command": "dotnet",
      "args": ["format", "ConfigService.sln", "--verify-no-changes", "--no-restore", "--include", "${staged}"],
      "include": ["**/*.cs"]
    },
    {
      "name": "gitleaks-staged",
      "group": "pre-commit",
      "command": "dotnet",
      "args": ["husky", "exec", ".husky/csx/gitleaks.csx"]
    }
  ]
}
```

- [ ] **Step 4: Write the gitleaks script**

Create `.husky/csx/gitleaks.csx`:

```csharp
using System.ComponentModel;
using System.Diagnostics;

var startInfo = new ProcessStartInfo("gitleaks", "git --pre-commit --staged --redact")
{
    UseShellExecute = false,
};

try
{
    using var process = Process.Start(startInfo);
    process.WaitForExit();
    return process.ExitCode;
}
catch (Win32Exception)
{
    Console.WriteLine("gitleaks is not installed; skipping the local secret scan. CI still runs it.");
    Console.WriteLine("Install: https://github.com/gitleaks/gitleaks#installing");
    return 0;
}
```

- [ ] **Step 5: Auto-install the hooks on restore**

Add to `src/ConfigService.Api/ConfigService.Api.csproj`, before the closing `</Project>`:

```xml
  <PropertyGroup>
    <HuskyRoot Condition="'$(HuskyRoot)' == ''">../../</HuskyRoot>
  </PropertyGroup>

  <Target Name="Husky" AfterTargets="Restore" Condition="'$(HUSKY)' != 0"
          Inputs="$(HuskyRoot).config/dotnet-tools.json"
          Outputs="$(HuskyRoot).husky/_/install.stamp">
    <Exec Command="dotnet tool restore" StandardOutputImportance="Low" StandardErrorImportance="High" />
    <Exec Command="dotnet husky install" StandardOutputImportance="Low" StandardErrorImportance="High"
          WorkingDirectory="$(HuskyRoot)" />
    <Touch Files="$(HuskyRoot).husky/_/install.stamp" AlwaysCreate="true"
           Condition="Exists('$(HuskyRoot).husky/_')" />
    <ItemGroup>
      <FileWrites Include="$(HuskyRoot).husky/_/install.stamp" />
    </ItemGroup>
  </Target>
```

Add to `.gitignore`:

```
.husky/_/
```

- [ ] **Step 6: Prove the format hook rejects a bad commit**

Break formatting in a file (add three spaces before `public` in `src/ConfigService.Api/Entities/IAuditable.cs`), then:

```bash
git add src/ConfigService.Api/Entities/IAuditable.cs
git commit -m "should be rejected"
```

Expected: the commit is refused with `dotnet format` reporting the file. Run `git checkout -- src/ConfigService.Api/Entities/IAuditable.cs` and `git reset`.

- [ ] **Step 7: Prove the secret hook rejects a fake key (if gitleaks is installed)**

Check: `gitleaks version`. If installed, create `tests/leak.txt` containing `aws_access_key_id = AKIAIOSFODNN7EXAMPLE`, then `git add tests/leak.txt && git commit -m "should be rejected"`.
Expected: refused with a gitleaks finding. Delete the file and `git reset`.
If gitleaks is not installed, the hook prints the skip message and the commit proceeds; note that in the PR body.

- [ ] **Step 8: Prove a good commit passes**

```bash
git add .husky .gitignore src/ConfigService.Api/ConfigService.Api.csproj
git commit -m "Add Husky.Net pre-commit hooks for format and secrets

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
```

Expected: both tasks run and the commit lands.

- [ ] **Step 9: Write the practice page**

Create `docs/practices/pre-commit-hooks.md`:

```markdown
# Pre-commit hooks

## What

Husky.Net installs a git pre-commit hook from the tool manifest. It runs
`dotnet format --verify-no-changes` on the staged C# files and, when the `gitleaks` binary is on
the path, `gitleaks git --pre-commit --staged`. The hook installs itself on the first
`dotnet restore` of the API project; CI sets `HUSKY=0` to skip that.

## Why

The fastest feedback is the feedback that arrives before the push. Formatting and leaked
secrets are the two failures that are cheapest to catch locally and most annoying to catch in
CI.

## Source

No Microsoft equivalent. Husky.Net: https://alirezanet.github.io/Husky.Net/. gitleaks:
https://github.com/gitleaks/gitleaks. `dotnet format`:
https://learn.microsoft.com/dotnet/core/tools/dotnet-format

## Enforced by

The hook on every `git commit`. The same checks run in CI as `tier0-format` and
`tier0-secrets`, so a skipped hook (`git commit --no-verify`, or no gitleaks binary) is still
caught.

## Opt out

`git commit --no-verify` skips the hook once. Set `HUSKY=0` in your environment to stop the
restore target installing it. Either way CI runs the same checks.

## Go deeper

Add a `pre-push` group that runs the unit tests: `dotnet husky add pre-push -c "dotnet test tests/ConfigService.Api.Tests"`.
```

- [ ] **Step 10: Commit the page and open PR**

```bash
git add docs/practices/pre-commit-hooks.md
git commit -m "Document the pre-commit hooks

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/06-pre-commit
gh pr create --base main --title "Tier 0: pre-commit hooks" --body "Backlog item 6. Verified: a mis-formatted staged file is rejected. gitleaks check: <installed and rejected the fake key | not installed, skip message shown>.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
```

---

### Task 7: CI workflow, Tier 0 jobs

**Files:**
- Create: `.github/workflows/ci.yml`, `docs/practices/ci-tier-0.md`

**Interfaces:**
- Produces: check names `tier0-format`, `tier0-build`, `tier0-unit`, `tier0-secrets`. Task 8 adds coverage steps to `tier0-unit`. Task 10 and Task 12 add steps to `tier0-build`. Task 15 requires these names in branch protection.

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/07-ci
```

- [ ] **Step 2: Write the workflow**

Create `.github/workflows/ci.yml`:

```yaml
name: ci

on:
  pull_request:
  push:
    branches: [main]

permissions:
  contents: read

concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true

env:
  HUSKY: 0
  DOTNET_NOLOGO: 1
  DOTNET_CLI_TELEMETRY_OPTOUT: 1

jobs:
  tier0-format:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7
      - uses: actions/setup-dotnet@a98b56852c35b8e3190ac28c8c2271da59106c68 # v6
        with:
          global-json-file: global.json
      - run: dotnet restore ConfigService.sln --locked-mode
      - run: dotnet format ConfigService.sln --verify-no-changes --no-restore

  tier0-build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7
      - uses: actions/setup-dotnet@a98b56852c35b8e3190ac28c8c2271da59106c68 # v6
        with:
          global-json-file: global.json
      - run: dotnet restore ConfigService.sln --locked-mode
      - run: dotnet build ConfigService.sln --no-restore --configuration Release

  tier0-unit:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7
      - uses: actions/setup-dotnet@a98b56852c35b8e3190ac28c8c2271da59106c68 # v6
        with:
          global-json-file: global.json
      - run: dotnet restore ConfigService.sln --locked-mode
      - run: dotnet test tests/ConfigService.Api.Tests --no-restore --configuration Release

  tier0-secrets:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7
        with:
          fetch-depth: 0
      - uses: gitleaks/gitleaks-action@e0c47f4f8be36e29cdc102c57e68cb5cbf0e8d1e # v3
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

- [ ] **Step 3: Validate the YAML locally**

Run: `dotnet format ConfigService.sln --verify-no-changes --no-restore && dotnet build ConfigService.sln --configuration Release --nologo -v q`
Expected: format clean, `0 Error(s)`. This is what the jobs will run.

- [ ] **Step 4: Push and watch the run**

```bash
git add .github/workflows/ci.yml
git commit -m "Add CI workflow with Tier 0 jobs

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/07-ci
gh pr create --base main --title "Tier 0: CI workflow" --body "Backlog item 7. Jobs: tier0-format, tier0-build, tier0-unit, tier0-secrets. Actions pinned by SHA.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
gh pr checks --watch
```

Expected: all four jobs green. Note the slowest job's duration; the Tier 0 budget is five minutes.

- [ ] **Step 5: Prove a failing check fails the run**

On the same branch, break formatting in `src/ConfigService.Api/Entities/IAuditable.cs` (three extra spaces), commit with `git commit --no-verify -m "ci: expect format failure"`, push, `gh pr checks --watch`.
Expected: `tier0-format` fails, the other three pass. Revert with `git revert --no-edit HEAD`, push, confirm green.

- [ ] **Step 6: Write the practice page**

Create `docs/practices/ci-tier-0.md`:

```markdown
# CI: Tier 0 jobs

## What

`.github/workflows/ci.yml` runs on every pull request and every push to `main`. Jobs are named
by tier so the required-check list on `main` reads as the roadmap: `tier0-format`,
`tier0-build`, `tier0-unit`, `tier0-secrets`. Restores run in locked mode. Actions are pinned
to commit SHAs. Superseded runs on the same branch are cancelled.

## Why

A pull request that passes Tier 0 is formatted, builds with every analyzer and audit rule
enabled, passes the unit tests with coverage on its changed lines, and adds no secret. That is
the definition of "always on". SHA pinning means a compromised tag cannot change what runs.

## Source

- GitHub Actions for .NET: https://learn.microsoft.com/dotnet/devops/github-actions-overview
- setup-dotnet: https://github.com/actions/setup-dotnet
- Pinning actions to a SHA: https://docs.github.com/actions/security-for-github-actions/security-guides/security-hardening-for-github-actions#using-third-party-actions

## Enforced by

Branch protection on `main` requires every `tier0-*` job (`docs/practices/repo-settings.md`).

## Opt out

Remove a job from the workflow and from the required checks. Both changes are visible in a
pull request.

## Go deeper

Tier 1 adds `tier1-integration`, `tier1-openapi-diff`, and `tier1-dast` to the same workflow,
plus separate CodeQL and mutation workflows.
```

- [ ] **Step 7: Commit the page**

```bash
git add docs/practices/ci-tier-0.md
git commit -m "Document the Tier 0 CI jobs

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push
```

---

### Task 8: Coverage report and diff coverage gate

**Files:**
- Modify: `Directory.Packages.props`, `tests/ConfigService.Api.Tests/ConfigService.Api.Tests.csproj`, `.github/workflows/ci.yml`, `.gitignore`
- Create: `docs/practices/diff-coverage.md`

**Interfaces:**
- Consumes: `tier0-unit` job from Task 7.
- Produces: `coverage.cobertura.xml` under `TestResults/`, uploaded as an artifact; the `diff-cover` step in `tier0-unit`.

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/08-diff-coverage
```

- [ ] **Step 2: Add the coverage extension**

In `Directory.Packages.props` add, keeping alphabetical order:

```xml
    <PackageVersion Include="Microsoft.Testing.Extensions.CodeCoverage" Version="18.11.2" />
```

In `tests/ConfigService.Api.Tests/ConfigService.Api.Tests.csproj` add to the package group:

```xml
    <PackageReference Include="Microsoft.Testing.Extensions.CodeCoverage" />
```

Run `dotnet restore ConfigService.sln` to update the lock file.

- [ ] **Step 3: Produce a report locally**

```bash
dotnet test tests/ConfigService.Api.Tests --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml --results-directory TestResults
find TestResults -name coverage.cobertura.xml
```

Expected: one path printed. Open it and note the form of the `filename` attributes (relative to the repo root, or absolute). Add `TestResults/` to `.gitignore` if not already covered (the existing `[Tt]est[Rr]esult*/` pattern covers it; confirm with `git check-ignore TestResults`).

- [ ] **Step 4: Run diff-cover locally against main with an untested change**

Install: `pip install diff-cover==10.5.1` (Python 3.10 or later; on Windows use `py -m pip`).

Add an untested branch to `src/ConfigService.Api/Endpoints/ApplicationEndpoints.cs` inside `GetAllAsync`, whose signature is `Task<Ok<IEnumerable<ApplicationResponse>>> GetAllAsync(IApplicationRepository repository, CancellationToken ct)`. Insert before the existing `var applications = ...` line:

```csharp
        if (ct.IsCancellationRequested)
        {
            return TypedResults.Ok(Enumerable.Empty<ApplicationResponse>());
        }
```

Rerun Step 3, then:

```bash
COV=$(find TestResults -name coverage.cobertura.xml | head -1)
diff-cover "$COV" --compare-branch=origin/main --fail-under=90 --exclude '**/Migrations/*' '**/Program.cs'
```

Expected: a report listing `ApplicationEndpoints.cs` with missing lines and a non-zero exit.

If instead diff-cover reports that no lines in the diff have coverage information, the `filename` attributes are absolute. Normalise them and rerun:

```bash
sed -i "s|filename=\"$(pwd)/|filename=\"|g" "$COV"
```

Record which form worked; the workflow step below includes the normalisation unconditionally because it is harmless when paths are already relative.

Revert the untested change: `git checkout -- src/ConfigService.Api/Endpoints/ApplicationEndpoints.cs`.

- [ ] **Step 5: Add the steps to the workflow**

Replace the `tier0-unit` job in `.github/workflows/ci.yml` with:

```yaml
  tier0-unit:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7
        with:
          fetch-depth: 0
      - uses: actions/setup-dotnet@a98b56852c35b8e3190ac28c8c2271da59106c68 # v6
        with:
          global-json-file: global.json
      - run: dotnet restore ConfigService.sln --locked-mode
      - run: >-
          dotnet test tests/ConfigService.Api.Tests --no-restore --configuration Release
          --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml
          --results-directory TestResults
      - uses: actions/upload-artifact@043fb46d1a93c77aae656e7c1c64a875d1fc6a0a # v7
        with:
          name: coverage
          path: TestResults/**/coverage.cobertura.xml
      - uses: actions/setup-python@5fda3b95a4ea91299a34e894583c3862153e4b97 # v7
        if: github.event_name == 'pull_request'
        with:
          python-version: "3.12"
      - if: github.event_name == 'pull_request'
        run: pip install diff-cover==10.5.1
      - if: github.event_name == 'pull_request'
        run: |
          COV=$(find TestResults -name coverage.cobertura.xml | head -1)
          sed -i "s|filename=\"$(pwd)/|filename=\"|g" "$COV"
          diff-cover "$COV" --compare-branch=origin/${{ github.base_ref }} --fail-under=90 \
            --exclude '**/Migrations/*' '**/Program.cs' \
            --format markdown:diff-cover.md
          cat diff-cover.md >> "$GITHUB_STEP_SUMMARY"
```

- [ ] **Step 6: Push and prove the gate**

```bash
git add Directory.Packages.props tests/ConfigService.Api.Tests .github/workflows/ci.yml
git commit -m "Report coverage and gate changed lines with diff-cover

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/08-diff-coverage
gh pr create --base main --title "Tier 0: diff coverage" --body "Backlog item 8. Coverage from Microsoft.Testing.Extensions.CodeCoverage; diff-cover gates changed lines at 90%, excluding migrations and Program.cs.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
gh pr checks --watch
```

Expected: green (this PR changes no covered source lines). Then reapply the untested change from Step 4, commit with `--no-verify`, push, watch: `tier0-unit` fails at the diff-cover step and the step summary shows the missing lines. Revert with `git revert --no-edit HEAD`, push, confirm green.

- [ ] **Step 7: Write the practice page**

Create `docs/practices/diff-coverage.md`:

```markdown
# Coverage on changed lines

## What

Unit tests run with Microsoft's code-coverage extension for the Testing Platform and emit a
Cobertura report. On pull requests, diff-cover compares the report with the diff against the
base branch and fails when fewer than 90% of the changed lines are executed by a test.
Migrations and `Program.cs` are excluded. There is no repo-wide coverage threshold.

## Why

A repo-wide percentage rewards testing easy code and says nothing about the change under
review. Coverage on changed lines asks one question a reviewer cares about: did this pull
request bring tests for the code it added? Mutation testing (Tier 1) then asks whether those
tests can fail.

## Source

- Code coverage with Microsoft.Testing.Platform: https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-extensions-code-coverage
- Microsoft on coverage targets: https://learn.microsoft.com/dotnet/core/testing/unit-testing-best-practices#code-coverage-and-code-quality
- diff-cover (no Microsoft equivalent): https://github.com/Bachmann1234/diff_cover

## Enforced by

The diff-cover step of the `tier0-unit` job, on pull requests only.

## Opt out

Lower `--fail-under` or add a path to `--exclude` in `ci.yml`. Add the reason to this page.

## Go deeper

Tier 1 adds Stryker.NET mutation testing as the quality gate; see the spec, items 19 and 20.
```

- [ ] **Step 8: Commit the page**

```bash
git add docs/practices/diff-coverage.md
git commit -m "Document coverage on changed lines

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push
```

---

### Task 9: Typed, validated configuration and user secrets

**Files:**
- Create: `src/ConfigService.Api/Options/DatabaseOptions.cs`, `tests/ConfigService.Api.IntegrationTests/StartupValidationTests.cs`, `docs/configuration.md`, `docs/practices/configuration-and-secrets.md`
- Delete: `src/ConfigService.Api/Data/ConfigDbContextFactory.cs`
- Modify: `src/ConfigService.Api/Program.cs`, `src/ConfigService.Api/appsettings.Development.json`, `src/ConfigService.Api/ConfigService.Api.csproj`, `tests/ConfigService.Api.IntegrationTests/ConfigServiceFactory.cs`, `README.md`

**Interfaces:**
- Produces: `ConfigService.Api.Options.DatabaseOptions` with `const string SectionName = "Database"` and `string ConnectionString`. Configuration key `Database:ConnectionString`, environment variable `Database__ConnectionString`. Task 10 wraps the persistence registration in a guard; it relies on the registration block introduced here.

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/09-configuration
```

- [ ] **Step 2: Write the failing startup test**

Create `tests/ConfigService.Api.IntegrationTests/StartupValidationTests.cs`:

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace ConfigService.Api.IntegrationTests;

public sealed class StartupValidationTests
{
    [Fact]
    public void CreateClient_WithoutConnectionString_ThrowsOptionsValidation()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

        var exception = Assert.Throws<OptionsValidationException>(() => factory.CreateClient());

        Assert.Contains("ConnectionString", exception.Message);
    }

    [Fact]
    public async Task Health_WithConnectionString_Returns200WithoutTouchingTheDatabase()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((_, config) =>
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Database:ConnectionString"] = "Host=unreachable;Database=x;Username=x;Password=x",
                    }));
            });
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
```

- [ ] **Step 3: Run it to verify it fails**

Run: `dotnet test tests/ConfigService.Api.IntegrationTests --filter-class "*StartupValidationTests"`
Expected: first test FAILS (no exception thrown: the app still reads `ConnectionStrings:Default` and starts with a null connection string). Second test FAILS for the same reason or passes; either is acceptable at this point.

- [ ] **Step 4: Add the options type**

Create `src/ConfigService.Api/Options/DatabaseOptions.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace ConfigService.Api.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}
```

- [ ] **Step 5: Bind, validate, and use it in Program.cs**

In `src/ConfigService.Api/Program.cs` add `using ConfigService.Api.Options;` and `using Microsoft.Extensions.Options;` to the usings. Replace:

```csharp
builder.Services.AddDbContext<ConfigDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
```

with:

```csharp
builder.Services.AddOptions<DatabaseOptions>()
    .BindConfiguration(DatabaseOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddDbContext<ConfigDbContext>((serviceProvider, options) =>
    options.UseNpgsql(serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString));
```

- [ ] **Step 6: Delete the design-time factory and its hard-coded connection string**

```bash
git rm src/ConfigService.Api/Data/ConfigDbContextFactory.cs
```

`dotnet ef` resolves `ConfigDbContext` through the app's host instead. Confirm after Step 9.

- [ ] **Step 7: Move the local connection string to user secrets**

```bash
dotnet user-secrets init --project src/ConfigService.Api
dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=configservice;Username=configservice;Password=configservice" --project src/ConfigService.Api
```

The first command adds `<UserSecretsId>` to the API csproj; keep it. Replace `src/ConfigService.Api/appsettings.Development.json` with:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

- [ ] **Step 8: Update the integration test factory**

In `tests/ConfigService.Api.IntegrationTests/ConfigServiceFactory.cs` change the in-memory key:

```csharp
                ["Database:ConnectionString"] = connectionString,
```

- [ ] **Step 9: Run the tests**

Run: `dotnet test tests/ConfigService.Api.IntegrationTests --filter-class "*StartupValidationTests"`
Expected: both PASS.

Run: `dotnet test ConfigService.sln` (Docker running)
Expected: all unit and integration tests pass.

Run: `docker compose up -d && dotnet ef migrations list --project src/ConfigService.Api -- --environment Development`
Expected: `20260908190701_InitialCreate` listed, no error. This proves `dotnet ef` works without the deleted factory; the `--environment Development` argument makes the host load user secrets.

- [ ] **Step 10: Write the configuration reference**

Create `docs/configuration.md`:

```markdown
# Configuration reference

Every setting the service reads. Settings bind to typed options classes and are validated
when the host starts; a missing required setting stops startup with a message naming it.

Sources, in the order ASP.NET Core applies them (later wins): `appsettings.json`,
`appsettings.{Environment}.json`, user secrets (Development only), environment variables,
command-line arguments.

| Key | Environment variable | Options type | Required | Default | Notes |
| --- | --- | --- | --- | --- | --- |
| `Database:ConnectionString` | `Database__ConnectionString` | `DatabaseOptions` | yes | none | Npgsql connection string. Locally: `dotnet user-secrets set "Database:ConnectionString" "..." --project src/ConfigService.Api`. |
| `Logging:LogLevel:Default` | `Logging__LogLevel__Default` | framework | no | `Information` | Standard logging configuration. |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Logging__LogLevel__Microsoft.AspNetCore` | framework | no | `Warning` | |
| `AllowedHosts` | `AllowedHosts` | framework | no | `*` | Host filtering. |
| `ASPNETCORE_ENVIRONMENT` | `ASPNETCORE_ENVIRONMENT` | framework | no | `Production` | `Development` enables auto-migrate, the OpenAPI endpoint, and the API reference UI. |

Adding a setting: add a property to an options class (or a new class in `src/ConfigService.Api/Options/`),
bind it in `Program.cs` with `ValidateDataAnnotations().ValidateOnStart()`, and add a row here.
```

- [ ] **Step 11: Update the README**

In `README.md`:

- In "Starting the database", after the `docker compose up -d` block, add:

```markdown
Then tell the service how to reach it. The connection string is not committed; store it once
per machine in user secrets:

```bash
dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=configservice;Username=configservice;Password=configservice" --project src/ConfigService.Api
```

Every setting the service reads is listed in `docs/configuration.md`.
```

- In "Running the service", replace the sentence that begins "By default this resolves the connection string the same way the app does in Development (`ConnectionStrings:Default` ..." through "the design-time factory `dotnet ef` uses to construct the `DbContext`)." with:

```markdown
`dotnet ef` builds the app's own host, so it reads the same configuration sources. Locally,
pass `-- --environment Development` so user secrets are loaded:

```bash
dotnet ef database update --project src/ConfigService.Api -- --environment Development
```
```

- Replace `ConnectionStrings__Default` with `Database__ConnectionString` wherever it appears.

- [ ] **Step 12: Write the practice page**

Create `docs/practices/configuration-and-secrets.md`:

```markdown
# Configuration and secrets

## What

Settings bind to typed options classes in `src/ConfigService.Api/Options/` with
`AddOptions<T>().BindConfiguration(...).ValidateDataAnnotations().ValidateOnStart()`. The
connection string lives in user secrets locally and in the `Database__ConnectionString`
environment variable elsewhere. `docs/configuration.md` lists every setting. No code reads
`Environment.GetEnvironmentVariable`; the configuration pipeline is the only path.

## Why

A missing or malformed setting fails at startup with the setting's name, not minutes later
with a connection error. Typed options give one place to see what the service needs. Keeping
the connection string out of committed files means the pattern for a real secret is already
in place when one arrives, and environment variables stay what they are meant to be: the
deployment-time override.

## Source

- Options pattern: https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options
- Options validation: https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options#options-validation
- Safe storage of app secrets in development: https://learn.microsoft.com/aspnet/core/security/app-secrets
- Configuration in ASP.NET Core: https://learn.microsoft.com/aspnet/core/fundamentals/configuration/

## Enforced by

`ValidateOnStart` at host start. `StartupValidationTests` proves a missing connection string
stops the host. Tier 2 architecture tests will forbid `Environment.GetEnvironmentVariable`
outside `Program.cs`.

## Opt out

Put the compose credentials back in `appsettings.Development.json`. You lose the demonstration
of the secrets path, not security: those credentials only open the local container defined in
`docker-compose.yml`.

## Go deeper

Tier 1 item 25 adds a readiness endpoint that checks the database. For deployed environments,
a secret store (Azure Key Vault, AWS Secrets Manager) is a configuration provider that slots
into the same pipeline; add it when a deploy target exists.
```

- [ ] **Step 13: Commit and open PR**

```bash
git add -A
git commit -m "Bind database settings to validated options; move the connection string to user secrets

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/09-configuration
gh pr create --base main --title "Tier 0: typed configuration and user secrets" --body "Backlog item 9. DatabaseOptions with ValidateOnStart; design-time factory with its hard-coded password deleted; StartupValidationTests proves the host refuses to start without a connection string.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
```

---

### Task 10: OpenAPI document generated at build time and committed

**Files:**
- Create: `docs/openapi/v1.json` (generated), `docs/practices/openapi-document.md`
- Modify: `Directory.Packages.props`, `src/ConfigService.Api/ConfigService.Api.csproj`, `src/ConfigService.Api/Program.cs`, `.github/workflows/ci.yml`

**Interfaces:**
- Consumes: persistence registration block from Task 9.
- Produces: `docs/openapi/v1.json`, the input for Tier 1's oasdiff and DAST jobs. The `isGeneratingOpenApi` guard in `Program.cs`.

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/10-openapi-document
```

- [ ] **Step 2: Add the package**

In `Directory.Packages.props`:

```xml
    <PackageVersion Include="Microsoft.Extensions.ApiDescription.Server" Version="10.0.11" />
```

In `src/ConfigService.Api/ConfigService.Api.csproj` package group:

```xml
    <PackageReference Include="Microsoft.Extensions.ApiDescription.Server">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
```

And a new property group in the same csproj:

```xml
  <PropertyGroup>
    <OpenApiGenerateDocumentsOnBuild>true</OpenApiGenerateDocumentsOnBuild>
    <OpenApiDocumentsDirectory>$(MSBuildProjectDirectory)/../../docs/openapi</OpenApiDocumentsDirectory>
    <OpenApiGenerateDocumentsOptions>--file-name v1</OpenApiGenerateDocumentsOptions>
  </PropertyGroup>
```

- [ ] **Step 3: Guard startup against the generator**

The generator runs the app's entry point. Without a connection string configured, `ValidateOnStart` would fail the build on a fresh clone. In `Program.cs` add `using System.Reflection;` and, after `var builder = WebApplication.CreateBuilder(args);`:

```csharp
var isGeneratingOpenApi = Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
```

Wrap the two persistence registrations (the `AddOptions<DatabaseOptions>()` chain and `AddDbContext<ConfigDbContext>(...)`) in:

```csharp
if (!isGeneratingOpenApi)
{
    ...
}
```

and change the auto-migrate condition to:

```csharp
if (app.Environment.IsDevelopment() && !isGeneratingOpenApi)
```

- [ ] **Step 4: Generate with no connection string available**

```bash
mkdir -p docs/openapi
env -u Database__ConnectionString dotnet build src/ConfigService.Api --nologo -v q
ls docs/openapi
```

Expected: `v1.json` exists, `0 Error(s)`. Open it and confirm it lists `/api/v1/applications`, `/api/v1/applications/{id}`, `/api/v1/configurations`, `/api/v1/configurations/{id}`.

- [ ] **Step 5: Confirm the guard is exercised by tests**

Run: `dotnet test ConfigService.sln`
Expected: all pass, including `StartupValidationTests` (the guard is false under the test host, so validation still fires).

- [ ] **Step 6: Add the drift check to CI**

In `.github/workflows/ci.yml`, in `tier0-build`, after the build step add:

```yaml
      - name: OpenAPI document is up to date
        run: git diff --exit-code -- docs/openapi/v1.json
```

- [ ] **Step 7: Prove drift fails the build**

Commit and push this task first:

```bash
git add -A
git commit -m "Generate the OpenAPI document at build time and commit it

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/10-openapi-document
gh pr create --base main --title "Tier 0: committed OpenAPI document" --body "Backlog item 10. docs/openapi/v1.json is generated by Microsoft.Extensions.ApiDescription.Server on every build; tier0-build fails if the committed copy differs.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
gh pr checks --watch
```

Expected: green. Then delete the `"/api/v1/applications/{id}"` path object from `docs/openapi/v1.json` by hand, commit with `--no-verify -m "ci: expect openapi drift"`, push, watch.
Expected: `tier0-build` fails at the drift step. Revert with `git revert --no-edit HEAD`, push, confirm green.

- [ ] **Step 8: Write the practice page**

Create `docs/practices/openapi-document.md`:

```markdown
# OpenAPI document, generated at build time

## What

`Microsoft.Extensions.ApiDescription.Server` runs the app's entry point during `dotnet build`
and writes the OpenAPI document to `docs/openapi/v1.json`. The file is committed. CI fails if a
build produces a different document than the one committed. `Program.cs` skips persistence
registration when the entry assembly is the generator, so a build needs no database
configuration.

## Why

The API contract becomes a reviewable file: a pull request that changes a response shape
shows the change in the diff. The committed document is the input for the Tier 1 breaking-
change check and the DAST scan, so those tools never need a running service to know the
contract.

## Source

- Generate OpenAPI documents at build time: https://learn.microsoft.com/aspnet/core/fundamentals/openapi/aspnetcore-openapi#generate-openapi-documents-at-build-time
- Customising runtime behaviour during generation: the same page, "Customize runtime behavior during build-time document generation"

## Enforced by

The "OpenAPI document is up to date" step of `tier0-build`.

## Opt out

Remove `OpenApiGenerateDocumentsOnBuild` from the API project and the drift step from CI. The
runtime `/openapi/v1.json` endpoint keeps working in Development.

## Go deeper

Tier 1 item 23 adds oasdiff to fail pull requests on breaking changes. Endpoint metadata
(`WithName`, `WithSummary`, `WithTags`) enriches the document when a consumer needs it.
```

- [ ] **Step 9: Commit the page**

```bash
git add docs/practices/openapi-document.md
git commit -m "Document the committed OpenAPI document

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push
```

---

### Task 11: API reference UI in Development

**Files:**
- Modify: `Directory.Packages.props`, `src/ConfigService.Api/ConfigService.Api.csproj`, `src/ConfigService.Api/Program.cs`, `src/ConfigService.Api/Properties/launchSettings.json`, `tests/ConfigService.Api.IntegrationTests/StartupValidationTests.cs`, `README.md`
- Create: `docs/practices/api-reference-ui.md`

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/11-api-reference-ui
```

- [ ] **Step 2: Write the failing test**

Add to `StartupValidationTests` (it already builds a host with a connection string in the second test; add a third):

```csharp
    [Fact]
    public async Task Scalar_OutsideDevelopment_IsNotMapped()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((_, config) =>
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Database:ConnectionString"] = "Host=unreachable;Database=x;Username=x;Password=x",
                    }));
            });
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/scalar", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
```

Run: `dotnet test tests/ConfigService.Api.IntegrationTests --filter-class "*StartupValidationTests"`
Expected: PASSES already (nothing is mapped). This test guards the Development-only condition once the UI is added; keep it.

- [ ] **Step 3: Add Scalar**

In `Directory.Packages.props`:

```xml
    <PackageVersion Include="Scalar.AspNetCore" Version="2.17.3" />
```

In the API csproj package group:

```xml
    <PackageReference Include="Scalar.AspNetCore" />
```

In `Program.cs` add `using Scalar.AspNetCore;` and change:

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
```

to:

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
```

In `launchSettings.json`, in the `http` profile only, change `"launchBrowser": false` to `"launchBrowser": true` and add `"launchUrl": "scalar"` on the next line, so `dotnet run` opens the reference UI.

- [ ] **Step 4: Verify**

Run: `dotnet test tests/ConfigService.Api.IntegrationTests --filter-class "*StartupValidationTests"`
Expected: PASS (still 404 outside Development).

Run: `docker compose up -d && dotnet run --project src/ConfigService.Api` and open `http://localhost:5033/scalar`.
Expected: the reference UI lists the eight operations. Stop the app.

Run: `dotnet build src/ConfigService.Api --nologo -v q && git status --short docs/openapi`
Expected: no change to `v1.json` (Scalar adds no operations).

- [ ] **Step 5: README**

In `README.md` "Trying the API", add before the `.http` paragraph:

```markdown
In Development the service also serves an interactive API reference at
`http://localhost:5033/scalar`, generated from the same OpenAPI document.
```

- [ ] **Step 6: Write the practice page**

Create `docs/practices/api-reference-ui.md`:

```markdown
# API reference UI

## What

Scalar renders the OpenAPI document as an interactive reference at `/scalar`, mapped only in
the Development environment.

## Why

Exploring and calling the API without writing a client is the fastest way to learn what a
service does. The `.http` file covers scripted calls; the UI covers discovery.

## Source

- Microsoft's OpenAPI documentation names Scalar as the interactive UI option for .NET 9 and later and states UIs should only be enabled in development: https://learn.microsoft.com/aspnet/core/fundamentals/openapi/using-openapi-documents#use-scalar-for-interactive-api-documentation
- Scalar (third party, no Microsoft UI ships in the box): https://github.com/scalar/scalar

## Enforced by

`StartupValidationTests.Scalar_OutsideDevelopment_IsNotMapped` proves the route is absent
outside Development.

## Opt out

Remove the package reference and the `MapScalarApiReference()` call. The `.http` file and the
raw `/openapi/v1.json` endpoint remain.

## Go deeper

None planned. Swagger UI via `Swashbuckle.AspNetCore.SwaggerUi` is the alternative Microsoft
documents.
```

- [ ] **Step 7: Commit and open PR**

```bash
git add -A
git commit -m "Serve a Scalar API reference in Development

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/11-api-reference-ui
gh pr create --base main --title "Tier 0: API reference UI (opt-out)" --body "Backlog item 11. Scalar at /scalar in Development only; test proves 404 elsewhere.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
```

---

### Task 12: Dependency register with a CI check

**Files:**
- Create: `docs/dependencies.md`, `scripts/CheckDependencyRegister.cs`, `docs/practices/dependency-register.md`
- Modify: `.github/workflows/ci.yml`, `AGENTS.md`

**Interfaces:**
- Consumes: `Directory.Packages.props` (Task 3).
- Produces: `dotnet run scripts/CheckDependencyRegister.cs` exits 1 and names each package without a row.

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/12-dependency-register
```

- [ ] **Step 2: Write the check as a file-based app**

Create `scripts/CheckDependencyRegister.cs`:

```csharp
#:property RestorePackagesWithLockFile=false

using System.Text.RegularExpressions;
using System.Xml.Linq;

var root = Directory.GetCurrentDirectory();
var propsPath = Path.Combine(root, "Directory.Packages.props");
var registerPath = Path.Combine(root, "docs", "dependencies.md");

var packages = XDocument.Load(propsPath)
    .Descendants("PackageVersion")
    .Select(element => (string?)element.Attribute("Include"))
    .Where(name => name is not null)
    .Select(name => name!)
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

var registered = File.ReadLines(registerPath)
    .Select(line => Regex.Match(line, @"^\|\s*`([^`]+)`\s*\|"))
    .Where(match => match.Success)
    .Select(match => match.Groups[1].Value)
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

var missing = packages.Except(registered).Order().ToList();
var stale = registered.Except(packages).Order().ToList();

foreach (var name in missing)
{
    Console.Error.WriteLine($"No row in docs/dependencies.md for package {name}");
}

foreach (var name in stale)
{
    Console.Error.WriteLine($"docs/dependencies.md lists {name}, which is not in Directory.Packages.props");
}

return missing.Count + stale.Count == 0 ? 0 : 1;
```

- [ ] **Step 3: Run it to verify it fails**

Run: `dotnet run scripts/CheckDependencyRegister.cs`
Expected: exit code 1 (the register file does not exist yet, so a `FileNotFoundException`; that is the failing state). Create an empty `docs/dependencies.md` and rerun.
Expected: one "No row" line per package, exit code 1.

- [ ] **Step 4: Write the register**

Create `docs/dependencies.md`:

```markdown
# Dependency register

One row per package in `Directory.Packages.props`. `scripts/CheckDependencyRegister.cs` fails
CI when a package has no row or a row has no package. The first column is the exact package id
in backticks.

| Package | Why it is here | What it replaces | Tier |
| --- | --- | --- | --- |
| `Microsoft.AspNetCore.Mvc.Testing` | Hosts the app in-process for integration tests | Hand-rolled test server | 0 |
| `Microsoft.AspNetCore.OpenApi` | Built-in OpenAPI document generation | Swashbuckle, NSwag | 0 |
| `Microsoft.EntityFrameworkCore` | Pinned so every project resolves the same EF Core version | Explicit per-project pins (ADR 0005) | 0 |
| `Microsoft.EntityFrameworkCore.Design` | Design-time support for `dotnet ef` migrations | Hand-written SQL migrations | 0 |
| `Microsoft.EntityFrameworkCore.Relational` | Pinned with the core package for the same reason | Explicit per-project pins (ADR 0005) | 0 |
| `Microsoft.Extensions.ApiDescription.Server` | Generates the OpenAPI document at build time | Committing a hand-fetched document | 0 |
| `Microsoft.NET.Test.Sdk` | Test host integration for `dotnet test` | none | 0 |
| `Microsoft.Testing.Extensions.CodeCoverage` | Cobertura coverage for the diff-coverage gate | coverlet | 0 |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | EF Core provider for PostgreSQL | raw Npgsql, Dapper | 0 |
| `NSubstitute` | Test doubles for repository interfaces in unit tests | Moq, hand-written fakes | 0 |
| `Scalar.AspNetCore` | Interactive API reference in Development (opt-out) | Swashbuckle.AspNetCore.SwaggerUi | 0 |
| `Testcontainers.PostgreSql` | Throwaway PostgreSQL for integration tests | EF InMemory provider, shared dev database | 0 |
| `xunit.runner.visualstudio` | Test discovery in IDEs | none | 0 |
| `xunit.v3` | Test framework on Microsoft.Testing.Platform | MSTest, NUnit | 0 |
```

Run: `dotnet run scripts/CheckDependencyRegister.cs`
Expected: no output, exit code 0. Then remove the `xunit.v3` row temporarily and rerun.
Expected: `No row in docs/dependencies.md for package xunit.v3`, exit code 1. Restore the row.

- [ ] **Step 5: Add the step to CI**

In `.github/workflows/ci.yml`, in `tier0-build`, after the OpenAPI drift step:

```yaml
      - name: Every package has a row in docs/dependencies.md
        run: dotnet run scripts/CheckDependencyRegister.cs
```

- [ ] **Step 6: Update AGENTS.md**

Replace the bullet:

```markdown
- **Every package justifies itself.** A new package needs a row in `docs/dependencies.md`
  once that file exists; until then, an ADR.
```

with:

```markdown
- **Every package justifies itself.** A new package needs a row in `docs/dependencies.md`
  saying why it is here and what it replaces. CI fails without it.
```

- [ ] **Step 7: Write the practice page**

Create `docs/practices/dependency-register.md`:

```markdown
# Dependency register

## What

`docs/dependencies.md` has one row per package: why it is here, what it replaces, and which
tier introduced it. A file-based .NET app in `scripts/` compares the register with
`Directory.Packages.props` and fails CI on a missing or stale row.

## Why

"It might be useful later" is the most common way a codebase grows a dependency it does not
need. Writing the reason down at the moment of adding forces the question, and keeps the
answer next to the version pin for the next person.

## Source

- File-based apps (`dotnet run file.cs`): https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/sdk#file-based-apps
- Principle: `docs/principles.md`, "YAGNI with a trigger"

## Enforced by

The "Every package has a row" step of `tier0-build`.

## Opt out

Remove the step from CI. The register becomes advisory.

## Go deeper

Extend the script to require a tier label per row to match the roadmap.
```

- [ ] **Step 8: Commit and open PR**

```bash
git add -A
git commit -m "Add the dependency register and a CI check that every package has a row

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/12-dependency-register
gh pr create --base main --title "Tier 0: dependency register" --body "Backlog item 12. Verified: removing a row fails the check with the package name.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
```

---

### Task 13: Dependabot

**Files:**
- Create: `.github/dependabot.yml`, `docs/practices/dependabot.md`

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/13-dependabot
```

- [ ] **Step 2: Write the config**

Create `.github/dependabot.yml`:

```yaml
version: 2
updates:
  - package-ecosystem: nuget
    directory: /
    schedule:
      interval: weekly
      day: monday
    groups:
      nuget:
        patterns: ["*"]
    commit-message:
      prefix: "deps"

  - package-ecosystem: github-actions
    directory: /
    schedule:
      interval: weekly
      day: monday
    groups:
      actions:
        patterns: ["*"]
    commit-message:
      prefix: "deps"
```

- [ ] **Step 3: Write the practice page**

Create `docs/practices/dependabot.md`:

```markdown
# Dependabot

## What

Weekly grouped pull requests for NuGet packages and GitHub Actions. NuGet updates land in
`Directory.Packages.props` and the lock files together. Action updates bump the pinned SHA and
the version comment.

## Why

Pinned versions only stay safe if something proposes the bump. One grouped pull request a week
is reviewable; one per package is noise that gets ignored.

## Source

- Dependabot version updates: https://docs.github.com/code-security/dependabot/dependabot-version-updates/configuration-options-for-the-dependabot.yml-file
- NuGet central package management support: the same page, `nuget` ecosystem

## Enforced by

Nothing blocks; each Dependabot pull request runs the Tier 0 checks like any other.

## Opt out

Delete `.github/dependabot.yml`. NuGet audit still fails the build on known vulnerabilities;
you lose the proposed fix.

## Go deeper

Enable auto-merge for grouped patch updates once the Tier 1 mutation gate is in place.
```

- [ ] **Step 4: Commit, open PR, confirm Dependabot picks it up**

```bash
git add .github/dependabot.yml docs/practices/dependabot.md
git commit -m "Configure grouped weekly Dependabot updates

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/13-dependabot
gh pr create --base main --title "Tier 0: Dependabot" --body "Backlog item 14. Grouped weekly updates for nuget and github-actions.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
```

After merge, the Insights tab shows "Dependency graph, Dependabot" with the two ecosystems; a first grouped PR arrives on the next Monday or immediately if any pin is behind.

---

### Task 14: Issue template, PR template, CONTRIBUTING

**Files:**
- Create: `.github/ISSUE_TEMPLATE/backlog-item.yml`, `.github/ISSUE_TEMPLATE/config.yml`, `.github/PULL_REQUEST_TEMPLATE.md`, `CONTRIBUTING.md`, `docs/practices/contribution-templates.md`

**Interfaces:**
- Produces: the issue form fields Task 17 fills when seeding.

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/14-templates
```

- [ ] **Step 2: Issue form**

Create `.github/ISSUE_TEMPLATE/backlog-item.yml`:

```yaml
name: Backlog item
description: A change with a reason to exist now and a way to prove it landed.
title: "<verb> <thing>"
labels: []
body:
  - type: dropdown
    id: tier
    attributes:
      label: Tier
      options:
        - tier-0 (always on)
        - tier-1 (before first deploy)
        - tier-2 (operating in production)
        - gated (trigger not fired)
    validations:
      required: true
  - type: dropdown
    id: theme
    attributes:
      label: Theme
      options: [build, testing, security, api, config, governance, template, dx]
    validations:
      required: true
  - type: textarea
    id: why-now
    attributes:
      label: Why now
      description: What breaks, costs, or stays unproven without this today? For a gated item, name the trigger.
    validations:
      required: true
  - type: textarea
    id: verification
    attributes:
      label: Verified by
      description: The command, test, or observable failure that proves it works.
    validations:
      required: true
  - type: input
    id: depends-on
    attributes:
      label: Depends on
      description: Issue numbers, or "none".
```

Create `.github/ISSUE_TEMPLATE/config.yml`:

```yaml
blank_issues_enabled: false
```

- [ ] **Step 3: Pull request template**

Create `.github/PULL_REQUEST_TEMPLATE.md`:

```markdown
## What changes

## What breaks without this

## Which test drove it

<!-- Name the test written first, or say why no test applies (docs, config). -->

## Tier and practice page

<!-- tier-0 / tier-1 / tier-2. Which docs/practices page was added or changed? -->

## Verification

<!-- Command run and its output, or the CI check that proves it. -->
```

- [ ] **Step 4: CONTRIBUTING**

Create `CONTRIBUTING.md`:

```markdown
# Contributing

## The loop

1. Branch from `main`.
2. Write the failing test. Run it and watch it fail:

   ```bash
   dotnet test tests/ConfigService.Api.Tests --filter-method "*YourTestName*"
   ```

   Or keep the loop running while you work:

   ```bash
   dotnet watch test --project tests/ConfigService.Api.Tests
   ```

3. Write the smallest change that makes it pass. Run it again.
4. Commit. The pre-commit hook checks formatting and secrets.
5. Open a pull request. Fill in every section of the template; "which test drove it" is not
   optional.

## Tests

- Unit tests (`tests/ConfigService.Api.Tests`) use NSubstitute for repository interfaces and
  need no database. They run in seconds.
- Integration tests (`tests/ConfigService.Api.IntegrationTests`) need Docker and cover only
  what needs the database (ADR 0003).
- Test names follow `Method_Scenario_Expectation`.

## Adding a dependency

Add the version to `Directory.Packages.props`, reference it by name in the project, and add a
row to `docs/dependencies.md` saying why it is here and what it replaces. CI fails without the
row.

## Adding a setting

Add a property to an options class, bind it with `ValidateOnStart()` in `Program.cs`, and add
a row to `docs/configuration.md`.

## Rationale

Not in code comments. A practice gets a page in `docs/practices/` (six parts: What, Why,
Source, Enforced by, Opt out, Go deeper). A decision gets an ADR in `docs/decisions/`.
```

- [ ] **Step 5: Practice page**

Create `docs/practices/contribution-templates.md`:

```markdown
# Contribution templates

## What

An issue form with required "Why now" and "Verified by" fields and tier and theme dropdowns; a
pull request template that asks what breaks without the change, which test drove it, and which
practice page changed; a `CONTRIBUTING.md` describing the test-first loop.

## Why

The questions people skip are the ones worth asking every time. Making them fields, not
guidelines, means every item and every change states its reason and its proof.

## Source

- Issue forms: https://docs.github.com/communities/using-templates-to-encourage-useful-issues-and-pull-requests/syntax-for-issue-forms
- Pull request templates: https://docs.github.com/communities/using-templates-to-encourage-useful-issues-and-pull-requests/creating-a-pull-request-template-for-your-repository

## Enforced by

Required fields in the issue form. The PR template is a prompt; a reviewer refuses a PR that
leaves "which test drove it" blank.

## Opt out

Delete the files under `.github/`.

## Go deeper

A workflow that fails a PR whose body still contains the template placeholders.
```

- [ ] **Step 6: Commit and open PR**

```bash
git add -A
git commit -m "Add issue form, PR template, and CONTRIBUTING

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/14-templates
gh pr create --base main --title "Tier 0: contribution templates" --body "Backlog item 15. The PR body you are reading was not written with the template because the template lands in this PR; the next one will be.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
```

Open `https://github.com/monomaniacal/dotnet-service-reference/issues/new/choose` after merge and confirm the form shows the five fields.

---

### Task 15: Repository settings

Requires Tasks 7 to 12 merged so the check names exist on `main`.

**Files:**
- Create: `docs/practices/repo-settings.md`

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/15-repo-settings
```

- [ ] **Step 2: Secret scanning, push protection, Dependabot alerts and security updates**

```bash
gh api -X PATCH repos/monomaniacal/dotnet-service-reference \
  --input - <<'EOF'
{"security_and_analysis":{"secret_scanning":{"status":"enabled"},"secret_scanning_push_protection":{"status":"enabled"}}}
EOF
gh api -X PUT repos/monomaniacal/dotnet-service-reference/vulnerability-alerts
gh api -X PUT repos/monomaniacal/dotnet-service-reference/automated-security-fixes
gh api repos/monomaniacal/dotnet-service-reference --jq '.security_and_analysis'
```

Expected: the last command prints `secret_scanning` and `secret_scanning_push_protection` as `enabled`.

- [ ] **Step 3: Branch protection on main**

```bash
gh api -X PUT repos/monomaniacal/dotnet-service-reference/branches/main/protection \
  --input - <<'EOF'
{
  "required_status_checks": {
    "strict": true,
    "contexts": ["tier0-format", "tier0-build", "tier0-unit", "tier0-secrets"]
  },
  "enforce_admins": true,
  "required_pull_request_reviews": {
    "required_approving_review_count": 0,
    "dismiss_stale_reviews": true
  },
  "restrictions": null,
  "allow_force_pushes": false,
  "allow_deletions": false,
  "required_linear_history": false,
  "required_conversation_resolution": true
}
EOF
```

- [ ] **Step 4: Prove a direct push is rejected**

```bash
git push origin HEAD:main
```

Expected: `remote: error: GH006: Protected branch update failed` (or the required-check message). Nothing lands.

- [ ] **Step 5: Practice page**

Create `docs/practices/repo-settings.md`:

```markdown
# Repository settings

## What

Applied with the GitHub CLI, recorded here so they can be reapplied:

- Branch protection on `main`: pull requests only, every `tier0-*` check required and up to
  date with `main`, enforced for admins, conversations resolved, no force pushes or deletion.
- Secret scanning with push protection.
- Dependabot alerts and automated security updates.

## Why

A convention is a request; a setting is a rule. With admins included, nobody, including the
maintainer, merges past a red check.

## Source

- Branch protection: https://docs.github.com/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches
- Secret scanning and push protection: https://docs.github.com/code-security/secret-scanning/introduction/about-push-protection
- Dependabot alerts: https://docs.github.com/code-security/dependabot/dependabot-alerts/about-dependabot-alerts

## Enforced by

GitHub, server side. `git push` to `main` fails.

## Opt out

Repository settings on GitHub. If the repo goes private without GitHub Advanced Security,
secret scanning and push protection stop; the `tier0-secrets` gitleaks job keeps running.

## Go deeper

When Tier 1 is adopted, add `tier1-integration`, `tier1-openapi-diff`, and `tier1-dast` to the
required contexts with the same command.
```

- [ ] **Step 6: Commit and open PR**

```bash
git add docs/practices/repo-settings.md
git commit -m "Record the repository settings applied for Tier 0

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/15-repo-settings
gh pr create --base main --title "Tier 0: repository settings" --body "Backlog item 13. Branch protection requiring the four tier0 checks, secret scanning with push protection, Dependabot alerts. Verified: a direct push to main is rejected with GH006.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
```

---

### Task 16: Roadmap

**Files:**
- Create: `docs/roadmap.md`
- Modify: `README.md` (routing header)

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/16-roadmap
```

- [ ] **Step 2: Write the roadmap**

Create `docs/roadmap.md`:

```markdown
# Roadmap

Practices are adopted in tiers. Each tier has an entry criterion, a CI job group, and an issue
label. This page describes the tiers and themes; the items themselves are GitHub issues, so
nothing is listed twice. Each link below is a live query.

## Tier 0: always on

Entry criterion: the repo exists. Required on every pull request by branch protection.

Landed: shared build settings and analyzers, central package management and lock files,
NuGet audit, tool manifest, pre-commit hooks, the `tier0-*` CI jobs, coverage on changed
lines, typed configuration with user secrets, the committed OpenAPI document, the API
reference UI, the dependency register, Dependabot, contribution templates, repository
settings. Each has a page in `docs/practices/`.

Open items: https://github.com/monomaniacal/dotnet-service-reference/issues?q=is%3Aissue+is%3Aopen+label%3Atier-0

## Tier 1: before first deploy

Entry criterion: something will consume the service outside the developer loop.

Themes: mutation testing as the test-quality gate, CodeQL, integration tests in CI, breaking-
change detection on the OpenAPI document, DAST, readiness endpoint, the `dotnet new` template.

Open items: https://github.com/monomaniacal/dotnet-service-reference/issues?q=is%3Aissue+is%3Aopen+label%3Atier-1

## Tier 2: operating in production

Entry criterion: the service is deployed and someone is on call for it.

Themes: architecture tests, observability, container scanning and SBOM, reusable workflows.

Open items: https://github.com/monomaniacal/dotnet-service-reference/issues?q=is%3Aissue+is%3Aopen+label%3Atier-2

## Gated

Items with a named trigger that has not fired. Each issue body states the trigger.

https://github.com/monomaniacal/dotnet-service-reference/issues?q=is%3Aissue+is%3Aopen+label%3Agated

## Themes

`theme:build`, `theme:testing`, `theme:security`, `theme:api`, `theme:config`,
`theme:governance`, `theme:template`, `theme:dx`. Query any theme with `label:theme:<name>`.

## Priority

Tier first, then whatever unblocks the most within a tier. The issue's "Depends on" field
carries the ordering.
```

- [ ] **Step 3: Point the README at it**

In `README.md`'s routing header, change the "Adopt a practice" bullet to:

```markdown
- **Adopt a practice:** `docs/practices/` has one page per practice with its rationale and
  Microsoft source. `docs/roadmap.md` explains the tiers. `docs/decisions/` holds the ADRs.
```

- [ ] **Step 4: Commit and open PR**

```bash
git add docs/roadmap.md README.md
git commit -m "Add the roadmap with tier entry criteria and label queries

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/16-roadmap
gh pr create --base main --title "Tier 0: roadmap" --body "Backlog item 17. Links resolve once Task 17 seeds the issues.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
```

---

### Task 17: Seed the backlog as issues

Run after Task 14 (labels and form) is merged. This is a one-time action; no script is kept.

- [ ] **Step 1: Labels**

```bash
R=monomaniacal/dotnet-service-reference
gh label create tier-0 --repo $R --color 0E8A16 --description "Always on; required on every PR"
gh label create tier-1 --repo $R --color FBCA04 --description "Before first deploy"
gh label create tier-2 --repo $R --color D93F0B --description "Operating in production"
gh label create gated  --repo $R --color BFDADC --description "Trigger not fired; see issue body"
for t in build testing security api config governance template dx; do
  gh label create "theme:$t" --repo $R --color 5319E7 --description "Theme: $t"
done
```

- [ ] **Step 2: Tier 1 issues**

One `gh issue create` per row of the spec's Tier 1 table. Body format matches the issue form's fields.

```bash
R=monomaniacal/dotnet-service-reference
mk() { gh issue create --repo $R --title "$1" --label "$2" --body "$3"; }

mk "Spike: Stryker.NET MTP runner with xunit.v3 4.0; record baseline in an ADR" "tier-1,theme:testing" $'**Why now**\nThe preview runner has a reported mutation-activation issue with xunit.v3. Nothing else in the tier makes sense until this is known.\n\n**Verified by**\nA deliberately broken assertion produces surviving mutants; ADR written with the baseline score.\n\n**Depends on**\nTier 0 CI.'
mk "Mutation testing workflow with threshold from baseline" "tier-1,theme:testing" $'**Why now**\nThe actual test-quality gate.\n\n**Verified by**\nDelete an assertion; score drops below the break threshold. Runs on PRs touching src/ and weekly; HTML report artifact.\n\n**Depends on**\nThe Stryker spike.'
mk "CodeQL workflow with build-mode none, PR and weekly" "tier-1,theme:security" $'**Why now**\nFree SAST on a public repo.\n\n**Verified by**\nAlerts tab populates; status check present on PRs.\n\n**Depends on**\nTier 0 CI.'
mk "Integration tests in CI (tier1-integration)" "tier-1,theme:testing" $'**Why now**\nDatabase behaviour verified on every PR.\n\n**Verified by**\nGreen on ubuntu runner with Testcontainers.\n\n**Depends on**\nTier 0 CI.'
mk "oasdiff breaking-change check on the committed OpenAPI document" "tier-1,theme:api" $'**Why now**\nContract changes are caught before consumers see them.\n\n**Verified by**\nRemove a response field; PR fails.\n\n**Depends on**\nCommitted OpenAPI document (Tier 0).'
mk "DAST: ZAP API scan against the running service in CI" "tier-1,theme:security" $'**Why now**\nRuntime checks the analyzers cannot do.\n\n**Verified by**\nReport artifact produced; job fails on an injected High alert. Rules file records every tuned alert with its reason.\n\n**Depends on**\nCommitted OpenAPI document; integration tests in CI (Postgres service container pattern).'
mk "Readiness endpoint with a database check, separate from liveness" "tier-1,theme:config" $'**Why now**\nA deployer needs to know when the service can take traffic.\n\n**Verified by**\nReadiness returns 503 with the database stopped; liveness still 200.\n\n**Depends on**\nTyped configuration (Tier 0).'
mk "dotnet new template package and template.yml smoke test" "tier-1,theme:template" $'**Why now**\nTier 0 is stable enough to package.\n\n**Verified by**\nCI packs the template, installs it, runs dotnet new, builds and tests the result.\n\n**Depends on**\nAll Tier 0 items.'
mk "Template bootstrap checklist" "tier-1,theme:template" $'**Why now**\nGenerated services need a first-day guide.\n\n**Verified by**\nChecklist walked once end to end on a generated service.\n\n**Depends on**\nThe template package.'
mk "Practice pages for every Tier 1 item" "tier-1,theme:governance" $'**Why now**\nSame rule as Tier 0: the rationale is the product.\n\n**Verified by**\nEvery page has the six parts; sources resolve.\n\n**Depends on**\nEach Tier 1 item.'
```

- [ ] **Step 3: Tier 2 and gated issues**

```bash
mk "Architecture tests project; library chosen in an ADR" "tier-2,theme:api" $'**Why now**\nTrigger: the service is deployed.\n\n**Verified by**\nA forbidden reference (endpoint to DbContext, contract to entity, environment read outside Program.cs) fails the test.\n\n**Depends on**\nnone'
mk "HTTP logging and OpenTelemetry via Microsoft packages" "tier-2,theme:config" $'**Why now**\nTrigger: the service is deployed and someone is on call.\n\n**Verified by**\nTraces visible in a local collector.\n\n**Depends on**\nnone'
mk "Dockerfile, Trivy scan, SBOM" "gated,theme:security" $'**Why now**\nTrigger: container deployment chosen.\n\n**Verified by**\nImage builds; scan and SBOM artifacts produced.\n\n**Depends on**\nDeploy target decision.'
mk "Reusable workflows and shared build props package" "gated,theme:build" $'**Why now**\nTrigger: a second service exists.\n\n**Verified by**\nThe second service consumes them with no copied files.\n\n**Depends on**\nA second service.'
mk "OpenAPI style lint with Spectral" "gated,theme:api" $'**Why now**\nTrigger: an external consumer depends on the contract.\n\n**Verified by**\nLint passes on the committed document.\n\n**Depends on**\nCommitted OpenAPI document.'
mk "Auth scheme" "gated,theme:security" $'**Why now**\nTrigger: a consumer needs identity, or a security practice needs demonstrating.\n\n**Verified by**\nIntegration test proves unauthenticated requests are rejected.\n\n**Depends on**\nnone'
mk "DELETE and pagination" "gated,theme:api" $'**Why now**\nTrigger: a consumer asks for them.\n\n**Verified by**\nEndpoint tests.\n\n**Depends on**\nnone'
mk "Conditional strip of the sample domain in the template" "gated,theme:template" $'**Why now**\nTrigger: renaming by hand proves painful twice.\n\n**Verified by**\n`dotnet new --sample false` builds.\n\n**Depends on**\nThe template package.'
mk "Devcontainer" "gated,theme:dx" $'**Why now**\nTrigger: onboarding a machine takes more than an hour.\n\n**Verified by**\nA fresh container builds and tests.\n\n**Depends on**\nnone'
mk "Second persistence adapter behind the repository interfaces, with contract tests" "gated,theme:api" $'**Why now**\nTrigger: the template package lands, or someone needs to run the service without Docker. Options and trade-offs: docs/decisions/0006-second-persistence-adapter.md.\n\n**Verified by**\nOne contract suite passes against both adapters; database-only tests unchanged.\n\n**Depends on**\nThe template package (for the first trigger).'
```

- [ ] **Step 4: Verify the count and the roadmap links**

```bash
gh issue list --repo $R --label tier-1 --json number --jq length
gh issue list --repo $R --label tier-2 --json number --jq length
gh issue list --repo $R --label gated --json number --jq length
```

Expected: `10`, `2`, `8`. Open each query link in `docs/roadmap.md` and confirm it lists the issues.

---

### Task 18: Practice-page audit and spec status

**Files:**
- Modify: `docs/specs/2026-09-13-reference-repo-design.md` (status line), possibly any `docs/practices/*.md`

- [ ] **Step 1: Branch**

```bash
git checkout main && git pull && git checkout -b tier0/18-audit
```

- [ ] **Step 2: Check every page has the six parts in order**

```bash
for f in docs/practices/*.md; do
  printf '%s: ' "$f"
  grep -E '^## (What|Why|Source|Enforced by|Opt out|Go deeper)$' "$f" | tr '\n' ' '
  echo
done
```

Expected: every line ends with `## What ## Why ## Source ## Enforced by ## Opt out ## Go deeper`. Fix any page that deviates.

- [ ] **Step 3: Check every Source link resolves**

```bash
grep -rhoE 'https?://[^ )>]+' docs/practices | sort -u | while read -r url; do
  code=$(curl -s -o /dev/null -w '%{http_code}' -L "$url")
  echo "$code $url"
done | grep -v '^200' || echo "all links resolve"
```

Expected: `all links resolve`. Fix any that do not.

- [ ] **Step 4: Confirm all Tier 0 checks are required and green**

```bash
gh api repos/monomaniacal/dotnet-service-reference/branches/main/protection --jq '.required_status_checks.contexts'
gh run list --repo monomaniacal/dotnet-service-reference --branch main --limit 1 --json conclusion --jq '.[0].conclusion'
```

Expected: the four `tier0-*` names; `success`.

- [ ] **Step 5: Update the spec status**

Change the spec's status line to:

```markdown
Status: Tier 0 implemented (items 1 to 18). Backlog lives in GitHub issues; see docs/roadmap.md.
```

- [ ] **Step 6: Commit and open PR**

```bash
git add docs
git commit -m "Audit practice pages and mark Tier 0 complete in the spec

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>"
git push -u origin tier0/18-audit
gh pr create --base main --title "Tier 0: audit and close" --body "Backlog item 16 (pages have six parts, sources resolve) and item 18 done. Tier 0 complete.

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
```

---

## Notes for the executor

- The spec's CI table lists `tier0-diff-coverage` as its own job. This plan runs diff-cover as a step inside `tier0-unit` so no artifact hand-off is needed. Four required checks, not five.
- Task 9 follows the spec and moves the local connection string to user secrets. The practice page's Opt out section records the trade-off. Do not put it back without updating the page.
- Task 10's entry-assembly guard is the mechanism Microsoft documents for build-time generation. If a future .NET release adds `OpenApiGenerationEnvironment` (documented for .NET 11), the guard can be replaced by generating in a dedicated environment.
- If `dotnet format --include ${staged}` in the Husky task rejects the path form on Windows, add `"pathMode": "relative"` to that task.
- If the coverage report's `filename` attributes are relative to the test project rather than the repo root, add `--src-roots src` to the diff-cover command and remove the `sed` line.
