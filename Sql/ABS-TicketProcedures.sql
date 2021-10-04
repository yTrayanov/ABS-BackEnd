USE AirlineBookingSystem;
GO

ALTER PROCEDURE dbo.usp_Tickets_Insert
	@username UsernameType,
	@seatId INTEGER,
	@flightId INTEGER,
	@passengerName NVARCHAR(50)
AS
BEGIN
	DECLARE @userId UserIdType = (SELECT Id FROM Users WHERE Username = @username)
	DECLARE @errorNumber INTEGER;
	DECLARE @errorName NVARCHAR(100);

	BEGIN TRY
		INSERT INTO Tickets (UserId ,FlightId , SeatId , PassengerName)
					VALUES (@userId , @flightId , @seatId , @passengerName);
		UPDATE Seats
		SET IsBooked=1
		WHERE Id=@seatId;

		UPDATE Sections
		SET AvailableSeatsCount = AvailableSeatsCount - 1
		WHERE Id IN (Select SectionId FROM Seats seat WHERE seat.id=@seatId)
	END TRY
	BEGIN CATCH
		IF((SELECT Id FROM Users WHERE Id=@userId ) IS NULL)
		BEGIN
			SELECT @errorNumber=Number , @errorName=[Name]
			FROM ErrorCodes 
			WHERE [Name] = 'User not Found';

			THROW @errorNumber,@errorName,1;
		END;

		ELSE IF((SELECT Id FROM Flights WHERE Id=@flightId) IS NULL)
			BEGIN
				SELECT @errorNumber=Number , @errorName=[Name]
				FROM ErrorCodes 
				WHERE [Name] = 'Flight not found';

				THROW @errorNumber,@errorName,1;
			END;
			
		ELSE IF((SELECT Id FROM Seats WHERE Id=@seatId) IS NULL)
			BEGIN
				SELECT @errorNumber=Number , @errorName=[Name]
				FROM ErrorCodes 
				WHERE [Name] = 'Seat not found';

				THROW @errorNumber,@errorName,1;
			END;
		THROW;
	END CATCH
END
GO
ALTER PROCEDURE dbo.usp_UserTickets_Select
	@username UsernameType
AS
BEGIN
	--select ticket
	SELECT 
		t.Id,
		t.PassengerName,
		t.FlightId,
		t.SeatId,
		u.Username
	FROM Tickets t
	INNER JOIN Users u ON u.Id = t.UserId
	WHERE u.Username = @username

	--select flight
	SELECT 
		f.Id,
		f.DepartureDate,
		f.LandingDate,
		f.FlightNumber,
		oa.[Name] AS OriginAirport,
		da.[Name] AS DestinationAirprot,
		a.[Name] AS Airline
	FROM Tickets t
	INNER JOIN Users u ON u.Id = t.UserId
	INNER JOIN Flights f ON f.Id = t.FlightId
		INNER JOIN Airports oa ON oa.Id = f.OriginAirportId
		INNER JOIN Airports da ON  da.Id =f.DestinationAirportId
		INNER JOIN Airlines a ON a.Id = f.AirlineId
	WHERE u.Username = @username;


	--select seats
	SELECT 
		seat.Id,
		seat.[Row],
		seat.[Column] ,
		section.SeatClass
	FROM Tickets t
	INNER JOIN Users u ON u.Id = t.UserId
	INNER JOIN Seats seat ON seat.Id = t.SeatId
		INNER JOIN Sections section ON section.Id = seat.SectionId
	WHERE u.Username = @username
END
GO