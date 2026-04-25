---
name: spec-driven-bootstrap
description: Bootstraps a new repository to run a spec-driven, TDD, deterministic workflow assisted by AI (docs-first, small feature slices, optional public contract). Stack-agnostic: use with any language/framework/delivery surface. Use when starting a new project, creating the initial docs skeleton, defining the first minimal feature slice, or setting up repeatable AI-driven execution templates.
---

# Spec-Driven Bootstrap (portable)

## Goal

Create a **repeatable AI-driven development workflow** (stack-agnostic):

- specs in `docs/` define semantics first
- tests prove each slice (core semantics + public interface, when applicable)
- minimal implementation respects boundaries
- determinism is enforced (same canonical input ⇒ same canonical output)
- the system/runtime does not invent defaults for ambiguous inputs

## When to use

- New repository setup / “project boilerplate”
- Creating a docs skeleton to support AI-driven development
- Defining and executing the first minimal feature slice (optional, but recommended)

## Operating rules

- Always cite the governing spec files in `docs/`.
- Do not invent feature semantics outside the agreed spec slice (docs).
- Prefer 1 atomic slice per change set.
- If semantics change, update **docs + tests + code** together.

## Quick template: atomic request

```md
Implemente apenas <passo único>.

Referências obrigatórias:
- <spec 1>
- <spec 2>

Arquivos esperados:
- <arquivo A>
- <arquivo B>

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- sem defaults ocultos no sistema/runtime (quando houver ambiguidade, falhar explicitamente ou pedir clarificação conforme o contrato do projeto).

Critério de pronto:
- <teste X passa>
- <erro Y é emitido>
```

## Additional resources

- See [reference.md](reference.md)
- Copy-ready atomic prompts: see [prompts.md](prompts.md)

