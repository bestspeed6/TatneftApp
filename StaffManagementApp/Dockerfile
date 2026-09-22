# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["StaffManagementApp.csproj", "./"]
RUN dotnet restore "./StaffManagementApp.csproj"

COPY . .
RUN dotnet publish "StaffManagementApp.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create uploads directory
RUN mkdir -p wwwroot/uploads/avatars

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "StaffManagementApp.dll"]
