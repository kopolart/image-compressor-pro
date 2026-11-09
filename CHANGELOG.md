# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned

- Application icon and high-DPI layout polish
- Batch processing for multiple files
- Drag-and-drop image loading
- Migration path for the ImageSharp dependency (see [SECURITY.md](SECURITY.md))

## [1.0.0] - 2026-11-08

### Added

- Load JPEG, PNG, WebP, BMP, GIF and TIFF images without locking the source file.
- Automatic EXIF orientation correction for phone photos.
- Resize with optional aspect-ratio lock and three interpolation modes
  (high quality, medium, fast).
- Quality slider for JPEG and WebP, with lossless WebP at 100%.
- Transparency preservation for PNG, WebP, GIF and TIFF; white flattening for
  JPEG and BMP.
- Live output-size estimation computed on a background thread with debouncing.
- Optional auto-preview when settings change.
- Save dialog that always re-encodes from the current settings.
- Global error handling that reports unexpected failures without crashing.
- CI build and automated GitHub Releases for tagged versions.
