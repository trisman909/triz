# Lanternfall agent rules

These instructions apply to the entire repository.

## Required startup ritual

Before planning, editing, testing, committing, or pushing:

1. Read `Docs/PROJECT_MEMORY.md` in full.
2. Read `Docs/PROJECT_PLAN.md`, `Docs/GAME_DESIGN.md`, and
   `Docs/ARCHITECTURE.md`.
3. Inspect the current Git status and recent commits. Treat the worktree as the
   authoritative state when it differs from the notes.
4. Update `Docs/PROJECT_MEMORY.md` whenever a phase, architectural decision,
   dependency, verification result, blocker, or next action changes.

Never claim a phase is complete until its documented Unity gates pass and the
phase has its own commit.

## Project mandate

Continue building Lanternfall as a commercial-quality, completely original 3D
action roguelite. Preserve deterministic run generation, modular architecture,
typed/event-driven boundaries, authored ScriptableObject content, accessible
input/readability, and stable 60 FPS as production constraints.

Do not copy copyrighted assets, layouts, characters, enemies, story, music, or
trade dress from any inspiration.

## Security

Never write credentials, access tokens, passwords, private keys, or connection
strings into this repository, command arguments, logs, commits, or memory
notes. Use the system credential manager or an authenticated GitHub CLI session.
If a credential is exposed, require revocation and replacement before use.

## Phase workflow

For every phase:

1. Implement the complete scoped milestone.
2. Regenerate affected Unity scenes/assets through the editor builder.
3. Run all relevant EditMode and PlayMode tests.
4. Run the Windows development-build smoke gate.
5. Update the plan and memory with exact evidence.
6. Commit one coherent milestone, then confirm a clean worktree.

Use Unity `6000.5.1f1` and the package versions pinned in
`Packages/manifest.json` unless an explicit, verified migration is made.

