FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
LABEL maintainer="hafslundnett@hafslundnett.no"
WORKDIR /app
COPY . .
RUN dotnet publish EventsResponseApi/EventsResponseApi.csproj -o ./out -c Release
    
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/EventsResponseApi/out .
ENTRYPOINT ["dotnet", "EventsResponseApi.dll"]
