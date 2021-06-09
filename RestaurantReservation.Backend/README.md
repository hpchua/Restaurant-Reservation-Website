## Project Structure

![image](https://user-images.githubusercontent.com/52247950/121307107-3a624a80-c932-11eb-8276-8dc7eadc76c2.png)

## NLog
> Source: https://www.youtube.com/watch?v=o5u4fE0t79k

1. Install NuGet Package 
	**NLog**
	**NLog.Web.AspNetCore**

2. Create ```nlog.config```

3. Modify ```Program.cs```

## Swagger (Swashbuckle)

> Source: https://www.c-sharpcorner.com/article/authentication-authorization-using-net-core-web-api-using-jwt-token-and/
>> Note: If the **System.Io.FileNotfoundException** occurs, follow this link https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-5.0&tabs=visual-studio#xml-comments

1. Install NuGet Package 
	**Swashbuckle.AspNetCore 5.5.1**
	**Swashbuckle.AspNetCore.Annotations 5.5.1**
	**Microsoft.AspNetCore.Mvc.Versioning**
	**Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer**

2. SwaggerConfigurations
- ```SwaggerGeneralConfig.cs```
- ```GenerateJsonFilter.cs```
- ```ApiVersionOperationFilter.cs```
- ```AuthorizationOperationFilter.cs```

3. Update ```Startup.cs```
> region **Swagger Generator, API Versioning, Swagger Middleware```

## Database Context Entity Framework - Code First Approach
> Source: https://www.youtube.com/watch?v=J7Ho7HS7i_Y

>> Connection string defined in ```RestaurantReservationSystem.API/appsettings.json```

1. Install NuGet Package 
	Infrastructure
	**Microsoft.EntityFrameworkCore.SqlServer**
	**Microsoft.EntityFrameworkCore.Tools**
	
	Core
	**Microsoft.AspNetCore.Identity**
	**Microsoft.AspNetCore.Identity.EntityFrameworkCore**

	Api
	**Microsoft.EntityFrameworkCore.Design**

2. Create ```ApplicationUser``` & ```DatabaseContext``` Class

3. Create Migrations
	```Add-Migration -Name "InitialDbCreation" -OutputDir "Data\Migrations"```

4. Run the Migrations
	```Update-Database```

5. Add on / Modify the table
	Just create a model with foreign key or some attributes and repeat the 2 steps above
	Add DbSet<NewTable> into ```DatabaseContext.cs```
	It will auto generate ID

6. Update ```Startup.cs```

## Seed Data

1. Follow ```IDbInitializer.cs``` interface and ```DbInitializer.cs``` class

2. Register Service into ```Startup.cs``` and Middleware

## JWT Bearer 
1. Install NuGet Package 
	**Microsoft.AspNetCore.Authentication.JwtBearer**
	**IdentityModel**

2. Add Configuration into ```appsetting.json```
- ValidAudience
- ValidIssuser
- Secret

3. Update ```Startup.cs```
> region **Json Web Token (JWT)```

##### For Generating Jwt
Refer to ```AuthenticateController > GenerateJwtToken```

### Refresh JWT Token
> Source: https://www.youtube.com/watch?v=AWnO_b8XIeA&list=PL4WEkbdagHIQVbiTwos0E38VghMJA06OT&index=9

## RabbitMQ (Publisher)

> Source: https://www.youtube.com/watch?v=rUKqaO8IQCE&list=PLXCqSX1D2fd_6bna8uP4-p3Y8wZxyB75G&index=8

1. Install NuGet Package 
	**Plain.RabbitMQ**

2. Add configuration on ```Startup.cs```
	- Connect to local host
	- Publisher with connection, exchangeName, exchange Type

3. Publishing message with JsonConvert.SerializeObject and routing key

## xUnit

> Source: 

>> Priority
https://github.com/asherber/Xunit.Priority

1. Install NuGet Package 
	**Microsoft.EntityFrameworkCore.InMemory**
	**Microsoft.AspNetCore.Mvc.Testing**
	**Xunit.Priority**
	**FluentAssertions**

2. Create ```PriorityAttribute.cs```, ```DefaultPriorityAttribute.cs```, ```PriorityOrderer.cs```

>> Test Case

1. Install NuGet Package 
	**Microsoft.AspNetCore.Mvc.Testing**
	**Microsoft.AspNetCore.App**
	**Microsoft.EntityFrameworkCore.InMemory**

2. Create ```ApiRoute.cs```, ```ApiService.cs```

https://www.youtube.com/watch?v=7roqteWLw4s
https://gist.github.com/Elfocrash/101ffc29947832545cdaebcb259c2f44

>>> Complete Example: 
https://www.c-sharpcorner.com/article/crud-operations-unit-testing-in-asp-net-core-web-api-with-xunit/

## SQL Server Agent 

> Source: https://www.youtube.com/watch?v=yttIsQvmrJc

Better choice to update the status on a regular basis each time rather than the list is generated and update status at the same time.

From here, I will create stored procedure to execute in sql server agent.

> Refresh_Token

```
USE [FinalRestaurantReservationDB]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID ( 'ClearExpiredRefreshToken', 'P' ) IS NOT NULL   
    DROP PROCEDURE ClearExpiredRefreshToken;  
GO

CREATE PROCEDURE [dbo].[ClearExpiredRefreshToken] AS
BEGIN
	SET NOCOUNT ON;

	DELETE 
	FROM Refresh_Token
	WHERE DATEDIFF(day, CAST(GETDATE() as date), CAST(ExpiryDate as date)) < 0;
END
GO
```

> Promotion

Update Promotion Status

```
USE [FinalRestaurantReservationDB]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID ( 'UpdatePromotionStatus', 'P' ) IS NOT NULL   
    DROP PROCEDURE UpdatePromotionStatus;  
GO

CREATE PROCEDURE [dbo].[UpdatePromotionStatus] AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Promotion
	SET isAvailable = 0
	WHERE DATEDIFF(Day, CAST(GETDATE() as date), CAST(EndDate as date)) < 0;
END
GO
```

Push Promotional Email

```
USE [FinalRestaurantReservationDB]
GO
/****** Object:  StoredProcedure [dbo].[PushPromotionalEmail]    Script Date: 19/05/2021 9:33:16 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID ( 'PushPromotionalEmail', 'P' ) IS NOT NULL   
    DROP PROCEDURE PushPromotionalEmail;  
GO

CREATE PROCEDURE [dbo].[PushPromotionalEmail]
AS
BEGIN
	SET NOCOUNT ON;

	-- Get the email address of each subscribed member
	DECLARE @UserEmail AS NVARCHAR(50)

	-- Get the email subject and content
	DECLARE @Subject AS NVARCHAR(100), @Content AS NVARCHAR(200);

	DECLARE cursor_Promotions CURSOR FOR 
		SELECT Name, CONCAT('<h1>', Description, '</h1> <p>', Content , '</p> <br/>' + '<a href="https://localhost:44326/Restaurants/Details/' + CAST(RestaurantID  as varchar(5)) +'">Restaurant Link</a>') AS Body
		FROM Promotion
		WHERE isAvailable = 1;

	OPEN cursor_Promotions;
		FETCH NEXT FROM cursor_Promotions  
		INTO @Subject, @Content;

		WHILE @@FETCH_STATUS = 0  
		BEGIN  
			-- Get the all the member subscribers
			DECLARE cursor_Users CURSOR FOR 
				SELECT Email
				FROM AspNetUsers
				WHERE isSubscriber = 1;

			OPEN cursor_Users;
				FETCH NEXT FROM cursor_Users
				INTO @UserEmail

				WHILE @@FETCH_STATUS = 0
				BEGIN 
					EXEC msdb.dbo.sp_send_dbmail
						@profile_name = 'T-Cube Company',
						@recipients = @UserEmail,
						@subject = @Subject,
						@body = @Content,
						@body_format = 'HTML'

					FETCH NEXT FROM cursor_Users  
					INTO @UserEmail;
				END
				CLOSE cursor_Users;
				DEALLOCATE cursor_Users;
			-- This is executed as long as the previous fetch succeeds.  
			FETCH NEXT FROM cursor_Promotions  
			INTO @Subject, @Content; 
		END
	CLOSE cursor_Promotions;
	DEALLOCATE cursor_Promotions;
END
```

> Restaurant_Schedule

```
USE [FinalRestaurantReservationDB]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID ( 'UpdateScheduleStatus', 'P' ) IS NOT NULL   
    DROP PROCEDURE UpdateScheduleStatus;  
GO

CREATE PROCEDURE [dbo].[UpdateScheduleStatus] AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Restaurant_Schedule
		SET Status = 2
		WHERE Status = 1 AND AvailableSeat = 0;

	UPDATE Restaurant_Schedule
	SET Status = 3
	WHERE Status <> 4 AND 
	      (DATEDIFF(Day, CAST(GETDATE() as date), CAST(ScheduleDate as date)) < 0 OR
		  ((DATEDIFF(Day, CAST(GETDATE() as date), CAST(ScheduleDate as date)) = 0) AND (CONVERT(time,EndTime) < CONVERT(time,GETDATE()))));
END
GO
```

> Booking
```
USE [FinalRestaurantReservationDB]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID ( 'UpdateBookingStatus', 'P' ) IS NOT NULL   
    DROP PROCEDURE UpdateBookingStatus;  
GO

CREATE PROCEDURE [dbo].[UpdateBookingStatus] AS
BEGIN
	SET NOCOUNT ON;

	UPDATE B
	SET B.BookingStatus = 'Expired'
	FROM Booking B
	INNER JOIN Booking_Detail BD ON B.BookingID = BD.BookingID
	INNER JOIN Restaurant_Schedule RS ON BD.ScheduleID = RS.ScheduleID
	WHERE B.BookingStatus = 'Pending' AND 
		  (DATEDIFF(Day, CAST(GETDATE() as date), CAST(ScheduleDate as date)) < 0 OR
		  ((DATEDIFF(Day, CAST(GETDATE() as date), CAST(ScheduleDate as date)) = 0) AND (CONVERT(time,RS.EndTime) < CONVERT(time,GETDATE()))));
END
GO
```
