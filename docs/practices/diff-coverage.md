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
