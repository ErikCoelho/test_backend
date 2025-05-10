FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["DigiPay.Wallet.Api/DigiPay.Wallet.Api.csproj", "DigiPay.Wallet.Api/"]
RUN dotnet restore "DigiPay.Wallet.Api/DigiPay.Wallet.Api.csproj"
COPY . .
WORKDIR "/src/DigiPay.Wallet.Api"
RUN dotnet build "DigiPay.Wallet.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DigiPay.Wallet.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DigiPay.Wallet.Api.dll"] 