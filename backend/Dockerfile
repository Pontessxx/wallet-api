FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG VERSION=1.0.0

WORKDIR /src

COPY AuthApi/AuthApi.csproj               AuthApi/
COPY Domain/Domain.csproj                 Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Application/Application.csproj       Application/
COPY AuthApi.slnx .

RUN dotnet restore AuthApi.slnx

COPY . .

RUN dotnet publish AuthApi/AuthApi.csproj \
    -c Release \
    -o /app/publish \
    -p:Version=${VERSION}

# =========================
# Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

ARG VERSION=1.0.0

WORKDIR /app

LABEL app="auth-api"
LABEL version="${VERSION}"

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AuthApi.dll"]