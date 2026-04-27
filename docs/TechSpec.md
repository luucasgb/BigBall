# BigBall — Technical Specification (Tech Spec)

**Versão:** 1.2  
**Data:** 23 de abril de 2026  
**Documento relacionado:** [PRD.md](./PRD.md) — requisitos de produto, regras de pontuação, lock, elegibilidade e auditoria **não** são repetidos aqui; este ficheiro cobre **stack**, integrações e decisões de implementação.

---

## 1. Objetivo deste documento

Definir a **stack**, os **limites de responsabilidade** entre componentes e os **requisitos de engenharia** necessários para implementar o MVP descrito no PRD, evitando duplicar texto normativo do produto.

---

## 2. Visão da arquitetura

| Camada | Tecnologia | Função |
| ------ | ---------- | ------ |
| Auth e Postgres gerido | **Supabase** | Autenticação (e-mail/senha, Google OAuth), `auth.users`, Postgres, Storage (avatars), políticas de rede/segredos no painel. |
| API e regras de negócio | **ASP.NET Core** (Web API, REST) | Única fonte de verdade para palpites, elegibilidade, pontuação, ranking, admin de plataforma (4.11 no PRD). |
| Jobs | **ASP.NET Core** (Hosted Service / Worker dedicado) ou processo separado na mesma solução | Sincronização com provedor de partidas, recálculo idempotente quando o feed substituir resultado manual. |
| Cliente web (MVP) | **Blazor WebAssembly** | UI pública e autenticada; chama a API com JWT emitido pelo Supabase Auth. |
| Cliente mobile (roadmap) | **.NET MAUI** | Mesmo contrato de API que o Blazor; **fora do escopo de implementação do MVP inicial** (PRD 1.2). |

Fluxo típico: o usuário autentica-se via **Supabase Auth** → o cliente obtém **access token (JWT)** → pedidos à **API .NET** com `Authorization: Bearer <jwt>` → a API valida o token e acede ao Postgres (Supabase) com permissões adequadas (ligação servidor + princípio do menor privilégio).

---

## 3. Supabase

### 3.1 Identidade e perfil

- **Conta:** `auth.users` é a **única** fonte de verdade para identidade (e-mail, providers OAuth, ids).
- **Perfil de produto:** tabela `profiles` com **`id` UUID = `auth.users.id`** (relação 1:1). Campos esperados incluem, no mínimo, alinhados ao PRD 4.2: nome de exibição, referência a avatar (URL ou path em Storage).
- **Não** existe tabela de “usuário de aplicação” espelhada além de `profiles`; FKs de domínio (`pool_memberships`, `predictions`, etc.) referenciam `profiles.id` (equivalente a `auth.users.id`).

### 3.2 Papéis (administrador de plataforma vs admin de bolão)

O PRD distingue **administrador de plataforma** (4.11) de **administrador de bolão**. A atribuição concreta é decisão de implementação, por exemplo:

- `profiles.is_platform_admin` (boolean), **ou**
- claim em `app_metadata` / `user_metadata` sincronizado via trigger ou processo de onboarding,

desde que a **API .NET** consiga avaliar autorização de forma consistente e testável. Documentar a opção escolhida aqui quando implementado.

### 3.3 Storage

- Bucket para **avatars** (público restrito ou assinatura de URL, conforme política de privacidade).
- Upload pode ser mediado pela API (service role) ou por políticas RLS no cliente; o PRD não impõe o caminho — o Tech Spec recomenda **não expor service role no Blazor** e preferir **API ou Supabase Storage com RLS** alinhada ao `id` do usuário.

### 3.4 Row Level Security (RLS)

Opcional no MVP se **todo** o acesso a dados sensíveis passar pela API com credencial de servidor. Se no futuro o cliente ler/escrever Postgres diretamente, as políticas RLS devem espelhar as mesmas regras da API. Para o MVP centrado em Blazor + API, **RLS pode ficar em modo restritivo mínimo** (apenas service/backend) até abrir acesso direto.

