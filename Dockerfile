FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /build

COPY "." "."

RUN dotnet publish

# ---

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS app

WORKDIR /app

COPY --from=build "/build/bin/Release/net10.0" "."

CMD ["./inwt-api-demo"]
