# Task Management Service

## Prerequisites
- .NET 8 SDK
- Docker and Docker Compose (optional, for containerized deployment)
- PostgreSQL (if running without Docker)

## Setup and Running

### Without Docker
1. Ensure PostgreSQL is running and create a database named taskmanagement
2. Update appsettings.json with your PostgreSQL connection string if needed
3. Run the API:
`bash
cd TaskManagement.Api
dotnet run