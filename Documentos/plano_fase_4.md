# Plano de Ação para Agentes — Fase 4 (Construção)

> Etapas 20 a 25 do Roteiro de Execução (marco E3, Aula 11). Aplicativo de Detecção de Tentativas de Phishing por Correio Eletrônico. Orienta agentes de codificação na construção, verificação e entrega, com base no Termo de Referência (TR), no Documento de Requisitos, no Documento de Arquitetura, no ADR e no Padrão de Codificação. As Fases 1 a 3 estão concluídas. As Fases 5 e 6 terão planos próprios, feitos só depois que a Fase 4 fechar.

---

## 1. Como usar este documento

1. As etapas são executadas na ordem numérica. Única exceção: a **Etapa 25** (base de validação) começa junto com a Etapa 20, porque as Etapas 22 a 24 dependem dela.
2. Uma etapa só termina quando **todos** os itens de "Pronto quando" forem verdadeiros e comprovados com a saída real dos comandos. "Parece funcionar" não é prova.
3. Cada tarefa tem um executor: **[AGENTE]** ou **[HUMANO]**. O agente nunca executa uma tarefa [HUMANO] (conta Google, instalação de software, teste visual, instalação por terceiro). Ao chegar nela, o agente PARA e lista exatamente o que o humano precisa fazer.
4. Termos normativos: **DEVE** (obrigatório), **NÃO DEVE** (proibido), **PARAR** (interromper o trabalho e pedir decisão ao humano).
5. O agente trabalha em uma etapa por vez e entrega o Relatório de Etapa (seção 7) ao terminá-la.

---

## 2. Regras absolutas

| Nº | Regra |
|---|---|
| R-01 | **Hierarquia de fontes:** TR > Documento de Requisitos > Documento de Arquitetura > ADR > Padrão de Codificação > este plano. Este plano só decide onde os documentos são silenciosos (seção 5). Se este plano contradisser um documento de nível superior, vale o documento, e o agente reporta a divergência. |
| R-02 | **Não inventar.** Se faltar informação, o agente PARA e escreve: `LACUNA: <o que falta> / OPÇÕES: <A, B> / RECOMENDAÇÃO: <uma delas>`, e espera a resposta. É proibido escolher sozinho. |
| R-03 | **Documentos existentes são somente leitura:** TR, Requisitos, Arquitetura, ADR e Padrão de Codificação. Se o código exigir mudança neles, o agente escreve `DIVERGÊNCIA: <documento, seção, o que o código faz>` e o humano edita. Só podem ser criados os documentos que uma etapa mandar criar. |
| R-04 | **Escopo excluído (TR 3.3):** NÃO DEVE existir interface gráfica elaborada (somente ícone na bandeja, menu e notificação nativa), servidor externo dedicado (MongoDB só em `localhost`) nem parceria com provedores. NÃO DEVE ser criada funcionalidade fora do TR 3.2. |
| R-05 | **Camadas (ADR 0005):** `Domain` não referencia nenhuma outra camada. `Application` referencia só `Domain`. `Infrastructure` e `Presentation` referenciam `Application` e `Domain`. `Presentation` só referencia `Infrastructure` dentro do `Program` (raiz de composição). Um namespace por camada. |
| R-06 | **Segurança (RNF-02, RNF-03):** a aplicação NÃO DEVE pedir nem guardar senha de e-mail. NÃO DEVE haver segredo no código (client id, client secret, token). NENHUM conteúdo de e-mail pode sair da máquina: são proibidas chamadas de rede além de IMAP, OAuth2 do Google e MongoDB local, e proibida qualquer telemetria. |
| R-07 | **Escrita (Arquitetura 3.6):** a aplicação só grava (a) o relatório `.txt`, (b) o cache de token do OAuth2 feito pela biblioteca, (c) o log da decisão P-16. NÃO DEVE existir nenhuma operação de escrita nas coleções do MongoDB. |
| R-08 | **Caixa de entrada intocada (Arquitetura 2.3.1):** a aplicação NÃO DEVE apagar, mover, marcar como lida nem alterar mensagens. A pasta é aberta somente-leitura. |
| R-09 | **Git (Padrão, seção 4):** um branch por funcionalidade, `feature/f-XX-nome-curto`; correções usam `fix/d-XXX`. Commits pequenos e descritivos. Merge na `main` só quando a Definition of Done do Documento de Requisitos (seção 11) for atendida. |
| R-10 | **Nomenclatura e SOLID:** conforme o Padrão de Codificação (seções 1 e 3). Uma classe por arquivo. Nomes de domínio em português. |
| R-11 | **Testes:** seguir a convenção já existente no repositório (confirmada na tarefa 20.1): uma única classe estática de testes com métodos `[Fact]` estáticos, executada pelo `Program`. NÃO DEVE ser criado projeto de testes separado. |
| R-12 | **Pacotes NuGet:** somente os da decisão P-03. Qualquer outro é uma LACUNA (R-02). |
| R-13 | **Código existente não muda sem ordem:** `EmailAnalist`, `IConsulta` e as classes de domínio da Etapa 18 já estão implementados e testados. NÃO DEVEM ser renomeados, ter assinatura alterada ou lógica reescrita, exceto por ordem humana expressa e registrada em `DocRequests.md` (como DR-003). |
| R-14 | **Prova:** nenhum item "Pronto quando" pode ser marcado ✔ sem a saída do comando ou a evidência anexada ao relatório. |
| R-15 | **Atraso:** se uma etapa não puder terminar, o agente PARA e reporta. Cortar escopo é decisão do humano e vai para o Controle de Alterações do TR (seção 7). O agente NÃO DEVE cortar nada sozinho. |

