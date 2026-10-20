# Perfumaria API — Checkpoint 004

Projeto desenvolvido para a disciplina de **Tecnologia em Análise e Desenvolvimento de Sistemas**, Checkpoint 004, sob orientação do professor **Alex Sander Resende de Deus** — FIAP.

## Integrantes

- Matheus Moya de Oliveira — RM 562822
- Ana Carolina Pereira Fontes — RM 562145
- Luisa Ganasevici de Abreu — RM 563403

## Descrição do Projeto

API REST para gerenciamento de um catálogo de perfumaria, construída em **ASP.NET Core 8** com persistência em **Oracle Database**. O domínio modela um relacionamento **N:N com atributo próprio** entre `Perfume` e `NotaOlfativa`, através da entidade de junção `PerfumeNota`, que carrega a posição da nota na pirâmide olfativa (Topo, Coração ou Fundo).

Além do CRUD completo das duas entidades principais, o projeto implementa quatro pilares de observabilidade e qualidade exigidos pelo checkpoint: **Health Checks**, **Logging estruturado com Correlation ID**, **Observabilidade via OpenTelemetry** (tracing manual e métricas dimensionais) e **testes unitários automatizados** cobrindo as camadas de Domínio, Serviço e Controller.

## Tecnologias Utilizadas

| Categoria | Tecnologia |
|---|---|
| Linguagem / Framework | C# / ASP.NET Core 8.0 (Web API) |
| ORM | Entity Framework Core 8.0.11 |
| Banco de Dados | Oracle Database (`oracle.fiap.com.br`), via `Oracle.EntityFrameworkCore` 8.23.60 |
| Logging | Serilog (`Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`) |
| Observabilidade | OpenTelemetry 1.18.0 (`Extensions.Hosting`, `Instrumentation.AspNetCore`, `Instrumentation.Http`, `Exporter.Console`) |
| Testes | xUnit, Moq, FluentAssertions |
| Documentação de API | Swagger / Swashbuckle.AspNetCore |
| IDE | Visual Studio 2022 (Windows) |
| Ferramenta de teste manual | Swagger UI |

## Arquitetura do Projeto

O projeto segue uma separação em camadas inspirada em Clean Architecture, priorizando inversão de dependência entre Domínio e Infraestrutura:

```
Perfumaria.API/
├── Controllers/              → Camada de apresentação HTTP (tradução requisição/resposta)
├── Aplicacao/
│   ├── DTOs/                 → Contratos de entrada/saída da API (nunca expõe entidades direto)
│   ├── Servicos/              → Orquestração de regra de aplicação (chama Domínio + Repositório)
│   └── Middlewares/           → CorrelationIdMiddleware (rastreabilidade de requisições)
├── Dominio/
│   ├── Entidades/             → Entidades ricas com validação no construtor (Perfume, NotaOlfativa, PerfumeNota)
│   ├── Enums/                 → GeneroPerfume, PosicaoNota, FamiliaOlfativa
│   └── Interfaces/            → Contratos de repositório (IPerfumeRepositorio, INotaOlfativaRepositorio)
└── Infraestrutura/
    ├── Dados/                 → AppDbContext (mapeamento EF Core → Oracle)
    ├── Repositorios/          → Implementação concreta dos contratos de Domínio
    ├── Health/                → BancoDadosHealthCheck (IHealthCheck customizado)
    └── Observabilidade/       → AplicacaoMetricas (Meter, Counter, ActivitySource)

Perfumaria.Tests.Unit/
├── Dominio/Entidades/         → Testes de validação das entidades ricas
├── Aplicacao/Servicos/        → Testes de Serviço com Mock<IRepositorio>
└── Controllers/                → Testes de Controller com Mock<IServico>
```

**Por que essa separação existe:** a camada de Domínio define **o que** o sistema precisa (via interfaces), sem conhecer **como** isso é implementado (EF Core, Oracle). Isso permite que os testes de Serviço rodem inteiramente em memória, usando `Mock<IPerfumeRepositorio>`, sem depender de um banco de dados real — e permite trocar o provedor de banco no futuro sem tocar em regra de negócio.

## Modelagem das Entidades

### Perfume

