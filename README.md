# DemoERP API

## Overview

DemoERP API is a RESTful backend service developed using ASP.NET Core and SQL Server. The project simulates a customer synchronization system between an external CRM and an ERP database.

The application demonstrates enterprise backend development practices including authentication, authorization, validation, database integration, API testing, and audit tracking.

---

## Features

### Customer Management

* Create / Sync Customer
* Retrieve Customer
* Update Customer
* Soft Delete Customer

### Validation

* Required CustomerId
* Required FirstName
* Required Email
* Email format validation
* Phone number validation (10 digits)

### Security

* JWT Authentication
* Role-Based Authorization
* Protected API Endpoints
* Swagger JWT Integration

### Database

* SQL Server
* Stored Procedures
* Duplicate Customer Detection
* Soft Delete Strategy
* Sync Logging

### Testing

* xUnit Integration Tests
* API Endpoint Validation Tests
* Authentication Tests

---

## Technology Stack

| Technology   | Purpose             |
| ------------ | ------------------- |
| ASP.NET Core | REST API            |
| SQL Server   | Database            |
| JWT          | Authentication      |
| Swagger      | API Documentation   |
| xUnit        | Integration Testing |
| GitHub       | Version Control     |

---

## API Endpoints

| Method | Endpoint              | Description          |
| ------ | --------------------- | -------------------- |
| POST   | /api/Auth/login       | Generate JWT Token   |
| POST   | /api/Customer/sync    | Create Customer      |
| GET    | /api/Customer/{crmId} | Get Customer         |
| PUT    | /api/Customer/update  | Update Customer      |
| DELETE | /api/Customer/{crmId} | Soft Delete Customer |

---

## Authentication Flow

1. User submits credentials.
2. JWT token is generated.
3. Swagger Authorize button accepts token.
4. Protected endpoints require valid JWT.

---

## Future Enhancements

* Refresh Tokens
* Password Hashing (BCrypt)
* User Management
* Audit Logging
* Repository Pattern
* Service Layer
* Docker Deployment
* CI/CD Pipeline

---

## Author

Henry Chan
BSc Computer Science (SFU)
15+ Years Software Quality Assurance Experience
