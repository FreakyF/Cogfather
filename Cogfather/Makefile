.PHONY: build run run-local ui worker workers stop down down-clean clean rabbit rabbit-stop

SLN := Cogfather.sln
HQ  := Cogfather.HQ.UI
WKR := Cogfather.Node.Worker
N   := 6

build:
	dotnet build $(SLN) -c Debug

ui: build
	dotnet run --no-build --project $(HQ)

worker: build
	dotnet run --no-build --project $(WKR)

workers: build
	@mkdir -p logs
	@if [ -f .worker-pids ]; then xargs kill 2>/dev/null < .worker-pids || true; rm -f .worker-pids; fi
	@for i in $$(seq 1 $(N)); do \
		NodeSettings__NodeId="worker-$$i" dotnet run --no-build --project $(WKR) > logs/worker-$$i.log 2>&1 & echo $$! >> .worker-pids; \
	done
	@echo "$(N) workers started in background. Logs: ./logs/worker-N.log"

# Pełny stack w Dockerze: HQ, nody, RabbitMQ, Prometheus, Loki, Grafana
run:
	docker compose up --build

# Lokalny tryb deweloperski bez Dockera (dotnet + lokalne workery)
run-local: build
	@mkdir -p logs
	@if [ -f .worker-pids ]; then xargs kill 2>/dev/null < .worker-pids || true; rm -f .worker-pids; fi
	@for i in $$(seq 1 $(N)); do \
		NodeSettings__NodeId="worker-$$i" dotnet run --no-build --project $(WKR) > logs/worker-$$i.log 2>&1 & echo $$! >> .worker-pids; \
	done
	@echo "$(N) workers started in background. Logs: ./logs/worker-N.log"
	dotnet run --no-build --project $(HQ)

stop:
	@if [ -f .worker-pids ]; then \
		xargs kill 2>/dev/null < .worker-pids || true; \
		rm -f .worker-pids; \
		echo "Workers stopped."; \
	else \
		echo "No worker PIDs on record."; \
	fi

# Zatrzymaj Docker stack
down:
	docker compose down

# Zatrzymaj Docker stack i usuń volumes (czysta baza)
down-clean:
	docker compose down -v

rabbit:
	docker run -d --name cogfather-rabbitmq \
		-p 5672:5672 -p 15672:15672 \
		-e RABBITMQ_DEFAULT_USER=guest \
		-e RABBITMQ_DEFAULT_PASS=guest \
		rabbitmq:4-management-alpine

rabbit-stop:
	docker rm -f cogfather-rabbitmq

clean:
	dotnet clean $(SLN)
