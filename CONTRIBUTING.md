# Contributing to Image Compressor & Resizer Pro

Thanks for taking the time to contribute. This document covers the workflow,
local setup and the standards we use for changes.

## Before you start

- Search [existing issues](https://github.com/kopolart/image-compressor-pro/issues)
  before opening a new one.
- For anything larger than a bug fix, open an issue first so the approach can be
  agreed on before you write code.
- By participating you agree to follow our
  [Code of Conduct](CODE_OF_CONDUCT.md).

## Development setup

### Prerequisites

- Windows 10 1903+ or Windows 11
- [.NET SDK 8.0 or later](https://dotnet.microsoft.com/download)
- Optional: Visual Studio 2022 with the **.NET desktop development** workload,
  which also installs the .NET Framework 4.8 targeting pack

### Build and run

```bash
git clone https://github.com/kopolart/image-compressor-pro.git
cd image-compressor-pro
dotnet build ImageCompression.sln -c Release
```

The executable is produced at
`ImageCompression/bin/Release/net48/ImageCompression.exe`.

To create a distributable folder:

```bash
dotnet publish ImageCompression/ImageCompression.csproj -c Release -o dist
```

## Project layout

| Path | Contents |
| --- | --- |
| `ImageCompression/MainForm.cs` | UI construction and event wiring only |
| `ImageCompression/Core/ImageLoader.cs` | File loading, EXIF orientation, WebP fallback |
| `ImageCompression/Core/ImageProcessor.cs` | Resizing and transparency flattening |
| `ImageCompression/Core/ImageEncoder.cs` | GDI+ and ImageSharp encoding |
| `ImageCompression/Core/OutputFormat.cs` | Format table (single source of truth) |
| `ImageCompression/Core/SizeEstimator.cs` | Debounced background size estimation |
| `docs/ARCHITECTURE.md` | Design notes and threading model |

Keep UI code in `MainForm`; image operations belong in `Core` and must remain
independent of any control instance.

## Coding standards

- Follow `.editorconfig`; Visual Studio applies it automatically.
- Match the existing style: four-space indentation, `_camelCase` private fields,
  braces on all control-flow statements.
- Comments explain *why*, not *what*. Do not add commented-out code.
- Every `IDisposable` you create must be disposed. Images passed between
  layers follow the ownership documented on the method that receives them.
- Do not add a dependency without discussing it first. New dependencies must be
  license-compatible with Apache-2.0.

## Commit messages

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add drag-and-drop image loading
fix: preserve alpha channel when saving WebP
docs: clarify build prerequisites
chore: bump actions/checkout to v4
```

Keep the subject in the imperative mood and under 72 characters.

## Pull requests

1. Fork the repository and create a topic branch:
   `git checkout -b feat/short-description`
2. Make focused changes; avoid unrelated refactors in the same pull request.
3. Verify the solution builds in Release configuration.
4. Test the affected flow manually, including the empty/no-image state and at
   least one transparent PNG.
5. For UI changes, attach a screenshot or short recording.
6. Update `CHANGELOG.md` under `[Unreleased]` when behavior changes.
7. Open the pull request using the template and link the related issue.

## Reporting bugs

Use the [bug report template](https://github.com/kopolart/image-compressor-pro/issues/new?template=bug_report.yml).
Include the version, Windows build, input format, expected and actual behavior,
and a screenshot when the UI is involved. Never attach private photos; create a
minimal sample instead.

## License

By contributing you agree that your contributions are licensed under the
Apache License 2.0, the same license as the project (inbound = outbound).
