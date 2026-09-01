# Cogfather | Byzantine-Fault-Tolerant Manufacturing Orchestrator

A distributed manufacturing simulation where HQ dispatches recipe-based orders to worker nodes and accepts results through Byzantine-fault-tolerant consensus.

## 📺 Demo & Operational Views

The secured Blazor dashboard provides:

* Order creation, report inspection, and inventory tracking
* Live node status, reputation, and network topology
* Real-time consensus events and distributed system logs
* Per-node fault injection: data manipulation, silent failure, hash tampering, inventory lying, and delayed responses

## 🏗️ Architecture & Context

* **Objective:** Maintain trustworthy production decisions despite incorrect, delayed, corrupted, or missing worker responses.
* **Architecture:** Clean Architecture with separate HQ and worker contexts, CQRS-style handling through MediatR, and infrastructure adapters for RabbitMQ, SQLite, SignalR, Identity, and telemetry.
* **Data Flow:**
    1. Workers register through periodic RabbitMQ heartbeats.
    2. HQ resolves recipes and recursively schedules required components.
    3. Orders are broadcast to all workers for independent execution.
    4. HQ verifies returned SHA-256 manifest hashes.
    5. A two-thirds quorum determines the result, updates inventory, and penalizes dissenting nodes.

## ⚖️ Design Decisions & Trade-offs

* **BFT Consensus:** Redundant execution detects dishonest workers and tolerates missing responses, at the cost of additional computation and messaging.
* **RabbitMQ:** Fanout exchanges distribute orders and reports, while direct routing targets individual nodes with fault commands. This improves decoupling but requires broker availability and retry handling.
* **Hybrid Persistence:** HQ data is stored in SQLite, while worker inventory remains in memory for easy simulation and reset. Worker state is therefore lost after restart.

## 🧠 Engineering Challenges

* **Byzantine Detection:** HQ independently verifies report hashes and treats malformed or unsuccessful output as a negative vote.
* **Quorum Availability:** `ceil(2n/3)` reports are sufficient, allowing a three-node cluster to progress if one worker remains silent.
* **Complex Recipes:** Recursive planning reuses existing inventory, schedules missing subcomponents, and prevents recipe cycles.
* **Reputation Tracking:** Confirmed Byzantine behavior reduces a node's persistent reputation score and is exposed through the UI and Prometheus.

## 🛠️ Tech Stack & Ecosystem

* **Core:** .NET 10, ASP.NET Core, Worker Services, MediatR
* **UI:** Blazor Server, Bootstrap, SignalR
* **Messaging:** RabbitMQ, Polly
* **Persistence & Security:** SQLite, Entity Framework Core, ASP.NET Core Identity, TOTP, CAPTCHA
* **Observability:** Serilog, Prometheus, Grafana, Loki
* **Infrastructure:** Docker Compose, GitLab CI/CD, SonarQube

## 🧪 Quality & Standards

* 119 passing xUnit tests across HQ and worker projects
* CI checks for formatting, Release builds, tests, coverage, and SonarQube analysis
* Health checks for SQLite and RabbitMQ with structured logging and Prometheus metrics

## 🙋‍♂️ Authors

**Kamil Fudala**

- [GitHub](https://github.com/FreakyF)
- [LinkedIn](https://www.linkedin.com/in/kamil-fudala/)

**Jan Chojnacki**

- [GitHub](https://github.com/Jan-Chojnacki)
- [LinkedIn](https://www.linkedin.com/in/jan-chojnacki-772b0530a/)

**Jakub Babiarski**

- [GitHub](https://github.com/JakubKross)
- [LinkedIn](https://www.linkedin.com/in/jakub-babiarski-751611304/)

## ⚖️ License

This project is licensed under the [MIT License](LICENSE).
