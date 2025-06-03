# Базовый образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем .csproj (относительно корня репозитория!)
COPY ["FastBite.Presentation/FastBite.Presentation.csproj", "FastBite.Presentation/"]

# Восстанавливаем зависимости
RUN dotnet restore "FastBite.Presentation/FastBite.Presentation.csproj"

# Копируем всё остальное
COPY . .

# Переход в директорию проекта
WORKDIR "/src/FastBite.Presentation"

# Сборка проекта
RUN dotnet build "FastBite.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Публикация
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FastBite.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FastBite.Presentation.dll"]