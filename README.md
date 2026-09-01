# Cogfather | Byzantine-Fault-Tolerant Manufacturing Orchestrator

A distributed manufacturing simulation in which a central headquarters decomposes recipe-driven production orders, dispatches them to independent worker nodes, and accepts output only after Byzantine-fault-tolerant consensus. The platform combines deterministic fault injection, node reputation, real-time supervision, and a complete observability stack.

## 📺 Demo & Operational Views
*Interactive visibility into production, consensus, and cluster health through the secured Blazor dashboard.*

### 🏭 Production Control
* **Dashboard:** Active-node counts, recent orders, completion status, inventory snapshots, and a live SignalR consensus stream.
* **Orders & Inventory:** Recipe-based order creation, per-order report inspection, nested component production, and centralized stock tracking.
* **System Logs:** A consolidated operational event feed populated by HQ and worker-node messages.

### 🛰️ Cluster Supervision & Fault Injection
* **Node Management:** Worker liveness, active fault state, Byzantine-fault count, and reputation score per node.
* **Topology:** A live graphical view of HQ-to-worker relationships, refreshed as nodes register and consensus events arrive.
* **Controlled Failure Modes:** Per-node activation of data manipulation, silent failure, hash tampering, inventory lying, and delayed responses.

## 🏗️ Architecture & Context
*High-level system design and execution model.*

* **Objective:** Demonstrate that a distributed production coordinator can continue making trustworthy decisions when part of its worker cluster returns incorrect, delayed, corrupted, or missing results.
* **Architecture Pattern:** Clean Architecture with separate HQ and worker bounded contexts. Domain and application layers remain independent of hosting and transport concerns; MediatR provides CQRS-style command/query dispatch, while infrastructure adapters supply RabbitMQ, SQLite, Identity, SignalR, and telemetry.
* **Data Flow:**
    1. **Discovery:** Workers publish periodic RabbitMQ heartbeats containing their identity, display name, timestamp, and inventory snapshot. HQ registers them as active nodes.
    2. **Planning:** HQ resolves the requested JSON recipe, recursively identifies craftable dependencies, consumes available central inventory, and creates any required sub-orders.
    3. **Distribution:** Each production order is published through a durable fanout exchange so every worker independently executes the same manifest.
    4. **Verification:** Workers return a production report and SHA-256 manifest hash. HQ independently reconstructs the canonical hash and converts malformed, tampered, or unsuccessful output into a failed vote.
    5. **Consensus:** Once reports from at least two thirds of active nodes have arrived, HQ evaluates the majority verdict, completes or fails the order, updates inventory, penalizes dissenting nodes, and broadcasts the result to the UI through SignalR.

## ⚖️ Design Decisions & Trade-offs
*Technical justifications for key distributed-system choices.*

* **Coordination: BFT Quorum over Single-Worker Trust**
    * **Context:** A production result must remain dependable even if an individual node behaves incorrectly or maliciously.
    * **Decision:** Dispatch identical work to the cluster and require agreement from at least two thirds of active workers before finalizing an order.
    * **Rationale:** Redundant execution turns worker output into independently comparable votes and permits outliers to be identified instead of silently accepted.
    * **Trade-off:** Every order consumes work on multiple nodes and completion waits for quorum, increasing compute and message volume in exchange for fault tolerance.

* **Messaging: RabbitMQ Fanout and Direct Exchanges**
    * **Context:** Production work must reach all replicas, while administrative fault commands must target exactly one worker.
    * **Decision:** Use durable fanout exchanges for orders and reports, a direct exchange keyed by node ID for fault control, and a dedicated queue for heartbeats.
    * **Rationale:** Exchange semantics encode the intended delivery topology and decouple HQ from worker process lifetimes; persistent messages and explicit acknowledgements improve delivery resilience.
    * **Trade-off:** Asynchronous delivery introduces duplicate/retry and ordering concerns and requires broker availability, retry policies, and correlation IDs throughout the workflow.

* **State: Persistent HQ with Ephemeral Worker Inventories**
    * **Context:** Orders, reports, reputation, authentication, and accepted inventory must survive HQ restarts, while worker state exists to simulate independent production replicas.
    * **Decision:** Persist authoritative HQ and Identity data in SQLite, but keep each worker's local inventory in memory.
    * **Rationale:** SQLite provides a lightweight transactional source of truth for the coordinator, while ephemeral nodes remain easy to scale, reset, and fault-test.
    * **Trade-off:** Restarted workers lose local inventory and SQLite constrains horizontal HQ scaling; this is appropriate for a deterministic simulation, not a globally distributed production deployment.

