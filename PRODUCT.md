# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

- **Primary**: .NET 10 / Blazor and `.NET MAUI Blazor Hybrid` developers building read-only entity inspectors, audit timelines, secret/credential viewers, assessment media players, and document preview lightboxes.

## Product Purpose

`klyroo-display` provides a headless NativeAOT presentation & file-preview core (`Klyroo.Display.Core`) paired with 19 read-only `<KlyDisplay>` blocks, constrained audio/video players, and a modal multi-format file previewer (`Klyroo.Display`).

Success means teams can render structured customer dossiers, before/after diffs, quota meters, masked API keys, constrained exam media players, and CSV/PDF/image lightboxes consistently without assembling brittle one-off components.

## Positioning

Unlike general UI kits that leave read-only dossiers, secret masking, quota-limited media playback (`Start`/`End` clips, `MaxPlays`, `Seekable=false`), and multi-format CSV/PDF/media lightboxes to custom app code, `Klyroo.Display` packages all 19 read-only patterns plus headless MIME/CSV parsing into a NativeAOT two-layer split under MIT.

## Operating Context

- **Packages**: `Klyroo.Display.Core` (headless MIME/extension detection, RFC 4180 CSV parser, display models, `FilePreviewFile`) and `Klyroo.Display` (`KlyDisplay`, constrained media players, `FilePreviewService` modal lightbox, `klyroo-display.css`).
- **Showcase**: Powers the `Inspector` tab (Facts, Seat Quota Meter, Masked Webhook Signing Key, Lifecycle Timeline) and `Preview CSV` modal lightbox in `Klyroo.Stack.Site` `/showcase`.

## Capabilities and Constraints

- `Klyroo.Display.Core` is 100% headless with `<IsAotCompatible>true</IsAotCompatible>` and `<IsTrimmable>true</IsTrimmable>`.
- 19 `DisplayKind` blocks (`Heading`, `Text`, `Alert`, `Callout`, `Facts`, `Stat`, `Breakdown`, `Meter`, `Diff`, `Code` with `secret` masking, `Timeline`, `Empty`, `Accordion`, etc.).
- Constrained audio/video playback enforcing clip bounds (`Start`/`End`), `MaxPlays` quotas, and optional seek locking.
- Ships precompiled zero-friction CSS (`klyroo-display.css`) alongside native Tailwind CSS v4 support.
- 100% MIT open-source license; strict `Klyroo.Display*` / `KlyDisplay*` / `--klyroo-*` naming.

## Brand Commitments

- **Name**: `Klyroo.Display` (`klyroo-display`), part of the **Klyroo** ecosystem.
- **Voice**: Structured, clear, and security-conscious for read-only inspection surfaces.

## Evidence on Hand

- Passing unit and bUnit test suite (`tests/Klyroo.Display.Tests`) plus NativeAOT publish verification in CI.
- Live Account Dossier inspector and CSV lightbox in `Klyroo.Stack.Site` `/showcase` and product docs at `/display`.
- Verified precompiled standalone CSS (`klyroo-display.css`).
- **Explicit absences (do not fabricate)**: No NuGet download stats, customer logos, or third-party testimonials exist yet.

## Product Principles

1. **Read-only clarity**: Inspection views should make copying IDs, checking thresholds, scanning timelines, and spotting empty states effortless.
2. **Security-first secret presentation**: Sensitive tokens render masked by default with explicit reveal and copy affordances.
3. **Deterministic media constraints**: Assessment and training playback rules (`Start`/`End`, `MaxPlays`, seek lock) are enforced reliably in C# state.
4. **Zero-dependency file preview**: Common CSV, image, PDF, audio, and video previews work locally without third-party SaaS viewers.

## Accessibility & Inclusion

- Durable standard: **WCAG 2.1 AA** across all 19 display blocks, media players, and lightbox modals: semantic headings and definition lists, keyboard-accessible copy/reveal/lightbox controls, subtitle track support for media players, and text alternatives for status meters and thresholds.
