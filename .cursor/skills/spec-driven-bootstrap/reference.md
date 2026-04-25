# Reference — Bootstrap (portable, feature-first)

## Recommended “project kit” contents

- `README.md` — narrative entry point
- `AGENTS.md` — map of truth sources + non-negotiables
- `docs/brief.md` — scope, constraints, determinism policy, non-goals
- `docs/project-guide.md` — boundaries + folder layout
- `docs/test-plan.md` — definition of coverage for this domain
- `docs/contract-test-plan.md` — ordering + fixtures/goldens conventions
- `docs/spec-driven-execution-guide.md` — operational sequence spec → test → code
- `docs/fases-execucao-templates.md` — atomic “implement only …” templates
- `docs/glossary.md` — optional human wording

Optional in your repo (not shipped here): ADRs under `docs/adrs/`, detailed contract doc, indicator catalog.

## Feature-first bootstrap rule

This template is meant to drive development as **features**, not as “protocol-specific plumbing” work items.

Use this decomposition:

- **Feature**: user-visible capability with a clear outcome (what changes for the user/consumer).
- **Spec slice**: the minimum semantic definition required to implement that feature.
- **Proof**: tests that fail first and pass only when the feature behaves as specified.

## How to define your first feature slice (generic)

When starting a new repo, define the first slice as:

- **Feature name**: `<FEATURE_NAME>`
- **User outcome** (1 sentence): `<WHAT_CHANGES_FOR_USER>`
- **Inputs** (structured): `<INPUTS>`
- **Outputs**: `<OUTPUTS>`
- **Rules/invariants** (1–5 bullets): `<INVARIANTS>`
- **Negative cases** (at least 1): `<INVALID_INPUT_BEHAVIOR>`
- **Done criteria**: tests passing + evidence recorded

Keep the slice small enough to fit in one reviewable change set.
