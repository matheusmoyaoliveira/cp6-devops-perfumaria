# Perfumaria API — CP6: Pipelines CI/CD no Azure DevOps

3º Checkpoint do 2º semestre de **DevOps Tools & Cloud Computing** (FIAP), com o professor **João Carlos Menk**. A base é a Perfumaria API do Checkpoint 4 de .NET, migrada do Oracle para o **Azure SQL** e entregue na nuvem por pipelines de **CI/CD no Azure DevOps**.

## Integrantes

- Matheus Moya de Oliveira — RM 562822
- Ana Carolina Pereira Fontes — RM 562145
- Luisa Ganasevici de Abreu — RM 563403

## Descrição do Projeto

API REST para gerenciar um catálogo de perfumaria, em **ASP.NET Core 8**, com persistência em **Azure SQL Database**. O domínio tem um relacionamento **N:N com atributo próprio** entre `Perfume` e `NotaOlfativa`, pela entidade de junção `PerfumeNota`, que guarda a posição da nota na pirâmide olfativa (Topo, Coração ou Fundo).

Na CP6, o projeto passou a ter o ciclo completo de entrega:

- os recursos de nuvem são criados por **scripts da Azure CLI**;
- o código fica no **Azure Repos**, com a branch `main` protegida por políticas;
- o trabalho é rastreado no **Azure Boards**: cada mudança nasce de uma Task;
- o **CI em YAML** compila, testa e empacota a API a cada mudança na `main`;
- a **Release clássica** aplica o DDL no Azure SQL e publica a API no **Azure App Service**, sem nenhum passo manual.

A alteração de código demonstrada de ponta a ponta foi uma regra de domínio nova: o **ano de lançamento do perfume deve estar entre 1900 e o ano atual**, coberta por 5 testes unitários novos.

**Evidência principal:** o vídeo da entrega (link no PDF), que mostra os passos 06 e 07 do enunciado e o CRUD com SELECT no banco.

## Tecnologias Utilizadas

| Categoria | Tecnologia |
|---|---|
| Linguagem / Framework | C# / ASP.NET Core 8.0 (Web API) |
| ORM | Entity Framework Core 8.0.11, provider `Microsoft.EntityFrameworkCore.SqlServer` |
| Banco de Dados | Azure SQL Database (plano Basic, 5 DTUs, região Central US) |
| Hospedagem | Azure App Service (Linux, plano F1, runtime `DOTNETCORE\|8.0`) |
| Infraestrutura como código | Scripts Bash com Azure CLI (`infra/`) |
| Controle de versão e trabalho | Azure Repos (Git) e Azure Boards (processo Agile) |
| CI | Azure Pipelines em YAML (`azure-pipeline.yml`), agente `ubuntu-24.04` |
| CD | Release clássica do Azure DevOps (`perfumaria-cd`), agente `windows-latest` |
| Logging | Serilog (`Serilog.AspNetCore`, `Sinks.Console`, `Sinks.File`) |
| Observabilidade | OpenTelemetry 1.18.0 e Health Checks |
| Testes | xUnit, Moq, FluentAssertions |
| Documentação da API | Swagger / Swashbuckle.AspNetCore |
| Ferramentas | `dotnet-ef` 8.0.11 fixado em `dotnet-tools.json` |

## Arquitetura do Projeto

O código segue uma separação em camadas inspirada em Clean Architecture, com inversão de dependência entre Domínio e Infraestrutura:

```
cp4-perfumaria-API/
├── Controllers/              → Apresentação HTTP (tradução requisição/resposta)
├── Aplicacao/
│   ├── DTOs/                 → Contratos de entrada/saída (a API nunca expõe entidades)
│   ├── Servicos/             → Casos de uso (chamam o Domínio e o Repositório)
│   └── Middlewares/          → CorrelationIdMiddleware (rastreabilidade)
├── Dominio/
│   ├── Entidades/            → Entidades ricas, que validam as próprias regras
│   ├── Enums/                → GeneroPerfume, PosicaoNota, FamiliaOlfativa
│   └── Interfaces/           → Contratos de repositório
├── Infraestrutura/
│   ├── Dados/                → AppDbContext (mapeamento EF Core → Azure SQL)
│   ├── Repositorios/         → Implementações dos contratos do Domínio
│   ├── Health/               → BancoDadosHealthCheck
│   └── Observabilidade/      → Métricas e ActivitySource
└── Migrations/               → Migration CriacaoInicialSqlServer

Perfumaria.Tests.Unit/        → Testes de Domínio, Serviço e Controller
infra/                        → Scripts da Azure CLI
docs/json/                    → Corpos das requisições do CRUD
script.sql                    → DDL idempotente gerado pelas migrations
azure-pipeline.yml            → Pipeline de CI
dotnet-tools.json             → Versão fixa do dotnet-ef
```

