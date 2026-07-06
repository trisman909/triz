# Lanternfall representative chamber review

Review build: `1.0.0-rc.2`  
Unity: `6000.5.1f1` / URP  
Scope: five representative chambers only

## What to review

Start in the Lantern Court, use the review portal, then move through each
chamber. Judge whether the location reads immediately from the gameplay camera
before inspecting individual props.

1. Drowned Narthex: cyan processional nave, flood channels, bells and reliquary.
2. Siltglass Observatory: sandy-violet radial routes around a broken orrery.
3. Ember Ossuary: red-orange split walkways around a furnace trench.
4. Gloam Orchard: green asymmetric root islands and memorial growth.
5. Stormvault Foundry: blue metallic grid, pylons and framed dynamo.

Combat systems remain playable in the integrated run chamber. The art-review
rooms are presentation proofs, not five finished campaign levels.

## What is presentable now

- The five locations no longer read as recolors of one stone kit.
- Gameplay-camera composition, landmark framing, atmosphere and traversal
  grammar differ materially between every chamber.
- The review versions are now spacious showroom chambers with open movement,
  wide loop paths and direct biome switching on keys `1`–`5`.
- Authored surfaces, fog, particles, lights, enemies, guardian ornaments,
  accessible telegraphs and pooled effects render through URP.
- Drowned Narthex survives a fresh reload as cyan/blue-green rather than
  magenta or violet while ACES/post-processing remains active.
- Menus, route HUD, run summary, remapping, subtitles, reduced motion,
  high-contrast telegraphs and flash/shake limits remain operational.
- Follow-on Phase 18A presentation work keeps the accepted chambers intact
  while moving the bearer and HUD toward a more reviewable game-facing style.

## What is still placeholder

- Character, enemy and guardian display meshes establish silhouette direction
  but are not final sculpted/rigged production characters.
- Modular environment pieces are original authored proxies with generated
  texture families, not final hand-finished asset sets.
- Audio is an original procedural complete-coverage bank, but still needs
  professional composition, performance, mix and mastering review.
- Review rooms demonstrate one composition per biome; the complete 40-room run
  has not received this art language.
- The representative review build is visual/player-review quality, not a final
  content-complete public release candidate.
- The bearer now reads more consistently across biomes, but still uses a
  procedural placeholder display rig rather than a final rigged production
  character.

## Approval boundary

Approve the biome identity and gameplay-camera language before full integration.
The next art phase should expand each vocabulary independently across the run,
replace display characters with production rigs, and tune performance on the
declared minimum PC. Do not treat this review build as a public-release
candidate.

## Verification evidence

- EditMode: 35/35 passed (`2026-07-05T21:12:21.8818311Z`).
- PlayMode: 15/15 passed (`TestResults/Phase17_PlayMode.xml`,
  `2026-07-06 00:06:26Z`–`00:06:43Z`).
- Release-readiness validation: passed
  (`2026-07-06T00:08:02.7604472Z`).
- Windows x64 strict build: `Builds/WindowsRC/Lanternfall.exe`,
  111,916,134 reported bytes.
- Packaged Direct3D 11 smoke: exit 0, `LANTERNFALL_SMOKE_PASS`
  (`TestResults/windows_rc_smoke_current.log`).
- Smoke route: hub, integrated run chamber and all five art-review chambers.

## Phase 18A follow-on review notes

- The user's representative-build review accepted biome switching, Drowned
  color, room scale, obstacle size, camera feel, HUD readability and general
  movement comfort as the baseline to preserve.
- Phase 18A therefore focused on bearer consistency, movement/dodge
  presentation and HUD professionalism without reworking chamber layouts.
- Final closeout fixes after the first rebuilt 18A player review:
  - HUD startup no longer flashes an incorrect `0/180` vitality state due to
    component load order.
  - The old root placeholder body renderer is removed so the runtime bearer
    does not appear as a duplicated capsule under the new procedural display.
- Current Phase 18A rebuild evidence:
  - EditMode 35/35 passed (`2026-07-06T11:24:11.2541375Z`).
  - PlayMode 17/17 passed (`2026-07-06T11:28:12.3396995Z`).
  - Readiness passed (`2026-07-06T11:29:43.2346166Z`).
  - Strict Windows build succeeded
    (`TestResults/phase18a_windows_build_manualfix.log`).
  - Packaged D3D11 smoke exited 0 with `LANTERNFALL_SMOKE_PASS`
    (`TestResults/phase18a_windows_rc_smoke_manualfix.log`).
