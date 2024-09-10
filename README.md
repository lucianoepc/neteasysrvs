################################################################################
#CONFIGURACIÓN: Creacion del estructura de carpetas
# > El proyecto de generación de una imagen es una solución, que está en git
# >	El proyecto principal de la solucion es lo que programa entry-point de la imagen.
# > Los proyectos adicionales de la solucion son: acceso a datos, lógica de negocio, las definiciones comunes, etc.
################################################################################

cd ~/code/own1/poc/net/webapi/easysrvs

01> Crear una un archivo solución 'EasySrvs.sln' en la ruta actual
dotnet new sln --name EasySrvs

02> Crea el proyecto de tipo webapi ubicada en folder './WebServices' que tambien contiene el archivo  'WebServices.csproj' 
dotnet new webapi -o ./WebServices

03> Adiciona a la solucion actual, el proyecto ubicado en el folder especificado
dotnet sln add ./WebServices


04> Listar la proyectos de la solucion
dotnet sln list

05> Crear el .gitignore
dotnet new gitignore

06> Adicionar los archivos que debe ignorar por usar VIM
vim .gitignore

#---------------------------------------------------------------------------
# Ignore for VIM
#---------------------------------------------------------------------------

# VIM - Archivos swap o temporales (formato '.filename.swp')
**/*.swp
**/*.swo

# VIM - Archivos de 'persistence undo' (formato '.filename.un~')
**/*.un~

# Log generados por: TMUX, ..
*.log

# Archivos internos de GIT
.git/*

# Archivos de tag generado por Universal CTags
tags

# Paquetes locales de NodeJS (binarios de NodeJs)
node_modules/*

# Mi carpeta de salida de binarios para pruebas internas
[Oo]ut/

#---------------------------------------------------------------------------
# Ignore for Visual Studio
#---------------------------------------------------------------------------

07> Crear .dockerignore
vim .dockerignore


**/.classpath
**/.dockerignore
**/.env
**/.git
**/.gitignore
**/.project
**/.settings
**/.toolstarget
**/.vs
**/.vscode
**/*.*proj.user
**/*.dbmdl
**/*.jfm
**/bin
**/charts
**/docker-compose*
**/compose*
**/Dockerfile*
**/node_modules
**/npm-debug.log
**/obj
**/secrets.dev.yaml
**/values.dev.yaml
README.md


09> En el proyecto principal, crear el archivo de configuracion para NLog

mkdir WebServices/log
vim WebServices/nlog.config

<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xsi:schemaLocation="NLog NLog.xsd"
    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
    autoReload="true"
    throwExceptions="false"
    internalLogLevel="Info"
    internalLogFile="${basedir}/log/internal.log" >

    <variable name="defaultLayout2" value="${longdate} ${level} [${threadname:whenEmpty=${threadid}}] ${logger} - ${message} ${exception:format=tostring}"/>
    <variable name="defaultLayout" value="${longdate} ${level} [${threadid}] ${logger} - ${message} ${exception:format=tostring}"/>

    <!-- the targets to write to -->
    <targets>
        <!-- write logs to file -->
        <target xsi:type="File" name="logfile" fileName="${basedir}/log/myapp.log"
            layout="${var:defaultLayout}" />
        <!-- write logs to console -->
        <target xsi:type="Console" name="logconsole"
            layout="${var:defaultLayout}" />
    </targets>

    <!-- rules to map from logger name to target -->
    <rules>
        <logger name="UC.Core.Job.*" minlevel="Trace" writeTo="logconsole" />
        <logger name="*" minlevel="Debug" writeTo="logfile,logconsole" />
    </rules>
</nlog>


10> En el proyecto principal, crear el archivo 'Dockerfile'

vim WebServices/Dockerfile

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
COPY ["WebServices/WebServices.csproj", "WebServices/"]
RUN dotnet restore "WebServices/WebServices.csproj"
COPY . .
WORKDIR "/src/WebServices"
RUN dotnet build "WebServices.csproj" -c Release -o /app/build

#--------------------------------------------------------------------------------------
FROM lbuild AS lpublish

