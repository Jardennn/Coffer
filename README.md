# Coffer - dev branch

This branch features the C# implementation of the MVP code, this is the implementation for the final tested product.
Expect untested and unfinished code from this branch, tested and finished code can be found on the `main` branch.

## Structure

The folder structure features 2 (currently, GUI will be added later) namespaces: `Coffer.Core` and `Coffer.Services`

* `Coffer.Core`: The CLI version of the program, handles output and execution of services.

* `Coffer.Services`: Class library that contains functions (services) that both the CLI and future GUI version use.
  * `Services folder`: This folder is for organizing, it contains the core services for running the backup (Copier, Verifier, Preflight, etc...).


## Running it

Currently only the CLI option is available. You need to have dotnet installed on your machine to run this, I personally use dotnet 10.

To run the CLI program, you'll need to be inside the `Coffer.Core` folder and run `dotnet run`

```
cd Coffer.Core && dotnet run
```

## Status and completion points

> The completion points here are going to be similar to the completion points on the `main` branch.
> Points here are representing milestones that were reached but not pushed to `main` or tested.

- [x] Core services and progress reporting (Copier, Verifier, Scanner)
- [x] Filters
- [x] Preflight checks
- [x] Profile save/load
- [ ] GUI version
- [ ] Full project complete
