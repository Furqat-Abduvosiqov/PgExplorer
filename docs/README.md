# PgExplorer

A modern PostgreSQL database explorer and management tool built with .NET 8 and Angular 17+.

## 📋 About

This project was created as a **test task for Webase Company** to demonstrate full-stack development skills using modern technologies and clean architecture principles.

## 🚀 Quick Start with Docker

The easiest way to run PgExplorer is using Docker Compose. The setup includes:
- **PostgreSQL** database
- **Nginx** web server for Angular frontend  
- **.NET 8** backend API

### Prerequisites
- [Docker](https://www.docker.com/get-started) installed on your machine
- [Docker Compose](https://docs.docker.com/compose/install/) installed

### Run the Application

```bash
# Clone the repository
git clone <repository-url>
cd PgExplorer

# Start all services
docker-compose up
```

That's it! 🎉

### Access the Application

Once all containers are running:
- **Frontend**: http://localhost:80
- **Backend API**: http://localhost:5000
- **API Documentation**: http://localhost:5000/swagger
- **PostgreSQL**: localhost:5432

## 🏗️ Architecture

PgExplorer follows **Clean Architecture** principles with **Vertical Slice Architecture** for better maintainability and scalability.

### Key Features:
- ✅ Feature-driven repository pattern
- ✅ Pure domain layer with zero dependencies
- ✅ CQRS with MediatR
- ✅ Server-side rendering (Angular Universal)
- ✅ Docker containerization
- ✅ Comprehensive testing strategy

## 📚 Documentation

Explore detailed documentation about the project:

### Architecture & Design
- 📖 [**Architecture Overview**](Architecture.md) - Complete architectural documentation with diagrams
- 🏢 [**Entities Documentation**](Entities.md) - Domain entities and data models
- 🔧 [**Feature Overview**](Feature%20Overview.md) - Detailed feature descriptions and capabilities

### Quick Links
- [Architecture Patterns](Architecture.md#architectural-patterns)
- [Technology Stack](Architecture.md#technology-stack)
- [Development Workflow](Architecture.md#development-workflow)
- [Domain Entities](Entities.md)
- [Available Features](Feature%20Overview.md)

## 🛠️ Technology Stack

### Backend
- **.NET 8** - Latest LTS framework
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database
- **MediatR** - CQRS implementation
- **FluentValidation** - Input validation

### Frontend
- **Angular 17+** - Modern SPA framework
- **TypeScript** - Type-safe development
- **Angular Universal** - Server-side rendering
- **RxJS** - Reactive programming

### Infrastructure
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **Nginx** - Web server
- **PostgreSQL** - Database server

## 🧪 Testing

The project includes comprehensive testing:

```bash
# Run unit tests
dotnet test tests/Pg.Explorer.Unit.Tests/

# Run integration tests
dotnet test tests/PgExplorer.Integration.Tests/
```

## 🌟 Key Features

- **Database Connection Management** - Secure connection handling with encryption
- **Query Builder & Executor** - Interactive SQL query interface  
- **Table Browser** - Navigate database schemas and tables
- **Data Manipulation** - CRUD operations on table data
- **Export Functionality** - Export data in various formats
- **Multi-Database Support** - Manage multiple PostgreSQL connections

## 🔧 Development

### Manual Setup (without Docker)

If you prefer to run the application manually:

1. **Prerequisites:**
   - .NET 8 SDK
   - Node.js 18+
   - PostgreSQL 12+

2. **Backend Setup:**
   ```bash
   cd src/Pg.Explorer.Host
   dotnet run
   ```

3. **Frontend Setup:**
   ```bash
   cd src/pg-explorer-client
   npm install
   npm start
   ```

4. **Database Setup:**
   - Create PostgreSQL database
   - Update connection string in `appsettings.json`
   - Run migrations: `dotnet ef database update`

## 🤝 Contributing

This project was created as a test task, but contributions and suggestions are welcome!

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## 📝 License

This project is created for demonstration purposes as part of a technical assessment for Webase Company.