---

## 4. Backend .NET

### 4.1 Projeto e runtime

- **.NET 8+** (LTS na data do documento).
- Projeto **ASP.NET Core Web API**, OpenAPI (Swashbuckle ou nativo) para contratos e testes.

### 4.2 Acesso a dados

- **Entity Framework Core** com provider **Npgsql** contra o Postgres do Supabase, **ou** Dapper onde queries complexas (ex. ranking) beneficiem de SQL explícito.
- **Migrations** versionadas no repositório; connection string apenas em servidor/CI (nunca no WASM).

### 4.3 Autenticação na API

- **JWT Bearer:** validação de tokens emitidos pelo Supabase (issuer, audience e JWKS conforme documentação atual do projeto Supabase).
- O `sub` do JWT identifica o usuário; joins com `profiles` para nome/avatar em respostas.

### 4.4 Autorização

- **Policy-based** authorization: membro de bolão, admin de bolão, admin de plataforma.
- Rate limiting nativo em rotas de autenticação/auxiliares expostas pela API (alinhado ao PRD, secção 10).

### 4.5 Integração com o provedor de partidas

- Cliente HTTP resiliente (**HttpClient** + **Polly**: retry, circuit breaker).
- Configuração por ambiente (`IOptions`): URL base, chave API, timeouts.
- **Adapter** isolado: mapeia o payload do fornecedor para o modelo canónico `Match` do domínio (PRD secção 8), segundo as regras de **mapeamento explícito** da **secção 6.2** (sem inferir TR a partir de “final” ambíguo).

### 4.6 Jobs agendados

- Sincronizar calendário/estado/placar; ao detetar atualização do provedor que substitui resultado manual, disparar **recálculo idempotente** de pontos e rankings (PRD 4.11).
- Implementação sugerida: **`IHostedService` / `BackgroundService`**, **Quartz.NET** ou **Hangfire** — escolha documentada no repositório quando o código existir.

---

## 5. Clientes .NET

### 5.1 Blazor WebAssembly (MVP)

- Consome a API REST; armazena tokens em memória/`localStorage` conforme trade-off de segurança vs UX (refresh token: seguir recomendações Supabase + ameaças XSS).
- Não embute **chaves** do provedor de dados desportivos nem **service role** do Supabase.

### 5.2 .NET MAUI (pós-MVP)

- Mesmo modelo: JWT no header; armazenamento seguro de credenciais (Keychain / Keystore).
- Partilha opcional de **cliente HTTP** / DTOs via biblioteca de projeto comum (`BigBall.Client.Core` ou similar) — decisão na implementação.

---

## 6. Provedor de dados esportivos

A escolha do fornecedor **não bloqueia** o modelo canónico `Match`, desde que o adapter preencha os campos necessários ao PRD (TR, pênaltis, início oficial para lock, etc.).