---

## 3. Estado de partida

| Artefato | Estado esperado |
|---|---|
| Termo de Referência, Documento de Requisitos, Documento de Arquitetura, ADR 0001 a 0006, Padrão de Codificação | Versões vigentes no repositório, concluídas |
| Classes de domínio (`ConteudoRelevante`, `Inconsistencia`, `RelatorioInconsistencias`, `EmailAnalist`), interface `IConsulta` e testes | Implementados e passando (Etapa 18), com dados fictícios |
| Capturador, `MongoDBInterface`, OAuth2, bandeja, notificação, relatório `.txt` | **Não existem** |

A tarefa 20.1 confirma isso lendo o repositório. Se o estado real for diferente, o agente reporta antes de continuar.

---

## 4. Decisões fixas (vindas da documentação — NÃO reabrir)

| Assunto | Decisão | Origem |
|---|---|---|
| Linguagem e plataforma | C# sobre .NET (SDK .NET 10, `net10.0`) | ADR 0001 |
| Banco de dados | MongoDB, serviço local em `localhost`, somente leitura pela aplicação | ADR 0002, Arquitetura 3.1 e 3.6 |
| Acesso ao e-mail | MailKit, IMAP sobre SSL/TLS, porta 993 | ADR 0003, RF-04 |
| Autenticação | OAuth2 (Google/Microsoft), sem senha | ADR 0004, RF-02, RNF-02 |
| Camadas | Domain, Application, Infrastructure, Presentation, em pastas | ADR 0005, Padrão 2 |
| Estrutura de consulta | `HashSet` em memória para o registro de encurtamento e o index de empresas; busca por chave exata para o template | ADR 0006, Arquitetura 2.5.3 |
| Contrato de acesso a dados | Uma única interface `IConsulta`, declarada no Domain, implementada por `MongoDBInterface` na Infrastructure | Arquitetura 1.6 |
| Relatório | `RelatorioInconsistencias` salvo em `.txt`, na pasta de documentos do usuário, subpasta `Phishing Reports` | Arquitetura 3.6 |
| Volumes da PoC | 200 domínios de encurtamento, 5.000 empresas, 5.000 templates, até 200 e-mails por dia | Arquitetura 2.4 |
| Desempenho | Análise em até 10 s após o recebimento | RNF-01, CA-07 |
| Interação | Somente ícone na bandeja e notificação nativa | RNF-07 |
| Distribuição | Build executável com base de teste incorporada | TR 4 |

---

## 5. Decisões de implementação adotadas por este plano

> Os documentos são silenciosos nestes pontos. Valem como regra enquanto o humano não mandar outra coisa.

