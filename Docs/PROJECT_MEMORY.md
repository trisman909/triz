# Lanternfall durable project memory

Last updated: 2026-07-02

This is the concise cross-session handoff. `AGENTS.md` requires every future
agent to read and maintain it. The worktree, Unity results, and Git history
remain authoritative.

## Product

- Working title: **Lanternfall**.
- Original isometric 3D action roguelite about carrying a dying sky-lantern
  through rearranging drowned ruins.
- Original pillars: tactical radiance geometry, three-slot Echo resonance, and
  route Vows whose consequences alter later encounters.
- Windows PC first; Unity `6000.5.1f1`, URP `17.5.0`, Input System `1.19.0`.
- The user approved the installed Unity Supported release instead of waiting
  for Unity 6.3 LTS.

## Repository and tools

- Intended remote: `https://github.com/trisman909/triz.git`.
- Never store credentials in this file or repository.
- Git executable: `C:\Program Files\Git\cmd\git.exe`.
- Unity executable:
  `C:\Program Files\Unity\Hub\Editor\6000.5.1f1\Editor\Unity.exe`.
- Repository-local Git identity: `Codex <codex@local>`.
- Unity batch logs/results are written under ignored `TestResults/`.
- Windows smoke builds are written under ignored `Builds/Windows/`.

## Verified phase history

| Phase | Commit | Evidence |
|---|---|---|
| 1 Foundation | `02a3789` | Compilation; 4/4 EditMode |
| 2 Player/camera | `06ad39f` | 5/5 EditMode; 1/1 PlayMode; Windows build |
| 3 Combat | `6b3260b` | 6/6 EditMode; 2/2 PlayMode; Windows build |
| 4 Enemy AI | `dc7a038` | 6/6 EditMode; 3/3 PlayMode; Windows build |
| 5 Run generation | `df0328b` | 7/7 EditMode; 4/4 PlayMode; 250 seeds; Windows build |
| 6 Rewards/economy | `bc81283` | 10/10 EditMode; 5/5 PlayMode; Windows build |
| 7 Biome/guardians | `04b8b36` | 11/11 EditMode; 6/6 PlayMode; three-scene Windows build |
| 8 Hub/persistence | `0baf064` | 13/13 EditMode; 6/6 PlayMode; four-scene Windows build |
| 9 Content catalog | `e8eb043` | 14/14 EditMode; 6/6 PlayMode; four-scene Windows build |
| 10 Experience/accessibility | `feat: deliver accessible game experience layer` | 17/17 EditMode; 8/8 PlayMode; four-scene Windows build |
| 11 Optimization/QA | `feat: add measurable release quality gates` | 21/21 EditMode; 9/9 PlayMode; readiness-gated Windows build |
| 12 Packaging/audit | `04fc84d` | 21/21 EditMode; 9/9 PlayMode; strict build; packaged hub-to-biome smoke |
| 13 Integrated run | `d0fe3f5` | 22/22 EditMode; 10/10 PlayMode; five-scene build; packaged hub-to-run smoke |
| 14 Pillar mechanics | `c553093` | 26/26 EditMode; 10/10 PlayMode; five-scene build; packaged pillar smoke |
| 15 Biomes/encounters | `e2d15cb` | 29/29 EditMode; 10/10 PlayMode; five-scene build; packaged biome smoke |

## Current state

Phase 10 is verified, committed and pushed. It includes a
responsive generated HUD, live keyboard/controller settings, persisted binding
overrides, a clamped accessibility runtime profile, original procedural
adaptive music, and an exactly 100-entry validated achievement catalog with
save-backed tracking. Final evidence is 17/17 EditMode, 8/8 PlayMode, and a
164,793,134-byte Windows development build. The user explicitly confirmed that
`trisman909/triz` is the intended remote and authorized pushing `master`.

Phase 11 is verified, committed and pushed as `3c9b474`. It adds a fixed-memory 60 FPS
telemetry window, declared minimum-PC budgets, four-tier/five-biome centralized
balance, English fallback and pseudo-locale QA, a 1,000-seed generation soak,
and a unified release-readiness validator. Evidence: 21/21 EditMode, 9/9
PlayMode including the live vertical slice, validator pass, and a
164,806,502-byte Windows development build.

Next actions:

Phase 12 is verified, committed and pushed as `04fc84d`. The production builder sets version
`1.0.0-rc.1`, strict non-development Windows x64 options, clean output,
core-file SHA-256 checksums and a JSON release manifest. A packaged D3D12 smoke
run on Intel UHD 620 traversed hub to first biome and exited 0 with
`LANTERNFALL_SMOKE_PASS`; final build size is 99,582,628 bytes. The work also
fixed real player corruption caused by secondary `LocalizationBootstrap` and
`BiomeAtmosphere` MonoBehaviours embedded in scenes; each component now has its
own matching source file, and no scene contains embedded MonoScripts.

The final audit proved the packaging pipeline but found the game is not
commercially complete. Integrated runs, core pillars and systemic biome/
encounter identities are now implemented through Phase 15, while production
presentation, achievements, accessibility, audio and minimum-PC proof remain.
`Docs/COMPLETION_AUDIT.md` is authoritative. The plan now continues through
Phases 13–17 rather than making a false completion claim.

## Phase 13 verified work

The worktree contains a verified persistent seeded 40-room route spanning five biome
indices, class-selection pedestals, class health/speed/loadout application,
room-to-room health persistence, integrated combat/guardian chamber
orchestration, exit gating, death/victory returns and statistics capture.
`RunChamber.unity` generation and five-scene build wiring are verified.

Evidence is 22/22 EditMode, 10/10 PlayMode, a 164,843,603-byte Windows
development build, and a packaged hub-to-`RunChamber` smoke exit 0. Next:
Phase 13 was committed and pushed as `d0fe3f5`.

