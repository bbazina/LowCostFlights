# LowCostFlight API - README

## Prerequisites

To run the LowCostFlight API, ensure the following tools and services are installed and running on your system:

### Required Software

1. **Docker**

   - Docker is used to run the required service Redis in isolated containers.
   - [Install Docker](https://docs.docker.com/get-docker/)

2. **.NET SDK 8.0**

   - The API is built using .NET 8.0.
   - [Install .NET SDK](https://dotnet.microsoft.com/download)

3. **Redis**

   - A caching mechanism used by the API.
   - Redis is configured to run in a Docker container.

4. **MS SQL Server**

   - The API uses an MS SQL database to store flight-related data.

### Setting Up Docker Container

#### Redis

Run the following command to start the Redis container:

```bash
docker run --name redis -d -p 6379:6379 redis
```

### Database Configuration

- The API uses Windows Authentication for MS SQL Server.
- Update the connection string in `appsettings.json` to match your configuration:

```json
"DefaultConnection": "Data Source=.;Initial Catalog=LowCostFlightDb;Integrated Security=True;TrustServerCertificate=True"
```

### Running Migrations

Before starting the API, apply migrations to create the necessary database tables:

```bash
dotnet ef database update
```

## What the API Does

The LowCostFlight API is a backend service that allows users to search for and filter low-cost flights. The key features of the API include:

1. **Flight Search and Filtering**:

   - Search flights based on origin, destination, departure date, return date, number of passengers, and currency.

2. **Caching**:

   - Results are cached using Redis to improve performance and reduce load on the database.

3. **Pagination**:

   - Supports paginated responses to manage large datasets efficiently.

4. **Amadeus API Integration**:

   - Fetches live flight data from the Amadeus API and stores it in the database.

### API Endpoints

- **GET /flights**:
  - Search for flights based on query parameters.

### Example Workflow

1. Start the required Docker container Redis.
2. Run the API using `dotnet run`.
3. Use an HTTP client (e.g., Postman) or frontend application to interact with the endpoints.