**Por que essa separação existe:** o Domínio define **o que** o sistema precisa (via interfaces), sem conhecer **como** isso é feito (EF Core, Azure SQL). Por isso os testes de Serviço rodam inteiramente em memória com `Mock<IPerfumeRepositorio>`, sem banco, e é por isso que o CI consegue rodar os testes sem acesso à nuvem. A mesma separação permitiu a migração Oracle → Azure SQL na CP6 **sem tocar em nenhuma regra de negócio**: mudaram só o provider no `Program.cs`, um tipo de coluna no `AppDbContext` e a migration.

## Arquitetura de Nuvem e CI/CD

```
Azure Boards (Task) ──> branch feature/* criada pela Task ──> Pull Request para a main
                                                  │  políticas: 1 revisor, work item obrigatório,
                                                  │  revisor padrão e Build Validation (CI)
                                                  ▼
                                        merge na main
                                                  │  trigger do YAML (main)
                                                  ▼
CI  perfumaria-ci (Linux)
    SDK 8 → restore → build → testes xUnit (publicados) → confere o script.sql
    → publish da API (.zip) → artefato "drop" (api/ + sql/)
                                                  │  continuous deployment trigger (só main)
                                                  ▼
CD  perfumaria-cd (Release clássica, Windows) — stage Producao
    1. Azure SQL Database deployment: aplica o script.sql (firewall temporário para o agente)
    2. Azure App Service deploy: publica o .zip e grava a connection string
                                                  ▼
Azure — rg-cp6-perfumaria (Central US)
    App Service app-cp6-perfumaria-rm562822 ──> Azure SQL sqldb-perfumaria
```

### Decisões de arquitetura

**App Service em vez de ACR/ACI.** O enunciado permite os dois. Escolhemos o App Service porque o fluxo "a Release consome o pacote gerado pelo CI" se encaixa direto num `.zip` entregue no Web App. Também dispensa a imagem Docker e o registry (o ACR cobra mesmo parado), e o plano F1 é gratuito.

**CI em YAML e CD como Release clássica.** O CI fica versionado junto com o código (`azure-pipeline.yml`). O CD é uma Release clássica porque o enunciado pede uma "Pipeline de Release" testada com **Create Release** e disparada logo após o build. É exatamente o modelo da Release clássica com gatilho de continuous deployment no artefato.

**O gatilho do CD é de continuous deployment, filtrado para a `main`.** O CI também roda em toda PR (Build Validation), mas esses builds não podem ir para produção. O filtro garante que só builds da `main`, ou seja, código já revisado e mergeado, disparem a Release.

**CI em Linux e CD em Windows.** O CI roda em `ubuntu-24.04`, que é mais rápido para .NET. A Release usa `windows-latest` porque a tarefa *Azure SQL Database deployment* depende de ferramentas do SQL Server (Invoke-Sqlcmd) que só existem no agente Windows. O deploy num Web App Linux funciona normalmente a partir dele.

**O script.sql é aplicado antes do deploy.** A API depende das tabelas. Com o banco atualizado primeiro, a versão nova nunca sobe apontando para um esquema antigo.

## Modelagem das Entidades

### Perfume — `T_PRF_PERFUMES`

