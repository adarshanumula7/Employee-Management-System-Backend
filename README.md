# Employee Management System - Backend (ASP.NET Core)

## Overview
A RESTful API for managing employee data, built with ASP.NET Core and SQL Server.

## Features
- CRUD operations for employees
- JWT authentication

## Technologies Used
- ASP.NET Core 6
- Entity Framework Core
- SQL Server
- Swagger/OpenAPI

## Prerequisites
- [.NET SDK 6+](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (local or remote)


## Installation

1. **Clone the repository**
```
git clone https://github.com/adarshanumula/Employee-Management-System-Backend.git
```

3. **Navigate to directory**
```
cd Employee-Management-System-Backend
```

5. **Configure the database**
- Update `appsettings.json` with your connection string:
- ```
  "ConnectionStrings": {
    "EMDBConn": "Server=.;Database=EmployeeManagementDB;Trusted_Connection=True;"
  }
  ```
  
  3. **Create the database and tables**
- Run the provided SQL script (`/sql/initial_setup.sql`) in your SQL Server.

4. **Run the backend**
 ```
dotnet run
```

## Usage
- API runs at `https://localhost:<port_number>/api/Employee/` in the frontend app.
- Use tools like Postman or the frontend app to interact.
- front-end app is [here](https://github.com/adarshanumula/Employee-Management-System-Frontend.git)

## API Endpoints

| Method | Endpoint                  | Description                 |
|--------|---------------------------|-----------------------------|
| POST   |``` /api/Employee/login ```      | Authenticate and get JWT    |
| POST   | ```/api/Employee/addemployee``` | Add a new employee          |
| GET    | ```/api/Employee/employeeslist``` | Get all employees         |
| GET    | ```/api/Employee/GetallRoles```  | Get all roles in the comapany |
| PUT    |```/api/Employee/updateemployee```| update an existing employee|
| DELETE |```/api/Employee/deleteemployee```| delete an existing employee|
