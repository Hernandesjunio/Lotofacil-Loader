# Prompt pack — Bootstrap (portable)

## Bootstrap: docs skeleton (no code)

```md
Implemente apenas o esqueleto mínimo de documentação do repositório para operar spec-driven (sem implementar código ainda): brief, guia de projeto, superfície pública de contrato (se existir), plano de testes e templates atômicos.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md
- docs/project-guide.md

Arquivos esperados:
- docs/brief.md
- docs/project-guide.md
- docs/test-plan.md
- docs/contract-test-plan.md
- docs/fases-execucao-templates.md

Regras:
- a definir de acordo com o projeto

Critério de pronto:
- existe um “mapa de verdade” claro;
```

## Primeira fatia mínima (opcional): fixture + testes vermelhos (antes do código)

```md
Implemente apenas a primeira fatia mínima end-to-end (nomeie como quiser: V0, P0, Slice-0, MVP-0) escrevendo:
- uma fixture/snapshot mínima;
- testes vermelhos (núcleo/semântica + contrato público)

antes de qualquer implementação funcional.

Referências obrigatórias:
- docs/test-plan.md
- docs/contract-test-plan.md

Escopo da fatia (preencher no pedido):
- <defina o objetivo end-to-end em 1 frase>
- <defina a entrada canônica e o output esperado>
- <liste 1–3 invariantes que serão provadas por teste>

Arquivos esperados:
- tests/fixtures/<synthetic_fixture>.json
- tests/<CoreOrDomain.Tests>/        # nome depende do seu stack/estrutura
- tests/<ContractOrInterface.Tests>/ # contrato público: API/CLI/SDK/etc.

Regras:
- TDD: testes devem falhar pelo motivo esperado;
- O teste de regra de negócio (comportamento) são mais importante do que simplesmente cobertura de testes
- Os cenários de negócio deverão estarem bem descricos
- Após a escrita do teste será necessário validar novamente se está atendendo a descrição de negócio (comportamento) caso aplicável
- cobrir pelo menos:
  - barreira canônica (normalização/sanitização/parsing) no ponto certo do pipeline
  - recorte/janela/escopo (se o seu domínio tiver esse conceito)
  - edge cases
  - teste que lança exceptions (testar parâmetros inválidos ou regra inválida)
  - 1 regra de negócio principal (comportamento fechado e testável)
  - 1 teste para cada caso negativo de contrato (request inválido) e 1 caso de “nome desconhecido” (se houver catálogo)  

Critério de pronto:
- existe teste explícito da barreira de normalização;
- existe teste de fórmula/regra para a regra de negócio principal;
- existe teste negativo do contrato para request inválido.
- cenários de negócio foram testados
- necessário validação do desenvolvedor dos testes
```

