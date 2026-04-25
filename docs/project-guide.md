# Guia de projeto (portátil e agnóstico)

## Objetivo

Ter fronteiras claras para que a **semântica** e o **contrato público** permaneçam estáveis conforme o sistema evolui — independentemente de stack, linguagem ou protocolo.

## Estrutura (PLACEHOLDER — será refinada tecnicamente depois)

> <PLACEHOLDER-ESTRUTURA>
> Esta estrutura é um **molde**. A intenção é a IA (ou você) preencher/ajustar depois do refinamento técnico.
> O importante é manter as **responsabilidades** abaixo, não os nomes.

### Opção A (camadas por responsabilidade)

```text
src/
  core/              # semântica canônica do domínio (regras, fórmulas, invariantes)
  app/               # orquestração/casos de uso (fluxos, validação cross-field)
  adapters/          # integrações/IO (providers, persistência, rede, serialização canônica)
  delivery/          # superfície pública (API/CLI/tools/SDK), parsing/binding, erros
tests/
  core/
  contract/
  fixtures/
docs/
  brief.md
  project-guide.md
  test-plan.md
  contract-test-plan.md
  spec-driven-execution-guide.md
  fases-execucao-templates.md
  glossary.md
  # no seu repo: contrato detalhado, catálogo de indicadores, ADRs, etc.
```

### Opção B (hexagonal / ports & adapters)

```text
src/
  domain/            # regras do domínio (puro)
  application/       # use cases
  ports/             # interfaces (entrada/saída)
  adapters/          # implementações de IO, persistência, integrações
  entrypoints/       # API/CLI/handlers
tests/
  domain/
  contract/
  fixtures/
docs/
  ...
```

## Regras de fronteira (o que evita ambiguidade)

- O **núcleo de semântica** não depende de transporte (API/CLI) nem de IO concreto.
- A **superfície pública** não contém cálculo do domínio; ela valida/parsa e chama casos de uso.
- Integrações (IO/persistência/rede) não definem semântica; apenas fornecem dados e primitivas técnicas (ex.: hash, canonicalização).
- Se o contrato público mudar, atualizar **docs + testes + código** juntos.

> <PLACEHOLDER-NOMES>
> Defina como o seu projeto chama “indicador”, “métrica”, “artefato”, “janela/recorte”, “contrato”.
> Isso reduz confusão em prompts e em testes.

