#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5003
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ABS-Tickets/ABS-Tickets.csproj", "ABS-Tickets/"]
COPY ["AirlineBookingSystem.Data/ABS.Data.csproj", "AirlineBookingSystem.Data/"]
COPY ["Common/Abs.Common.csproj", "Common/"]
RUN dotnet restore "ABS-Tickets/ABS-Tickets.csproj"
COPY . .
WORKDIR "/src/ABS-Tickets"
RUN dotnet build "ABS-Tickets.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ABS-Tickets.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ABS-Tickets.dll"]