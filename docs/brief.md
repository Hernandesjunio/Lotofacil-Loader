# Brief (portátil e agnóstico)

## Objetivo (modelo)

Construir um sistema que:

- produz **artefatos determinísticos** do seu domínio (indicadores, métricas, relatórios, transformações, validações);
- expõe esses artefatos por uma **superfície pública regida por contrato**;
- entrega outputs **explicáveis** (critérios aplicados, recortes/janelas, versões, metadados).

> <PLACEHOLDER-PRODUTO>
> - Nome do produto: `<PRODUCT_NAME>`
> - Problema que resolve (1 frase): `<ONE_LINER>`
> - Público-alvo (quem consome): `<AUDIENCE>`

## Não-objetivos (modelo)

- prometer ou sugerir “previsão” / garantia de resultado;
- inferir parâmetros ausentes “por conveniência” (defaults ocultos);
- depender de interpretação de linguagem natural pelo runtime do servidor/sistema para cumprir o contrato.

## Modelo de consumo (agnóstico de protocolo)

Defina aqui como consumidores chamam o sistema.

> <PLACEHOLDER-SUPERFICIE>
> Escolha **um** modelo principal (pode ter secundários):
> - API (HTTP/gRPC)
> - CLI
> - SDK/biblioteca
> - “Tools” (um catálogo de comandos com schema)
> - Eventos/filas
>
> Para o template, assuma sempre: **entrada estruturada** + **validação explícita** + **erros estáveis**.

## Política de determinismo (modelo)

- Mesma entrada canônica ⇒ mesma saída canônica (quando aplicável).
- Se existir geração/aleatoriedade:
  - replay/reprodutibilidade **somente** com `seed` explícito;
  - sem `seed`, permitir variação, mas **registrar/explicar** o que foi aplicado.

## Metadados de rastreabilidade (placeholders)

> <PLACEHOLDER-METADADOS>
> Defina quais campos existem no envelope de saída. Exemplos comuns:
> - `system_version` / `contract_version`
> - `dataset_version` (se há dataset/snapshot)
> - `deterministic_hash` (se o contrato exige)
> - `execution_id` (se útil para logging/observabilidade)
>
> Importante: estes nomes são placeholders; feche-os em documentação normativa adicional no seu `docs/` (e alinhe provas em `docs/contract-test-plan.md`).

## Referências

- `docs/spec-driven-execution-guide.md`
- `docs/fases-execucao-templates.md`
- `docs/contract-test-plan.md`
- `docs/test-plan.md`
- `docs/project-guide.md`