RUN dotnet publish "WebServices.csproj" -c Release -o /app/publish /p:UseAppHost=false

#--------------------------------------------------------------------------------------
FROM lbase AS lfinal
WORKDIR /app
COPY --from=lpublish /app/publish .

ENTRYPOINT ["dotnet", "PoC.EasySrvs.WebServices.dll"]


08> Crear un repositorio git
git init

################################################################################
#CONFIGURACIÓN: Configurar basicas del proyectos
################################################################################

1> En cada proyecto, cambiar el espacio de nombres por defecto y el nombre de salida del binario 
   Se esta indicando que el nombre del binario coincida con el nampespace por defecto

    <PropertyGroup>
        <TargetFramework>net7.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>        
        <RootNamespace>PoC.EasySrvs.WebServices</RootNamespace>
	    <AssemblyName>PoC.EasySrvs.WebServices</AssemblyName>
    </PropertyGroup>



2> En cada proyecto '.csproj' adicionar sus proyectos dependientes (NO APLICA)

    <ItemGroup>
        <ProjectReference Include="../UC.Core.Common/UC.Core.Common.csproj" />
        <ProjectReference Include="../Common/Common.csproj" />
        <ProjectReference Include="../Data/Data.csproj" />
        <ProjectReference Include="../BusinnessLogic/BusinessLogic.csproj" />
    </ItemGroup>



################################################################################
#CONFIGURACIÓN: Habilitar NLog para la solución
# > La mayoria de los proyectos solo requieren acceder a MEL (capa de abstracion de Microsoft)
# > El proyecto principal (el webservice) requier configurar NLog, por ello este siempre requiere adiciona la libreria de NLog.
# > El archivo de configuracion de NLog se realiza solo en el proyecto principal
################################################################################

1> Adicionar el paquete en el de proyecto '.csproj' que lo requieren (generalmente el proyecto principal)

	dotnet add ./WebServices package NLog.Extensions.Logging

Tambien puede adicionar (colocar el principal y este resolver y descargara todos las referencias que requiere):

	<ItemGroup>
		<PackageReference Include="NLog.Extensions.Logging" Version="5.3.5" />
	</ItemGroup>
	
	
	dotnet restore


2> En el proyecto principal, indicar que el archivo de configuracion siempre se copia a la salida de los binarios compilados.


	<ItemGroup>
		<None Update="nlog.config" CopyToOutputDirectory="Always" />
	</ItemGroup>
	

3> En el proyecto principal, en el archivo 'Program.cs' indicar el log por defecto que debe usar MEL


Si usas WebHost:

		using NLog.Extensions.Logging;
		
		. . . . . . . . . . . . . . . . . . . 
		. . . . . . . . . . . . . . . . . . . 
		

		builder.Service.AddLogging(loggingBuilder =>
			{
                //Eliminar los proveedores de logging configurados por defecto por el default builder
                loggingBuilder.ClearProviders();
        
                loggingBuilder.AddNLog();

            });

Si usas GenericHost:

		using NLog.Extensions.Logging;
		
		. . . . . . . . . . . . . . . . . . . 
		. . . . . . . . . . . . . . . . . . . 
		

		builder.ConfigureLogging(loggingBuilder =>
            {
                //Eliminar los proveedores de logging configurados por defecto por el default builder
                loggingBuilder.ClearProviders();
        
                loggingBuilder.AddNLog();

            });


Opcionalmente, Si desea tener control de log antes y despues que el servio Host de .Net este funcionando, cree un logger estatico solo para este escenarios

using NLog;
using NLog.Extensions.Logging;
using UC.Core.Job;

public class Program
{
    private static NLog.Logger sLogger= NLog.LogManager.CreateNullLogger();

    public static async Task Main(string[] pArgs)
    {
        //1. Crear logger para la clase principal 'Program'
        sLogger = LogManager.GetCurrentClassLogger();


        try
        {
            sLogger.Log(NLog.LogLevel.Debug, "Iniciando el programa...");

            IHost host= Program.sCreateHost(pArgs);

            await Program.sRunHost(host);

            sLogger.Log(NLog.LogLevel.Debug, "El programa Finalizó...");
        }
        catch (Exception ex)
        {
            sLogger.Error(ex, "Ocurrio un error al iniciar el programa");
            //throw ex;
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            LogManager.Shutdown();
        }

    }