| Campo | Tipo (C#) | Coluna (Azure SQL) | Observação |
|---|---|---|---|
| Id | int | `ID` (int, PK, Identity) | Chave primária auto-incremento |
| Nome | string | `NOME` (nvarchar(150)) | Obrigatório |
| Marca | string | `MARCA` (nvarchar(100)) | Obrigatório |
| Genero | enum `GeneroPerfume` | `GENERO` (nvarchar(20)) | Persistido como texto |
| VolumeMl | int | `VOLUME_ML` (int) | Deve ser > 0 |
| Preco | decimal | `PRECO` (decimal(10,2)) | Deve ser > 0 |
| AnoLancamento | int | `ANO_LANCAMENTO` (int) | Entre 1900 e o ano atual (regra da CP6) |

### NotaOlfativa — `T_PRF_NOTAS_OLFATIVAS`

| Campo | Tipo (C#) | Coluna (Azure SQL) | Observação |
|---|---|---|---|
| Id | int | `ID` (int, PK, Identity) | Chave primária auto-incremento |
| Nome | string | `NOME` (nvarchar(100)) | Obrigatório |
| Familia | enum `FamiliaOlfativa` | `FAMILIA` (nvarchar(20)) | Persistido como texto |

### PerfumeNota — `T_PRF_PERFUME_NOTA` (entidade de junção, N:N)

| Campo | Tipo (C#) | Coluna (Azure SQL) | Observação |
|---|---|---|---|
| PerfumeId | int | `PERFUME_ID` (int, FK → `T_PRF_PERFUMES`) | Parte da chave composta; `ON DELETE CASCADE` |
| NotaOlfativaId | int | `NOTA_OLFATIVA_ID` (int, FK → `T_PRF_NOTAS_OLFATIVAS`) | Parte da chave composta; `ON DELETE CASCADE` |
| Posicao | enum `PosicaoNota` | `POSICAO` (nvarchar(20)) | Topo / Coração / Fundo |

**Por que uma entidade de junção explícita, e não o N:N implícito do EF Core:** a mesma nota pode ocupar posições diferentes da pirâmide dependendo do perfume (a Bergamota pode ser de topo num e de coração em outro). Como `Posicao` é um atributo **da relação**, e não de uma das entidades, ele exige uma entidade explícita. A chave composta (`PERFUME_ID` + `NOTA_OLFATIVA_ID`) impede associar a mesma nota duas vezes ao mesmo perfume.

**Por que `ON DELETE CASCADE`:** ao apagar um perfume ou uma nota, as associações dele somem junto, e não ficam linhas órfãs na tabela de junção apontando para um registro que não existe.

**Por que enums como texto (`HasConversion<string>()`):** o dado fica legível direto no banco, no Query editor do Portal, sem precisar saber que `0 = Masculino`.

### A regra do ano de lançamento (alteração da CP6)

A validação está na própria entidade `Perfume`, no método privado `ValidarAnoLancamento`, chamado **no construtor (POST) e no `AtualizarDados` (PUT)**:

- **Fica no Domínio, e não no Controller,** porque "um perfume não pode ter sido lançado no ano 3000" é uma regra sobre o que é um perfume válido. Qualquer caminho que crie ou altere um perfume passa pela entidade.
- **Um método só, chamado nos dois lugares,** para o limite ser ajustado num ponto só.
- **Roda antes das atribuições:** com um ano inválido, o objeto não é alterado pela metade.
- **O limite superior é `DateTime.UtcNow.Year`**, calculado na hora, para a regra não ficar desatualizada quando o ano virar.
- **A resposta ao cliente:** a entidade lança `ArgumentException`, e o controller a traduz para **400 Bad Request** com `{ "erro": "O ano de lançamento deve estar entre 1900 e <ano atual>. (Parameter 'anoLancamento')" }`.

## Configuração do Banco de Dados

As tabelas são criadas pelo **`script.sql`**, na raiz, gerado pelo Entity Framework a partir das migrations:

```bash
dotnet ef migrations script --idempotent --project cp4-perfumaria-API --output script.sql
```

**Por que idempotente:** a Release aplica o script **a cada deploy**. Com `--idempotent`, cada bloco consulta a tabela `__EFMigrationsHistory` e só cria o que ainda não existe. Num `CREATE TABLE` comum, o segundo deploy falharia com "a tabela já existe".

**Por que o CI confere o script:** se alguém criar uma migration e esquecer de gerar o `script.sql` de novo, a Release continuaria aplicando o script antigo, e a mudança nunca chegaria ao banco. O CI gera o script a partir das migrations, compara com o arquivo versionado e **falha** se forem diferentes, mostrando o comando a rodar.

A connection string **nunca** é versionada:

- **Na nuvem:** fica no variable group `vg-cp6-perfumaria`, marcada como secreta. A Release grava no App Service como a variável de ambiente `ConnectionStrings__DefaultConnection`. O `__` é como o .NET lê `ConnectionStrings:DefaultConnection` a partir de uma variável de ambiente.
- **Localmente:** fica no User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=tcp:<servidor>.database.windows.net,1433;Initial Catalog=sqldb-perfumaria;User ID=<usuario>;Password=<senha>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" --project cp4-perfumaria-API
```

## Infraestrutura na Azure (scripts da CLI)

Todos os recursos ficam no Resource Group `rg-cp6-perfumaria`, criados pelos scripts de `infra/`:

| Script | Cria |
|---|---|
| `variaveis.sh` | Nomes e região compartilhados (sem segredos) |
| `01-criar-resource-group.sh` | Resource Group `rg-cp6-perfumaria` (Central US) |
| `02-criar-banco.sh` | SQL Server `sql-cp6-perfumaria-rm562822`, as regras de firewall e o banco `sqldb-perfumaria` (Basic) |
| `03-criar-app-service.sh` | App Service Plan Linux `asp-cp6-perfumaria` (F1) e o Web App `app-cp6-perfumaria-rm562822` (.NET 8) |
| `99-apagar-recursos.sh` | Apaga o Resource Group inteiro (fim do projeto) |

```bash
cp infra/.env.example infra/.env      # preencher SQL_ADMIN_PASSWORD
bash infra/01-criar-resource-group.sh
bash infra/02-criar-banco.sh
bash infra/03-criar-app-service.sh
```

**Decisões:**

- **Um script por recurso:** cada parte da infraestrutura é criada e conferida separadamente, e um erro num passo não esconde os outros. Todos usam `set -euo pipefail`, então param no primeiro erro.
- **Região Central US:** na assinatura de estudante, o Azure SQL recusa novas criações nas outras regiões permitidas pela política (`RegionDoesNotAllowProvisioning`). A disponibilidade foi consultada antes, pela API de *capabilities* do `Microsoft.Sql`, e o App Service foi colocado na mesma região do banco.
- **Firewall do Azure SQL:** a regra `0.0.0.0` é a forma da Azure de liberar só os serviços internos dela (o App Service), e não a internet. Uma segunda regra libera o IP de quem roda o script, para as consultas de evidência no Query editor.
- **A senha do administrador** vem de `infra/.env`, que fica fora do Git. Só o modelo `infra/.env.example` é versionado.
- **Custo:** o plano F1 é gratuito, e o Azure SQL Basic custa cerca de US$ 5 por mês, cobrado por hora. Ao fim da avaliação, `99-apagar-recursos.sh` remove tudo.

## Pipelines no Azure DevOps

### CI — `perfumaria-ci` (`azure-pipeline.yml`)

| # | Passo | Por quê |
|---|---|---|
| 1 | `UseDotNet@2` (SDK 8.0.x) | A mesma versão do projeto (`net8.0`) |
| 2 | Restore | Baixa os pacotes NuGet uma vez só |
| 3 | Build (`--no-restore`) | Compila a API e os testes em Release |
| 4 | Testes xUnit (`--no-build`, `publishTestResults: true`) | Publica os resultados na aba **Tests** da execução |
| 5 | Conferência do `script.sql` | Gera o DDL a partir das migrations e falha se o arquivo do repositório estiver desatualizado |
| 6 | Publish da API (`zipAfterPublish`) | Gera o pacote `.zip` que a Release entrega |
| 7 | Cópia do `script.sql` | Leva o DDL junto no artefato |
| 8 | `PublishBuildArtifacts@1` (`drop`) | O formato de artefato nativo da Release clássica |

- **Gatilhos:** `trigger` na `main` (todo merge dispara o CI) e a **Build Validation** na política da `main` (toda PR roda o CI antes do merge). No Azure Repos, a validação de PR é feita pela política de branch: o `pr:` do YAML só vale para repositórios no GitHub.
- O `dotnet-ef` vem do **tool manifest** (`dotnet tool restore`), na versão 8.0.11, a mesma da máquina de desenvolvimento. Assim, o script gerado no agente é idêntico ao gerado localmente.

### CD — `perfumaria-cd` (Release clássica)

- **Artefato:** `_perfumaria-ci`, com o **continuous deployment trigger** ligado e o filtro de branch `main`. O pull request trigger fica desligado.
- **Stage `Producao`** (gatilho *After release*, agente `windows-latest`), com o variable group `vg-cp6-perfumaria` ligado:
  1. **Azure SQL Database deployment:** SQL Server Authentication com `$(SqlUser)` e `$(SqlPassword)`. Aplica `drop/sql/script.sql` e usa o firewall *AutoDetect* com *Delete Rule After Task Ends*, ou seja, abre uma regra só para o IP do agente e apaga ao terminar.
  2. **Azure App Service deploy:** um Web App on Linux com o pacote `drop/api/*.zip`, o runtime `DOTNETCORE|8.0`, o startup command `dotnet cp4-perfumaria-API.dll` e o app setting `-ConnectionStrings__DefaultConnection "$(ConnectionString)"`.
- **Acesso à Azure:** pela service connection `sc-azure-cp6`, do tipo Azure Resource Manager com **workload identity federation** (não guarda segredo) e **escopo limitado ao Resource Group** `rg-cp6-perfumaria`, com o papel Contributor só nele.

### Fluxo de trabalho no Repos e no Boards

- **Branch `main` protegida:** mínimo de 1 revisor (o autor pode aprovar, como pede o enunciado), work item obrigatório, revisor padrão obrigatório e a Build Validation com o `perfumaria-ci`.
- **Toda mudança nasce de uma Task do Azure Boards:** a branch é criada pela própria Task, o que liga automaticamente a branch, os commits e a PR ao work item.
- **Na conclusão da PR:** *Complete associated work items* fecha a Task, e a branch **não** é apagada, para preservar o histórico.
- **Processo Agile:** a Task tem os estados New → Active → Closed, os mesmos que o enunciado pede.

## Endpoints

### Perfumes

| Verbo | Rota | Descrição |
|---|---|---|
| GET | `/api/Perfumes` | Lista todos os perfumes, com as notas associadas |
| GET | `/api/Perfumes/{id}` | Busca um perfume por ID |
| POST | `/api/Perfumes` | Cria um perfume |
| PUT | `/api/Perfumes/{id}` | Atualiza um perfume |
| DELETE | `/api/Perfumes/{id}` | Remove um perfume (e as associações, por cascade) |
| POST | `/api/Perfumes/{id}/notas` | Associa uma nota existente ao perfume, com a posição na pirâmide |

### Notas Olfativas

| Verbo | Rota | Descrição |
|---|---|---|
| GET | `/api/NotasOlfativas` | Lista todas as notas |
| GET | `/api/NotasOlfativas/{id}` | Busca uma nota por ID |
| POST | `/api/NotasOlfativas` | Cria uma nota |
| PUT | `/api/NotasOlfativas/{id}` | Atualiza uma nota |
| DELETE | `/api/NotasOlfativas/{id}` | Remove uma nota |

### Diagnóstico

| Verbo | Rota | Descrição |
|---|---|---|
| GET | `/health` | Health Check com teste real de conexão ao Azure SQL |

## Como testar o CRUD (JSONs)

Swagger em produção: `https://app-cp6-perfumaria-rm562822.azurewebsites.net/swagger`. O plano F1 hiberna sem uso, então o primeiro acesso pode levar de 30 a 60 segundos.

| # | Operação | Endpoint | Corpo | Resposta esperada |
|---|---|---|---|---|
| 1 | Criar nota | `POST /api/NotasOlfativas` | `docs/json/notas-olfativas/post.json` | 201 |
| 2 | Consultar nota | `GET /api/NotasOlfativas/{id}` | — | 200 |
| 3 | Atualizar nota | `PUT /api/NotasOlfativas/{id}` | `docs/json/notas-olfativas/put.json` | 204 |
| 4 | Criar perfume | `POST /api/Perfumes` | `docs/json/perfumes/post.json` | 201 |
| 5 | Atualizar perfume | `PUT /api/Perfumes/{id}` | `docs/json/perfumes/put.json` | 204 |
| 6 | Associar nota ao perfume | `POST /api/Perfumes/{id}/notas` | `docs/json/perfumes/associar-nota.json` | 204 |
| 7 | Consultar perfume com a nota | `GET /api/Perfumes/{id}` | — | 200 |
| 8 | Ano de lançamento inválido | `POST /api/Perfumes` | `docs/json/perfumes/post-ano-invalido.json` | 400 |
| 9 | Remover perfume | `DELETE /api/Perfumes/{id}` | — | 204 |
| 10 | Remover nota | `DELETE /api/NotasOlfativas/{id}` | — | 204 |

No passo 6, troque o `notaOlfativaId` do JSON pelo ID devolvido no passo 1.

Conferência no banco (Query editor do Portal da Azure):

```sql
SELECT * FROM T_PRF_NOTAS_OLFATIVAS;
SELECT * FROM T_PRF_PERFUMES;
SELECT * FROM T_PRF_PERFUME_NOTA;

SELECT p.NOME AS PERFUME, n.NOME AS NOTA, pn.POSICAO
FROM T_PRF_PERFUME_NOTA pn
JOIN T_PRF_PERFUMES p ON p.ID = pn.PERFUME_ID
JOIN T_PRF_NOTAS_OLFATIVAS n ON n.ID = pn.NOTA_OLFATIVA_ID;
```

## Testes Unitários

São **39 testes** (xUnit, Moq e FluentAssertions):

- **Domínio:** a validação das entidades, incluindo os 5 testes da regra de ano de lançamento. Eles cobrem os dois limites (1899 e 0 recusados, o ano seguinte recusado), os valores exatos da borda (1900 e o ano atual aceitos) e a regra no `AtualizarDados`. Os testes conferem o nome do parâmetro da exceção, para garantir que quem barrou foi a regra do ano.
- **Serviço:** as regras de aplicação, com `Mock<IRepositorio>`.
- **Controller:** a tradução HTTP (200, 201, 204, 400, 404), com `Mock<IServico>`.

Eles rodam com `dotnet test` e no CI, com os resultados publicados na aba **Tests** de cada execução.

## Pré-requisitos e Como Executar Localmente

1. **Pré-requisitos:** .NET 8 SDK, Azure CLI (para os scripts de `infra/`) e um IP liberado no firewall do Azure SQL.
2. Restaurar os pacotes e a ferramenta do EF Core (a versão vem de `dotnet-tools.json`):
   ```bash
   dotnet restore
   dotnet tool restore
   ```
3. Configurar a connection string no User Secrets (ver "Configuração do Banco de Dados").
4. Executar a API:
   ```bash
   dotnet run --project cp4-perfumaria-API
   ```
5. Acessar o Swagger em `https://localhost:<porta>/swagger` e o Health Check em `/health`.
6. Executar os testes:
   ```bash
   dotnet test
   ```

## Segurança e Boas Práticas

- **Nenhuma credencial está versionada.** Localmente, a connection string fica no User Secrets. Na nuvem, fica no variable group `vg-cp6-perfumaria` como variável secreta, e aparece como `***` nos logs das pipelines.
- **A senha do administrador do banco**, usada pelos scripts, fica em `infra/.env`, coberto pelo `.gitignore`. Só o `infra/.env.example` é versionado.
- **A service connection** usa workload identity federation, sem senha guardada, e só tem permissão no Resource Group da CP6.
- **O `appsettings.json`** versionado só tem configuração não sensível (Logging, Serilog, AllowedHosts).
- **O `.gitattributes`** força o final de linha LF nos `.sh`, para os scripts rodarem igual no Git Bash e em agentes Linux.
- **As pastas `bin/` e `obj/`** e os arquivos `.env` estão no `.gitignore`.

## Considerações Finais

Além da API REST com relacionamento N:N, observabilidade e testes da CP4, a CP6 entrega o ciclo completo de DevOps:

- a infraestrutura criada por script;
- o código no Azure Repos, com a `main` protegida por revisão, tarefa obrigatória e build de validação;
- o CI em YAML, com testes publicados e conferência do DDL;
- a Release clássica, que atualiza o Azure SQL e publica a API no App Service automaticamente a cada merge;
- os segredos protegidos fora do código.
