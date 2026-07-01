# Lanternfall production plan

Updated: 2026-07-01

| Phase | Deliverable | Status |
|---|---|---|
| 0 | Original identity, scope, risks | Complete |
| 1 | Unity/URP foundation, deterministic core, tests | Complete |
| 2 | Player movement, dodge, input, isometric camera | Complete |
| 3 | Damage, weapons, abilities, pooling | Complete |
| 4 | Enemy state machines and encounter director | Complete |
| 5 | Modular rooms and seeded run generation | Complete |
| 6 | Rewards, resonance builds, economy | Complete |
| 7 | First-biome vertical slice and three bosses | Complete |
| 8 | Hub, saves, meta progression, NPC quests | Complete |
| 9 | Five-biome content production | Complete |
| 10 | UI, audio, accessibility, 100 achievements | Complete |
| 11 | Optimization, balance, QA, localization | Complete |
| 12 | Windows RC pipeline and completion audit | Complete |
| 13 | Integrated 25–45 minute run and class selection | Complete |
| 14 | Radiance geometry, Vows, full Echo reward cadence | Complete |
| 15 | Five playable biomes and distinct release encounters | In progress |
| 16 | Production presentation, audio, UI and achievement wiring | Pending |
| 17 | Minimum-PC QA and final Windows release candidate | Pending |

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

## Phase 4 verification

- [x] Readable chase, telegraph, attack, recovery and death state lifecycle.
- [x] Eight authored roles with role-specific spacing and movement behavior.
- [x] Frenzied, Bulwark and Volatile elite modifier vocabulary.
- [x] Summoner budget and encounter start/clear event tracking.
- [x] Allocation-free projectile ownership filtering fixed for real encounters.
- [x] EditMode tests: 6/6 passed.
- [x] PlayMode integration tests: 3/3 passed.
- [x] Windows x64 development smoke build succeeded (164,681,931 bytes).

## Phase 5 verification

- [x] Deterministic main route plus treasure, shop and secret branches.
- [x] Combat, elite, treasure, shop, shrine, puzzle, secret, challenge,
  mini-boss, boss, event and healing room vocabulary.
- [x] Guaranteed mini-boss, healing and boss pacing beats.
- [x] Runtime greybox route presenter and dedicated generation sandbox.
- [x] 250-seed invariant test for critical topology and pacing.
- [x] EditMode tests: 7/7 passed.
- [x] PlayMode integration tests: 4/4 passed.
- [x] Two-scene Windows x64 smoke build succeeded (164,689,823 bytes).

## Phase 6 verification

- [x] Gold, Souls and Ancient Crystals with checked non-negative balances.
- [x] Deterministic weighted rewards and six authored Echo relics.
- [x] Three-slot harmony, clash and awakening resonance evaluation.
- [x] Atomic shop purchase/refund behavior and duplicate protection.
- [x] Keyboard/controller pickup interaction and three sandbox choices.
- [x] Durable `AGENTS.md` startup rule and project memory handoff.
- [x] EditMode tests: 10/10 passed.
- [x] PlayMode integration tests: 5/5 passed.
- [x] Two-scene Windows x64 smoke build succeeded (164,704,327 bytes).

## Phase 7 verification

- [x] Drowned Narthex biome atmosphere and playable guardian arena.
- [x] Health-driven three-phase boss model with enrage threshold.
- [x] Intro, telegraph, recovery, phase and cinematic death hooks.
- [x] Boss health/phase/defeat signals and dynamic camera zoom.
- [x] The Ashen Bell, Prism Stag and Root Choir guardian definitions.
- [x] Shockwave, charge and bounded-summoning attack identities.
- [x] EditMode tests: 11/11 passed.
- [x] PlayMode integration tests: 6/6 passed.
- [x] Three-scene Windows x64 smoke build succeeded (164,730,255 bytes).

## Phase 8 verification

- [x] Versioned save DTO covering unlocks, achievements, cosmetics, settings,
  statistics, best-run seed and quests.
- [x] SHA-256 envelope, atomic replacement, backup recovery and migration.
- [x] Horizontal unlock model with duplicate-safe stable IDs.
- [x] Persistent multi-step NPC quest journal and completion unlocks.
- [x] Playable Lantern Court hub with three NPCs and a descent portal.
- [x] Hub-first run lifecycle and run-start statistics persistence.
- [x] EditMode tests: 13/13 passed.
- [x] PlayMode regression tests: 6/6 passed.
- [x] Four-scene Windows x64 smoke build succeeded (164,754,911 bytes).

## Phase 9 verification

- [x] Validated release catalog with unique, non-empty stable IDs.
- [x] Five distinct Bearer classes with passives, starting weapons and stats.
- [x] Five atmospheric biome definitions.
- [x] Forty uniquely named enemies across eight behavioral archetypes.
- [x] Fifteen multi-phase guardians across eight attack-pattern families.
- [x] Five authored starting weapons and two active abilities.
- [x] EditMode tests: 14/14 passed.
- [x] PlayMode regression tests: 6/6 passed.
- [x] Four-scene Windows x64 smoke build succeeded (164,767,000 bytes).

