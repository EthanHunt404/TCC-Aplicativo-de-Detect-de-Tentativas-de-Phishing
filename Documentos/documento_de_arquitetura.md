# Documento de Arquitetura

> Etapa 19 do Roteiro de Execução (Fase 3). Consolida as classes de domínio, o algoritmo de análise e a estrutura de dados definidos nas etapas 14 a 17. As decisões arquiteturais que fundamentam este documento estão registradas separadamente no ADR.

---

## 1. Classes de Domínio — Regras de Negócio

> Etapa 14 do Roteiro de Execução (Fase 2). Lista apenas as classes que representam algo do problema em si — sem interfaces de repositório e sem classes de coordenação/serviço (essas ficam para quando a arquitetura em camadas for desenhada, Etapa 15).

### 1.1 ConteudoRelevante

Conhece o remetente e os hyperlinks de um e-mail recebido, e diz se contém algum hyperlink.

- `ConteudoBruto` (uma cópia do conteúdo bruto)
- `EndereçoRemetente` (endereço de correio eletrônico)
- `EmpresaRemetente` (nome da suposta empresa remetente)
- `Hyperlinks` (lista de `strings`)

### 1.2 Inconsistencia

Descreve um motivo específico pelo qual uma verificação falhou.

- `Tipo`
- `Descricao`

### 1.3 RelatorioInconsistencias

Reúne as inconsistências encontradas em uma mensagem e registra quando foi gerado.

- `Conteudo` (referência a `ConteudoRelevante.ConteudoBruto`)
- `Inconsistencias` (lista de `Inconsistencia`)
- `DataGeracao`

### 1.4 EmailAnalist

Executa o algoritmo de análise (seção 2) sobre um `ConteudoRelevante` e devolve um `RelatorioInconsistencias` quando o correio eletrônico é suspeito, ou nada quando não há o que reportar.

- `Consulta` (referência a `IConsulta`, recebida na construção)
- `RelatorioInconsistencias` Analisar(`ConteudoRelevante` conteudo)

### 1.5 Classes fora de domínio — implementações sólidas

`Capturador`: (captura o correio eletrônico para uma fila e filtra o conteúdo, entregando um `ConteudoRelevante` apenas quando um hyperlink é encontrado) 
`MongoDBInterface`: (uma interface generica na database MongoDB para facilitar as consultas do `Algoritmo de Analise`)

---

## 2. Algoritmo de Análise

### 2.1 Objetivo

Analisar os correios eletrônicos recebidos e identificar aqueles que apresentam inconsistências nos hyperlinks.

### 2.2 Processamento

```text
CORREIO ELETRÔNICO recebido é capturado

hyperlinks ← verificar existência de hyperlinks
             no conteúdo do correio eletrônico

SE hyperlinks estiver vazio:
    ignorar o correio eletrônico

PARA CADA hyperlink:

    verificar domínio e padrão do hyperlink
    no Registro de domínios de encurtamento

empresa ← identificar empresa sendo impersonada
           no conteúdo do correio eletrônico
           utilizando o Registro de empresas

template ← consultar o Template específico
           da empresa identificada

PARA CADA hyperlink:

    domínio ← extrair domínio do hyperlink
    endereço ← extrair endereço de correio eletrônico

    comparar domínio e endereço com todos os
    domínios oficiais registrados no template

SE houver alguma inconsistência:
    marcar o correio eletrônico como suspeito
    registrar as inconsistências
    gerar relatório
    notificar o usuário

CASO CONTRÁRIO:
    não marcar o correio eletrônico como suspeito
```

### 2.3 Etapas

#### 2.3.1 Captura

O sistema monitora a caixa de entrada principal e captura os correios eletrônicos recebidos.

A captura não impede o recebimento do correio eletrônico na caixa de entrada principal.

#### 2.3.2 Filtragem

O conteudo do correio eletronico e verificado para existencia de hyperlinks

Correios eletrônicos sem hyperlinks são ignorados.

#### 2.3.3 Verificação de encurtamento

Cada hyperlink capturado é verificado contra o Registro de domínios de encurtamento de links.

A verificação considera os domínios registrados e os padrões de links encurtados associados a eles.

#### 2.3.4 Identificação da empresa

O nome da empresa sendo impersonada dentro do conteudo do correio eletronico é capturado referenciando o Index de empresas.

#### 2.3.5 Consulta do template

Após a identificação da empresa, o Template específico registrado no banco, da empresa é consultado para obter seus domínios oficiais.

#### 2.3.6 Verificação de domínios oficiais

O domínio de cada hyperlink e endereço de correio eletronico e comparado com todos os domínios oficiais registrados no template da empresa identificada.

#### 2.3.7 Resultado

Caso uma das verificações apresente uma inconsistência:

- o correio eletrônico é marcado como suspeito;
- as inconsistências identificadas são registradas;
- é gerado um relatório;
- o usuário é notificado.

Caso não sejam encontradas inconsistências, o correio eletrônico não é marcado como suspeito.

### 2.4 Operações principais e volume estimado

Lista de entidades x operação dominante no algoritmo, com o volume estimado de cada uma.

| Entidade | Operação dominante | Volume estimado |
|---|---|---|
| Registro de domínios de encurtamento de links | Consulta | 200 domínios |
| Index de empresas | Consulta | 5.000 empresas |
| Templates específicos de uma empresa | Consulta | 5.000 templates |

O volume do Registro de domínios de encurtamento é uma ordem de grandeza: domínios de encurtamento surgem e desaparecem diariamente, e a lista é mantida manualmente pelo operador (seção 3.6). O número de templates acompanha o de empresas do Index.

