# Deploy — BigBall

Arquitetura de produção:

| Componente | Serviço | Custo |
|---|---|---|
| `BigBall.Api` (.NET 8) | Railway (Docker, sempre ligada) | ~US$ 5/mês (plano Hobby) |
| `BigBall.Web` (Blazor WASM) | Cloudflare Pages (estático) | grátis |
| Banco + Auth | Supabase (já provisionado) | grátis |

O fluxo após a configuração inicial: **push no `master` → Railway rebuilda a API e o GitHub Actions republica o frontend**. Nenhum passo manual.

---

## 1. Connection string de produção (Supabase pooler)

O endpoint direto (`db.<ref>.supabase.co:5432`) é **somente IPv6** e o Railway não tem saída IPv6 — use o **Session pooler** (IPv4):

1. No dashboard da Supabase: **Connect** (topo) → aba **Session pooler**.
2. Monte a connection string no formato .NET (não copie a URI `postgresql://`):

```
Host=aws-0-<regiao>.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<project-ref>;Password=<SENHA>;SSL Mode=Require;Trust Server Certificate=true
```

> Use o pooler em **Session mode (porta 5432)**, não Transaction mode (6543): as migrations do EF Core no startup precisam de session mode.

## 2. API no Railway

1. Crie conta em [railway.com](https://railway.com) e assine o plano **Hobby** (US$ 5/mês, necessário para serviço sempre ligado).
2. **New Project → Deploy from GitHub repo** → selecione `luucasgb/BigBall`, branch `master`. O Railway detecta o `Dockerfile` da raiz e o `railway.json` (health check em `/health`).
3. Em **Variables** do serviço, adicione:

| Variável | Valor |
|---|---|
| `ConnectionStrings__Supabase` | connection string do passo 1 |
| `Supabase__ProjectUrl` | `https://<project-ref>.supabase.co` |
| `Supabase__PublishableKey` | (mesma do user-secrets local) |
| `Supabase__ServiceRoleKey` | (mesma do user-secrets local) |
| `FlashScore__ApiKey` | chave da RapidAPI |
| `CorsOrigins__0` | URL do frontend, ex. `https://bigball.pages.dev` (sem barra final — preencha após o passo 3) |
| `MatchProviderSync__Enabled` | `false` (mude para `true` em dias de jogo; consome cota da RapidAPI) |

4. Em **Settings → Networking → Generate Domain**, gere a URL pública (ex. `bigball-api-production.up.railway.app`). Informe a porta `8080` se for solicitada.
5. Verifique: `https://<dominio-railway>/health` deve responder `{"status":"ok",...}`.

## 3. Frontend no Cloudflare Pages

> **O Actions já dispara no merge para `master` — é esperado.** O workflow `deploy-web.yml` roda `on: push: branches: [master]`, então todo merge para `master` o aciona. Enquanto os passos abaixo não estiverem completos (secrets + projeto Pages), esse run **falha** (✗) — normal. Depois de concluí-los, vá em **Actions → Deploy BigBall.Web → Re-run jobs** (ou faça um novo push) para ficar verde.

1. Crie conta em [dash.cloudflare.com](https://dash.cloudflare.com).
2. Crie o projeto Pages (uma única vez, via direct upload). **Atenção à UI nova:** o botão **Create application** abre o fluxo "Create a Worker" ("Ship something new"), que é dos Workers, não do Pages. Para chegar no Pages:
   - Nessa tela "Ship something new", clique no link **"Looking to deploy Pages? Get started"** no rodapé.
   - Escolha **Upload assets** (direct upload) e dê o nome `bigball` (se o nome estiver ocupado, escolha outro e atualize `CLOUDFLARE_PAGES_PROJECT` em `.github/workflows/deploy-web.yml`).
   - Pode subir um arquivo qualquer só para criar o projeto — o workflow sobrescreve no primeiro deploy.
3. Crie um API token em **My Profile → API Tokens → Create Token**, com a permissão **Account → Cloudflare Pages → Edit**.
4. No GitHub (`Settings → Secrets and variables → Actions → Secrets`), use **New repository secret** (não _environment secret_ — o `deploy-web.yml` não declara `environment:`, então só lê secrets de repositório) e crie:
   - `CLOUDFLARE_API_TOKEN` — o token do passo anterior
   - `CLOUDFLARE_ACCOUNT_ID` — visível na URL do dashboard ou em **Workers & Pages → Account Details → Account ID** (na lateral direita)
5. Edite `BigBall.Web/wwwroot/appsettings.Production.json` com a URL do Railway (passo 2.4), **com barra final**:

```json
{ "ApiBase": "https://<dominio-railway>.up.railway.app/" }
```

6. Faça push no `master` — o workflow `deploy-web.yml` publica o site em `https://bigball.pages.dev`.
7. Volte ao Railway e confira que `CorsOrigins__0` aponta exatamente para essa origem (https, sem barra final), senão o navegador bloqueia as chamadas à API.

## 4. Smoke test

1. Abra `https://bigball.pages.dev`, faça login e navegue (exercita Auth Supabase + API + banco).
2. `https://<dominio-railway>/health` → 200.
3. Logs da API: painel do Railway → serviço → **Logs** (Serilog escreve no console em produção).

## Notas operacionais

- **1 réplica apenas**: as migrations rodam no startup da API; não escale horizontalmente sem antes mover as migrations para fora do startup.
- **Sync ao vivo**: `MatchProviderSync__Enabled=true` reinicia o serviço com o poller ativo. O orçamento diário (`MatchProviderSync__DailyRequestBudget`) também pode ser definido por variável.
- **Logs locais**: o sink de arquivo (`logs/`) e o Seq existem apenas em `appsettings.Development.json`; produção loga só no console (capturado pelo Railway).
- **User-secrets continuam valendo localmente** — nada muda no fluxo de desenvolvimento.
