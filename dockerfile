FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
COPY . .
RUN dotnet restore
RUN dotnet publish Score.sln -c release -o /app --no-self-contained --no-restore

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS score-migration
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["./Migration"]

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine as score-server
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["./Server"]