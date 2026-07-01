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

