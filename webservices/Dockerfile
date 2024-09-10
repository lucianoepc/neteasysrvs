#--------------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS lbase

WORKDIR /app
RUN mkdir -p /app/log
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# https://aka.ms/vscode-docker-dotnet-configure-containers
#RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
#USER appuser

#--------------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS lbuild

WORKDIR /src
COPY ["webservices/webservices.csproj", "webservices/"]
RUN dotnet restore "webservices/webservices.csproj"
COPY . .
WORKDIR "/src/webservices"
RUN dotnet build "webservices.csproj" -c Release -o /app/build

#--------------------------------------------------------------------------------------
FROM lbuild AS lpublish

RUN dotnet publish "webservices.csproj" -c Release -o /app/publish /p:UseAppHost=false

#--------------------------------------------------------------------------------------
FROM lbase AS lfinal
WORKDIR /app
COPY --from=lpublish /app/publish .

ENTRYPOINT ["dotnet", "PoC.EasySrvs.WebServices.dll"]
