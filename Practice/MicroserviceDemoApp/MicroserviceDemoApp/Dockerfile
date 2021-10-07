FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["MicroserviceDemoApp/MicroserviceDemoApp.csproj", "MicroserviceDemoApp/"]
RUN dotnet restore "MicroserviceDemoApp/MicroserviceDemoApp.csproj"
COPY . .
WORKDIR "/src/MicroserviceDemoApp"
RUN dotnet build "MicroserviceDemoApp.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "MicroserviceDemoApp.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MicroserviceDemoApp.dll"]