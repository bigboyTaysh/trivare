# AI Rules for Trivare

## Project Description

**Trivare** is an MVP trip planning web application that helps users organize travel information, store documents, and discover attractions using AI and Google Places API.

### Core Features
- **Trip Management**: CRUD operations for trips with days, transport, accommodation, and attractions
- **AI-Powered Search**: Integration with OpenRouter.ai to filter and recommend places based on user preferences
- **Place Discovery**: Google Places API integration for automatic attraction suggestions
- **File Storage**: Upload and manage trip-related files (PDF, PNG, JPEG, max 5MB, 10 files per trip)
- **User Management**: JWT-based authentication, profile management, GDPR-compliant account deletion
- **Admin Panel**: Metrics dashboard tracking trips, searches, and AI usage (60-day window)

### Constraints
- Maximum 10 trips per user
- No trip sharing between users
- No mobile app or public API in MVP
- Admin panel restricted to admin role only

### Tech Stack

**Frontend (Client/)**
- Astro 5 - Modern web framework with islands architecture
- React 19 - Interactive UI components
- TypeScript 5 - Type safety
- Tailwind CSS 4 - Utility-first styling
- Shadcn/ui - Reusable React components

**Backend (Server/)**
- .NET 9 - REST API platform
- Azure SQL - Managed relational database
- Cloudflare R2 - File storage (no egress fees)
- OpenRouter.ai - LLM aggregator for AI features
- Google Places API - Place suggestions

**DevOps**
- GitHub Actions - CI/CD automation
- Azure - Cloud hosting for frontend, backend, and database

### Project Structure
```
Client/          # Astro + React frontend
  src/
    components/  # React/Astro components
    pages/       # Astro pages (routing)
    services/    # API integration
    layouts/     # Page layouts
    config/      # Configuration

Server/          # .NET 9 backend
  Api/           # ASP.NET Core Web API
  Application/   # Business logic layer
  Domain/        # Domain entities
  Infrastructure/# Data access & external services
```

The project follows Clean Architecture principles in the backend with clear separation between API, Application, Domain, and Infrastructure layers.

## CODING_PRACTICES

### Guidelines for SUPPORT_LEVEL

#### SUPPORT_EXPERT

- Favor elegant, maintainable solutions over verbose code. Assume understanding of language idioms and design patterns.
- Highlight potential performance implications and optimization opportunities in suggested code.
- Frame solutions within broader architectural contexts and suggest design alternatives when appropriate.
- Focus comments on 'why' not 'what' - assume code readability through well-named functions and variables.
- Proactively address edge cases, race conditions, and security considerations without being prompted.
- When debugging, provide targeted diagnostic approaches rather than shotgun solutions.
- Suggest comprehensive testing strategies rather than just example tests, including considerations for mocking, test organization, and coverage.


### Guidelines for VERSION_CONTROL

#### GITHUB

- Use pull request templates to standardize information provided for code reviews
- Configure required status checks to prevent merging code that fails tests or linting
- Use GitHub Actions for CI/CD workflows to automate testing and deployment
- Implement CODEOWNERS files to automatically assign reviewers based on code paths
- Use GitHub Projects for tracking work items and connecting them to code changes

#### GIT

- Use conventional commits to create meaningful commit messages
- Use feature branches with descriptive names following conventional commit types
- Write meaningful commit messages that explain why changes were made, not just what
- Keep commits focused on single logical changes to facilitate code review and bisection
- Use interactive rebase to clean up history before merging feature branches
- Leverage git hooks to enforce code quality checks before commits and pushes

#### CONVENTIONAL_COMMITS

- Follow the format: type(scope): description for all commit messages
- Use consistent types (feat, fix, docs, style, refactor, test, chore) across the project
- Define clear scopes based on {{project_modules}} to indicate affected areas
- Include issue references in commit messages to link changes to requirements
- Use breaking change footer (!: or BREAKING CHANGE:) to clearly mark incompatible changes
- Configure commitlint to automatically enforce conventional commit format



## DEVOPS

### Guidelines for CLOUD

#### AZURE

- Use Azure Resource Manager (ARM) templates or Bicep for infrastructure as code
- Implement Azure AD for authentication and authorization
- Use managed identities instead of service principals when possible