## Phase 14 verified work

Phase 14 is committed and pushed as `c553093`. It adds a movable
`RadianceField`, archetype-specific light responses, danger/reward beyond the
lantern, five stable-tag class passives, three selectable Vows with fulfillment
or break consequences, persistent Echo/gold state and deterministic
post-encounter reward choices. Evidence: 26/26 EditMode, 10/10 PlayMode,
readiness validation, a 164,855,989-byte Windows development build and packaged
smoke exit 0.

## Phase 15 verified work

Every room now receives one of five deterministic biome architecture
vocabularies, two distinct elemental hazards and its authored atmosphere. All
40 enemies receive stable ID-derived body proportions, palettes and archetype
ornaments while retaining eight role behaviors and Radiance responses. All 15
guardians receive stable body/crown identities, phase behavior and one of eight
pattern-specific arena languages.

The release catalog rejects duplicate display names or incomplete enemy-role
and guardian-pattern coverage. The packaged smoke probe verifies that
`RunChamber` creates its biome presenter and 14 generated pieces. Evidence:
release-readiness pass, 29/29 EditMode, 10/10 PlayMode, a 164,871,537-byte
Windows development build, and packaged D3D12 smoke exit 0 on Intel UHD 620.
Next: Phase 16 production presentation, audio, UI, accessibility and
achievement wiring. The project is not yet commercially complete.

## Phase 16 active playtest checkpoint

Phase 16 now has save-versioned progress for all 100 metric-driven
achievements, gameplay event wiring, an immutable victory/defeat summary, a
live eight-room route display, real Vow/resonance status, interactive
keyboard/mouse/controller rebinding, gameplay subtitles, high-contrast ground
telegraphs, fixed-pool combat VFX, procedural actor motion ornaments, and an
original 12-cue SFX/ambience bank.

Automated evidence is 31/31 EditMode and 13/13 PlayMode. The five-scene Windows
development build is 164,915,721 bytes and its expanded packaged smoke exits 0.
A strict `1.0.0-rc.2` playtest package is the next action before Phase 17.
Phase 16 remains in progress because the underlying scene and actor geometry
still looks procedurally greybox-like; do not represent this checkpoint as
commercially finished.

## Phase 17 representative production-art review checkpoint

The current worktree intentionally combines the verified Phase 16
systems checkpoint with a five-chamber production-art replacement pass. Five
independent original texture sources, five material-response families, 55
biome-role prefabs, five character displays, five enemy displays, five
guardian displays and five playable `ArtReview_*` scenes are generated by
`ProductionArtBuilder`. The external concept-proxy GLB remains outside
`Assets` and is reference-only.

The first same-layout presentation was rejected. The current replacement uses
different traversal grammars: a processional drowned nave, radial observatory,
split furnace trench, irregular root clearing and broad machine grid. A
controlled URP material-isolation capture proved the Drowned albedo, normal,
mask, Lit shader and prefab references are correct. Its purple scene cast was
caused by the default skybox/reflection dominating dark wet stone; generated
Volume profiles also contained null component references. The builder now
forces a neutral custom-reflection environment, validates/normalizes URP Lit
materials and persists Volume components as sub-assets.

Fresh clean-process captures now confirm all five chambers reload consistently.
Drowned Narthex is cyan/blue-green rather than purple; Siltglass remains a
sandy-violet radial observatory, Ember a red-orange furnace trench, Gloam a
green irregular root clearing and Stormvault a blue metallic machine grid.
The profiles retain ACES. Drowned disables camera post-processing because an
Intel/D3D12 driver-path fault was independently shown to fold its cyan emission
violet; its authored ACES profile remains valid for later platform retesting.

Runtime-created materials are backed by Resources material assets so player
shader stripping cannot remove URP Lit, Unlit or Particles/Unlit. This was
found by the first packaged smoke, fixed, rebuilt and verified. Authored actor
materials no longer receive unnecessary property blocks. The actor ornament
reconfiguration path also now clears its destroyed rig reference before
rebuilding, preserving guardian/enemy readability after scene startup.

Final evidence: 35/35 EditMode, 14/14 PlayMode, release-readiness pass, strict
Windows x64 `1.0.0-rc.2` build (109,136,226 reported bytes), and packaged
D3D12 smoke exit 0 with `LANTERNFALL_SMOKE_PASS` after traversing the hub,
integrated run chamber and all five representative chambers. Full-game art
integration must not begin until the user approves this review build. This
checkpoint is not commercial completion.

## Architecture invariants

- `Lanternfall.Core` owns deterministic rules and has no Unity engine
  dependency.
- `Lanternfall.Gameplay` owns runtime presentation/adapters and depends inward.
- Unity global random is presentation-only; authoritative selection uses named
  deterministic streams.
- ScriptableObjects are immutable authored definitions; mutable run state is
  allocated at runtime.
- Hot combat paths avoid managed allocations; frequent objects are pooled.
- Saves will use versioned DTOs, atomic replacement, backups, and migrations.
- Biome and encounter visuals derive from authoritative biome/encounter data
  and never mutate deterministic run state.
- `GameplayPresentationSignals` is the semantic boundary consumed by pooled
  SFX, VFX and subtitle presenters; gameplay does not depend on those outputs.
- Achievement progress and unique discovery contexts are save-versioned in
  schema v3; `RunSummaryData` is captured before mutable run state is released.

## Known operational notes

- Unity startup can take roughly 30–70 seconds on this machine.
- Initial Windows builds can take several minutes; poll logs rather than
  assuming a hang.
- Early licensing handshake warnings resolve to a valid Unity Personal license
  and have not affected successful tests/builds.
- The GitHub token previously pasted into chat must be revoked. It is not in
  the repository and must never be used.
