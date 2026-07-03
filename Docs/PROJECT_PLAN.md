# Lanternfall production plan

Updated: 2026-07-03

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
| 15 | Five playable biomes and distinct release encounters | Complete |
| 16 | Production presentation, audio, UI and achievement wiring | Verified checkpoint |
| 17 | Five-chamber production-art direction and player review build | Awaiting approval |

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

## Phase 15 verification

- [x] Every integrated room builds deterministic architecture for Drowned
  Narthex, Siltglass Orrery, Ember Ossuary, Gloam Orchard or Stormvault
  Foundry.
- [x] Every biome combines authored atmosphere with two elemental floor
  hazards and twelve seeded traversal props.
- [x] All 40 biome-roster enemies receive stable unique palettes, body
  proportions and archetype ornaments.
- [x] Eight enemy roles retain distinct behavior and role-specific Radiance
  responses.
- [x] All 15 guardians receive stable body/crown identities, multi-phase
  behavior and one of eight pattern-specific arena languages.
- [x] Release validation rejects duplicate biome/enemy/guardian display names
  and incomplete role or guardian-pattern coverage.
- [x] Regenerated all five scenes with no missing or embedded scripts.
- [x] EditMode tests: 29/29 passed.
- [x] PlayMode integration/performance tests: 10/10 passed.
- [x] Five-scene Windows x64 development build succeeded (164,871,537 bytes).
- [x] Packaged biome-aware D3D12 smoke passed on Intel UHD 620 with exit code
  0.
- [ ] Commercial completion: production presentation/audio/UI/accessibility
  wiring and minimum-PC release QA remain in Phases 16–17.

## Phase 16 playtest checkpoint

- [x] All 100 achievements have persistent schema-v3 metric progress and live
  gameplay event sources.
- [x] Victory/defeat captures time, seed, rooms, enemies, guardians, currency,
  Echoes and Vow outcomes for a hub run-summary panel.
- [x] HUD route, Vow and resonance status are authoritative rather than
  placeholders.
- [x] Pause UI can select, listen for, persist and reset keyboard, mouse and
  controller bindings.
- [x] Gameplay-driven subtitles and high-contrast attack telegraphs honor the
  active accessibility profile.
- [x] Original fixed-bank movement, combat, enemy, guardian, reward, ambience
  and UI audio cues are integrated.
- [x] Fixed-pool combat VFX and procedural actor motion ornaments are
  integrated without changing deterministic combat state.
- [x] EditMode tests: 31/31 passed.
- [x] PlayMode integration tests: 13/13 passed.
- [x] Five-scene Windows x64 development build succeeded (164,915,721 bytes).
- [x] Expanded packaged smoke passed with exit code 0.
- [x] Strict `1.0.0-rc.2` Windows player built and packaged smoke-tested.
- [ ] Full-game replacement of primitive scene/actor geometry is deliberately
  deferred until the Phase 17 representative chambers receive approval.

## Phase 17 representative art review checkpoint

- [x] Five independent original texture seeds produce five distinct surface
  families; the shared stone seed is not used as a recolor shortcut.
- [x] Five gameplay-camera chambers use different spatial grammars: drowned
  processional nave, radial observatory, split furnace trench, irregular root
  clearing and broad foundry grid.
- [x] Each chamber has biome-specific silhouettes, modular props, material
  response, environmental story, landmark framing, particles, fog and light.
- [x] Fresh clean-load captures confirm Drowned Narthex is cyan/blue-green and
  the other four retain their intended palettes under ACES profiles.
- [x] Production materials, runtime telegraphs, pooled VFX, actor ornaments and
  guardian spectacle use build-preserved URP shaders; no Built-in/magenta
  renderer material remains in the representative scenes.
- [x] Authored actor materials bypass runtime `MaterialPropertyBlock` tinting;
  procedural fallback geometry alone receives color overrides.
- [x] Reduced motion, flash limiting, camera-shake suppression, subtitles and
  high-contrast telegraphs remain wired and PlayMode-verified.
- [x] EditMode: 35/35; PlayMode: 14/14; release-readiness validation passed.
- [x] Strict Windows x64 `1.0.0-rc.2` build succeeded (109,136,226 reported
  bytes) and its D3D12 packaged smoke traversed hub, run chamber and all five
  review chambers with exit code 0.
- [ ] Obtain player approval of the five representative chambers.
- [ ] Do not begin full-game art integration until that approval is received.
- [ ] Commercial completion remains blocked by the open audit items, including
  human run-timing, manual device/audio/usability review and minimum-PC 60 FPS
  capture.