| Nº | Decisão |
|---|---|
| P-01 | **Plataforma:** Windows 10/11 x64. TFM `net10.0-windows` com `UseWindowsForms` ligado **somente** para `NotifyIcon` e menu de contexto. NÃO DEVE existir nenhum `Form`. |
| P-02 | **Provedor da PoC:** Gmail, uma conta de teste, `imap.gmail.com`, porta 993, escopo OAuth2 `https://mail.google.com/`. Microsoft não é implementado na PoC (limitação a registrar no Manual). |
| P-03 | **Pacotes permitidos:** `MailKit` (traz `MimeKit`), `Google.Apis.Auth`, `MongoDB.Driver`, `HtmlAgilityPack`, e o xUnit já presente. |
| P-04 | **Configuração:** `oauth.local.json` ao lado do executável, no `.gitignore`, com `{"client_id":"","client_secret":"","email":""}`. `appsettings.json` versionado com `{"mongo_uri":"mongodb://localhost:27017","mongo_database":"phishing_poc"}`. Arquivo `oauth.local.json` ausente, ilegível ou com campo vazio equivale a **sem autorização**. |
| P-05 | **Token OAuth2:** `FileDataStore` do `Google.Apis.Auth` em `%AppData%\PhishingDetector\tokens`. |
| P-06 | **Coleções** (banco `phishing_poc`), com os formatos exatos da Arquitetura 3.2 a 3.4: `shorteners` (um único documento `{"list":[{"domain","patterns":[...]}]}`), `companies` (um único documento `{"companies":[...]}`), `templates` (um documento por empresa `{"company","official_domains":[...]}`, índice único em `company`). |
| P-07 | **Carga:** `shorteners` e `companies` são lidos uma vez na partida e mantidos em `HashSet` (comparação sem diferenciar maiúsculas). Alterar a base exige reiniciar o aplicativo. |
| P-08 | **Extração (Capturador):** hyperlink = (a) atributo `href` de cada `<a>` do HTML, (b) cada `http://` ou `https://` encontrado no texto puro. Só URL absoluta http/https vale. `mailto:`, `tel:`, links relativos e `href` vazio são **ignorados**. Sem duplicatas, na ordem de aparição. `ConteudoBruto` = corpo HTML se existir, senão o texto puro. Remetente = primeiro endereço de `From`. |
| P-09 | **Lógica de análise:** encurtamento, domínio oficial, remetente e identificação da empresa já estão no `EmailAnalist` e na implementação de `IConsulta`. A Infrastructure NÃO DEVE duplicar essa lógica. Ela só fornece dados. |
| P-10 | **Fila e classes de coordenação (Application):** `ServicoDeAnalise`, `IGravadorRelatorio`, `INotificador`, `StatusMonitoramento`. A fila entre Capturador e `ServicoDeAnalise` é um `Channel<ConteudoRelevante>` limitado a 100 itens (`FullMode = Wait`). |
| P-11 | **Monitoramento (Capturador):** abre `INBOX` somente-leitura, usa IMAP IDLE renovado a cada 5 minutos, guarda **em memória** o maior UID visto, ignora mensagens que já existiam na partida e, após reconexão, busca somente UIDs maiores que o guardado. |
| P-12 | **Relatório `.txt`:** UTF-8 sem BOM, arquivo `relatorio_AAAAMMDD_HHMMSS_mmm.txt`, conteúdo: linha `RELATÓRIO DE INCONSISTÊNCIAS`, `Data de geração: <DataGeracao>`, `Total de inconsistências: <n>`, uma linha `<i>. [<Tipo>] <Descricao>` por inconsistência, linha `--- CONTEÚDO ANALISADO ---`, e o `ConteudoBruto`. |
| P-13 | **Notificação:** balão do `NotifyIcon` (toast nativo do Windows), título `E-mail suspeito detectado`, texto `Um e-mail foi marcado como suspeito. Clique para abrir o relatório.` O clique abre o `.txt` no programa padrão. |
| P-14 | **Modos de execução do `Program`:** sem argumento = aplicativo; `--testes` = bateria sem serviços externos; `--testes-integracao` = métodos `Integracao_*`, que exigem MongoDB com a base carregada; `--injetar-amostras` = analisa os `.eml` de `base/amostras`; `--demo-notificacao` = mostra uma notificação de exemplo. Os dois primeiros retornam código 0 se tudo passar e 1 caso contrário. |
| P-15 | **Base de teste:** dados **fictícios** ("padrões manufaturados", TR 3.3), carregados por `base/seed.mongo.js` (mongosh), executado pelo operador, nunca pela aplicação. |
| P-16 | **Log:** `%AppData%\PhishingDetector\logs\app.log`. Registra UID, etapa e tempo em milissegundos. NÃO DEVE registrar endereço, assunto, corpo ou link. |
| P-17 | **Documentos novos** ficam na pasta de documentos já existente no repositório; se não houver, em `/docs`. Evidências em `/docs/evidencias`. |

---

## 6. Mapa das etapas

| Fase | Etapa | Aula | Executor principal | Saída |
|---|---|---|---|---|
| 4 | 20 Estrutura por camadas | 8 | Agente | Projeto compilando com pastas por camada e testes de arquitetura |
| 4 | 21 Bandeja, menu e notificação | 8 e 9 | Agente | Presentation navegável, sem regra de negócio |
| 4 | 22 Persistência e fluxo com dados reais | 10 | Agente | `MongoDBInterface`, relatório `.txt`, `ServicoDeAnalise` |
| 4 | 23 Rede assíncrona: OAuth2 e IMAP | 10 | Agente + Humano | Autorização e monitoramento funcionando |
| 4 | 24 Fila e operação sem conexão | 10 | Agente + Humano | Sem perda nem duplicação ao reconectar |
| 4 | 25 Base de validação | 11 | Agente + Humano | Seed, amostras `.eml`, conta de teste |

---

## 7. Relatório de Etapa (formato obrigatório)

Ao terminar (ou PARAR em) uma etapa, o agente entrega exatamente isto:

