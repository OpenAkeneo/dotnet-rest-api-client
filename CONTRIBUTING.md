# Contributing to OpenAkeneo.RestApiClient

Thank you for your interest in contributing. All contributions are welcome — bug reports, feature requests, documentation improvements, and pull requests.

---

## Getting Started

1. Fork the repository and clone your fork
2. Copy `appsettings.example.json` to `appsettings.json` in both `OpenAkeneo.RestApiClient.SandboxTests/` and (if applicable) your local test project, and fill in your credentials
3. Open `OpenAkeneo.RestApiClient.sln` in Visual Studio or Rider
4. Build the solution: `dotnet build`

---

## Making Changes

- Keep changes focused — one concern per pull request
- Follow the existing code style (see `.editorconfig`)
- Add or update XML doc comments on any public API you touch
- Run the build before opening a PR: `dotnet build --configuration Release`

---

## Pull Requests

- Target the `main` branch
- Include a clear description of what changed and why
- Reference any related issues

---

## Sandbox Tests

The `OpenAkeneo.RestApiClient.SandboxTests` project runs against a live Akeneo instance. Be aware:

- **Requires a pre-configured live Akeneo instance** — tests will not pass on a fresh instance without prior data setup
- **Not idempotent** — some resources created by tests cannot be deleted via the API (Akeneo does not support DELETE for all resource types)
- **Credentials must never be committed** — use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local dev (`dotnet user-secrets set ...`). See `appsettings.example.json` for the expected keys
- Sandbox tests are not required to pass in CI — they serve as integration validation against a real environment

Unit tests (`OpenAkeneo.RestApiClient.UnitTests`) require no credentials and are the primary CI gate.

---

## Reporting Security Issues

Please do not open a public GitHub issue for security vulnerabilities. See [SECURITY.md](SECURITY.md) for the responsible disclosure process.
