#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
ENV ASPNETCORE_URLS=http://+:5001
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ABS-Auth/ABS-Auth.csproj", "ABS-Auth/"]
COPY ["AirlineBookingSystem.Data/ABS.Data.csproj", "AirlineBookingSystem.Data/"]
COPY ["Common/Abs.Common.csproj", "Common/"]
RUN dotnet restore "ABS-Auth/ABS-Auth.csproj"
COPY . .
WORKDIR "/src/ABS-Auth"
RUN dotnet build "ABS-Auth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ABS-Auth.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ABS-Auth.dll"]
