# Lanternfall durable project memory

Last updated: 2026-07-06

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

## Permanent production goal

Lanternfall should become a polished, replayable, original 3D roguelite that
feels spacious, readable, atmospheric, and satisfying to play.

Do not treat “technically working” as finished. A feature is only accepted
when it feels good in actual play, looks intentional, and is understandable to
the player.

### Permanent creative direction

- Dark mystical roguelite atmosphere.
- Distinct biome identity.
- Spacious rooms, not cramped obstacle mazes.
- Smooth readable combat.
- Professional UI/HUD.
- Clear player feedback.
- Strong movement and dodge feel.
- Original identity, not a clone of Binding of Isaac.

### Permanent player-feel rules

- Rooms should feel big and movement-friendly.
- The player should have space to move, dodge, aim, and circle enemies.
- Obstacles should be smaller, fewer, and intentional.
- Decorative props should usually stay near edges/background or have no
  collider.
- Avoid cluttering rooms just because space exists.
- Keep wide movement lanes and loop paths.
- The camera should stay comfortably centered with only mild look-ahead.
- Moving upward should not make the player feel blind to the area
  behind/around them.
- Combat must remain readable before it becomes visually fancy.

### Permanent player-character rules

- The player character should not randomly change because of biome.
- The player should remain visually consistent unless the player explicitly
  selected a different class.
- Class identity should come from weapon, silhouette, VFX, attack style, and
  UI, not accidental swaps.

### Permanent polish rules

- HUD must look like a real game UI, not default Unity placeholder UI.
- Movement should not feel like a static object sliding.
- Dodge should have clear visual/audio feedback.
- Hits, damage, boss health, rewards, pickups, and room completion must be
  obvious.
- Every build should be judged by actual play-feel, not only automated tests.

### Permanent workflow rules

- Work in small milestones.
- Stop after each milestone for player review.
- Do not continue automatically into the next major phase.
- Do not chase “commercial complete” in one long autonomous run.
- Do not add new content before existing content feels good.
- Prefer improving the core loop over adding more enemies, bosses, biomes, or
  systems.
- Preserve accepted work unless a test or playtest proves it is broken.
- Avoid broad rewrites unless genuinely necessary.
- Use targeted tests while developing.
- Run full tests/build/smoke only at milestone end.
- Update project docs and commit only after verification passes.

### Current milestone priority order

1. Player presentation and HUD professionalism.
2. Full-run gameplay feel: room purpose, healing, rewards, pacing, boss
   clarity.
3. Class identity and combat feedback.
4. Save/progression clarity.
5. Full-game art integration.
6. Balance and polish.
7. Release preparation only after the game is genuinely fun and stable.

### Permanent definition of done

A milestone is not done just because it compiles.

It is done when:

- tests pass
- build works
- smoke passes
- documentation is updated
- changes are committed
- the player can understand and feel the improvement in-game
- no major accepted feature regressed

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

The user accepted the Phase 17 representative review chambers and redirected
the next milestone to Phase 18A: player presentation and HUD professionalism.
Do not start full-game art integration yet; the next milestone after this
checkpoint remains gameplay-feel work rather than broad content expansion.
`trisman909/triz` remains the intended remote and the user has authorized
pushing `master`.

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

The review correction pass enlarged the playable floors around the real player
controller, moved spawns inward for better gameplay-camera framing, added
instant biome switching on keys `1`–`5`, tightened HUD scaling/text clarity,
and refreshed clean-load captures after the chamber corrections.

Fresh clean-process captures now confirm all five chambers reload consistently.
Drowned Narthex is cyan/blue-green under active ACES/post-processing rather
than magenta or purple; Siltglass remains a sandy-violet radial observatory,
Ember a red-orange furnace trench, Gloam a green irregular root clearing and
Stormvault a blue metallic machine grid.

