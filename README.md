# Template kit — spec-driven + desenvolvimento orientado por IA (agnóstico)

Esta pasta é um **template portátil** para você copiar para um repositório novo e iniciar um processo **spec-driven, TDD e determinístico**, desenhado para ser consumido por **agentes de IA** via uma **superfície pública regida por contrato**.

> <PLACEHOLDER-PROTOCOLO>
> Este kit é **agnóstico**: a superfície pública pode ser “tools”, API, CLI, SDK, eventos, etc.
> Defina o protocolo/transporte depois, no refinamento técnico (não aqui).

## O que este kit entrega

- **Docs-first**: semântica em `docs/` antes de qualquer implementação.
- **Templates atômicos** (“implemente apenas X”) para evitar pedidos amplos/ambíguos.
- **Skills/Rules do Cursor** para tornar o fluxo repetível com IA.

## O que este kit não entrega

- Não entrega especificação do seu domínio (isso é seu).
- Não entrega uma arquitetura fixa (isso vem do refinamento técnico).
- Não promete “melhor predição”, “maior chance” nem qualquer garantia de resultado.

## Como usar num repositório novo

1. Copie o conteúdo desta pasta para a raiz do seu repositório.
2. Preencha os blocos `<PLACEHOLDER-...>` nos arquivos de `docs/`.
3. Execute sempre na ordem: **spec → testes → implementação mínima → evidência**.

## Comece por aqui

- `AGENTS.md` (mapa de fontes de verdade e regras não negociáveis)
- `docs/brief.md` (escopo, restrições e não-objetivos)
- `docs/project-guide.md` (fronteiras e molde de pastas)
- `docs/spec-driven-execution-guide.md` (ordem prática de execução)
- `docs/fases-execucao-templates.md` (pedidos atômicos copy/paste)
- `docs/contract-test-plan.md` (fixtures, goldens, testes de contrato)
- `docs/test-plan.md` (camadas e matriz de cobertura)
- `docs/glossary.md` (opcional, linguagem humana)

## Cursor (opcional)

- `.cursor/skills/spec-driven-bootstrap/` — *skill* de arranque (`SKILL.md`, `reference.md`, `prompts.md`)
- `.cursor/skills/spec-driven-slice-executor/` — *skill* de fatia única (`SKILL.md`, `reference.md`)
- `.cursor/rules/human-summary-style.md` — regra de resumos para humanos

### Por que fatiar pedidos (e por vezes threads)

- **Janela de contexto**: tudo o que entra na conversa (histórico, anexos, respostas) compete pelo mesmo limite; threads muito longas ou pedidos gigantes empurram specs para fora do “foco útil” do modelo.
- **Ondas**: poucos ficheiros ou um recorte por pedido reduz omissão e deriva face ao que está em `docs/`.
- **Thread nova** (opcional): ao saltar de fase grande (ex.: só docs → primeiro código), um arranque limpio evita arrastar mensagens que já não interessam.

