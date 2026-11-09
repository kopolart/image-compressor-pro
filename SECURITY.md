# Security policy

## Supported versions

| Version | Supported |
| --- | --- |
| Latest release | Yes |
| Older releases | No |

Only the most recent release receives security fixes. Please reproduce issues
on the latest release before reporting them.

## Reporting a vulnerability

Do **not** open a public issue for a security problem.

Use GitHub's private vulnerability reporting:

1. Open https://github.com/kopolart/image-compressor-pro/security/advisories/new
2. Describe the issue, affected version, and reproduction steps.
3. Include a sample image if the problem is triggered by a crafted file
   (keep it as small as possible).

You can expect an initial response within 7 days. Once a fix is available, a
patched release will be published and the advisory will be made public with
credit to the reporter, unless you ask to remain anonymous.

## Scope

Image Compressor & Resizer Pro runs fully offline and does not transmit images,
telemetry, or identifiers. Relevant security areas are:

- Memory safety and denial of service while decoding untrusted images
- Path handling when saving output files
- Dependency vulnerabilities in the image codecs

Out of scope: issues that require an attacker to already control the user's
machine or replace the application binaries.

## Dependency advisories

NuGet audit reports the following advisories for SixLabors.ImageSharp 2.1.11,
the last Apache-2.0 release of the library:

- GHSA-gwg2-r3hj-4w44 (moderate)
- GHSA-j3p4-wp97-rph4 (high)
- GHSA-j9gm-c75j-xc9q (high)
- GHSA-jjfr-hcj7-qf5w (high)
- GHSA-wmxv-xphr-5c9g (moderate)

The practical risk for this application is limited to decoding crafted image
files that the user explicitly opens. Mitigations:

- Only open images from sources you trust.
- Run the application with a standard user account.
- If you need the fixes, upgrade ImageSharp to 3.1.x or later. Note that this
  moves the dependency to the Six Labors Split License, which may require a
  commercial license for some organizations; the upgrade is therefore a
  deliberate licensing decision rather than a routine bump.
