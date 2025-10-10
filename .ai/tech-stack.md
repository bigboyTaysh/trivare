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

-   **GitHub Actions:** A tool for automating CI/CD (Continuous Integration/Continuous Deployment) processes. Used for automatically building, testing, and deploying the application after every code change.
-   **Azure:** A cloud platform from Microsoft. It is used to host both the frontend and backend of the application, as well as the database, ensuring scalability, reliability, and security.
