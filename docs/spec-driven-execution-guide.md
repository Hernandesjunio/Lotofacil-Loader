# Guia de execução spec-driven (portátil e agnóstico)

## Definição

“Spec-driven” aqui significa:

1. the source of truth comes first in `docs/`
2. each implementation is a **single explicit slice**
3. each slice has tests that prove it
4. code without a clear spec reference is suspicious
5. semantic changes require coordinated updates to **docs + tests + code**

## Regra operacional

Before asking an agent to implement something, answer:

- Which spec slice am I materializing?
- What minimal set of files should it touch?
- Which test proves it is correct?
- Does it fit in one reviewable change set?

If any answer is unclear, the slice is not ready.

## Ordem recomendada de trabalho

- Congelar a base (fronteiras, política de determinismo, princípios do contrato)
- Preparar o esqueleto mínimo do repositório (só o que compila/roda testes)
- Criar fixture/snapshot mínima + testes vermelhos da V0
- Implementar núcleo de semântica mínima (apenas o necessário para passar os testes)
- Implementar primitivas técnicas de determinismo **somente se o seu contrato exigir**
  - ex.: canonicalização de input, hashing, versionamento de dataset/snapshot
- Implementar casos de uso/orquestração (se aplicável)
- Implementar a superfície pública por contrato
- Fechar a V0 por evidência (rodar suítes, registrar resultados)

> <PLACEHOLDER-STACK>
> Este guia não impõe linguagem/framework. Ajuste os nomes de camadas/pastas conforme `docs/project-guide.md`.

