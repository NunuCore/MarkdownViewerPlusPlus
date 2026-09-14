# Changelog

## 0.8.2-local-image-fix.1

* Fixed relative local image paths in the preview.
* Relative paths are resolved against the directory of the open Markdown file.
* Absolute Windows paths continue to work.
* `file:///` paths continue to work.
* HTTP and HTTPS images continue to use the existing web loading path.
* Local SVG loading remains supported.
