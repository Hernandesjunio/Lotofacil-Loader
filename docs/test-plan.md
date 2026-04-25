# Test plan (portable)

This document defines what “coverage” means for the domain and contract.

## Coverage principle

Treat coverage as complete only when each item below has:

1. a deterministic positive test
2. a boundary/negative test
3. traceability to a spec slice in `docs/`

## Test layers

- Formula tests (pure domain calculations on small fixtures)
- Contract tests (schemas, error codes, determinism, envelopes)
- Integration tests (end-to-end with fixtures or real data snapshots)
- Property tests (invariants: determinism, ordering, finitude, bounds)

## Coverage matrix (placeholder)

- Indicators/metrics: every name/version you declare normatively under `docs/` (this kit does not ship a catalog file; see `docs/brief.md` and `docs/project-guide.md`)
- Public surface (API/CLI/tools/SDK): every operation you declare normatively under `docs/`
- Errors: all error codes declared in your contract document (when you add one)

