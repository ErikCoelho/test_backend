# DigiPay.Auth

API de autenticação para o projeto DigiPay, implementando autenticação JWT e utilizando PostgreSQL para armazenamento.

## Tecnologias Utilizadas

- .NET 9.0
- Entity Framework Core
- PostgreSQL
- JWT para autenticação
- Docker (opcional, para PostgreSQL e/ou API)

## Requisitos

1. [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Para PostgreSQL, você pode escolher:
   - [PostgreSQL](https://www.postgresql.org/download/) (instalação local, versão 12 ou superior)
   - [Docker Desktop](https://www.docker.com/products/docker-desktop/) (para executar PostgreSQL em container)

## Opções de Execução

Há várias maneiras de executar este projeto, dependendo de suas preferências:

### Opção 1: Tudo em Docker (Recomendado)

Execute todo o ambiente (PostgreSQL + API) em containers Docker:

```powershell
cd DigiPay.Auth
.\Scripts\start-docker-all.ps1
```

Para parar:

```powershell
cd DigiPay.Auth
.\Scripts\stop-docker-all.ps1
```

### Opção 2: Somente PostgreSQL em Docker, API Local

1. Inicie o PostgreSQL no Docker:

```powershell
cd DigiPay.Auth
.\Scripts\start-postgres.ps1
```

2. Execute a API localmente:

```powershell
cd DigiPay.Auth
.\Scripts\start-all.ps1
```

Para parar o PostgreSQL:

```powershell
cd DigiPay.Auth
.\Scripts\stop-postgres.ps1
```

### Opção 3: Instalação Local (Sem Docker)

1. Instale o PostgreSQL localmente e crie um banco de dados chamado `digipay_auth`
2. Atualize a string de conexão no arquivo `appsettings.json` se necessário
3. Execute a API localmente:

```powershell
cd DigiPay.Auth/DigiPay.Auth.Api
.\Scripts\run-api.ps1
```

## Configuração do Banco de Dados

Se você estiver usando o PostgreSQL localmente (não no Docker), configure o seguinte:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=digipay_auth;Username=postgres;Password=postgres"
}
```

## Aplicar Migrações ao Banco de Dados Manualmente

Para criar o banco de dados e aplicar as migrações:

```bash
cd DigiPay.Auth/DigiPay.Auth.Api
dotnet ef database update
```

Alternativamente, você pode usar o script PowerShell incluído:

```powershell
cd DigiPay.Auth/DigiPay.Auth.Api
.\Scripts\update-database.ps1
```

## API Endpoints

### Autenticação

- **Registrar Usuário**: POST `/api/auth/register`
- **Login**: POST `/api/auth/login`

### Endpoints Protegidos (Requerem Autenticação)

- **Informações do Usuário**: GET `/api/secured/userinfo`
- **Teste de Endpoint Seguro**: GET `/api/secured/test`

## Usuários de Teste

A aplicação é inicializada com os seguintes usuários para teste:

1. Administrador
   - Username: `admin`
   - Password: `admin123`

2. Usuário Regular 1
   - Username: `user1`
   - Password: `user123`

3. Usuário Regular 2
   - Username: `user2`
   - Password: `user123`

## Testando a API com Swagger

1. Execute a aplicação
2. Acesse a interface do Swagger em: https://localhost:5001/swagger (ou http://localhost:5000/swagger)
3. Use os endpoints de autenticação para obter um token JWT
4. Clique no botão "Authorize" no topo da página e adicione o token no formato: `Bearer {seu_token}`
5. Agora você pode acessar os endpoints protegidos

## Parando os Serviços

Para parar o PostgreSQL no Docker:

```powershell
cd DigiPay.Auth
.\Scripts\stop-postgres.ps1 