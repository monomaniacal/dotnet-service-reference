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
