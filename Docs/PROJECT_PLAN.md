# Lanternfall production plan

Updated: 2026-07-01

| Phase | Deliverable | Status |
|---|---|---|
| 0 | Original identity, scope, risks | Complete |
| 1 | Unity/URP foundation, deterministic core, tests | Complete |
| 2 | Player movement, dodge, input, isometric camera | Complete |
| 3 | Damage, weapons, abilities, pooling | Complete |
| 4 | Enemy state machines and encounter director | In progress |
| 5 | Modular rooms and seeded run generation | Pending |
| 6 | Rewards, resonance builds, economy | Pending |
| 7 | First-biome vertical slice and three bosses | Pending |
| 8 | Hub, saves, meta progression, NPC quests | Pending |
| 9 | Five-biome content production | Pending |
| 10 | UI, audio, accessibility, 100 achievements | Pending |
| 11 | Optimization, balance, QA, localization | Pending |
| 12 | Windows release candidate | Pending |

## Phase rules

Each phase must import without errors, pass relevant EditMode/PlayMode tests,
meet its performance budget, update documentation, and land as one coherent
commit before the next phase begins.

## Release definition

- 25–45 minute hub-to-run loop; five skill-distinct classes.
- At least 40 readable enemy families and 15 multi-phase bosses.
- Keyboard/mouse and controller remapping plus reduced motion, scalable UI,
  subtitles, color-safe telegraphs, and configurable flash/shake.
- Stable 60 FPS on the declared minimum Windows PC.
- Versioned, migrated, atomic saves; no S0/S1 defects or unlicensed content.

## Current gate

- [x] Unity 6000.5.1f1 Supported project and URP/Input System dependencies.
- [x] Assembly boundaries and deterministic run-domain utilities.
- [x] Automated tests for random streams, events, stats, and room graph.
- [x] Install Unity Editor 6000.5.1f1 in Hub.
- [x] Resolve packages and import the project.
- [x] Pass compilation and all EditMode tests (4/4).
- [x] Install Git for Windows.
- [x] Commit Phase 1.

## Phase 2 verification

- [x] Device-neutral Input System action map with keyboard/mouse and gamepad
  bindings.
- [x] Accelerated CharacterController movement, sprint, facing and gravity.
- [x] Directional dodge with bounded invulnerability and cooldown.
- [x] Fixed isometric camera with damped follow, shake and boss-zoom hooks.
- [x] Generated URP combat sandbox with lighting, collisions and build setup.
- [x] EditMode tests: 5/5 passed.
- [x] PlayMode integration tests: 1/1 passed.
- [x] Windows x64 development smoke build succeeded (164,646,971 bytes).

## Phase 3 verification

- [x] Deterministic damage pipeline with critical hits, elements and armor.
- [x] Event-driven health/death component and damageable contract.
- [x] Reusable component pool with 32 prewarmed projectiles.
- [x] Three authored weapons: Cinder Staff, Prism Bow and Echo Blades.
- [x] Two authored abilities: Radiant Burst and Gloam Well.
- [x] Mouse/gamepad aiming, primary-fire and ability input bindings.
- [x] Sandbox projectile combat and three armored target dummies.
- [x] EditMode tests: 6/6 passed.
- [x] PlayMode integration tests: 2/2 passed.
- [x] Windows x64 development smoke build succeeded (164,668,599 bytes).
