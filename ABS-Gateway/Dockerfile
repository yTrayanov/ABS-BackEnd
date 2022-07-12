#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ABS-Gateway/ABS-Gateway.csproj", "ABS-Gateway/"]
COPY ["AirlineBookingSystem.Data/ABS.Data.csproj", "AirlineBookingSystem.Data/"]
COPY ["Common/Abs.Common.csproj", "Common/"]
RUN dotnet restore "ABS-Gateway/ABS-Gateway.csproj"
COPY . .
WORKDIR "/src/ABS-Gateway"
RUN dotnet build "ABS-Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ABS-Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ABS-Gateway.dll"]