    private static IHost sCreateHost(string[] pArgs)
    {
        //1. Creando el HostBuilder
        var builder= Host.CreateDefaultBuilder(pArgs);
        
        //2. Configuraciones del Host
        //builder.ConfigureHostConfiguration(hostConfig =>
        //    {
        //        hostConfig.SetBasePath(Directory.GetCurrentDirectory());
        //        hostConfig.AddJsonFile("hostsettings.json", optional: true);
        //        hostConfig.AddEnvironmentVariables(prefix: "PREFIX_");
        //        hostConfig.AddCommandLine(args);
        //    });
        
        //builder.UseContentRoot("/path/to/content/root");
        //builder.UseEnvironment("Development");

        //2. Configurando el Logging
        builder.ConfigureLogging(loggingBuilder =>
            {
                //Eliminar los proveedores de logging configurados por defecto por el default builder
                loggingBuilder.ClearProviders();

                //Adicionar lo proveedor de log deseados
                //loggingBuilder.AddSimpleConsole(options =>
                //    {
                //        options.SingleLine= true;
                //        options.TimestampFormat= "yyyy/MM/dd HH:mm:ss.ffffff ";
                //    });

                //loggingBuilder.AddSystemdConsole(options =>
                //    {
                //        options.TimestampFormat= "yyyy/MM/dd HH:mm:ss.ffffff ";
                //    });

                loggingBuilder.AddNLog();

            });
        
        
        //3. Configuracion de los servicios adicionales del HostBuilder
        builder.ConfigureServices((context, services) =>
            {
                //Configuración de opciones del host para el servicio
                services.Configure<HostOptions>(options =>
                    {
                        //Periodo de gracia para terminar todos los servicios hospedados (por defecto es 5s) para un cierro controlado del host.
                        options.ShutdownTimeout= TimeSpan.FromSeconds(15);
        
                        //Comportamiento del host en caso que ocurra una excepción en el servicio hospedados (por defecto: StopHost)
                        //options.BackgroundServiceExceptionBehavior= BackgroundServiceExceptionBehavior.Ignore
                    });

                //Adicionando servicios: Los hosted services
                services.AddSingleton<JobGroupBuilder>(pServiceProvider => {
                        //Creando el objeto
                        return new MssqlJobBuilder("Group01",
                            pServiceProvider.GetRequiredService<ILogger<MssqlJobBuilder>>(),
                            pServiceProvider.GetRequiredService<ILoggerFactory>(),
                            pServiceProvider.GetRequiredService<IConfiguration>()
                            );
                    });

                //Adicionando servicios: Los que se inyectaran (como parametros del contructor) del host services
                services.AddHostedService<HostedJobGroup>();

                //services.AddScoped();
            });
        
        //3. Instanciando el host y ejecutando los servicios hospedados
        sLogger.Log(NLog.LogLevel.Debug, "Construyendo Host con el builder...");
        IHost host= builder.Build();
        return host;
    }

    private static async Task sRunHost(IHost pHost)
    {
        sLogger.Log(NLog.LogLevel.Debug, "Ejecutando asincronamente el Host y esperando el resultado ...");
        await pHost.RunAsync();
    }
    
}


################################################################################
#LOCALMENTE: Compilar y Ejecutar
################################################################################

cd ~/code/own1/poc/net/webapi/easysrvs

1> Sin usar contenedores

//Limpiar y reconstuir toda la solución
dotnet clean
dotnet restore

//Compilar la solucion
dotnet build

//Ejecutar en debug el proyecto principal y sus dependencias
dotnet run --project webservices

