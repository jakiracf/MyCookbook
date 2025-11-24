FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./ ./

RUN dotnet restore ./src/Api/MyCookbook.Api.csproj
RUN dotnet publish ./src/Api/MyCookbook.Api.csproj -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out ./

#add localhost later
ENV ASPNETCORE_URLS=http://+:5132
EXPOSE 5132

ENTRYPOINT ["dotnet", "MyCookbook.Api.dll"]
