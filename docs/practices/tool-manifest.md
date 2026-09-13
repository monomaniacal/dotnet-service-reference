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
