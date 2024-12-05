# Usa una imagen base de .NET 8 SDK para construir la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo de proyecto y restaurar dependencias
COPY GruasUcabLoggingMS_Keycloak/GruasUcabLoggingMS_Keycloak.csproj ./GruasUcabLoggingMS_Keycloak/
COPY LogginMS.Application/LogginMS.Application.csproj ./LogginMS.Application/
COPY LogginMS.Service/LogginMS.Service.csproj ./LogginMS.Service/
RUN dotnet restore ./GruasUcabLoggingMS_Keycloak/GruasUcabLoggingMS_Keycloak.csproj

# Copia el resto de los archivos y construye la aplicación
COPY GruasUcabLoggingMS_Keycloak/. ./GruasUcabLoggingMS_Keycloak/
COPY LogginMS.Application/. ./LogginMS.Application/
COPY LogginMS.Service/. ./LogginMS.Service/
WORKDIR "/src/GruasUcabLoggingMS_Keycloak"
RUN dotnet publish -c Release -o /app/out

# Usa una imagen base de .NET 8 Runtime para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Exponer los puertos para la aplicación
EXPOSE 7133
EXPOSE 5230

# Comando para ejecutar la aplicación
ENTRYPOINT ["dotnet", "GruasUcabLoggingMS_Keycloak.dll"]

