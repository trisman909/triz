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
| 6 Rewards/economy | `feat: add Echo resonance rewards and economy` | 10/10 EditMode; 5/5 PlayMode; Windows build |
| 7 Biome/guardians | `feat: add first biome guardian vertical slice` | 11/11 EditMode; 6/6 PlayMode; three-scene Windows build |

## Current state

Phase 7 is verified and ready for its milestone commit. Phase 8 (hub, saves,
meta progression and NPC quests) is next.

Next actions:

1. Commit Phase 7.
2. Implement versioned save DTOs, atomic storage, backup rotation and migration.
3. Build the hub scene and run lifecycle transitions.
4. Add horizontal unlocks and the first NPC questline state model.

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
