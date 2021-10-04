USE AirlineBookingSystem;
GO
ALTER FUNCTION dbo.fn_GetAirportId 
		(@airportName NVARCHAR(3))
RETURNS INTEGER
AS
BEGIN

DECLARE @airportId INTEGER;

	SELECT @airportId = Id 
	FROM Airports 
	WHERE [Name]= @airportName

	RETURN @airportId;

END
GO

ALTER PROCEDURE usp_Flights_Insert
	@originAirportName NVARCHAR(3),
	@destinationAirportName NVARCHAR(3),
	@airlineName NVARCHAR(50),
	@flightNumber NVARCHAR(50),
	@departureDate DATETIME,
	@landingDate DateTime
AS
BEGIN
	DECLARE @originAirportId INTEGER = dbo.fn_GetAirportId(@originAirportName);
	DECLARE @destinationAirportId INTEGER = dbo.fn_GetAirportId(@destinationAirportName);
	DECLARE @errorNumber INTEGER;
	DECLARE @errorName NVARCHAR(100);

	IF(@originAirportId IS NULL OR @destinationAirportId IS NULL)
		BEGIN
			SELECT @errorNumber=Number 
			FROM ErrorCodes 
			WHERE [Name] = 'Airport not found';

			THROW @errorNumber,'Airport not found',1;
		END
	
	DECLARE @airlineId INTEGER = (SELECT Id FROM Airlines WHERE [Name] = @airlineName);

	IF(@airlineId IS NULL)
		BEGIN
			SELECT @errorNumber=Number , @errorName=[Name]
			FROM ErrorCodes 
			WHERE [Name] = 'Airline not found';

			THROW @errorNumber,@errorName,1;
		END

	BEGIN TRY
		INSERT INTO Flights (OriginAirportId , DestinationAirportId , AirlineId , FlightNumber , DepartureDate , LandingDate)
		VALUES (@originAirportId , @destinationAirportId , @airlineId , @flightNumber , @departureDate , @landingDate);
	END TRY
	BEGIN CATCH
		IF((SELECT COUNT(Id) FROM Flights WHERE FlightNumber = @flightNumber) <> 0)
			BEGIN
				SELECT @errorNumber=Number , @errorName=[Name]
				FROM ErrorCodes 
				WHERE [Name] = 'FlightNumber already exists';

				THROW @errorNumber,@errorName,1;
			END;
		THROW;
	END CATCH
END
GO

ALTER PROCEDURE dbo.usp_FilterFlights_Select
	@originAirport NVARCHAR(3),
	@destinationAirport NVARCHAR(3),
	@departureDate DATETIME,
	@membersCount INTEGER = 1
AS
BEGIN

	DECLARE @originAirportId INTEGER = dbo.fn_GetAirportId(@originAirport);
	DECLARE @destinationAirportId INTEGER = dbo.fn_GetAirportId(@destinationAirport);

	IF(@originAirportId IS NULL OR @destinationAirport IS NULL)
		BEGIN
			DECLARE @errorNumber INTEGER = (SELECT Number FROM ErrorCodes WHERE [Name] = 'Airport not found');
			THROW @errorNumber,'Airport not found',1;
		END

	SELECT 
		f.Id,
		f.FlightNumber,
		f.DepartureDate,
		f.LandingDate,
		oa.[Name] AS OriginAirport,
		da.[Name] AS DestinationAirport,
		a.[Name] AS Airline
	FROM Flights f
	INNER JOIN Airports oa ON oa.Id = f.OriginAirportId
	INNER JOIN Airports da ON da.Id = f.DestinationAirportId
	INNER JOIN Airlines a ON a.Id = f.AirlineId
	INNER JOIN Sections s ON s.FlightId = f.Id
	WHERE f.OriginAirportId = @originAirportId
		AND f.DestinationAirportId = @destinationAirportId
		AND FORMAT(f.DepartureDate, 'yyyy-MM-dd') = @departureDate
		AND s.AvailableSeatsCount >= @membersCount
END
GO

--CREATE TYPE FlightIdList AS TABLE (FlightId INTEGER); 
GO
ALTER PROCEDURE dbo.usp_FlightsByMultipleIds_Select
	@flightIds FlightIdList READONLY
AS
BEGIN
	--FLIGHTS
	SELECT 
		f.Id,
		oa.[Name] AS OriginAirport,
		da.[Name] AS DestinationAirport
	FROM Flights f
	INNER JOIN Airports oa  ON f.OriginAirportId = oa.Id
	INNER JOIN Airports da ON f.DestinationAirportId = da.Id
	WHERE f.Id IN (SELECT FlightId FROM @flightIds);

	--SECTIONS
	SELECT
		section.Id,
		section.SeatClass,
		section.FlightId
	FROM Sections section
	INNER JOIN Flights f ON section.FlightId = f.Id

	--SEATS
	SELECT
		section.SeatClass,
		seat.Id,
		seat.[Row],
		seat.[Column],
		seat.IsBooked,
		seat.SectionId
	FROM Sections section
	INNER JOIN Seats seat ON seat.SectionId = section.Id
	WHERE section.FlightId IN (SELECT FlightId FROM @flightIds);
	
END
GO

ALTER PROCEDURE dbo.usp_AllFlights_Select
AS
BEGIN
	SELECT 
		f.Id,
		f.FlightNumber,
		f.DepartureDate,
		f.LandingDate,
		originAirport.[Name] AS OriginAirport,
		destinationAirport.[Name] AS DestinationAirport,
		airline.[Name] AS Airline
	FROM Flights f
	INNER JOIN Airports originAirport ON originAirport.Id = f.OriginAirportId
	INNER JOIN Airports destinationAirport ON destinationAirport.Id = f.DestinationAirportId
	INNER JOIN Airlines airline ON airline.Id = f.AirlineId
END
GO

ALTER PROCEDURE dbo.usp_FlightById_Select
	@flightId INTEGER
AS
BEGIN
	--flight select
	SELECT 
		f.FlightNumber,
		originAirport.[Name] AS OriginAirport,
		destinationAirprot.[Name] AS DestinationAirport,
		airline.[Name] AS Airline
	FROM Flights f
	INNER JOIN Airports originAirport ON originAirport.Id = f.OriginAirportId
	INNER JOIN Airports destinationAirprot ON destinationAirprot.Id = f.DestinationAirportId
	INNER JOIN Airlines airline ON airline.Id = f.AirlineId
	WHERE f.Id = @flightId

	--select sections
	SELECT
		Id,
		SeatClass
	FROM Sections section
	WHERE section.FlightId = @flightId 
		AND section.AvailableSeatsCount <> section.[Rows]* section.[Columns]

	--select seats
	SELECT
		t.PassengerName,
		u.Username,
		seat.Id,
		seat.[Row],
		seat.[Column],
		SectionId
	FROM Seats seat
	INNER JOIN Sections section ON section.Id = seat.SectionId
	INNER JOIN Tickets t ON t.SeatId = seat.Id
	INNER JOIN Users u ON u.Id = t.UserId
	WHERE section.FlightId = @flightId AND seat.IsBooked=1
END
GO