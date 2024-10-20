## 0.0.1
API has two endpoints each be able to deal with post requests entailing authentication and registration. 

## 0.0.2
API now uses API Keys for authentication, which is done by using middleware. A unnecessary assembly was removed from the DatabaseHelper.cs. A method was created in the Authentication.cs to generate API Keys. This method is currently unused but will be used in the future. 

## 0.0.3
Token Bucket rate limiting has been configured in the program.cs. Changes has been made to various files to support this. Currently the rate limiting has only been applied to the Authentication endpoint. 

## 0.0.4
Token bucket configuration readability and maintainability has been improved. Also one the rate limit has been succeeded the correct error code is returned. 

## 0.0.5
Various changes have been made to improve code readability and maintainability. Also error handling has been added to various methods. 

## 0.0.6
Dependency Injection is now used to query the database. The relevant scripts have been updated to support this. 

## 0.0.7
API key header has been updated. 

## 0.1.0
Various additional methods, classes and scripts have been created to support JWT Tokens for the API. Also multiple methods have been redone to improve efficiency. Authentication endpoint has changed due to the JWT changes. Additional validations have been added to models. Environments variables are able to located through the configuration file.

## 0.1.1
Removed the .http file due to avoid a potential leak if sensitive information is not removed before pushing to repo. Correct changes suggested by CodeQL on Github such as preventing logging injection. As well providing safeguards to user controlled methods. Added a Misc.cs script to store various utility methods.

## 0.1.2
Corrected additional changes from CodeQL, refresh tokens aren't provided as plaintext direct in the json response. Instead refresh token are now provided in a cookie.  

## 0.1.3
### Changed
- Created a Dockerfile to utilize a multi-stage build for the TMS_API application.
- Introduced a build stage using the .NET SDK image (mcr.microsoft.com/dotnet/sdk:8.0) for restoring dependencies and publishing the application.
- Added a runtime stage using the .NET ASP.NET image (mcr.microsoft.com/dotnet/aspnet:8.0) to run the application.
- Installed the New Relic agent during the runtime stage to monitor application performance.
- Configured environment variables for New Relic profiling and application binding.

### Notes
- The multi-stage build optimizes image size by separating build and runtime dependencies.
- Installing the New Relic agent allows for better monitoring and performance tracking of the application.
- This update enables application performance monitoring.
- The DockerFile won't be used in a production environment due to secrets being passed in ARG.

## 0.1.4
### Added
- A .dockerignore file was created to specify which file types and directories should be excluded during the Docker build process. This helps to streamline the build by preventing unnecessary files from being included, ultimately reducing the image size and improving build times.

###  Fixed
- Resolved SQL Server connectivity issues by establishing a network bridge, ensuring that the necessary communication channels are available between the API and SQL Server. Additionally, the .env file was excluded from the build process to maintain security and avoid exposing sensitive configurations.
-  Updated the URL from "http://localhost:5188" to "http://*:5188" to facilitate port binding for the Docker container, allowing it to accept requests from external sources rather than just localhost.
- Modified the loading mechanism for the .env file, ensuring it is only loaded when the API__KEY environment variable is not found. This change aims to streamline configuration management, with plans for a more dynamic and flexible solution in a future update.

### Removed
- The New Relic agent was removed from the Dockerfile.

### Notes
- The new relic agent was removed to facilitate easier debugging and manual performance improvements, rather than depending on third-party software.

## 0.1.5
### Added
-  Added Endpoint Performance Metrics section to the [README](README.MD)

### Notes
- Endpoint Performance Metric was added to track and monitor performance for each endpoints. Additional changes are expected to added in the future. 