```text
RELATÓRIO DE ETAPA <nº> — <nome>
Situação: CONCLUÍDA | PARADA
Tarefas: <nº da tarefa> — feita | pendente de HUMANO | bloqueada (motivo)
Arquivos criados/alterados: <lista de caminhos>
Comandos executados: <comando> -> <resultado literal resumido>
Pronto quando: <item> ✔ (prova) | <item> ✘ (motivo)
DIVERGÊNCIAS: <lista ou "nenhuma">
LACUNAS: <lista ou "nenhuma">
Ação pedida ao humano: <lista ou "nenhuma">
```

---

## 8. Etapas 20 a 25

### 8.1 Etapa 20 — Preparar o ambiente e criar o projeto com pastas por camada

**Entrada:** Documento de Arquitetura concluído; repositório com as classes de domínio e os testes da Etapa 18.

**Tarefas**

| Nº | Executor | Tarefa |
|---|---|---|
| 20.1 | Agente | **Levantamento, somente leitura.** Listar arquivos, TFM, pacotes, namespace raiz, onde estão as classes de domínio e a classe de testes, e como o `Program` executa os testes. Nada é alterado nesta tarefa. O resultado vai no relatório. |
| 20.2 | Agente | Criar as pastas `/Domain`, `/Application`, `/Infrastructure`, `/Presentation` e `/Testes` na raiz do projeto. Namespaces: `<Raiz>.Domain`, `<Raiz>.Application`, `<Raiz>.Infrastructure`, `<Raiz>.Presentation`, `<Raiz>.Testes`. |
| 20.3 | Agente | Mover para `/Domain`: `ConteudoRelevante`, `Inconsistencia`, `RelatorioInconsistencias`, `EmailAnalist`, `IConsulta`. Um tipo por arquivo. Só mudam o `namespace` e os `using`. |
| 20.4 | Agente | Mover a classe de testes para `/Testes`, mantendo a convenção de R-11. |
| 20.5 | Agente | Criar `.gitignore` padrão .NET (`bin/`, `obj/`, `.vs/`, `*.user`) mais `oauth.local.json`. |
| 20.6 | Agente | Criar dois testes de arquitetura por reflexão: `Dominio_NaoReferencia_OutrasCamadas` (nenhum campo, propriedade, parâmetro, retorno, base ou interface de tipo do namespace `Domain` pertence aos namespaces `Application`, `Infrastructure` ou `Presentation`) e `Application_NaoReferencia_InfrastructureNemPresentation` (mesma verificação para `Application`). |
| 20.7 | Agente | Commit na `main` ou em `feature/f-00-estrutura`, mensagem `estrutura por camadas`. |

**Pronto quando**
1. `dotnet build` termina com 0 erros.
2. O `Program` executa a bateria e todos os testes passam, inclusive os dois de arquitetura.
3. `git diff -M` mostra, nos arquivos movidos, apenas mudanças de `namespace` e `using`.
4. Existem as quatro pastas de camada e `/Testes`; `.gitignore` contém `oauth.local.json`.

**NÃO DEVE:** renomear classes, mudar lógica, criar projetos separados por camada, instalar pacotes, mudar o comportamento atual do `Program`.

---

### 8.2 Etapa 21 — Implementar bandeja, menu e notificação, sem regra de negócio

**Entrada:** Etapa 20 concluída. Mapa de navegação (única "tela" do sistema, RNF-07):

| Elemento | Comportamento |
|---|---|
| Ícone na bandeja (ícone `SystemIcons.Shield`, tooltip = status atual) | Clique direito abre o menu |
| Item `Status: <texto>` | Desabilitado, só informativo |
| Item `Autorizar conta de e-mail` | Nesta etapa mostra balão `Função ainda não ligada`. A Etapa 23 liga ao OAuth2. |
| Item `Abrir pasta de relatórios` | Abre `Documentos\Phishing Reports`. Se não existir, mostra balão `Nenhum relatório gerado ainda`. |
| Item `Sair` | Remove o ícone e encerra o processo |
| Notificação de e-mail suspeito | Texto da decisão P-13; o clique abre o relatório |

**Tarefas**

| Nº | Executor | Tarefa |
|---|---|---|
| 21.1 | Agente | Ajustar o `.csproj` conforme P-01 (`net10.0-windows`, `UseWindowsForms`). |
| 21.2 | Agente | Em `Application`: `enum StatusMonitoramento { SemAutorizacao, Conectando, Monitorando, Reconectando, BaseIndisponivel }` e `interface INotificador { void Notificar(string caminhoRelatorio); }`. |
| 21.3 | Agente | Em `Presentation`: `IconeBandeja` (ícone + menu) e `NotificadorBandeja : INotificador`. Os dois só tratam eventos de interface. NÃO DEVEM conter regra de negócio. |
| 21.4 | Agente | No `Program`: argumento `--testes` executa a bateria (P-14); sem argumento inicia o aplicativo na bandeja. Instância única (mutex `PhishingDetector.SingleInstance`): a segunda instância encerra imediatamente. |
| 21.5 | Agente | Argumento `--demo-notificacao`: cria um `.txt` de exemplo na pasta temporária e mostra a notificação apontando para ele. |
| 21.6 | Humano | Executar `--demo-notificacao` e `dotnet run` (sem argumento), conferir ícone, menu de 4 itens e clique na notificação. Anexar captura de tela em `/docs/evidencias/etapa21`. |

