FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /source
COPY Directory.Build.props global.json ./
COPY src/ src/
COPY README.md ./
COPY demo/Directory.Build.props demo/
COPY demo/SharpMail.Api/ demo/SharpMail.Api/
RUN dotnet publish demo/SharpMail.Api -c Debug -o /app
FROM mcr.microsoft.com/dotnet/aspnet:11.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "SharpMail.Api.dll"]
