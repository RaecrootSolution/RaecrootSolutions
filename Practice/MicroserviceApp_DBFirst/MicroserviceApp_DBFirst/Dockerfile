FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["MicroserviceApp_DBFirst/MicroserviceApp_DBFirst.csproj", "MicroserviceApp_DBFirst/"]
RUN dotnet restore "MicroserviceApp_DBFirst/MicroserviceApp_DBFirst.csproj"
COPY . .
WORKDIR "/src/MicroserviceApp_DBFirst"
RUN dotnet build "MicroserviceApp_DBFirst.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "MicroserviceApp_DBFirst.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MicroserviceApp_DBFirst.dll"]