**Pronto quando**
1. Build com 0 erros; `--testes` passa (incluindo os testes de arquitetura).
2. O ícone aparece, o menu tem exatamente 4 itens (`Status`, `Autorizar conta de e-mail`, `Abrir pasta de relatórios`, `Sair`), e `Sair` encerra o processo.
3. `--demo-notificacao` mostra o balão e o clique abre o arquivo (confirmado pelo humano).
4. Uma segunda execução simultânea encerra sem criar segundo ícone.
5. Busca no código: nenhuma classe herda de `Form` (`: Form` não ocorre).

**NÃO DEVE:** criar janelas, formulários, telas de configuração, ícones personalizados, ou chamar OAuth, IMAP ou MongoDB.

---

### 8.3 Etapa 22 — Persistência: `MongoDBInterface`, relatório `.txt` e fluxo com dados reais

**Entrada:** Etapa 21 concluída; base de teste e amostras da Etapa 25 (tarefas 25.1 a 25.6) prontas; contrato `IConsulta` existente.

**Tarefas**

| Nº | Executor | Tarefa |
|---|---|---|
| 22.1 | Humano | Confirmar que o serviço MongoDB está ativo em `localhost:27017`, que o `mongosh` abre e que a base foi carregada na tarefa 25.5. |
| 22.2 | Agente | Criar `appsettings.json` (P-04) e a leitura dele com `System.Text.Json`. |
| 22.3 | Agente | Em `Infrastructure`: `MongoDBInterface : IConsulta`. Implementa **exatamente** as assinaturas existentes de `IConsulta`. Carrega `shorteners` e `companies` uma vez (P-07). `ConsultarTemplateEmpresa` é uma busca por igualdade exata em `templates.company`. Tipos de retorno: os já declarados. |
| 22.4 | Agente | `IdentificarEmpresa(conteudo)`: dividir o texto em palavras (separadores = qualquer caractere que não seja letra ou dígito), formar sequências de 1 a 3 palavras consecutivas, comparar cada uma, sem diferenciar maiúsculas, com o `HashSet` de empresas, e devolver a **primeira empresa encontrada na ordem do texto**, com a grafia do index. Nenhuma encontrada: devolver o valor "nenhum" previsto pela assinatura existente. |
| 22.5 | Agente | Em `Application`: `interface IGravadorRelatorio { Task<string> GravarAsync(RelatorioInconsistencias relatorio); }` (devolve o caminho do arquivo). Em `Infrastructure`: `GravadorRelatorioTxt`, formato da P-12, pasta `Documentos\Phishing Reports` (criada se faltar). O caminho-base vem pelo construtor, para os testes usarem pasta temporária. |
| 22.6 | Agente | Em `Infrastructure`: `ExtratorDeConteudo`, método estático `ConteudoRelevante? Extrair(MimeMessage mensagem)` com as regras da P-08. Devolve `null` quando não há hyperlink (isso implementa a filtragem, RF-05 e RF-06). |
| 22.7 | Agente | Em `Application`: `ServicoDeAnalise(ChannelReader<ConteudoRelevante>, EmailAnalist, IGravadorRelatorio, INotificador)` com `ExecutarAsync(CancellationToken)`. Para cada item: chamar `EmailAnalist.Analisar`; se devolver relatório, `GravarAsync` e depois `Notificar(caminho)`. Qualquer exceção em um item é registrada no log (P-16) e o laço **continua**. |
| 22.8 | Agente | No `Program`: carregar a base, criar o `Channel`, iniciar o `ServicoDeAnalise` em `Task.Run` (fora da thread de interface). Modo `--injetar-amostras`: ler cada `.eml` de `base/amostras` (`MimeMessage.Load`), chamar `ExtratorDeConteudo.Extrair` e escrever no `Channel`. |
| 22.9 | Agente | Testes sem serviços externos: `ExtratorDeConteudo` com as amostras S-01 a S-15 (Etapa 25), `GravadorRelatorioTxt` em pasta temporária (nome do arquivo e linhas conforme P-12), `ServicoDeAnalise` com `IConsulta` em memória, gravador e notificador falsos. Testar DR-003: empresa não identificada (inclusive retorno `nenhum`) é ignorada mesmo com link encurtado. Testes `Integracao_*`: `MongoDBInterface` contra a base real. |
| 22.10 | Humano | Rodar `--testes-integracao` e `--injetar-amostras`; conferir relatórios e notificações. Evidências em `/docs/evidencias/etapa22`. |