Carga de entrada: o usuário-alvo recebe até 200 correios eletrônicos por dia. Cada correio eletrônico com hyperlink dispara uma passada pelas três entidades acima.

---

## 3. Estrutura de Dados

### 3.1 Banco de dados

A PoC utilizará MongoDB como banco de dados documental. A base será composta por três tipos de documentos:

1. Registro de domínios de encurtamento de links;
2. Index de empresas;
3. Templates específicos de uma empresa.

A base será hospedada localmente.

#### 3.1.1 Justificação

A equipe, consistindo de uma unica pessoa com os papeis de projetista, pesquisador e executor, esta acostumado com essa tecnologia, e nenhuma necessidade dentro desse PoC descorda com o uso dessa tecnologia.

### 3.2 Registro de domínios de encurtamento de links

Esse documento registra um domínio conhecido de serviço de encurtamento de links e os padrões utilizados pelos links encurtados.

```json
{
  "list": [
	{
		"domain": "bit.ly",
		"patterns": [
			"bit.ly/*"
			...
		]
	},
	...
  ]
}
```

### 3.3 Index de empresas

Esse documento registra uma empresa e é utilizado para procurar o nome da empresa sendo impersonada no correio eletrônico.

```json
{
	companies = [
		EmpresaExemplo1
		EmpresaExemplo2
		...
	]
}
```

Após a identificação da empresa, o registro de template específico correspondente será consultado.

### 3.4 Template de padrao de Registro Empresas

Cada documento seguindo esse padrao, registra uma certa empresa e seus domínios oficiais.

```json
{
  "company": "Empresa Exemplo",
  "official_domains": [
    "empresa.com.br",
    "www.empresa.com.br"
  ]
}
```

Os domínios dos hyperlinks encontrados no correio eletrônico serão comparados com todos os domínios oficiais registrados no template da empresa identificada.

### 3.5 Fluxo de consulta

```text
Correio eletrônico
       │
       ▼
Index de empresas
       │
       │ empresa identificada
       ▼
Registro de template específico
       │
       │ domínios oficiais
       ▼
Comparação com os hyperlinks
```

O Registro de domínios de encurtamento de links é consultado para verificar se os hyperlinks correspondem a domínios ou padrões conhecidos de encurtamento.

### 3.6 Persistência do relatório de inconsistências

O `RelatorioInconsistencias` é o único dado efetivamente gravado pela aplicação, é salvo localmente como arquivo `.txt`, na pasta padrão de documentos do usuário, dentro de uma subpasta `Phishing Reports`.
As demais estruturas descritas neste documento (domínios de encurtamento, index de empresas, templates de empresa) são somente consultadas pela aplicação. 
Qualquer adição, remoção ou atualização nelas é feita manualmente por um operador diretamente na base — a aplicação não possui nenhuma operação de escrita sobre essas coleções

## 4. Rastreabilidade dos Requisitos de Prioridade Alta

> O Documento de Requisitos v1.0 não classifica formalmente os requisitos por prioridade. Para esta rastreabilidade, adota-se o critério: todo RF do núcleo funcional (RF-01 a RF-11) é Alta, 
por não existir escopo "desejável" nesta PoC — tudo o que está especificado é obrigatório. RNF-01 a RNF-04 são Alta por serem eliminatórios (TR 3.3/6.1) ou por corresponderem ao critério de tempo/volume exigido pelo AV1 (T4). 
RNF-05 a RNF-07 permanecem Média e ficam fora desta tabela, por não determinarem estrutura de camadas ou de dados.

| Requisito | Descrição resumida | Onde é atendido na arquitetura |
|---|---|---|
| RF-01 | Operar continuamente em segundo plano | Camada Presentation (ADR 0005) — bandeja do sistema |
| RF-02 | Autenticação OAuth2, sem senha | ADR 0004; camada Infrastructure |
| RF-03 | Bloquear análise sem autorização válida | Camada Infrastructure (guarda de OAuth2) — **ainda sem classe própria** |
| RF-04 | Monitorar IMAP/SSL (porta 993) | `Capturador` (§1.4); ADR 0003 (MailKit); camada Infrastructure |
| RF-05 | Identificar mensagens com hyperlink | `ConteudoRelevante` (§1.1); Algoritmo §2.3.2 |
| RF-06 | Ignorar mensagens sem hyperlink | Algoritmo §2.2 / §2.3.2 |
| RF-07 | Validar hyperlink (encurtamento, domínio oficial, remetente) | Algoritmo §2.3.3 e §2.3.6; Estrutura de Dados §3.2 e §3.4 |
| RF-08 | Marcar mensagem como suspeita | Algoritmo §2.3.7; `Inconsistencia` (§1.2) |
| RF-09 | Notificar o usuário | Algoritmo §2.3.7; camada Presentation (ADR 0005) |
| RF-10 | Gerar relatório de inconsistências | `RelatorioInconsistencias` (§1.3); Estrutura de Dados §3.6 |
| RF-11 | Identificar empresa impersonada | Algoritmo §2.3.4; Estrutura de Dados §3.3 (Index de empresas) |
| RNF-01 | Analisar em até 10s | **sem verificação formal — prorrogada para Fase 5 (testes)** |
| RNF-02 | Não armazenar senha do usuário | ADR 0004; nenhuma estrutura de dados deste documento guarda credencial |
| RNF-03 | Processamento local (client-side) | Algoritmo completo roda no cliente (§2.2–§2.3); |