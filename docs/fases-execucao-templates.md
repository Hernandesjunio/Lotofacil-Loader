# Atomic execution templates (portable)

These templates are **copy/paste** requests for AI-assisted development. Keep them atomic: one objective, one proof.

> <PLACEHOLDER-LINGUA>
> Se o seu time prefere inglês, traduza estes templates mantendo a mesma estrutura.

## Template — Atomic request (base)

```md
Implemente apenas <passo único>.

Referências obrigatórias:
- <docs/...>

Arquivos esperados:
- <arquivo A>
- <arquivo B>

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- respeitar fronteiras de arquitetura;
- sem defaults ocultos no servidor;
- manter determinismo quando aplicável.

Critério de pronto:
- <testes passam>
- <erro canônico é emitido>
```

## Phase 0 — Freeze baseline (no feature)

```md
Implemente apenas um checkpoint de alinhamento normativo (sem codar feature): arquitetura, contrato, determinismo e não-objetivos.

Referências obrigatórias:
- docs/brief.md
- docs/project-guide.md
- docs/contract-test-plan.md
- docs/spec-driven-execution-guide.md

Arquivos esperados:
- AGENTS.md (se necessário)
- docs/ (nota curta, se necessário)

Critério de pronto:
- baseline explícito e rastreável; nenhuma contradição crítica escondida.
```

## Phase 1 — Repo skeleton (compile + test)

```md
Implemente apenas o esqueleto mínimo do repositório para compilar e rodar testes, sem cenografia adicional.

Referências obrigatórias:
- docs/project-guide.md

Arquivos esperados:
- src/
- tests/

Critério de pronto:
- build e testes “vazios” rodam; fronteiras estão claras.
```

## Phase 2 — Fixture + red tests (V0)

```md
Implemente apenas uma fixture mínima e testes vermelhos da V0 (domínio + contrato) antes de qualquer implementação funcional.

Referências obrigatórias:
- docs/brief.md
- docs/spec-driven-execution-guide.md
- docs/test-plan.md
- docs/contract-test-plan.md

Arquivos esperados:
- tests/fixtures/<synthetic_fixture>.json
- tests/<Domain.Tests>/
- tests/<ContractTests>/

Critério de pronto:
- testes falham pelo motivo correto e descrevem o comportamento esperado.
```

