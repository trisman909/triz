# Lanternfall durable project memory

Last updated: 2026-07-01

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

## Current state

Phase 10 is verified, committed and pushed. It includes a
responsive generated HUD, live keyboard/controller settings, persisted binding
overrides, a clamped accessibility runtime profile, original procedural
adaptive music, and an exactly 100-entry validated achievement catalog with
save-backed tracking. Final evidence is 17/17 EditMode, 8/8 PlayMode, and a
164,793,134-byte Windows development build. The user explicitly confirmed that
`trisman909/triz` is the intended remote and authorized pushing `master`.

Phase 11 is verified and ready to commit. It adds a fixed-memory 60 FPS
telemetry window, declared minimum-PC budgets, four-tier/five-biome centralized
balance, English fallback and pseudo-locale QA, a 1,000-seed generation soak,
and a unified release-readiness validator. Evidence: 21/21 EditMode, 9/9
PlayMode including the live vertical slice, validator pass, and a
164,806,502-byte Windows development build.

Next actions:

1. Commit and push Phase 11.
2. Build the Phase 12 Windows release candidate with production build settings,
   version metadata, release manifest and checksums.
3. Run the full final audit against every release requirement.

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

## Known operational notes

- Unity startup can take roughly 30–70 seconds on this machine.
- Initial Windows builds can take several minutes; poll logs rather than
  assuming a hang.
- Early licensing handshake warnings resolve to a valid Unity Personal license
  and have not affected successful tests/builds.
- The GitHub token previously pasted into chat must be revoked. It is not in
  the repository and must never be used.
