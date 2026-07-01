# Architecture

`Lanternfall.Core` owns deterministic rules and has no scene or prefab
dependencies. Future Combat, World, Progression, UI, and Audio assemblies depend
inward through interfaces and typed events. ScriptableObjects contain immutable
authored definitions; runtime state remains separate.

Each run has a 64-bit master seed. Named substreams prevent cosmetic random
calls from changing rooms or loot. Unity's global Random is presentation-only.

Hot paths allocate no managed memory after warm-up. Projectiles, effects,
damage numbers, and common enemies are pooled. Save data uses versioned DTOs,
atomic replacement, backups, checksums, and stepwise migrations.

The HUD is a reusable runtime presenter driven by `Health`, inventory and typed
guardian signals; scene builders attach it without embedding scene-specific
state. `AccessibilityRuntime` is the read-only presentation boundary for the
saved settings profile. Input binding overrides remain Input System JSON in the
versioned save DTO. Adaptive audio consumes encounter signals and currently
uses an original procedural motif so every build is complete and license-safe.

Achievements are immutable catalog entries with stable IDs. The release catalog
is builder-generated and build-validated at exactly 100 entries, while mutable
progress and unlock ownership stay in save-backed runtime state.
