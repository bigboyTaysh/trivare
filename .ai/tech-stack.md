# Trivare Project Tech Stack

Below is a summary of the technologies selected for the Trivare project, based on the analysis conducted.

## Frontend

-   **Astro 5:** A modern framework for building fast websites. Used as the foundation of the application, integrating other technologies and ensuring high performance through its "islands architecture."
-   **React 19:** A library for creating interactive user interface components. Used within Astro to build dynamic application elements such as forms, lists, and management panels.
-   **TypeScript 5:** A statically typed superset of JavaScript. It ensures type safety, which facilitates the development and maintenance of the code, minimizing errors during application runtime.
-   **Tailwind CSS 4:** A utility-first CSS framework for rapid and consistent interface styling. It allows for the creation of a modern design without leaving the HTML/JSX code.
-   **Shadcn/ui:** A collection of ready-to-use, reusable UI components for React, built on top of Tailwind CSS. It speeds up development by providing aesthetic and accessible elements such as buttons, forms, and modals.

## Backend

-   **.NET 9:** A high-performance and scalable development platform from Microsoft. Used to build the REST API that will handle the application's business logic, user authorization, and communication with the database.

## Database

-   **Azure SQL:** A managed relational database service in the Microsoft Azure cloud. It provides high availability, scalability, and data security, integrating seamlessly with the rest of the Azure ecosystem. It is used to store all application data.

## File Storage

-   **Cloudflare R2:** An object storage service with no egress fees. It will be used for the secure storage of files uploaded by users, such as tickets (PDF) or photos (PNG, JPEG), ensuring fast and inexpensive access to resources.

## Artificial Intelligence

-   **OpenRouter.ai:** A platform that aggregates various language models (LLMs). It will be used to implement AI-based features, such as recommending and filtering places based on user preferences.

## CI/CD and Hosting

-   **GitHub Actions:** Automates the CI/CD pipeline with two workflows:
    -   **Pull Request Workflow:** Runs linting and unit tests on every PR to `master`
    -   **Production Workflow:** Deploys to production on push to `master` - runs tests, builds frontend, and deploys to both Cloudflare Pages and Render in parallel
-   **Cloudflare Pages:** Hosts the Astro frontend with global CDN distribution. Deployment triggered via GitHub Actions using Wrangler CLI.
-   **Render:** Hosts the .NET 9 backend API with automatic deployments using Docker. Configured via `render.yaml` (uses `Server/Dockerfile`) with health check endpoint at `/health`.
-   **Azure SQL:** Cloud-hosted managed database service providing high availability and scalability.

### Deployment Configuration

**Required GitHub Secrets:**
-   `CLOUDFLARE_API_TOKEN`, `CLOUDFLARE_ACCOUNT_ID`, `CLOUDFLARE_PROJECT_NAME`
-   `RENDER_API_KEY`, `RENDER_SERVICE_ID`

**Required Render Environment Variables:**
All variables must be configured in Render Dashboard (Environment tab):
-   `ASPNETCORE_ENVIRONMENT` = `Production`
-   `CONNECTION_STRING` (Azure SQL connection string)
-   `Jwt__SecretKey` (min 32 chars)
-   `Jwt__Issuer` = `Trivare`
-   `Jwt__Audience` = `Trivare`
-   `Jwt__AccessTokenExpirationMinutes` = `15`
-   `Jwt__RefreshTokenExpirationDays` = `7`
-   `CloudflareR2__AccountId`, `CloudflareR2__AccessKeyId`, `CloudflareR2__SecretAccessKey`, `CloudflareR2__BucketName`
-   `CloudflareR2__PresignedUrlExpirationMinutes` = `60`
-   `SmtpSettings__Server`, `SmtpSettings__SenderEmail`, `SmtpSettings__Username`, `SmtpSettings__Password`
-   `SmtpSettings__Port` = `587`
-   `SmtpSettings__SenderName` = `Trivare`
-   `GooglePlaces__ApiKey`, `GooglePlaces__MaxResults` = `20`, `GooglePlaces__SearchRadiusMeters` = `5000`
-   `OpenRouter__ApiKey`, `OpenRouter__Model` = `anthropic/claude-3.5-sonnet`, `OpenRouter__BaseUrl` = `https://openrouter.ai/api/v1`, `OpenRouter__ResultCount` = `8`
-   `CORS_ALLOWED_ORIGINS` (comma-separated, e.g., `https://your-app.pages.dev`)

**Deployment Flow:**
1. Push to `master` → GitHub Actions triggered
2. Lint & test code (frontend + backend)
3. Build frontend (Astro) → Upload artifacts
4. Deploy frontend to Cloudflare Pages (parallel)
5. Deploy backend to Render via API (parallel)
6. Verify deployment status
