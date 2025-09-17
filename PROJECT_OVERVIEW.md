# Project Overview

## Purpose
This solution demonstrates modern .NET file processing with async/streaming, robust error handling, and comprehensive testing. It is suitable for production scenarios requiring scalable and reliable file operations.

## Structure
- **FileProcessorApp/**: Console app entry point
- **FileProcessorLib/**: Library with `FileProcessor` class
- **FileProcessorTests/**: xUnit test suite (27 tests)
- **.gitignore**: Ignores build artifacts and IDE files

## Technology Stack
- .NET 7.0
- xUnit for testing

## Key Features
- Async/await and streaming for file operations
- Input validation and custom exceptions
- Comprehensive unit tests (edge/corner cases)
- Proper .gitignore for repository hygiene

## Build & Test Instructions
1. **Restore dependencies:**
   ```powershell
   dotnet restore
   ```
2. **Build solution:**
   ```powershell
   dotnet build
   ```
3. **Run tests:**
   ```powershell
   dotnet test
   ```

## Code Review Findings & Improvements
- Synchronous file IO replaced with async/streaming for scalability
- Added input validation and custom exceptions
- Expanded test coverage to 27 cases, including edge scenarios
- Improved repository hygiene with .gitignore

## Git Workflow Guidance
- Commit only source code and essential config
- Exclude build/output files using .gitignore
- Use feature branches for changes, merge via pull requests
- Run tests before pushing changes

---

For questions or improvements, see inline code comments and test documentation.