| Campo | Tipo (C#) | Coluna (Oracle) | Observação |
|---|---|---|---|
| Id | int | `ID` (NUMBER, PK, Identity) | Chave primária auto-incremento |
| Nome | string | `NOME` (NVARCHAR2(150)) | Obrigatório |
| Marca | string | `MARCA` (NVARCHAR2(100)) | Obrigatório |
| Genero | enum `GeneroPerfume` | `GENERO` (NVARCHAR2(20)) | Persistido como texto, não int |
| VolumeMl | int | `VOLUME_ML` (NUMBER) | Deve ser > 0 |
| Preco | decimal | `PRECO` (NUMBER(10,2)) | Deve ser > 0 |
| AnoLancamento | int | `ANO_LANCAMENTO` (NUMBER) | |

### NotaOlfativa

| Campo | Tipo (C#) | Coluna (Oracle) | Observação |
|---|---|---|---|
| Id | int | `ID` (NUMBER, PK, Identity) | Chave primária auto-incremento |
| Nome | string | `NOME` (NVARCHAR2(100)) | Obrigatório |
| Familia | enum `FamiliaOlfativa` | `FAMILIA` (NVARCHAR2(20)) | Persistido como texto |

### PerfumeNota (entidade de junção — relacionamento N:N)

| Campo | Tipo (C#) | Coluna (Oracle) | Observação |
|---|---|---|---|
| PerfumeId | int | `PERFUME_ID` (NUMBER, FK) | Parte da chave composta |
| NotaOlfativaId | int | `NOTA_OLFATIVA_ID` (NUMBER, FK) | Parte da chave composta |
| Posicao | enum `PosicaoNota` | `POSICAO` (NVARCHAR2(20)) | Topo / Coração / Fundo |

**Por que uma entidade de junção explícita, em vez do N:N implícito do EF Core:** a mesma `NotaOlfativa` pode ocupar posições diferentes da pirâmide olfativa dependendo do perfume em que está inserida (ex: Bergamota é nota de topo em um perfume, mas pode ser nota de coração em outro). Como `Posicao` é um atributo da **relação em si**, e não de nenhuma das duas entidades isoladamente, o EF Core exige uma entidade explícita com chave composta — o recurso de N:N implícito só se aplica quando a tabela de junção não carrega nenhum dado próprio.

**Por que enums persistidos como texto (`HasConversion<string>()`), não como int:** legibilidade direta ao consultar o banco via SQL Developer — evita ter que decorar que `0 = Masculino`, `1 = Feminino`, etc.

## Configuração do Banco de Dados

A conexão com o Oracle é feita via **User Secrets** do .NET, nunca commitada em texto no repositório. O `appsettings.json` não contém nenhuma credencial real.

Para configurar localmente:

```bash
dotnet user-secrets set "ConnectionStrings:OracleConnection" "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=oracle.fiap.com.br)(PORT=1521))(CONNECT_DATA=(SID=orcl)))"
```

## Endpoints

### Perfumes

| Verbo | Rota | Descrição |
|---|---|---|
| GET | `/api/Perfumes` | Lista todos os perfumes com suas notas associadas |
| GET | `/api/Perfumes/{id}` | Busca um perfume específico por ID |
| POST | `/api/Perfumes` | Cria um novo perfume |
| PUT | `/api/Perfumes/{id}` | Atualiza os dados de um perfume existente |
| DELETE | `/api/Perfumes/{id}` | Remove um perfume |
| POST | `/api/Perfumes/{id}/notas` | Associa uma nota olfativa existente ao perfume, com posição na pirâmide |

### Notas Olfativas

| Verbo | Rota | Descrição |
|---|---|---|
| GET | `/api/NotasOlfativas` | Lista todas as notas olfativas |
| GET | `/api/NotasOlfativas/{id}` | Busca uma nota específica por ID |
| POST | `/api/NotasOlfativas` | Cria uma nova nota olfativa |
| PUT | `/api/NotasOlfativas/{id}` | Atualiza os dados de uma nota existente |
| DELETE | `/api/NotasOlfativas/{id}` | Remove uma nota olfativa |

### Diagnóstico

| Verbo | Rota | Descrição |
|---|---|---|
| GET | `/health` | Health Check da conexão com o banco Oracle |

## Testes Realizados (Swagger UI)

### 1. Health Check

Confirmação de que a API está saudável e a conexão com o Oracle está ativa.

![Health Check](cp4-perfumaria-API/Docs/Evidencias/01-health-check.png)

### 2. Criação de Nota Olfativa

```json
{
  "nome": "Bergamota",
  "familia": "Citrico"
}
```

![Post Nota Olfativa](cp4-perfumaria-API/Docs/Evidencias/02-post-nota-olfativa.png)

Retorno `201 Created` com o ID gerado da nota.

### 3. Criação de Perfume

```json
{
  "nome": "Sauvage",
  "marca": "Dior",
  "genero": "Masculino",
  "volumeMl": 100,
  "preco": 649.90,
  "anoLancamento": 2015
}
```

![Post Perfume](cp4-perfumaria-API/Docs/Evidencias/03-post-perfume.png)

Retorno `201 Created` com o ID gerado do perfume.

### 4. Associação de Nota ao Perfume (relacionamento N:N)

```json
{
  "notaOlfativaId": 1,
  "posicao": "Topo"
}
```

![Post Associar Nota](cp4-perfumaria-API/Docs/Evidencias/04-post-associar-nota.png)

Retorno `204 No Content`, confirmando a criação do vínculo `PerfumeNota`.

### 5. Consulta do Perfume com Nota Populada

![Get Perfume com Notas](cp4-perfumaria-API/Docs/Evidencias/05-get-perfume-com-notas.png)

O array `notas` no corpo da resposta confirma que o relacionamento N:N foi persistido e é recuperado corretamente via `Include`/`ThenInclude`.

### 6. Atualização de Perfume

![Put Perfume](cp4-perfumaria-API/Docs/Evidencias/06-put-perfume.png)

Retorno `204 No Content`, confirmando a atualização.

### 7. Validação de Dados Inválidos

```json
{
  "nome": "",
  "marca": "Dior",
  "genero": "Masculino",
  "volumeMl": 100,
  "preco": 599.90,
  "anoLancamento": 2015
}
```

![Post Perfume Validacao 400](cp4-perfumaria-API/Docs/Evidencias/07-post-perfume-validacao-400.png)

Retorno `400 Bad Request`, evidenciando a validação de domínio (guard clauses no construtor da entidade `Perfume`).

### 8. Remoção de Perfume

![Delete Perfume](cp4-perfumaria-API/Docs/Evidencias/08-delete-perfume.png)

Retorno `204 No Content`.

### 9. Confirmação de Remoção

![Get Perfume 404](cp4-perfumaria-API/Docs/Evidencias/09-get-perfume-404.png)

Nova consulta ao mesmo ID retorna `404 Not Found`, confirmando a exclusão efetiva no banco.

### 10. Testes Unitários Automatizados

![Testes Unitarios](cp4-perfumaria-API/Docs/Evidencias/10-testes-unitarios.png)

34 testes executados, cobrindo Domínio (validação de entidades), Serviço (regras de aplicação com Mock de Repositório) e Controller (tradução HTTP com Mock de Serviço) — 100% de aprovação.

## Pré-requisitos e Como Executar Localmente

1. **Pré-requisitos**: .NET 8 SDK, acesso ao Oracle da FIAP (`oracle.fiap.com.br:1521`, SID `orcl`), Visual Studio 2022 (ou `dotnet` CLI).

2. Clonar o repositório:
   ```bash
   git clone <url-do-repositorio>
   cd cp4-perfumaria-API
   ```

3. Configurar a connection string via User Secrets (ver seção "Configuração do Banco de Dados" acima).

4. Restaurar pacotes:
   ```bash
   dotnet restore
   ```

5. Instalar a ferramenta CLI do EF Core, na mesma versão do projeto:
   ```bash
   dotnet tool install --global dotnet-ef --version 8.0.11
   ```

6. Aplicar as migrations (cria as tabelas no Oracle, caso ainda não existam):
   ```bash
   $env:ASPNETCORE_ENVIRONMENT="Development"   # PowerShell
   dotnet ef database update
   ```

7. Executar a API:
   ```bash
   dotnet run
   ```

8. Acessar o Swagger em `https://localhost:<porta>/swagger` e o Health Check em `https://localhost:<porta>/health`.

9. Executar os testes unitários:
   ```bash
   cd ../Perfumaria.Tests.Unit
   dotnet test
   ```

## Segurança e Boas Práticas

- Nenhuma credencial de banco de dados está versionada no repositório. A connection string real é armazenada exclusivamente via **User Secrets** (`dotnet user-secrets`), que mantém o arquivo `secrets.json` fora da árvore do projeto, estruturalmente impossível de ser commitado.
- O `appsettings.json` versionado contém apenas configuração não sensível (`Logging`, `Serilog`, `AllowedHosts`).
- As pastas `bin/` e `obj/` estão listadas no `.gitignore`, evitando versionar artefatos de build.

## Considerações Finais

O projeto entrega uma API REST completa sobre um domínio de perfumaria com relacionamento N:N modelado corretamente (entidade de junção com atributo próprio), além dos quatro pilares de observabilidade exigidos pelo checkpoint: Health Checks funcionais, logging estruturado com rastreabilidade via Correlation ID, instrumentação manual de tracing e métricas dimensionais via OpenTelemetry, e suíte de testes unitários cobrindo Domínio, Serviço e Controller com 100% de aprovação.
