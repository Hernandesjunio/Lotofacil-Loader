# Contract test plan (portable)

## Goal

Make the public surface testable and stable:

- request/response schemas
- error codes + `details`
- determinism of payloads and hashes (where required)
- (if multiple transports exist) semantic parity between transports

## Fixtures and goldens

- Store fixtures under `tests/fixtures/`.
- Store goldens under `tests/fixtures/golden/` (or a convention of your choice).
- Update goldens only when the change is intentional and the spec is updated.

## Minimal V0 contract tests (recommended)

- Envelope fields exist and are stable
- Unknown metric/tool argument produces `UNKNOWN_*` code with stable `details`
- Invalid request produces `INVALID_REQUEST`