**Pronto quando**
1. Build com 0 erros; `--testes` passa.
2. `--testes-integracao` passa: identifica `BancoExemplo` em um texto de amostra; devolve o template com os domínios oficiais; empresa desconhecida devolve "nenhum".
3. `--injetar-amostras` gera um `.txt` e uma notificação para cada amostra **suspeita** da tabela 8.6 e nenhum para as não suspeitas e ignoradas (S-11 é ignorada conforme a decisão registrada na seção 9).
4. Busca no código: `InsertOne`, `InsertMany`, `UpdateOne`, `UpdateMany`, `ReplaceOne`, `DeleteOne`, `DeleteMany`, `CreateOne` e `Indexes` **não ocorrem** fora de `base/`.
5. Busca no código: `new MongoClient` aparece somente em `MongoDBInterface`.

**NÃO DEVE:** alterar `IConsulta`; reimplementar regra de análise na Infrastructure; criar índice ou coleção pela aplicação. A alteração expressamente aprovada de `EmailAnalist` está limitada à DR-003.

---

### 8.4 Etapa 23 — Mover rede para execução assíncrona: autorização OAuth2 e monitoramento IMAP

**Entrada:** Etapa 22 concluída.

**Tarefas**

| Nº | Executor | Tarefa |
|---|---|---|
| 23.1 | Humano | No Google Cloud: criar projeto, tela de consentimento OAuth em modo **Teste**, adicionar a conta de teste como usuário de teste, criar credencial **Aplicativo para computador** e preencher `oauth.local.json` (P-04). O arquivo NÃO é versionado. |
| 23.2 | Agente | Em `Infrastructure`: `ConfiguracaoOAuth` (lê `oauth.local.json`; qualquer falha = inválida) e `GuardaOAuth2` com `Task<string?> ObterTokenAsync(bool interativo)`: configuração inválida devolve `null`; token salvo e renovável devolve o token de acesso; sem token e `interativo = false` devolve `null`; sem token e `interativo = true` abre o fluxo no navegador (P-02, P-05). |
| 23.3 | Agente | Ligar o item de menu `Autorizar conta de e-mail` a `ObterTokenAsync(true)`, atualizando o status. |
| 23.4 | Agente | Em `Infrastructure`: `Capturador`, com `event Action<StatusMonitoramento> StatusAlterado` e `Task IniciarAsync(ChannelWriter<ConteudoRelevante>, CancellationToken)`. Fluxo: pedir token à `GuardaOAuth2`; **sem token, não conecta e emite `SemAutorizacao`** (RF-03, CA-06); com token, conectar por MailKit (`SaslMechanismOAuth2`), abrir `INBOX` somente-leitura, registrar o maior UID existente, entrar em IDLE (P-11); a cada mudança de contagem, baixar as mensagens novas, chamar `ExtratorDeConteudo.Extrair` e escrever no `Channel` quando o resultado não for `null`. |
| 23.5 | Agente | O `Program` liga `StatusAlterado` ao tooltip e ao item `Status:` do menu. |
| 23.6 | Humano | Autorizar pelo menu, enviar à conta de teste a amostra S-02 (conteúdo da tabela 8.6) e conferir notificação e relatório; conferir que o e-mail continua **não lido** na caixa (R-08). |

**Pronto quando**
1. Build com 0 erros; `--testes` passa (inclui testes de `ConfiguracaoOAuth` e `GuardaOAuth2` sem arquivo, que devolvem `null`).
2. O status vira `Monitorando` após a autorização (humano).
3. O e-mail S-02 enviado à conta real gera relatório e notificação, e continua não lido (humano).
4. Busca no código: `.Result`, `.Wait()`, `.GetAwaiter().GetResult()` e `Thread.Sleep` **não ocorrem** em `Application`, `Infrastructure` e `Presentation`.
5. O menu da bandeja responde enquanto o aplicativo conecta (humano).

**NÃO DEVE:** pedir senha, aceitar login por senha de aplicativo, guardar token em arquivo próprio, marcar/mover/apagar mensagens, processar mensagens que já existiam na partida.

---

### 8.5 Etapa 24 — Implementar a fila e testar a operação sem conexão

**Entrada:** Etapas 22 e 23 concluídas.

**Tarefas**