Runtime-created materials are backed by Resources material assets so player
shader stripping cannot remove URP Lit, Unlit or Particles/Unlit. This was
found by the first packaged smoke, fixed, rebuilt and verified. Authored actor
materials no longer receive unnecessary property blocks. The actor ornament
reconfiguration path also now clears its destroyed rig reference before
rebuilding, preserving guardian/enemy readability after scene startup.

Final evidence: 35/35 EditMode (`2026-07-05T21:12:21.8818311Z`), 15/15
PlayMode (`TestResults/Phase17_PlayMode.xml`, run started `2026-07-06
00:06:26Z` and finished `00:06:43Z`), release-readiness pass
(`2026-07-06T00:08:02.7604472Z`), accepted D3D11 clean-load captures for all
five representative chambers, strict Windows x64 `1.0.0-rc.2` build
(`Builds/WindowsRC/Lanternfall.exe`, manifest built `2026-07-06T00:18:02.8753950Z`,
111,916,134 reported bytes), and fresh packaged Direct3D 11 smoke exit 0 with
`LANTERNFALL_SMOKE_PASS` (`TestResults/windows_rc_smoke_current.log`) after
traversing the hub, integrated run chamber and all five representative
chambers. Drowned Narthex remains cyan/blue-green with ACES/post-processing
active in the accepted representative capture path and the packaged D3D11
review build. Full-game art integration must not begin until the user approves
this review build. This checkpoint is not commercial completion.

## Phase 18A player-presentation checkpoint

Phase 18A fixes the confusing review-build bearer swap by removing the
biome-specific review avatar path and attaching a runtime
`PlayerPresentation` component to the playable bearer in hub, run, sandbox
and representative scenes. The bearer now resolves its visual identity from
the selected class or active run class, not from biome. The procedural
presentation rig provides class-specific palette/weapon silhouettes plus
lightweight bob, lean, lantern and weapon sway, footstep dust, dodge trail and
dodge-burst feedback without requiring a humanoid animation rig.

`GameHud` was rebuilt into a darker framed runtime HUD with clearer vitality,
readiness, route and run-status hierarchy. It now surfaces ability/dodge
readiness, calling/weapon context and a more intentional announcement/title
language instead of the earlier placeholder layout. A final closeout fix added
`Start()` refresh logic so startup health text no longer depends on component
`Awake()` ordering. `PlayerPresentation` also now removes the old root
placeholder renderer so the bearer does not appear as a duplicated capsule
under the new procedural rig.

Batch verification on this machine still requires the repository
`Phase17BatchVerificationRunner` path without forced `-quit`; native
`-runTests` remains unreliable immediately after domain reload. One packaged
smoke teardown crash was traced to the smoke probe's explicit quit path and
fixed by switching to a short delayed normal `Application.Quit()`.

Current Phase 18A evidence in the worktree:

- EditMode: 35/35 passed (`2026-07-06T11:24:11.2541375Z`).
- PlayMode: 17/17 passed (`2026-07-06T11:28:12.3396995Z`).
- Release-readiness validation: passed
  (`2026-07-06T11:29:43.2346166Z`).
- Strict Windows x64 `1.0.0-rc.2` build succeeded
  (`TestResults/phase18a_windows_build_manualfix.log`,
  `Builds/WindowsRC/Lanternfall.exe`,
  111,840,950 reported bytes).
- Packaged Direct3D 11 smoke exited 0 with `LANTERNFALL_SMOKE_PASS`
  (`TestResults/phase18a_windows_rc_smoke_manualfix.log`).

Manual closeout notes for this checkpoint are intentionally narrow: the user's
earlier review already accepted room scale, movement comfort, dodge comfort,
camera framing and Drowned color. The final 18A closeout only changed HUD
startup presentation and removal of the old root placeholder body, so the last
manual pass focused on startup/player-presentation correctness rather than
retesting chamber layout or room art. Phase 18B should begin with full-run
gameplay feel, not new content.

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
