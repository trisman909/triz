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
- Authored surfaces, fog, particles, lights, enemies, guardian ornaments,
  accessible telegraphs and pooled effects render through URP.
- Drowned Narthex survives a fresh reload as cyan/blue-green rather than
  magenta or violet.
- Menus, route HUD, run summary, remapping, subtitles, reduced motion,
  high-contrast telegraphs and flash/shake limits remain operational.

## What is still placeholder

- Character, enemy and guardian display meshes establish silhouette direction
  but are not final sculpted/rigged production characters.
- Modular environment pieces are original authored proxies with generated
  texture families, not final hand-finished asset sets.
- Audio is an original procedural complete-coverage bank, but still needs
  professional composition, performance, mix and mastering review.
- Review rooms demonstrate one composition per biome; the complete 40-room run
  has not received this art language.
- Drowned camera post-processing is disabled as a verified driver-specific
  color-preservation workaround pending broader GPU testing.

## Approval boundary

Approve the biome identity and gameplay-camera language before full integration.
The next art phase should expand each vocabulary independently across the run,
replace display characters with production rigs, and tune performance on the
declared minimum PC. Do not treat this review build as a public-release
candidate.

## Verification evidence

- EditMode: 35/35 passed.
- PlayMode: 14/14 passed.
- Release-readiness validation: passed.
- Windows x64 strict build: 109,136,226 reported bytes.
- Packaged Direct3D 12 smoke: exit 0, `LANTERNFALL_SMOKE_PASS`.
- Smoke route: hub, integrated run chamber and all five art-review chambers.
