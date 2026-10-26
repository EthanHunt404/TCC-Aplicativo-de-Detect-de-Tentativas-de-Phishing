Registro de Decisões Arquiteturais (ADR)

Aplicativo de Detecção de Tentativas de Phishing por Correio Eletrônico. Documento único, addendado a cada nova decisão — não um arquivo por ADR.

Nº	Decisão	Status
0001	Linguagem e plataforma: C# / .NET	Aceita
0002	Banco de dados: MongoDB	Aceita
0003	Biblioteca de acesso IMAP: MailKit	Aceita
0004	Autenticação: OAuth2 (Google/Microsoft)	Aceita
0005	Arquitetura em camadas (Domain / Application / Infrastructure / Presentation)	Aceita
0006	Estrutura de consulta: tabela de dispersão em memória	Aceita

ADR 0001 — Linguagem e plataforma: C# / .NET

Data: 25/09/2026 
Status: Aceita
Contexto: É necessário escolher a linguagem/plataforma de implementação do aplicativo.
Decisão: C# sobre .NET.
Justificativa: A equipe esta mais familiar com C# e o ambiente .NET

ADR 0002 — Banco de dados: MongoDB

Data: 25/09/2026 (revisada em 21/10/2026: decisão mantida; contexto e justificativa reescritos com operação predominante e volume)
Status: Aceita
Contexto: A base de templates de empresa, o Index de empresas e o registro de domínios de encurtamento precisam de um meio de armazenamento local (RNF-04). A operação predominante é a consulta (a aplicação não escreve nessas bases) e o volume estimado é de 200 domínios de encurtamento, 5.000 empresas e 5.000 templates, com até 200 correios eletrônicos por dia (Documento de Arquitetura, seção 2.4).
Decisão: MongoDB, hospedado localmente (localhost), como serviço separado da aplicação principal, com três tipos de documento: Registro de domínios de encurtamento, Index de empresas e Templates específicos de empresa (um documento por empresa, seguindo o padrão de registro específico de empresas, em que cada documento registra uma certa empresa e seus domínios oficiais).
Justificativa: Os templates são documentos independentes, sem relações entre si e sem necessidade de junções, o que corresponde ao modelo documental; com a consulta como operação predominante e o volume estimado acima, o MongoDB atende com folga o limite de 10 segundos por mensagem (Documento de Arquitetura, seção 2.5). A familiaridade da equipe foi usada apenas como critério de desempate.

ADR 0003 — Biblioteca de acesso IMAP: MailKit

Data: 25/09/2026 
Status: Aceita
Contexto: É necessário monitorar a caixa de entrada via IMAP sobre SSL/TLS (RF-04).
Decisão: Biblioteca MailKit (C#).
Justificativa: Biblioteca padrão de mercado para IMAP em C#, dento do ambiente .NET, alem da equipe possuir alta familiaridade com a biblioteca em questao

ADR 0004 — Autenticação: OAuth2 (Google/Microsoft)

Data: 25/09/2026 
Status: Aceita
Contexto: O acesso à caixa de entrada real do usuário precisa de autorização, sem armazenar senha (RNF-02).
Decisão: Fluxo OAuth2 padrão dos provedores (Google/Microsoft).
Justificativa: Exigência do próprio provedor para qualquer aplicação de terceiro acessar IMAP; não há alternativa dentro do escopo.

ADR 0005 — Arquitetura em camadas (Domain / Application / Infrastructure / Presentation)

Data: 25/09/2026 
Status: Aceita
Contexto: O roteiro de execução (Etapa 15) e os critérios C10–C12 do AV1 exigem separação de camadas com as dependências apontando para o domínio.
Decisão: Quatro camadas — Domain (regras de negócio, sem infraestrutura), Application (casos de uso, orquestra domínio + repositórios), Infrastructure (implementações concretas: Mongo, MailKit, OAuth2), Presentation (bandeja do sistema, notificações nativas).
Justificativa: Exigência do roteiro e da avaliação. As interfaces de repositório ficam declaradas no Domain (inversão de dependência), implementadas no Infrastructure.

ADR 0006 — Estrutura de consulta: tabela de dispersão em memória

Data: 21/10/2026
Status: Aceita
Contexto: O algoritmo consulta, para cada hyperlink, o Registro de domínios de encurtamento e, para cada correio eletrônico, o Index de empresas (volume estimado de 200 domínios e 5.000 empresas). Foram comparadas, por estimativa, lista com varredura sequencial e tabela de dispersão (Documento de Arquitetura, seção 2.5).
Decisão: O Registro de domínios de encurtamento e o Index de empresas são consultados por tabela de dispersão em memória, carregada da base. A consulta ao Template da empresa é feita por chave exata (nome da empresa).
Justificativa: O número de operações da varredura cresce com o número de domínios e de empresas; o da tabela não. Pelas estimativas, as duas estruturas respeitam o limite de 10 segundos, então a decisão se apoia no crescimento do custo, não no limite de tempo.
