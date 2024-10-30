# Postgres Docker

```powershell

docker run --rm -it `
    -p 5432:5432 `
    -e POSTGRES_PASSWORD=AbC123! `
    postgres:latest

```

## TimescaleDB Docker

```powershell

docker run --rm -it `
    -p 5432:5432 `
    -e POSTGRES_PASSWORD=AbC123! `
    timescale/timescaledb:latest-pg16

```