## 🧠 Engineering Challenges
*Analysis of non-trivial implementation problems and their resolutions.*

* **Challenge: Detecting Byzantine Reports Without Trusting Their Sender**
    * **Problem:** A worker can claim success while changing the component, quantity, or manifest hash, so transport-level delivery alone cannot establish correctness.
    * **Implementation:** Every worker hashes a canonical manifest representation with SHA-256. HQ recomputes that value from the received correlation ID, recipe ID, and quantity; invalid hashes, empty output, and insufficient-inventory results become negative votes. The consensus engine then identifies reports that disagree with the accepted verdict.
    * **Outcome:** Tampered results are rejected at the coordinator, attributed to their source node, and surfaced through both metrics and the administrative UI.

* **Challenge: Preserving Production Progress Under Missing or Delayed Replies**
    * **Problem:** Waiting for every registered worker would let one crashed or silent node block the entire manufacturing pipeline.
    * **Implementation:** A quorum collector calculates `ceil(2n/3)` from currently active nodes and triggers evaluation as soon as that many reports exist. RabbitMQ consumers use manual acknowledgements and retry policies, while each report is tied to its order by a correlation ID.
    * **Outcome:** A three-node deployment can finalize work after two consistent reports, allowing one node to fail silently without preventing progress.

* **Challenge: Planning Multi-Level Recipes Without Double-Consuming Inventory**
    * **Problem:** Finished products can depend on intermediate products, which may themselves require other recipes; cycles and partial stock make naive recursive dispatch unsafe.
    * **Implementation:** HQ recursively walks the recipe graph, calculates craft counts from output ratios, consumes existing inventory before scheduling dependencies, accumulates pending sub-orders, and guards the traversal with a processing set.
    * **Outcome:** Complex products are translated into a deterministic sequence of prerequisite orders while reusing available stock and avoiding recursive cycles.

* **Challenge: Turning Faults into Long-Term Trust Signals**
    * **Problem:** A single consensus verdict handles one order but does not communicate a worker's historical reliability.
    * **Implementation:** HQ records each dissenting node as Byzantine, increments its fault count, and reduces its reputation by 20 points to a floor of zero. The score is persisted, exported to Prometheus, and visualized in node and topology views.
    * **Outcome:** Operators can distinguish isolated failures from repeatedly untrustworthy nodes and observe degradation over time.

## 🛠️ Tech Stack & Ecosystem
* **Core:** .NET 10, C#, ASP.NET Core, Worker Services
* **Application Design:** Clean Architecture, MediatR, FluentValidation, CQRS-style commands and queries
* **Interface & Real-Time Updates:** Blazor Server, Bootstrap, SignalR
* **Messaging:** RabbitMQ with durable fanout/direct exchanges, manual acknowledgements, and Polly retry policies
* **Persistence & Security:** SQLite, Entity Framework Core, ASP.NET Core Identity, TOTP, CAPTCHA, secure cookies, and hardened response headers
* **Observability:** Serilog, Prometheus, Grafana, and Loki
* **Infrastructure:** Docker, Docker Compose, GitLab CI/CD

## 🧪 Quality & Standards
* **Testing Strategy:**
    * **HQ:** xUnit coverage of consensus, quorum collection, recursive order issuance, report handling, inventory, registration, validation, queries, and fault-control commands.
    * **Workers:** xUnit coverage of production execution, fault injection, manifest hashing, and local inventory behavior.
    * **Verified Baseline:** 119 automated tests pass across the HQ and worker suites.
* **Automated Quality Gates:** GitLab CI verifies formatting, performs Release builds, runs tests with Cobertura and JUnit reporting, and executes SonarQube analysis on merge requests and the default branch.
* **Operational Standards:** Health checks cover both SQLite contexts and RabbitMQ; Prometheus exposes order, consensus, heartbeat, reputation, fault, and inventory metrics; structured logs are shipped to Loki.
* **Engineering Principles:** Dependency inversion between domain/application code and infrastructure, immutable shared transport contracts, explicit message acknowledgement, observable failure handling, and reproducible containerized deployment.

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
