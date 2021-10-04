USE AirlineBookingSystem;
GO
ALTER FUNCTION fn_GetUserRoles (@userId UserIdType)
RETURNS TABLE
AS RETURN SELECT r.[Name]
	FROM Roles r
	INNER JOIN UserRoles ur ON r.Id =ur.RoleId
	WHERE ur.UserId = @userId
GO
ALTER PROCEDURE usp_Initial_Insert
AS
BEGIN
	SET NOCOUNT ON;
	IF((SELECT COUNT(*) FROM Airlines) = 0)
		INSERT INTO Airlines VALUES ('DELTA'), ('GAMA');

		IF((SELECT COUNT(*) FROM Airports) = 0)
			INSERT INTO Airports VALUES ('LAA') , ('NYC');

			
		IF((SELECT COUNT(*) FROM Roles) = 0)
				INSERT INTO Roles VALUES ('User') , ('Admin');

		IF(
			(SELECT COUNT(*)
			 FROM Users u
			 INNER JOIN UserRoles ur ON u.Id = ur.UserId
			 INNER JOIN Roles r ON ur.RoleId = r.Id
			 WHERE r.[Name] = 'Admin') = 0)
				DECLARE @adminRoleId INTEGER = (SELECT Id FROM Roles WHERE [Name] = 'Admin')
				DECLARE @adminId NVARCHAR(255) = NEWID();

				INSERT INTO Users (Id , Username , Email , [Status] , PasswordHash) 
					VALUES (@adminId ,'admin' , 'admin@admin.bg' , 0 , 'admin123')

				INSERT INTO UserRoles (UserId , RoleId) 
					VALUES (@adminId , @adminRoleId);

					IF((SELECT COUNT(*) FROM ErrorCodes) = 0)
		BEGIN
			INSERT INTO ErrorCodes (Number,[Name])
			VALUES
				(50010 , 'Username already exists'),
				(50011 , 'Email already exists'),
				(50012 , 'Airport name already exists'),
				(50013, 'Airline name already exists'),
				(50014, 'SeatClass already exists'),
				(50015, 'FlightNumber already exists'),
				(50020, 'User not found'),
				(50021, 'Flight not found'),
				(50022, 'Seat not found'),
				(50023, 'Airport not found'),
				(50024, 'Airline not found'),
				(50030, 'Invalid Credentials');
		END

END
GO
ALTER PROCEDURE usp_RegisterUsers_Insert
	@username UsernameType,
	@password PasswordType,
	@email EmailType
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		DECLARE @userRoleId INTEGER = (SELECT Id FROM Roles WHERE [Name] = 'User')
		DECLARE @userId UserIdType = NEWID();
		DECLARE @errorNumber INTEGER;
		DECLARE @errorName NVARCHAR(100);

		INSERT INTO Users (Id , Username , Email , [Status] , PasswordHash) 
			VALUES (@userId ,@username , @email , 0 , @password)
		INSERT INTO UserRoles (UserId , RoleId) 
			VALUES (@userId , @userRoleId);
	END TRY
	BEGIN CATCH
		IF((SELECT COUNT(*) FROM Users WHERE Username=@username) <> 0)
		BEGIN
			SELECT @errorNumber=Number , @errorName=[Name]
			FROM ErrorCodes 
			WHERE [Name] = 'Username already exists';

			THROW @errorNumber,@errorName,1;
		END;
			
		IF((SELECT COUNT(*) FROM Users WHERE Email=@email) <> 0)
		BEGIN
			SELECT @errorNumber=Number , @errorName=[Name]
			FROM ErrorCodes 
			WHERE [Name] = 'Email already exists';

			THROW @errorNumber,@errorName,1;
		END;

		THROW;
	END CATCH
END
GO
ALTER PROCEDURE ups_LoginUser_Update
	@username UsernameType,
	@password PasswordType,
	@status NVARCHAR(20)
AS
BEGIN
	DECLARE @userId UserIdType;
	DECLARE @errorNumber INTEGER;
	DECLARE @errorName NVARCHAR(100);

	SELECT @userId = Id 
	FROM Users 
	WHERE Username = @username AND PasswordHash = @password
	
	IF(@userId IS NULL)
		BEGIN
			SELECT @errorNumber=Number , @errorName=[Name]
			FROM ErrorCodes 
			WHERE [Name] = 'Invalid Credentials';

			THROW @errorNumber,@errorName,1;
		END;

	UPDATE Users 
	SET [Status] = @status
	WHERE Username = @username 
		AND PasswordHash = @password 
		AND [Status] <> @status

	SELECT * FROM fn_GetUserRoles (@userId)
END
GO
ALTER PROCEDURE usp_LogoutUser_Update
	@username UsernameType,
	@status INTEGER
AS
BEGIN
	DECLARE @currentStatus UserIdType = (SELECT [Status] FROM Users WHERE Username = @username);
	DECLARE @errorNumber INTEGER;
	DECLARE @errorName NVARCHAR(100);

	IF(@currentStatus IS NULL)
		BEGIN
			SELECT @errorNumber=Number , @errorName=[Name]
			FROM ErrorCodes 
			WHERE [Name] = 'User not found';

			THROW @errorNumber,@errorName,1;
		END;

	UPDATE Users
	SET [Status]= @status
	WHERE Username = @username AND [Status] <> @status
END
GO
ALTER PROCEDURE ups_GetUserRoles_Select
	@username UsernameType,
	@status INTEGER
AS
BEGIN
	DECLARE @userId UserIdType;
	DECLARE @userStatus INTEGER;
	DECLARE @errorNumber INTEGER;
	DECLARE @errorName NVARCHAR(100);
	
	SELECT @userId= Id , @userStatus = [Status] 
	FROM Users 
	WHERE Username=@username;


	IF(@userId IS NULL)
		BEGIN
				SELECT @errorNumber=Number , @errorName=[Name]
				FROM ErrorCodes 
				WHERE [Name] = 'User not found';
				THROW @errorNumber,@errorName,1;
			END;

	IF(@userStatus = @status)
		SELECT * FROM fn_GetUserRoles (@userId)
END
GO
ALTER PROCEDURE ups_CheckUserStat_Select
	@username UsernameType
AS
BEGIN
	SELECT [Status]
	FROM Users
	WHERE Username = @username
END
GO