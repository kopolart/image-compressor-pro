# Third-party notices

Image Compressor & Resizer Pro is distributed under the Apache License 2.0.
It depends on the following third-party packages. Full license texts are
available in each package's repository and in the NuGet package cache after
restore.

## Runtime dependencies

| Package | Version | License | Project |
| --- | --- | --- | --- |
| SixLabors.ImageSharp | 2.1.11 | Apache-2.0 | https://github.com/SixLabors/ImageSharp |
| System.Buffers | 4.5.1 | MIT | https://github.com/dotnet/runtime |
| System.Memory | 4.5.4 | MIT | https://github.com/dotnet/runtime |
| System.Numerics.Vectors | 4.5.0 | MIT | https://github.com/dotnet/runtime |
| System.Runtime.CompilerServices.Unsafe | 5.0.0 | MIT | https://github.com/dotnet/runtime |
| System.Text.Encoding.CodePages | 5.0.0 | MIT | https://github.com/dotnet/runtime |

`SixLabors.ImageSharp` is used exclusively to decode and encode WebP images,
because GDI+ has no WebP codec. It is intentionally pinned to the 2.1.x line,
the last line published under Apache-2.0. ImageSharp 3.x and later are
published under the Six Labors Split License; see
[SECURITY.md](SECURITY.md#dependency-advisories) for the trade-offs.

## Development-only tooling

Build and CI use the .NET SDK, MSBuild and GitHub Actions, which are not
redistributed with the application.

## Fonts

The application uses the Segoe UI font supplied with Windows. No font files are
redistributed.
