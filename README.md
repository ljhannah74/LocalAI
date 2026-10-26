# LocalAI Text Analyzer

A full-stack AI-powered text analysis application built with **React, TypeScript, ASP.NET Core 10, and Ollama**.

LocalAI performs sentiment analysis, named entity extraction, and potential personally identifiable information (PII) detection using a locally hosted language model.

The application runs without requiring Microsoft Azure or a paid cloud AI API.

## Features

- Sentiment analysis: Positive, Negative, Neutral, or Mixed
- Named entity extraction
- Potential PII identification
- Structured JSON responses using Ollama's JSON schema support
- REST API built with ASP.NET Core 10
- React and TypeScript frontend
- Dependency injection and strongly typed configuration
- Error handling for unavailable models, invalid responses, and timeouts
- Automated unit and API integration tests

## Technology Stack

| Component | Technology |
|---|---|
| Frontend | React, TypeScript, Vite |
| Backend | ASP.NET Core 10 |
| Shared library | C# / .NET 10 |
| AI runtime | Ollama |
| Default model | Llama 3.2 1B |
| Testing | xUnit, WebApplicationFactory |
| Operating system | Developed on Linux Mint |

## Architecture

```text
React + TypeScript
        |
        v
ASP.NET Core Web API
        |
        v
LocalAI.Core
        |
        v
Ollama HTTP API
        |
        v
Llama 3.2 1B
```

The frontend sends text to the Web API, which uses a reusable service interface to communicate with Ollama. The AI response is deserialized into a strongly typed C# model and returned to the frontend.

## Project Structure

```text
LocalAI/
├── LocalAI.Api/
├── LocalAI.Api.Tests/
├── LocalAI.Core/
├── LocalAI.Core.Tests/
├── localai-web/
├── Program.cs
├── LocalAI.csproj
└── LocalAI.sln
```

The root project is a console application for testing the AI integration independently of the Web API.

## Prerequisites

- .NET 10 SDK
- Node.js compatible with the installed Vite version
- npm
- Ollama
- Llama 3.2 1B model

## Getting Started

### 1. Install Ollama

Follow the installation instructions at https://ollama.com.

Download the default model:

```bash
ollama pull llama3.2:1b
```

Ensure the Ollama service is running.

### 2. Configure the API

The API reads its Ollama settings from `LocalAI.Api/appsettings.json`.

```json
{
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "Model": "llama3.2:1b",
    "TimeoutSeconds": 180
  }
}
```

The model name can be changed to another model installed in Ollama.

### 3. Start the API

From the solution directory:

```bash
dotnet run --project LocalAI.Api --urls http://localhost:5000
```

### 4. Start the React frontend

In another terminal:

```bash
cd localai-web
npm install
npm run dev
```

Open the local address displayed by Vite, typically http://localhost:5173.

### 5. Analyze text

Enter text into the application and select **Analyze Text**.

The application displays the detected sentiment, named entities, and potential PII.

## API

### POST `/api/textanalysis`

Example request:

```json
{
  "text": "I ordered a laptop from Contoso and it arrived damaged."
}
```

Example response:

```json
{
  "sentiment": "Negative",
  "entities": ["Contoso"],
  "pii": []
}
```

Results depend on the language model and may vary between requests.

## Automated Testing

Run the complete .NET test suite:

```bash
dotnet test
```

The solution includes:

- Unit tests for the Ollama integration service
- API integration tests using `WebApplicationFactory`
- Tests for invalid input, malformed AI responses, missing models, and unavailable services

Tests use simulated AI responses and do not require Ollama to be running.

## Limitations

LocalAI is an educational and portfolio project.

- AI-generated classifications may be inaccurate.
- Potential PII detection is not guaranteed to identify all sensitive information.
- The default 1B-parameter model prioritizes low hardware requirements over analytical accuracy.
- Local inference performance depends on available CPU, memory, and GPU resources.
- The application is intended for local development and is not yet hardened for public deployment.

Do not rely on this application as the sole method of detecting or removing sensitive information.

## Future Improvements

- Automated GitHub Actions CI
- Additional AI models and provider selection
- Document summarization
- Improved response validation
- Analysis history
- Embeddings and retrieval-augmented generation (RAG)
- Enhanced frontend design and accessibility

## Purpose

LocalAI demonstrates how to integrate locally hosted generative AI into a modern full-stack application using established software engineering practices, including dependency injection, configuration management, REST APIs, and automated testing.