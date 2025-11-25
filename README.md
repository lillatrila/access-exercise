# Hotel Room Allocation System

A .NET 9.0 console application for allocating hotel rooms based on availability and guest requirements.

## Prerequisites

- **.NET 9.0 SDK** or later ([download](https://dotnet.microsoft.com/download))

## Getting Started

### 1. Install Dependencies

Dependencies are automatically restored when you build:

```bash
cd /path-to-repo
dotnet restore
```

Or skip this step—dependencies will be installed automatically during the build.

### 2. Build the Application

```bash
dotnet build
```

### 3. Run the Application

The application requires two data files as arguments:

```bash
cd myapp
dotnet run -- --hotels hotels.json --bookings bookings.json
```

Replace `hotels.json` and `bookings.json` with the correct file paths if they are located elsewhere. For now, the two files have been included in the repo - inspired by the examples in the test but with small modifications.

## Running Tests

Run all unit tests:

```bash
cd /path-to-repo
dotnet test myapp.Tests
```

Run a specific test class:

```bash
dotnet test myapp.Tests --filter "AllocationServiceTests"
```

Run tests with detailed output:

```bash
dotnet test myapp.Tests --verbosity detailed
```

The test suite includes 199 unit tests covering allocation logic, availability calculations, command parsing, and more.
