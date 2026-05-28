# Cogfather

A Byzantine-fault-tolerant distributed manufacturing system built with ASP.NET Core 10, Blazor, RabbitMQ, and SQLite.

HQ issues production orders to a cluster of worker nodes. Results are evaluated by a BFT consensus engine (requires more than 2/3 agreement). The system supports fault injection, real-time monitoring, and node reputation tracking.

## Architecture

```
┌─────────────────────────┐        RabbitMQ (fanout)
│  Cogfather HQ (Blazor)  │ ──────────────────────────┐
│  • Order management     │                           │
│  • BFT consensus        │ ◄── reports (direct) ─────┤
│  • Inventory tracking   │                    ┌──────┴──────────────┐
│  • Node reputation      │                    │  Worker Node (×3)   │
│  • Real-time UI         │                    │  • Execute recipes  │
└─────────────────────────┘                    │  • Fault injection  │
                                               └─────────────────────┘
```

## Quick Start (local development)

### Prerequisites
- .NET 10 SDK
- Docker + Docker Compose

### Run with Docker Compose

```bash
# From the Cogfather/ directory
docker compose up --build

# HQ dashboard available at http://localhost:8080
# Default credentials: admin / Admin123!
```

### Run locally (for development)

```bash
# 1. Start RabbitMQ
docker compose up rabbitmq -d

# 2. Start HQ
dotnet run --project Cogfather.HQ.UI

# 3. Start workers (in separate terminals or background)
make run   # uses the Makefile if present
# or:
for i in 1 2 3; do
  NODE_ID="worker-$i" dotnet run --project Cogfather.Node.Worker &
done
```

## Running Tests

```bash
cd Cogfather
dotnet test

# With coverage report
dotnet test --collect:"XPlat Code Coverage" --results-directory coverage
```

Current coverage: **HQ.Application 96.7% · HQ.Domain 95.6% · Node.Application 80% · Node.Domain 85%**

## Configuration Reference

| Variable | Service | Description |
|---|---|---|
| `Node__Id` | Node | UUID for this worker node |
| `Node__DisplayName` | Node | Human-readable label shown in HQ |
| `RabbitMq__Host` | Both | RabbitMQ hostname |
| `RabbitMq__Port` | Both | AMQP port (default `5672`) |
| `RabbitMq__Username` | Both | RabbitMQ username |
| `RabbitMq__Password` | Both | RabbitMQ password |
| `Fault__ActiveFault` | Node | Fault mode integer (0=None, 1=DataManipulation, 2=SilentFailure, 3=HashTampering, 4=DelayedResponse) |
| `Fault__DelaySeconds` | Node | Delay in seconds for `DelayedResponse` mode |
| `ConnectionStrings__HqDb` | HQ | SQLite path for main DB |
| `ConnectionStrings__AuthDb` | HQ | SQLite path for auth DB |
| `RecipeBook__FilePath` | HQ | Path to `recipes.json` |
| `ASPNETCORE_ENVIRONMENT` | HQ | `Development` or `Production` |

## Fault Injection Modes

Fault modes are set per-node from the **Nodes** page in HQ and broadcast via RabbitMQ.

| Mode | Behaviour |
|---|---|
| `None` | Normal operation |
| `DataManipulation` | Node reports incorrect component IDs / amounts → Byzantine fault detected |
| `SilentFailure` | Node executes but sends no report → order may still reach quorum with remaining nodes |
| `HashTampering` | Node sends a corrupted manifest hash → flagged as Byzantine |
| `DelayedResponse` | Node delays its report by a configurable number of seconds |

## Project Structure

```
Cogfather/
├── Cogfather.Contracts/          # Shared message DTOs (RabbitMQ)
├── Cogfather.HQ.Domain/          # HQ domain model (entities, value objects, enums)
├── Cogfather.HQ.Application/     # CQRS commands, queries, consensus engine
├── Cogfather.HQ.Infrastructure/  # EF Core (SQLite), RabbitMQ, Serilog sinks
├── Cogfather.HQ.UI/              # Blazor Server UI + minimal API endpoints
├── Cogfather.Node.Domain/        # Node domain model + fault injection contracts
├── Cogfather.Node.Application/   # Production execution command handler
├── Cogfather.Node.Infrastructure/# RabbitMQ consumers/publishers
├── Cogfather.Node.Worker/        # Worker host entry point
├── Cogfather.HQ.Tests/           # xUnit tests for HQ layers
├── Cogfather.Node.Tests/         # xUnit tests for Node layers
└── docker-compose.yml            # Full-stack local deployment
```