## Phase 10 verification

- [x] Responsive 720p-to-4K HUD for vitality, guardian health, cooldown,
  run currency, Echo inventory, statuses and route orientation.
- [x] Pause/settings surface operable from keyboard and controller.
- [x] Persisted Input System binding overrides and settings profile.
- [x] Scalable UI, subtitles, reduced motion, camera-shake suppression,
  flash clamping and high-contrast telegraph preferences.
- [x] Original, license-safe procedural adaptive score with exploration,
  combat, guardian and secret states.
- [x] Exactly 100 validated achievements across progression, exploration and
  mastery, with idempotent save-backed unlock tracking.
- [x] Regenerated HUD/audio integration in all playable scenes.
- [x] EditMode tests: 17/17 passed.
- [x] PlayMode integration tests: 8/8 passed.
- [x] Four-scene Windows x64 smoke build succeeded (164,793,134 bytes).

## Phase 11 verification

- [x] Fixed-memory 300-frame telemetry window and explicit minimum-PC budgets.
- [x] Live first-biome frame/population PlayMode budget gate.
- [x] Central four-tier, five-biome health/damage/reward balance profile.
- [x] Target run duration constrained to the 25–45 minute release definition.
- [x] English source catalog with fallback and deterministic pseudo-locale QA.
- [x] Localized runtime HUD boundaries and per-profile locale persistence.
- [x] Unified release-readiness validator for content, achievements, balance,
  localization and four enabled build scenes.
- [x] Deterministic generation soak expanded to 1,000 seeds.
- [x] EditMode tests: 21/21 passed.
- [x] PlayMode integration/performance tests: 9/9 passed.
- [x] Readiness-gated four-scene Windows x64 build succeeded
  (164,806,502 bytes).

## Phase 12 verification

- [x] Strict, non-development Windows x64 build path with version metadata.
- [x] Clean output recreation prevents stale release files.
- [x] JSON release manifest with five independently verified SHA-256 entries.
- [x] Packaged-player smoke mode verifies camera, player, input, health and
  telemetry services after hub startup.
- [x] Fixed invalid embedded `LocalizationBootstrap` MonoScript serialization
  by moving the component to its correctly named source file.
- [x] `1.0.0-rc.1` packaged D3D12 player started and exited cleanly on Intel
  UHD 620 with `LANTERNFALL_SMOKE_PASS`.
- [x] EditMode tests: 21/21 passed.
- [x] PlayMode integration/performance tests: 9/9 passed.
- [x] Strict Windows build succeeded (99,582,628 bytes).
- [x] Requirement-by-requirement commercial completion audit recorded in
  `Docs/COMPLETION_AUDIT.md`.
- [ ] Commercial completion: audit found runtime/content gaps, now scoped into
  Phases 13–17.

## Phase 13 verification

- [x] Persistent authoritative run session survives room scene reloads.
- [x] Forty deterministic main-route rooms span five biome indices and five
  guardian milestones.
- [x] One hundred sampled seeds remain inside the authored 25–45 minute pacing
  target.
- [x] Five in-world class-selection pedestals with visible selected state.
- [x] Selected class health, movement speed, weapon and ability are applied in
  the integrated chamber.
- [x] Health, seed, elapsed time, kills and guardian progress persist across
  rooms.
- [x] Combat, elite, challenge, mini-boss, guardian, healing and utility room
  orchestration with locked/unlocked exit flow.
- [x] Death and victory return to the hub and update persistent statistics.
- [x] Generated `RunChamber` and five-scene readiness/build configuration.
- [x] EditMode tests: 22/22 passed.
- [x] PlayMode integration tests: 10/10 passed.
- [x] Five-scene Windows x64 development build succeeded (164,843,603 bytes).
- [x] Packaged hub-to-`RunChamber` smoke passed with exit code 0.

## Phase 14 verification

- [x] Movable planar `RadianceField` centered on the carried lantern.
- [x] Eight enemy archetypes receive light-dependent speed, windup, armor or
  aggression responses.
- [x] Enemies beyond the lantern deal 25% more damage and pay enhanced rewards.
- [x] Vanguard guard, Wayfinder radius, Gloamstep boundary, Cantor sequence and
  Artificer refraction passives execute through stable authored tags.
- [x] Three visible Vow choices with no-hit, timed-clear and beyond-light
  conditions.
- [x] Fulfilled/broken Vows alter reward yield and following encounter size.
- [x] Deterministic three-choice Echo rewards after encounters and in treasure
  rooms.
- [x] Gold, three-slot Echo chain and class passive state survive room reloads.
- [x] Non-damageable reward/Vow triggers no longer absorb projectiles.
- [x] EditMode tests: 26/26 passed.
- [x] PlayMode integration tests: 10/10 passed.
- [x] Five-scene Windows x64 development build succeeded (164,855,989 bytes).
- [x] Packaged hub-to-pillar-chamber smoke passed with exit code 0.
