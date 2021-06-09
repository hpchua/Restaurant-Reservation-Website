# Procedures

Other NuGet Package
**Swashbuckle.AspNetCore**
**Microsoft.AspNetCore.Mvc**

## Dapper

> Source: https://www.youtube.com/watch?v=F4faJc_mvII

>> Connection string defined in ```EmailService/appsettings.json```

1. Install NuGet Package 
	**Dapper**
	**System.Data.SqlClient**

2. Create ```IDataProvider``` & ```DataProvider``` class to process db

## RabbitMQ (Subscriber)

> Source: https://www.youtube.com/watch?v=rUKqaO8IQCE&list=PLXCqSX1D2fd_6bna8uP4-p3Y8wZxyB75G&index=8

1. Install NuGet Package 
	**Plain.RabbitMQ**

2. Add configuration on ```Startup.cs```
	- Connect to local host
	- Subscriber with connection, exchangeName, queueName, routing key, exchange Type

3. Create ```DataCollector.cs``` class to consume message 