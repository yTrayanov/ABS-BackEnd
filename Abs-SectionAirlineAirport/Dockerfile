#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5004
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Abs-SectionAirlineAirport/Abs-SectionAirlineAirport.csproj", "Abs-SectionAirlineAirport/"]
COPY ["AirlineBookingSystem.Data/ABS.Data.csproj", "AirlineBookingSystem.Data/"]
COPY ["Common/Abs.Common.csproj", "Common/"]
RUN dotnet restore "Abs-SectionAirlineAirport/Abs-SectionAirlineAirport.csproj"
COPY . .
WORKDIR "/src/Abs-SectionAirlineAirport"
RUN dotnet build "Abs-SectionAirlineAirport.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Abs-SectionAirlineAirport.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Abs-SectionAirlineAirport.dll"]
