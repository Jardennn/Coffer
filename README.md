# Coffer

Cross-platform local backup tool with smart filtering and integrity verification.

> Still under work in progress. Python MVP on the `mvp` branch,
> C# implementation and untested in development code on `dev`.

## Planned features
- Smart file filtering (type, size, date, exclusion)
- Chunked transfer with progress reporting
- SHA256 integrity verification
- Backup profiles saved between runs (Saves the last run profile and asks the user about saving that profile in storage)
- Cross-platform (Windows, Linux, MacOS)

## Status and completion points

- [x] Core services and progress reporting (Copier, Verifier, Scanner)
- [x] Filters
- [ ] Preflight checks
- [ ] Profile save/load
- [ ] GUI version
- [ ] Full project complete