//Publicar en realease y ejecutar
rm out/*
mkdir -p out/log
dotnet publish webservices -c Release -o out
dotnet ./out/PoC.EasySrvs.WebServices.dll


2> Usando Podman
podman build -f webservices/Dockerfile -t lucianoepc/easysrvs:1.0.0 .
podman image ls

podman run --rm -p 8080:8080 -t lucianoepc/easysrvs:1.0.0 

//Luego de las pruebas, esto detendra el contenedor (una vez detenido, se elimina automaticamente)
podman container ls
podman container stop XXXXX

//Publicar en DockerHub (https://hub.docker.com/)
#podman login -u lucianoepc
#podman tag quyllur/easysrvs:1.0.0 lucianoepc/easysrvs:1.0.0
podman push lucianoepc/easysrvs:1.0.0

3> Usando NerdCtl
systemctl --user start containerd
systemctl --user status containerd.service

nerdctl build -f WebServices/Dockerfile -t quyllur/easysrvs:1.0.0 .
nerdctl image ls

nerdctl run --rm -p 8080:8080 -t quyllur/easysrvs:1.0.0 

//Luego de las pruebas, esto detendra el contenedor (una vez detenido, se elimina automaticamente)
nerdctl container ls
nerdctl container stop XXXXX

//Publicar en DockerHub (https://hub.docker.com/)
nerdctl login -u lucianoepc
nerdctl tag quyllur/easysrvs:1.0.0 lucianoepc/easysrvs:1.0.0
nerdctl push quyllur/easysrvs:1.0.0


################################################################################
#LOCALMENTE: Validaciones
################################################################################

1> Sin usar contenedores

curl http://localhost:5000/api/weather

curl http://localhost:5000/swagger/v1/swagger.json
http://localhost:5000/swagger/index.html


2> Usando contenedores: Podman
curl http://localhost:8080/api/weather

curl http://localhost:8080/swagger/v1/swagger.json
http://localhost:8080/swagger/index.html

3> Usando contenedores: ContainerD/NerdCtl
curl http://localhost:8080/api/weather

curl http://localhost:8080/swagger/v1/swagger.json
http://localhost:8080/swagger/index.html

4> Usando kubernates - ingress

curl http://test1a.app.k8s1.quyllur.home/api/weather

################################################################################
#REMOTAMENTE: Subir a un repositorio GitLab
#	SSH   : git@gitlab.com:quyllur/support/test-01.git
#	HTTPS : https://gitlab.com/quyllur/support/test-01.git
#Configuración del cliente SSH ("~/.ssh/config") para conectarse por git en "gitlab.com":
#	Host Alias: "mygitlab" usa C:\Users\LucianoWin\.ssh\id_rsa
#	Host Alias: "mygitlab-builder" usa E:\Development\UserKeys\SSH\id_rsa_rhocp_builder_nocrip
################################################################################

git status
git add
git commit

#Validar el usuario y correo del git local
#git config --list

#Listar los repositorios remotos
#git remote -v

#Adiciona/corregir un repositorio remoto
#git remote add gitlab-bld mygitlab-builder:quyllur/support/test-01.git
#git remote add gitlab-adm mygitlab:quyllur/support/test-01.git
#git remote set-url gitlab-bld mygitlab-builder:quyllur/support/test-01.git
#git remote set-url gitlab-adm mygitlab:quyllur/support/test-01.git
#git remote remove gitlab-bld
#git remote remove gitlab-adm

#Actualizar una rama del repositorio remoto
#Solicitar el merge
#Onwer valida la solicitud y realizar el merge en la rama "main"
git push gitlab-bld main


################################################################################
#REMOTAMENTE: Pruebas usando RH OCP
################################################################################

oc get bc -n ppoc-test-infra
oc start-build bulc-test-01 --follow -n ppoc-test-infra
oc delete pod/XXXX


oc port-forward services/srvc-test-01 8080:8080 -n ppoc-test-infra

curl -H 'ApiKey: 6bd4da79-7408-44e5-93fd-5da461a97873' -H 'Content-Type: application/json' -H 'my-header: my-value' \
http://localhost:8080/api/weather
http://localhost:8080/api/weather/id=1
http://intapi09.apps.openshiftucts.continental.edu.pe:8080/api/weather

