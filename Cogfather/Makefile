.PHONY: build run ui worker workers stop clean

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
	@rm -f .worker-pids
	@for i in $$(seq 1 $(N)); do \
		dotnet run --no-build --project $(WKR) > logs/worker-$$i.log 2>&1 & echo $$! >> .worker-pids; \
	done
	@echo "$(N) workers started in background. Logs: ./logs/worker-N.log"

run: build
	@mkdir -p logs
	@rm -f .worker-pids
	@for i in $$(seq 1 $(N)); do \
		dotnet run --no-build --project $(WKR) > logs/worker-$$i.log 2>&1 & echo $$! >> .worker-pids; \
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

clean:
	dotnet clean $(SLN)
