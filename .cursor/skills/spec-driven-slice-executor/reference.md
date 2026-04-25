# Reference — Slice executor (portable, feature-first)

## Valid slice definition

A slice must include:

- a single objective
- explicit spec references
- tests that fail before implementation and pass after
- done criteria that are observable

## Common slice types

### Feature slice (default)

- Define the feature in terms of **user/consumer outcome**:
  - **Feature name**: `<FEATURE_NAME>`
  - **Outcome** (1 sentence): `<WHAT_CHANGES_FOR_USER>`
  - **Inputs**: `<INPUTS>`
  - **Outputs**: `<OUTPUTS>`
  - **Rules/invariants**: `<INVARIANTS>`
  - **Negative cases**: `<INVALID_INPUT_BEHAVIOR>`
- Write tests first:
  - at least 1 positive deterministic test
  - at least 1 boundary/negative test
  - additional property tests only when they reduce ambiguity (ordering, bounds, finitude, idempotency, etc.)
- Implement minimally until the tests pass.

### Public interface / contract slice (when needed)

- Use only when the slice changes the **public interface** (API/CLI/SDK schema, error codes, versioning, determinism guarantees).
- Update the normative contract/spec in `docs/` first (this kit ships `docs/brief.md` at high level and `docs/contract-test-plan.md` for how to prove the contract).
- Write contract tests:
  - success case(s)
  - error case(s) with stable `code/message/details`
  - determinism checks only if your contract requires them
- Implement minimal parsing/binding/validation to satisfy the contract.

### Drift fix

- Classify drift (semantic vs surface/transport vs structural vs evidence).
- Decide: bring code to spec OR revise spec (explicitly).
- Add regression tests.

