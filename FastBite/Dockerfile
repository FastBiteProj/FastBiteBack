# ===== STAGE 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем csproj файлы и восстанавливаем зависимости
COPY ./FastBite.Core/FastBite.Core.csproj ./FastBite.Core/
COPY ./FastBite.Shared/FastBite.Shared.csproj ./FastBite.Shared/
COPY ./FastBite.Infastructure/FastBite.Infastructure.csproj ./FastBite.Infastructure/
COPY ./FastBIte.Implementation/FastBIte.Implementation.csproj ./FastBIte.Implementation/
COPY ./FastBite.Presentation/FastBite.Presentation.csproj ./FastBite.Presentation/

# Восстанавливаем зависимости
RUN dotnet restore ./FastBite.Presentation/FastBite.Presentation.csproj

# Копируем остальные файлы
COPY . .

# Публикуем приложение в Release-режиме
RUN dotnet publish ./FastBite.Presentation/FastBite.Presentation.csproj -c Release -o /app/publish

# ===== STAGE 2: Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Копируем собранные файлы из предыдущего stage
COPY --from=build /app/publish .

# Открываем порт (если Render требует конкретный — Render сам это пробросит)
EXPOSE 80

# Указываем команду запуска
ENTRYPOINT ["dotnet", "FastBite.Presentation.dll"]