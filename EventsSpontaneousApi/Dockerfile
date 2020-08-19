FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
LABEL maintainer="hafslundnett@hafslundnett.no"
WORKDIR /app
COPY . .
RUN dotnet publish EventsSpontaneousApi/EventsSpontaneousApi.csproj -o ./out -c Release
    
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 as runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "EventsSpontaneousApi.dll"]
