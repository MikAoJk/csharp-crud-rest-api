FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENV TZ="Europe/Oslo"
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "CSharpCrudRestApi.dll"]
