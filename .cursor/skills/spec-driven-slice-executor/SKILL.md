---
name: spec-driven-slice-executor
description: Executes one atomic spec-driven slice at a time (feature slice, optional public contract slice, drift fix) using the loop spec → tests → minimal implementation → determinism checks → evidence. Stack-agnostic: use with any language/framework/delivery surface. Use when implementing a new feature, adding/updating a public contract, freezing goldens, or fixing spec↔code drift.
---

# Spec-Driven Slice Executor (portable)

## Goal

Ship one **explicit slice** with proof:

- spec is closed (docs)
- tests are written first
- minimal code passes the tests
- determinism is verified where required
- docs/tests/code stay aligned

## Execution loop

1. Identify governing specs (`docs/*`)
2. Write tests first (domain and/or contract)
3. Implement minimally (respect boundaries)
4. Verify determinism + run suites
5. Update docs only when semantics changed (in the same change set)

## Constraints

- One slice per change set.
- No invented semantics outside the agreed spec slice (docs) / ADR when applicable.
- No hidden defaults in the system/runtime for ambiguous requests.

## Additional resources

- See [reference.md](reference.md)

