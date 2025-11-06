# Trivare

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com)
[![Version](https://img.shields.io/badge/version-0.0.1-blue)](https://github.com)
[![License](https://img.shields.io/badge/license-MIT-green)](./LICENSE)

Trivare is an MVP trip planning web application that helps users organize travel information, store documents, and discover attractions using AI and Google Places API.

## Table of Contents

- [Project Description](#project-description)
- [Tech Stack](#tech-stack)
- [Getting Started Locally](#getting-started-locally)
- [Available Scripts](#available-scripts)
- [Testing](#testing)
- [Project Scope](#project-scope)
- [Project Status](#project-status)
- [License](#license)

## Project Description

Trivare is designed to streamline the trip planning process. It allows users to manage all aspects of their journey, from creating detailed daily itineraries to storing important documents securely. With AI-powered recommendations and integration with Google Places, discovering new attractions tailored to your preferences has never been easier.

## Tech Stack

### Frontend

- **[Astro 5](https://astro.build/)**: Modern web framework with an islands architecture.
- **[React 19](https://react.dev/)**: Library for building interactive UI components.
- **[TypeScript 5](https://www.typescriptlang.org/)**: Type safety for robust code.
- **[Tailwind CSS 4](https://tailwindcss.com/)**: Utility-first CSS framework for rapid styling.
- **[Shadcn/ui](https://ui.shadcn.com/)**: Reusable and accessible React components.

### Backend

- **[.NET 9](https://dotnet.microsoft.com/)**: High-performance, cross-platform framework for building REST APIs.
- **[Azure SQL](https://azure.microsoft.com/en-us/products/azure-sql/database/)**: Managed relational database service.
- **[Cloudflare R2](https://www.cloudflare.com/products/r2/)**: S3-compatible object storage with no egress fees.

### DevOps

- **[GitHub Actions](https://github.com/features/actions)**: CI/CD automation for building, testing, and deploying.
- **[Azure](https://azure.microsoft.com/)**: Cloud platform for hosting the frontend, backend, and database.

## Getting Started Locally

To get a local copy up and running, follow these simple steps.

### Prerequisites

- **Node.js**: It's recommended to use a version manager like `nvm`.
- **.NET 9 SDK**: Download and install from the [official .NET website](https://dotnet.microsoft.com/download/dotnet/9.0).

### Installation & Setup

#### Repository

```sh
git clone https://github.com/bigboyTaysh/trivare.git
```

#### Frontend (Client)

1.  Navigate to the `Client` directory:
    ```sh
    cd Client
    ```
2.  Install NPM packages:
    ```sh
    npm install
    ```
3.  Start the development server:
    ```sh
    npm run dev
    ```

#### Backend (Server)

1.  **Setup Environment Variables**

    Copy the environment template and configure your local settings:

    ```sh
    cp .env.template .env
    ```

    Edit `.env` to set a secure `SA_PASSWORD` for the database.

2.  **Start SQL Server with Docker**

    This project uses Docker to run a local SQL Server instance. Make sure you have [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.

    From the root of the repository, run:

    ```sh
    docker-compose up -d
    ```

        This will start a SQL Server container. The database password is configured via the `SA_PASSWORD` environment variable in the `.env` file.

3.  **Configure Connection String and Launch Settings**

    a. Copy the appsettings template and configure your local settings:

    ```sh
    cp Server/Api/appsettings.Development.json.template Server/Api/appsettings.Development.json
    ```

    b. Copy the launch settings template:

    ```sh
    cp Server/Api/Properties/launchSettings.json.template Server/Api/Properties/launchSettings.json
    ```

    c. Update the `Password` in `appsettings.Development.json` to match the `SA_PASSWORD` you set in `.env`.

4.  **Apply Database Migrations**

    To set up the database schema, you'll need the .NET Entity Framework Core tools.

    a. Install `dotnet-ef` as a global tool (if you haven't already):

    ```sh
    dotnet tool install --global dotnet-ef
    ```

    b. Navigate to the `Server/Api` directory and apply the migrations:

    ```sh
    cd Server/Api
    dotnet ef database update
    ```

5.  **Run the API**

    Once the database is set up, you can run the backend.

    ```sh
    dotnet run
    ```

## Available Scripts

In the `Client` directory, you can run the following scripts:

- `npm run dev`: Starts the development server.
- `npm run build`: Builds the app for production.
- `npm run preview`: Previews the production build locally.
- `npm run lint`: Lints the codebase for errors.
- `npm run format`: Formats the code using Prettier.

## Testing

Trivare uses a comprehensive multi-layered testing strategy to ensure code quality and reliability.

### Project Structure

```
Server/
├── Application.Tests/        # Unit tests for application services
├── Domain.Tests/            # Unit tests for domain entities
└── Infrastructure.Tests/    # Unit tests for repositories and infrastructure

Client/
├── src/
│   └── **/*.test.tsx        # Unit tests for components and hooks
└── e2e/                     # End-to-end tests
```

### Tech Stack

#### Backend (.NET 9)

- **Unit Tests**: xUnit + FluentAssertions + Moq

#### Frontend (Astro + React + TypeScript)

- **Unit Tests**: Vitest + React Testing Library + jsdom
- **E2E Tests**: Playwright (Chromium)

### Running Tests

The easiest way to run tests is using the provided test script with specific arguments:

```bash
# Run all tests (default behavior)
./test.sh

# Run unit tests only
./test.sh unit

# Run E2E tests only
./test.sh e2e

# Run E2E tests in debug mode (with browser visibility)
./test.sh e2e:debug
```

When running all tests, the script will:

1. Start the backend API service locally
2. Start the frontend dev server locally
3. Run backend unit tests
4. Run frontend unit tests
5. Run E2E tests
6. Stop the local services

#### Backend Tests Only

```bash
# All backend tests
cd Server && dotnet test

# Specific test project
dotnet test Domain.Tests/Domain.Tests.csproj
dotnet test Application.Tests/Application.Tests.csproj
dotnet test Infrastructure.Tests/Infrastructure.Tests.csproj

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

#### Frontend Tests Only

```bash
cd Client

# Unit tests
npm run test          # Watch mode
npm run test:run      # Run once
npm run test:ui       # With UI
npm run test:coverage # With coverage

# E2E tests
npm run test:e2e          # Headless
npm run test:e2e:headed   # With browser
npm run test:e2e:debug    # Debug mode
npm run test:e2e:ui       # With UI
```

### Test Database

- **Unit Tests**: Use in-memory database for isolation
- **Integration Tests**: Use in-memory database (EF Core InMemory)
- **E2E Tests**: Use dedicated SQL Server test instance

To start the test database manually:

```bash
docker-compose --profile test up -d sqlserver-test
```

**Note**: The test script automatically starts and stops the test database when running E2E tests. No manual setup required.

### Troubleshooting

#### Debugging

- Use `dotnet test --logger "console;verbosity=detailed"` for verbose output
- Use `npm run test:ui` to debug frontend tests
- Use `npm run test:e2e:debug` for E2E debugging

## Project Scope

### Core Features

- **Trip Management**: Full CRUD operations for trips, including days, transport, accommodation, and attractions.
- **AI-Powered Search**: Integration with OpenRouter.ai to filter and recommend places based on user preferences.
- **Place Discovery**: Google Places API integration for automatic attraction suggestions.
- **File Storage**: Upload and manage trip-related files (PDF, PNG, JPEG, max 5MB, 10 files per trip).
- **User Management**: JWT-based authentication, profile management, and GDPR-compliant account deletion.
- **Admin Panel**: A metrics dashboard tracking trips, searches, and AI usage over a 60-day window.

### Constraints

- Maximum of 10 trips per user.
- No trip sharing between users.
- No mobile app in the MVP.
- The admin panel is restricted to users with an admin role only.

## Project Status

This project is currently an **MVP (Minimum Viable Product)** and is under active development. New features and improvements are planned for future releases.

## License

This project is licensed under the MIT License. See the `LICENSE` file for more details.
PR test