| Nº | Executor | Tarefa |
|---|---|---|
| 24.1 | Agente | Em `Infrastructure`: `PoliticaDeReconexao` (classe pura) que devolve a espera da tentativa *n*: 5, 10, 30, 60 s, e 60 s nas seguintes. E `ControleDeUid` (classe pura) com `Registrar(uid)`, `MaiorUid` e `Novos(IEnumerable<uid>)` (somente os maiores que `MaiorUid`, sem repetição). |
| 24.2 | Agente | `Capturador`: ao perder a conexão emitir `Reconectando`, esperar conforme `PoliticaDeReconexao`, reconectar e buscar somente `ControleDeUid.Novos`. Cada mensagem é processada **uma única vez**. |
| 24.3 | Agente | Token recusado ou revogado: emitir `SemAutorizacao`, **parar** as tentativas (sem laço de repetição) e aguardar o item de menu `Autorizar conta de e-mail`. |
| 24.4 | Agente | MongoDB inacessível na partida: emitir `BaseIndisponivel`, tentar a carga de novo a cada 30 s, e **só iniciar o Capturador depois da carga**. Sem reiniciar o aplicativo. |
| 24.5 | Agente | Testes sem serviços externos para `PoliticaDeReconexao` (sequência 5, 10, 30, 60, 60) e `ControleDeUid` (UIDs repetidos e fora de ordem). |
| 24.6 | Humano | Teste manual **sem autorização**: remover `oauth.local.json`, abrir o aplicativo, enviar S-02 à conta de teste e confirmar que nada é analisado (status `Sem autorização`, nenhum relatório). |
| 24.7 | Humano | Teste manual **sem rede**: com o aplicativo em `Monitorando`, desligar a rede (modo avião), enviar S-02 e S-03 por outro aparelho, religar a rede. Confirmar que o status passa por `Reconectando`, volta a `Monitorando` e gera **exatamente um** relatório para cada mensagem. |
| 24.8 | Humano | Teste manual **MongoDB parado**: parar o serviço, abrir o aplicativo (status `Base indisponível`), iniciar o serviço e confirmar que o aplicativo passa a `Monitorando` sozinho. |
| 24.9 | Agente | Registrar o observado em cada teste manual em `/docs/Registro_de_Integracao_e_Falhas.md` (seção "Etapa 24"), com data e o resultado literal. |

**Pronto quando**
1. Build com 0 erros; `--testes` passa.
2. Os três testes manuais foram feitos e registrados, com o comportamento esperado.
3. Nenhum relatório duplicado após a reconexão; nenhuma mensagem perdida.

**NÃO DEVE:** gravar UID ou estado em disco (R-07); criar fila persistente.

---

### 8.6 Etapa 25 — Montar a base de validação (não há módulo físico neste projeto)

**Entrada:** TR 5 (critérios de aceitação), Arquitetura 2.4 e 3.2 a 3.4. **Começar junto com a Etapa 20.**

**Tarefas**

| Nº | Executor | Tarefa |
|---|---|---|
| 25.1 | Humano | Criar uma conta Gmail **dedicada ao projeto** (nunca a conta pessoal) e instalar o MongoDB Community Server como serviço em `localhost:27017`, mais o `mongosh`. |
| 25.2 | Agente | Criar `base/seed.mongo.js` (mongosh, repetível: apaga o conteúdo das três coleções antes de inserir). Dados fictícios: `shorteners` com 1 documento de 200 domínios (20 conhecidos: `bit.ly`, `tinyurl.com`, `t.co`, `ow.ly`, `is.gd`, `buff.ly`, `rebrand.ly`, `cutt.ly`, `shorturl.at`, `tiny.cc`, `rb.gy`, `t.ly`, `v.gd`, `bl.ink`, `s.id`, `lnkd.in`, `goo.gl`, `tr.im`, `qr.ae`, `adf.ly`, cada um com `patterns` `["<dominio>/*"]`, mais 180 sintéticos `curto0001.exemplo` a `curto0180.exemplo`); `companies` com 1 documento de 5.000 nomes; `templates` com 5.000 documentos; índice único em `templates.company`. |
| 25.3 | Agente | Empresas de teste (10, com `official_domains` = `<nome sem espaços, minúsculo>.com.br` e `www.<...>.com.br`): `BancoExemplo`, `LojaExemplo`, `EnergiaExemplo`, `TelecomExemplo`, `SeguroExemplo`, `Cartao Exemplo`, `Correio Exemplo`, `Streaming Exemplo`, `Mercado Exemplo`, `Banco Nacional Exemplo`. As demais 4.990 são `EmpresaSintetica0001` a `EmpresaSintetica4990`, com domínio `empresasintetica0001.com.br` e `www.` correspondente. |
| 25.4 | Agente | Ao final do script, imprimir as contagens (200 / 5.000 / 5.000), conferir que toda empresa do index tem template e que o índice único existe, e imprimir `BASE OK` ou `BASE INVALIDA`. |
| 25.5 | Humano | Executar `mongosh --file base/seed.mongo.js` e confirmar `BASE OK`. |
| 25.6 | Agente | Criar os 15 arquivos `base/amostras/S-xx.eml` da tabela abaixo (texto RFC 822 válido, `Content-Type` HTML ou texto, UTF-8). O conteúdo de cada um **DEVE** corresponder à coluna "Conteúdo". O resultado esperado é fixo e não pode ser alterado depois da execução. |

**Catálogo de amostras e resultado esperado**

