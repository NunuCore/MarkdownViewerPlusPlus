# MarkdownViewerPlusPlus local image fix

This repository contains a small community fix for MarkdownViewer++ 0.8.2.

The fix restores support for relative local image paths in Markdown previews, for example:

```markdown
![Example](./images/example.png)
```

## Problem

MarkdownViewer++ can render the Markdown correctly while still failing to load a local image referenced with a relative path. The preview then shows a broken image placeholder.

The problem is in the local image loader. It attempts to construct an absolute `System.Uri` from the image source before relative path handling can run. Relative paths such as `./images/example.png` cause that step to fail, and the exception is silently ignored.

## Fix

The image loader now checks relative local paths before absolute URI handling. Relative paths are resolved against the directory of the currently opened Markdown file.

The change keeps support for:

* relative local image paths
* absolute Windows paths
* `file:///` paths
* HTTP and HTTPS images
* local SVG files

## Tested version

* MarkdownViewer++ 0.8.2.0
* Assembly version 0.8.2.25525

The patched DLL was tested with a Markdown file that references PNG files from a sibling image directory.

## Installation

1. Close Notepad++ completely.
2. Back up the existing `MarkdownViewerPlusPlus.dll`.
3. Copy the patched DLL from `bin/` into the MarkdownViewerPlusPlus plugin directory.
4. Replace the existing DLL.
5. Start Notepad++ again.

A common installation path is:

```text
C:\Program Files\Notepad++\plugins\MarkdownViewerPlusPlus\
```

For portable installations, use the matching `plugins\MarkdownViewerPlusPlus` directory.

## Source change

The source level fix is included in `src/MarkdownViewerRenderer.cs` and as a standalone patch in `patches/relative-local-image-fix.patch`.

The original project is available at:

https://github.com/nea/MarkdownViewerPlusPlus

Relevant reports include:

* https://github.com/nea/MarkdownViewerPlusPlus/issues/73
* https://github.com/nea/MarkdownViewerPlusPlus/issues/134

## Scope

This repository only covers the relative local image path issue. It is not a continuation of the original project and does not provide general MarkdownViewer++ support.

## License

The original project uses the MIT License. The original copyright notice and license text are included in `LICENSE.md`.

This is an unofficial community fix and is not affiliated with the original author.
