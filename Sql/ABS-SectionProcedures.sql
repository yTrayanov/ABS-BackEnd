USE AirlineBookingSystem;
GO

ALTER PROCEDURE dbo.usp_Sections_Insert
	@rows TINYINT,
	@columns TINYINT,
	@seatClass TINYINT,
	@flightNumber NVARCHAR(255)
AS
BEGIN
	DECLARE @flightId INTEGER = (SELECT Id FROM Flights WHERE FlightNumber=@flightNumber);
	DECLARE @errorNumber INTEGER;
	DECLARE @errorName NVARCHAR(100);

	IF(@flightId IS NULL)
		BEGIN
			SELECT @errorNumber=Number , @errorName=[Name]
			FROM ErrorCodes 
			WHERE [Name] = 'User not found';

			THROW @errorNumber,@errorName,1;
		END;
	IF((SELECT COUNT(Id) 
		FROM Sections
		WHERE FlightId = @flightId AND SeatClass = @seatClass) > 0)
			BEGIN
				SELECT @errorNumber=Number , @errorName=[Name]
				FROM ErrorCodes 
				WHERE [Name] = 'SeatClass already exists';

				THROW @errorNumber,@errorName,1;
			END;

	INSERT INTO Sections ([Rows] , [Columns] , AvailableSeatsCount , SeatClass , FlightId)
				VALUES (@rows , @columns , @rows*@columns , @seatClass , @flightId);

	DECLARE @sectionId INTEGER = SCOPE_IDENTITY();
	DECLARE @row TINYINT = 1;
	DECLARE @col TINYINT = 1;

	WHILE @row <= @rows
	BEGIN
		WHILE @col <= @columns
		BEGIN
			INSERT INTO Seats ([Row] , [Column], SectionId)
					VALUES (@row , @col, @sectionId);
			SET @col= @col + 1
		END
		SET @row = @row + 1;
		SET @col = 1;
	END
END
GO
ALTER PROCEDURE dbo.usp_Airport_Insert
	@name NVARCHAR(3)
AS
BEGIN

	BEGIN TRY
		INSERT INTO Airports ([Name]) VALUES (@name);
	END TRY
	BEGIN CATCH
		IF((SELECT COUNT(*) FROM Airports WHERE [Name]= @name) <> 0)
		BEGIN
			DECLARE @errorNumber INTEGER;
			DECLARE @errorName NVARCHAR(100);

			SELECT @errorNumber=Number , @errorName=[Name]
			FROM ErrorCodes 
			WHERE [Name] = 'Airport name already exists';

			THROW @errorNumber,@errorName,1;
		END;
		THROW;
	END CATCH
END
Go

ALTER PROCEDURE dbo.usp_Airline_Insert
	@name NVARCHAR(6)
AS
BEGIN
	BEGIN TRY
		INSERT INTO Airlines ([Name]) VALUES (@name);
	END TRY
	BEGIN CATCH
		IF((SELECT COUNT(*) FROM Airlines WHERE [Name]= @name) <> 0)
			BEGIN
				DECLARE @errorNumber INTEGER;
				DECLARE @errorName NVARCHAR(100);

				SELECT @errorNumber=Number , @errorName=[Name]
				FROM ErrorCodes 
				WHERE [Name] = 'Airline name already exists';

				THROW @errorNumber,@errorName,1;
			END;
		THROW;
	END CATCH
END
GO