| Fase | Provedor alvo | Observação |
| ---- | ------------- | ---------- |
| **Desenvolvimento e testes do MVP** | [SportsAPI Pro](https://docs.sportsapipro.com/introduction) | REST e WebSocket; avaliar tier **Football** e latência adequada ao volume da Copa. |
| **Produção (a confirmar)** | SportsAPI Pro *ou* migração | Critérios: custo, cobertura Copa 2026, SLA, qualidade do placar (TR vs prorrogação vs pênaltis). Alternativa citada no PRD: [FlashScore na RapidAPI](https://rapidapi.com/rapidapi-org1-rapidapi-org-default/api/flashscore4) após análise comparativa. |

### 6.1 Requisitos de engenharia (partidas)

1. Parametrizar **chaves e URLs** por ambiente (dev/staging/prod); **nunca** expor API keys no Blazor WASM nem em repositórios públicos.
2. Manter o **mapeamento campo-a-campo** por provedor na **secção 6.2** (tabelas vivas); ao implementar, pode duplicar um resumo em comentário no código do adapter apontando para o commit referenciado em 6.1.3.
3. Registar **versão/commit** (e data) sempre que o **payload do provedor** ou a **tabela de mapeamento** mudar — rastreabilidade junto à auditoria do PRD 4.11.

### 6.2 Mapeamento explícito no adapter (TR, prorrogação e pênaltis)

**Decisão:** cada integração com um fornecedor de dados desportivos deve definir e manter um **mapeamento explícito** (caminho JSON / nome de campo / transformação documentada) do payload do fornecedor para o **modelo canónico** usado pelo BigBall. O placar de **tempo regulamentar (TR)** usado nas faixas 1–5 do PRD **só** pode ser preenchido a partir de **campos do feed que correspondam inequivocamente** a “gols ao fim do TR (90 min + acréscimos)” **ou** a partir do **fluxo manual global** do PRD 4.11.

**Proibição:** **não** deduzir TR a partir de um único par “mandante × visitante” rotulado como *final* / *full time* / equivalente **quando** o próprio feed (ou outros campos mapeados) indicar **prorrogação** ou **disputa de pênaltis** **e** não existir, no mapeamento, origem explícita para o placar **ao fim do TR**. Nesses casos o adapter marca lacuna conforme coluna **Gap** abaixo e o sistema segue o PRD 4.11 até o feed ou o mapeamento permita TR confiável (mantendo a **precedência do provedor** quando este atualizar dados inequívocos).

**Modelo canónico (alvos do mapeamento)** — nomes orientativos; o schema persistido pode diferir:

| Alvo canónico | Uso |
| ------------- | --- |
| ID estável da partida no fornecedor | Jobs, deduplicação, correlação manual ↔ feed |
| Início oficial da partida | Lock de palpites (PRD 4.7) |
| Estado / fase da partida | Elegibilidade, UI, decisão de recálculo |
| Gols mandante **ao fim do TR** | Pontuação faixas 1–5 |
| Gols visitante **ao fim do TR** | Idem |
| `went_to_extra_time` (booleano, recomendado) | Validação cruzada com “final” ambíguo; UI |
| `went_to_penalty_shootout` (booleano) | Bônus +3 |
| Identificador do **vencedor na disputa de pênaltis** | Bônus +3 (PRD 4.8) |
| Metadados de **origem vigente** do resultado | PRD 4.6, 4.11 |

#### 6.2.1 Tabela de mapeamento — SportsAPI Pro (MVP)

Preencher **Campo / path no payload** durante a implementação, com base na documentação real do tier contratado. A coluna **Gap** = *sim* quando o fornecedor **não** expuser dado suficiente para preencher o alvo canónico.

| Alvo canónico | Campo / path no payload (SportsAPI Pro) | Notas | Gap |
| ------------- | ---------------------------------------- | ----- | --- |
| ID estável da partida | *a preencher* | | |
| Início oficial | *a preencher* | Fuso e formato normalizados para UTC no domínio | |
| Estado da partida | *a preencher* | Alinhar a “não iniciada / ao vivo / concluída” usado no produto | |
| Gols mandante (TR) | *a preencher* | **Não** mapear para placar após prorrogação | |
| Gols visitante (TR) | *a preencher* | Idem | |
| Houve prorrogação | *a preencher* | Se ausente, documentar implicação na coluna Notas | |
| Houve disputa de pênaltis | *a preencher* | | |
| Vencedor na disputa de pênaltis | *a preencher* | | |

#### 6.2.2 Provedor adicional ou migração (ex.: FlashScore via RapidAPI)

Criar **nova** tabela com a mesma estrutura de alvos canónicos; **não** reutilizar paths do SportsAPI Pro sem revisão. Critérios de escolha de produção permanecem na tabela introdutória da secção 6.

#### 6.2.3 Comportamento quando Gap = sim ou TR inválido

O job de sincronização **não inventa** gols de TR. Se a partida, pelo calendário interno, exigir resultado para pontuação e o canónico **TR** estiver indisponível ou marcado como gap: aplicar **PRD 4.11** (administrador de plataforma); quando o feed passar a fornecer TR mapeável, **prevalece** o provedor e dispara-se recálculo idempotente.

---

## 7. Pontuação, ranking e materialização

- As **faixas de pontos** e a **cadeia de desempate** até às contagens por faixa são normativas no **PRD 4.8–4.10**; a implementação deve ser **determinística** e coberta por **testes unitários** (.NET) sobre o motor de pontuação.
- **Persistência de pontos** por (`user_id`, `pool_id`, `match_id`): materializada (tabela) *vs* calculada on read — decisão de performance documentada ao implementar; o PRD assume **0** para partidas sem palpite válido.

### 7.1 Sorteio final (1..n), neutro/testemunha e auditoria (PRD 4.8)

As regras de produto estão no **PRD 4.8** (critérios de aceite e glossário). Aqui ficam **regras operacionais e de implementação** já fechadas.

**Numeração 1..n em cada rodada**

1. Considerar apenas os **n** empatados **na posição em disputa** na rodada atual.
2. Ordenar esses **n** por **nome de exibição** (`profiles`, PRD 4.2) em **ordem alfabética crescente** (comparação de *string* com cultura e opções fixadas no código — recomendação: `StringComparison` / `CompareInfo` consistente entre ambientes, p.ex. `CultureInfo.GetCultureInfo("pt-BR")`, `ignoreCase: true` se o produto assim o definir no UI).
3. Se dois nomes forem **iguais** após a normalização escolhida, desempatar por **`user_id` (UUID) em ordem lexicográfica** do texto canónico do GUID — garante **determinismo** sem juízo humano.
4. Após ordenar, atribuir **1** ao primeiro da lista, **2** ao segundo, …, **n** ao último.

**Elegibilidade de quem conduz o sorteio (por ordem)** — alinhado ao **PRD 4.8**

1. **Administrador do bolão** (membro com papel admin naquele `Pool`), se **não** for um dos **n** empatados na rodada.
2. Se **(1)** não se aplicar (administrador entre os **n** ou papel admin inexistente), **qualquer outro membro** do mesmo bolão **fora** dos **n** empatados.
3. Se **(2)** não se aplicar (não existe membro do bolão fora dos **n**), o usuário do fluxo na UI designa **testemunha**: **qualquer pessoa não participante** do bolão (identificação mínima para auditoria: nome livre + opcional contacto, conforme decisão de UX/privacidade), que realiza o sorteio **fora** da app ou regista o valor sorteado de forma auditável (ver abaixo).

**Auditoria (persistência mínima sugerida)**

Para cada **rodada** de sorteio dentro de um bolão: instante, identificadores dos **n** empatados, mapeamento **número ↔ `user_id`**, identificação de quem conduziu (**admin de bolão** / **membro** / **testemunha** + `user_id` se aplicável ou texto livre para testemunha), **valor sorteado**, e referência à **posição** no ranking em que o bloco empatado ocorria. Tabelas exemplificativas: `tie_break_round`, `tie_break_assignment`, `tie_break_draw` (nomes ajustáveis ao schema EF).

**Rodadas sucessivas**

Quando for necessário ordenar **todo** o bloco de **n**, após cada sorteio o vencedor da rodada “sobe” na ordenação final; os **n − 1** remanescentes repetem o processo com **nova** numeração **1..n − 1** (mesma regra alfabética e mesma cadeia de elegibilidade do neutro).

---

## 8. Observabilidade e qualidade

- **Logging:** Serilog (ou equivalente) com correlação de request.
- **Testes:** xUnit; testes de integração da API com base de dados de teste ou Testcontainers quando fizer sentido.
- **CI:** build + testes em cada push/PR.

---

## 9. Controlo de versões deste documento

Alterações de stack ou de integrações que impactem o PRD devem ser **refletidas aqui**; o PRD deve apenas referenciar este ficheiro para detalhe técnico, mantendo regras de negócio no PRD.
