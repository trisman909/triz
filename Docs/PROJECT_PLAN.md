# Lanternfall production plan

Updated: 2026-07-01

| Phase | Deliverable | Status |
|---|---|---|
| 0 | Original identity, scope, risks | Complete |
| 1 | Unity/URP foundation, deterministic core, tests | Complete |
| 2 | Player movement, dodge, input, isometric camera | Pending |
| 3 | Damage, weapons, abilities, pooling | Pending |
| 4 | Enemy state machines and encounter director | Pending |
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
