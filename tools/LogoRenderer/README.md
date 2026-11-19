# LogoRenderer

Draws the application logo and writes the files the application and the README use:

| File | Used by |
|------|---------|
| `assets/app.ico` | `ApplicationIcon` of the desktop project, and the icon every window shows |
| `assets/logo-256.png` | Anywhere a raster logo is wanted |

`assets/logo.svg` is the same drawing as a vector. It is written by hand from the numbers
in `LogoPainter.cs`, which is the source of truth: change the painter first, then bring the
SVG in line with it.

## Running it

From the repository root, on Windows:

```
dotnet run --project tools/LogoRenderer -- assets
```

Add `--preview` after the directory to also write `preview-<size>.png` for every icon frame,
which is the easiest way to check how the small frames read. Do not commit the previews.

The tool only draws into off-screen bitmaps; it never opens a window.

## The icon file

`app.ico` holds seven frames: 16, 24, 32, 48, 64, 128 and 256 px, all 32-bit with alpha.
The 256 px frame is stored as PNG, the way Windows expects a frame of that size; the
smaller ones are uncompressed bitmaps, which every icon reader understands. Frames below
32 px drop the fine detail (the spine line and the controller's buttons) and keep their
edges on whole pixels, so they stay sharp instead of turning grey.
