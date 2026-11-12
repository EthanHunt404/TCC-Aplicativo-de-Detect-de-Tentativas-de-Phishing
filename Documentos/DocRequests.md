# DocRequests

## DR-001 — Branch por fase na Fase 4

- **Origem:** instrução explícita do usuário nesta conversa.
- **Regra vigente:** o plano da Fase 4, regra R-09, e o Padrão de Codificação, seção 4, definem uma branch por funcionalidade (`feature/f-XX-nome-curto`) e commits pequenos por incremento funcional verificável.
- **Solicitação:** para manter um histórico próprio da fase, usar a branch `feature/fase-4` e registrar commits periódicos dos incrementos concluídos nesta fase.
- **Impacto:** exceção limitada ao fluxo de versionamento da Fase 4. Não altera requisitos, arquitetura, escopo funcional ou comportamento do produto.
- **Tratamento de alterações preexistentes:** arquivos já modificados, removidos ou não rastreados antes do trabalho nesta fase não serão incluídos nos commits desta solicitação.

## DR-002 — Conflito entre domínios de encurtadores reais e dados fictícios

- **Origem:** análise da Etapa 25 antes da tarefa 25.2.
- **Conflito:** a tarefa 25.2 exige cadastrar 20 domínios reais de serviços de encurtamento (`bit.ly`, `tinyurl.com` e outros); a regra 25.13 proíbe usar empresas ou domínios reais na base de validação.
- **Alternativa A:** autorizar uma exceção explícita à regra 25.13 apenas para os 20 domínios de encurtamento nomeados em 25.2; manter fictícios todos os domínios empresariais e sintéticos.
- **Alternativa B:** manter a proibição absoluta de domínios reais e revisar a lista da tarefa 25.2 para usar 20 domínios sintéticos.
- **Recomendação:** A, pois preserva os vetores exatos exigidos pela tarefa 25.2 e restringe domínios reais à coleção de encurtadores, sem introduzir empresas reais.
- **Decisão humana:** alternativa A aprovada em 12/11/2026.
- **Estado:** resolvido. A exceção aplica-se somente aos 20 domínios reais explicitamente listados em 25.2; nomes e domínios empresariais permanecem fictícios. O seed será criado, mas sua execução continua reservada à tarefa humana 25.5.
