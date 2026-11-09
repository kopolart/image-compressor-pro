# Architecture

This document describes how Image Compressor & Resizer Pro is put together and
why the main design decisions were made. It covers the current codebase;
roadmap items are not included.

## Goals

- Keep image work independent from the UI so it can be tested and changed safely.
- Never freeze the window, even while encoding large images.
- Do not hold a lock on the source file.
- Match the estimate with the real saved output by using one encoding pipeline.
- Support WebP without adopting a copyleft or commercial codec license.

## High-level view

```text
┌──────────────────────────── MainForm (UI thread) ────────────────────────────┐
│                                                                             │
│  Browse ─► ImageLoader ─► originalImage ─► ImageProcessor ─► pbOriginal      │
│                                │                 │                           │
│                                │                 ├─► pbProcessed (preview)    │
│                                │                 └─► ImageEncoder (save)     │
│                                │                                             │
│                                └─► SizeEstimator ─► Task.Run(ImageEncoder)  │
│                                          │                                   │
│                                          └─► estimate label (BeginInvoke)    │
└─────────────────────────────────────────────────────────────────────────────┘
```

`ImageCompression.Core` contains the image logic. `MainForm` only builds
controls, reads user input and wires events.

## Responsibilities

| Type | Responsibility |
| --- | --- |
| `Program` | Enables visual styles, installs global exception handlers, starts the form. |
| `MainForm` | Control tree, settings state, preview rendering, debounce timers, save dialog. |
| `ImageLoader` | Reads the file into memory, decodes with GDI+, applies EXIF orientation, falls back to ImageSharp. |
| `ImageProcessor` | High-quality resize and white-background flattening. |
| `ImageEncoder` | Format dispatch, JPEG quality parameters, WebP encoding, flattening decision. |
| `OutputFormatInfo` | One table describing every output format: name, dialog filter, GDI+ format, quality and transparency support. |
| `SizeEstimator` | Debounced background encoding, stale-result rejection, UI-thread marshaling. |

## Data flows

### Loading an image

1. `Browse Image` opens an `OpenFileDialog` filtered to supported extensions.
2. `ImageLoader.LoadWithoutLockingFile` reads the whole file into a byte array,
   so the app never keeps a file handle open.
3. GDI+ decodes the bytes into a `Bitmap` and applies the EXIF orientation tag
   by rotating the copy.
4. If GDI+ rejects the format (`ArgumentException`), ImageSharp decodes it and
   saves a lossless PNG into memory; the PNG is then loaded with GDI+ so the rest
   of the app only deals with `System.Drawing.Image`.
5. The new bitmap replaces the previous one only after a successful load. The
   old images are detached from the picture boxes before disposal, so a repaint
   cannot touch a disposed image.

### Resizing and preview

`ImageProcessor.Resize` draws the source into a new bitmap with
`Graphics.DrawImage`, high-quality compositing, and a `TileFlipXY` wrap mode to
avoid edge artifacts. The interpolation mode comes from the "Quality Mode"
combo box.

### Estimating output size

```text
settings change
   └─ SizeEstimator.Request()          (restart 350 ms debounce)
        └─ debounce tick
             ├─ create resized snapshot on the UI thread (GDI+ is not thread-safe)
             ├─ capture format + quality and bump the request version
             └─ Task.Run(encode to MemoryStream)
                  ├─ dispose the snapshot
                  └─ if version still current: BeginInvoke -> update label
```

Two debounce timers exist for different jobs: `SizeEstimator` (350 ms) for the
size label and `MainForm.autoPreviewTimer` (400 ms) for the optional preview.
They are deliberately independent so the label still updates when auto-preview
is off.

### Saving

`Save Image` does **not** reuse the preview bitmap. It resizes the original
again with the current settings and encodes straight to the chosen file. This
guarantees the saved file always matches the visible settings.

`ImageEncoder` distinguishes three cases:

- **JPEG** &mdash; GDI+ encoder with an explicit `Encoder.Quality` parameter.
- **WebP** &mdash; GDI+ has no WebP codec, so the bitmap is written to a
  lossless PNG stream, decoded by ImageSharp, then encoded as WebP. Quality 100
  selects lossless WebP; lower values select lossy.
- **Everything else** &mdash; GDI+ `Image.Save` with the format from the table.
  JPEG and BMP are first flattened onto a white background because they cannot
  represent alpha.

## Ownership rules

| Object | Owner | Rule |
| --- | --- | --- |
| Bitmap returned by `ImageLoader` | `MainForm` | Disposed on replacement and on form close. |
| Bitmap returned by `ImageProcessor.Resize` | Caller | Caller disposes. |
| Snapshot passed to `SizeEstimator` | `SizeEstimator` | Disposed on the worker thread in a `finally` block. |
| Flattened copy inside `ImageEncoder` | `ImageEncoder` | Created and disposed locally. |
| Image passed to `ImageEncoder` | Caller | Never disposed by the encoder. |

GDI+ images are not thread-safe. Only the UI thread creates, draws and disposes
`Bitmap` instances; background threads only read already-created snapshots for
encoding.

## Design decisions

**GDI+ first, ImageSharp only for WebP.** GDI+ is part of Windows, has no extra
deployment cost and is fast for the common formats. ImageSharp is pinned to
2.1.11, the last Apache-2.0 release, because the project must stay permissively
licensed.

**A single format table.** `OutputFormatInfo.All` drives the combo box, dialog
filters, quality enablement and encoder behavior. Adding a format means touching
one table entry plus, if needed, one encoder branch.

**PNG as the bridge between imaging stacks.** Both directions (ImageSharp to
GDI+ on load, GDI+ to ImageSharp on save) use an in-memory lossless PNG, which
keeps alpha intact and avoids reimplementing pixel conversion.

**Versioned estimation.** Rapid slider movement starts many encodes. A version
counter means only the newest result reaches the UI, and disposing the estimator
invalidates everything in flight.

**Explicit event suppression.** Changing width can programmatically change
height when the aspect ratio is locked. A `suppressSettingsEvents` flag prevents
re-entrant handlers and duplicate estimates.

**Always re-encode on save.** Reusing a cached preview would risk saving a stale
image after the user changes settings but does not press Process.

## Extending

### Add an output format

1. Add a value to `OutputFormat`.
2. Add one row to `OutputFormatInfo.All` with its display name, dialog filter,
   GDI+ format, quality and transparency flags.
3. If the format has no GDI+ encoder (like WebP), add a branch in
   `ImageEncoder.EncodeToStream` and update the file dialog filter accordingly.

### Add an interpolation mode

1. Add the item to `cmbInterpolation` in `MainForm`.
2. Map the new index in `GetInterpolationMode`.

Keep the combo box order and the mapping in sync.

## Known limitations

- One image at a time; no batch queue.
- Animated GIFs are decoded as a single frame when re-encoded.
- EXIF orientation is applied to pixels but metadata is not copied to the output.
- ICC color profiles are not preserved.
- The layout is fixed-size and the executable has no DPI-awareness manifest, so
  Windows bitmap-scales the window on high-DPI displays.
- Saving writes directly to the target path; an I/O failure during the write can
  leave a partial file.
- Size estimation performs a full encode per debounced change; very large
  images can still cost noticeable CPU time.
