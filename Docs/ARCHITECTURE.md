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

Balance multipliers live in one authored profile covering four difficulty tiers
and five biome depths. Runtime performance telemetry uses a fixed ring buffer;
it never allocates on update. Localization is stable-key based, always falls
back to English source, and supports `qps-ploc` pseudo-localization for layout
and glyph QA.

`ReleaseReadinessValidator` is the shared local/CI/build boundary. It rejects
missing or invalid content, achievement, localization and balance catalogs, or
an incomplete build-scene list before a player build starts.

`RunSession` is the authoritative, scene-independent run aggregate. It owns the
master seed, selected class, deterministic 40-room plan, biome/room cursor,
elapsed time, persisted health and run statistics. `RunChamberController`
adapts the current room plan into combat, guardian or utility presentation;
scene reloads never regenerate or mutate the route. `HubController` owns the
session lifecycle because it already persists across hub/run transitions.

`RadianceField` is the geometric authority for carried-light exposure; enemy
brains query it without coupling to visual `Light` components. Class passives
are applied from stable tags by `ClassPassiveController`. Vow state, room
measurements, outcomes and downstream modifiers live in `RunSession`, so scene
presentation cannot reroll or erase consequences.

The content catalog now owns Echo definitions alongside classes, biomes,
enemies and guardians. `RunPlayerInitializer` rehydrates gold and the
three-slot Echo chain on each chamber load, while deterministic room rewards
write stable IDs back into the session.