| Amostra | Conteúdo | Resultado esperado |
|---|---|---|
| S-01 | Texto cita `BancoExemplo`; remetente `avisos@bancoexemplo.com.br`; um link `https://www.bancoexemplo.com.br/extrato` | Não suspeito. Sem relatório, sem notificação |
| S-02 | Cita `BancoExemplo`; remetente `seguranca@bancoexemplo-seguro.com`; link `https://bancoexemplo-seguro.com/login` | Suspeito. Relatório com ao menos 2 inconsistências (link e remetente) |
| S-03 | Cita `BancoExemplo`; remetente oficial; link `https://bit.ly/3AbCdEf` | Suspeito. Relatório com inconsistência de encurtamento |
| S-04 | Cita `BancoExemplo`; links oficiais; remetente `avisos@bancoexemplo.net` | Suspeito. Relatório com inconsistência de remetente |
| S-05 | Texto sem nenhum link | Ignorado. Nada é analisado, gravado ou notificado |
| S-06 | HTML só com `<a href="mailto:contato@bancoexemplo.com.br">` e `<a href="/ajuda">` | Ignorado (nenhum hyperlink válido, P-08) |
| S-07 | Cita `BancoExemplo`; remetente oficial; link `https://www.bancoexemplo.com.br@evil.example/login` | Suspeito (o domínio real do link é `evil.example`) |
| S-08 | Cita `BancoExemplo`; remetente `AVISOS@BancoExemplo.com.br`; link `HTTPS://WWW.BANCOEXEMPLO.COM.BR/Fatura` | Não suspeito (sem diferenciar maiúsculas) |
| S-09 | Cita `Banco Nacional Exemplo`; remetente `avisos@banconacionalexemplo.com.br`; link oficial | Não suspeito (empresa de 3 palavras identificada) |
| S-10 | Cita `Banco Nacional Exemplo`; remetente oficial; link `https://banconacionalexemplo.top/entrar` | Suspeito |
| S-11 | Não cita nenhuma empresa do index; link `https://site-desconhecido.example/x` | Ignorada por completo, mesmo contendo hyperlink, conforme decisão DR-003 (sem relatório e sem notificação) |
| S-12 | Somente texto puro: cita `BancoExemplo`; remetente oficial; `Acesse https://www.bancoexemplo.com.br/atendimento` | Não suspeito. Conta como hyperlink (P-08) |
| S-13 | HTML malformado (tags sem fechar); cita `BancoExemplo`; remetente oficial; `<a href="https://bancoexemplo-seguro.com/x">` | Suspeito, sem exceção |
| S-14 | HTML com 20 links, 19 oficiais de `LojaExemplo` e 1 `https://lojaexemplo-promo.com/x`; remetente oficial | Suspeito. Serve de medida de tempo (CA-07) |
| S-15 | Remetente válido, corpo vazio | Ignorado |

**Pronto quando**
1. `mongosh --file base/seed.mongo.js` imprime as contagens 200, 5.000 e 5.000 e `BASE OK`.
2. Existem 15 arquivos `S-01.eml` a `S-15.eml`, e um teste confirma que todos abrem no `MimeKit` sem exceção.
3. A conta de teste existe e a conta pessoal do autor não é usada em nenhum lugar do projeto.

**NÃO DEVE:** usar empresas ou domínios reais, usar a conta pessoal, colocar a base de teste em servidor externo.

---

---

## 9. Pendências para o humano resolver ANTES de liberar o plano

| Nº | Pendência | Recomendação |
|---|---|---|
| 1 | Confirmar as decisões P-01 a P-17 da seção 5 (principalmente P-02 só Gmail, P-04 `oauth.local.json`, P-06 nomes de coleção, P-10 classes de coordenação). | Aprovar como estão: é o caminho mais curto |
| 2 | Confirmar o ID `F-XX` de cada funcionalidade no Documento de Requisitos vigente (nomes de branch) e se o RF-11 existe nele. | Informar a lista vigente ao agente |
| 3 | **Resolvida em 12/11/2026 (DR-003):** quando nenhuma empresa é identificada, ignorar a mensagem inteira, inclusive hyperlinks e encurtadores. | Decisão explícita do usuário: sem empresa conhecida, ignorar para evitar falsos positivos |

---

## Controle de Versão do Documento

| Versão | Data | Item alterado | Razão e impacto |
|---|---|---|---|
| 1.1 | 12/11/2026 | R-13, Etapa 22, amostra S-11 e Pendência 3 | DR-003: empresa não identificada implica ignorar a mensagem inteira, inclusive hyperlinks e encurtadores; exceção limitada à ordem humana registrada. |
| 1.0 | 28/10/2026 | Emissão inicial | Plano de ação da Fase 4 (Etapas 20 a 25) para agentes de codificação, dividido por fase para caber nos limites de contexto. |
