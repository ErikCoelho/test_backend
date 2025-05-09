FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["DigiPay.Auth.Api/DigiPay.Auth.Api.csproj", "DigiPay.Auth.Api/"]
RUN dotnet restore "DigiPay.Auth.Api/DigiPay.Auth.Api.csproj"
COPY . .
WORKDIR "/src/DigiPay.Auth.Api"
RUN dotnet build "DigiPay.Auth.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DigiPay.Auth.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DigiPay.Auth.Api.dll"] 