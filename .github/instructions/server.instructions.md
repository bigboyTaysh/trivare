---
applyTo: 'Server/**'
---
## CODING_PRACTICES

### Guidelines for ARCHITECTURE

#### CLEAN_ARCHITECTURE

- Strictly separate code into layers: Infrastructure, Application, Domain, and API
- Ensure dependencies point inward, with inner layers having no knowledge of outer layers
- Implement domain entities that encapsulate business rules without framework dependencies
- Use interfaces (ports) and implementations (adapters) to isolate external dependencies
- Create use cases that orchestrate entity interactions for specific business operations
- Implement mappers to transform data between layers to maintain separation of concerns
- Do not use CQRS or Command-Query Separation pattern unless there is a clear benefit for complexity management

### Guidelines for DOCUMENTATION

#### SWAGGER

- Define comprehensive schemas for all request and response objects
- Use semantic versioning in API paths to maintain backward compatibility
- Implement detailed descriptions for endpoints, parameters, and domain-specific concepts
- Configure security schemes to document authentication and authorization requirements
- Use tags to group related endpoints by resource or functional area
- Implement examples for all endpoints to facilitate easier integration by consumers


## BACKEND

### Guidelines for DOTNET

#### ENTITY_FRAMEWORK

- Use the repository and unit of work patterns to abstract data access logic and simplify testing
- Implement eager loading with Include() to avoid N+1 query problems for entity relationships
- Use migrations for database schema changes and version control with proper naming conventions
- Apply appropriate tracking behavior (AsNoTracking() for read-only queries) to optimize performance
- Implement query optimization techniques like compiled queries for frequently executed database operations
- Use value conversions for complex property transformations and proper handling of custom data types

#### ASP_NET

- Use API controllers with model binding and validation attributes for complex data models
- Apply proper response caching with cache profiles and ETags for improved performance on high-traffic endpoints
- Implement proper exception handling with ExceptionFilter or middleware to provide consistent error responses
- Use dependency injection with scoped lifetime for request-specific services and singleton for stateless services

## DATABASE

### Guidelines for SQL

#### SQLSERVER

- Use parameterized queries to prevent SQL injection
- Implement proper indexing strategies based on query patterns
- Use stored procedures for complex business logic that requires database access to business entities
