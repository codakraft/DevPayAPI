#!/bin/bash

# Docker Management Script for LendingSolution MSSQL Database
echo "🚀 LendingSolution Docker Management Script"
echo "============================================"

case "$1" in
    "start")
        echo "📦 Starting MSSQL Docker container..."
        cd LendingSolution.API && docker-compose up -d
        echo "⏳ Waiting for SQL Server to start up..."
        sleep 10
        echo "✅ MSSQL container started successfully!"
        echo "🔗 Connection details:"
        echo "   Server: localhost,1433"
        echo "   Database: LendingDev"
        echo "   Username: sa"
        echo "   Password: YourStrong!Passw0rd"
        ;;
    "stop")
        echo "🛑 Stopping MSSQL Docker container..."
        cd LendingSolution.API && docker-compose down
        echo "✅ MSSQL container stopped successfully!"
        ;;
    "restart")
        echo "🔄 Restarting MSSQL Docker container..."
        cd LendingSolution.API && docker-compose down && docker-compose up -d
        echo "⏳ Waiting for SQL Server to start up..."
        sleep 10
        echo "✅ MSSQL container restarted successfully!"
        ;;
    "logs")
        echo "📋 Showing MSSQL container logs..."
        docker logs lending_sqlserver_db --tail 20
        ;;
    "status")
        echo "📊 Docker container status..."
        docker ps --filter "name=lending_sqlserver_db"
        ;;
    "connect")
        echo "🔌 Connecting to MSSQL (requires sqlcmd)..."
        echo "If you don't have sqlcmd installed, install it with:"
        echo "  brew install sqlcmd"
        echo ""
        echo "Connecting to database..."
        sqlcmd -S localhost,1433 -U sa -P 'YourStrong!Passw0rd' -d LendingDev
        ;;
    "migrate")
        echo "🗃️ Running Entity Framework migrations..."
        dotnet ef database update --project LendingSolution.Infrastructure --startup-project LendingSolution.API
        echo "✅ Migrations completed!"
        ;;
    "reset")
        echo "⚠️  This will DELETE ALL DATA and recreate the database!"
        read -p "Are you sure? (y/N): " -n 1 -r
        echo ""
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            echo "🗑️ Dropping database and recreating..."
            dotnet ef database drop --project LendingSolution.Infrastructure --startup-project LendingSolution.API --force
            dotnet ef database update --project LendingSolution.Infrastructure --startup-project LendingSolution.API
            echo "✅ Database reset completed!"
        else
            echo "❌ Operation cancelled."
        fi
        ;;
    *)
        echo "Usage: ./docker-manage.sh [command]"
        echo ""
        echo "Available commands:"
        echo "  start     - Start the MSSQL Docker container"
        echo "  stop      - Stop the MSSQL Docker container"
        echo "  restart   - Restart the MSSQL Docker container"
        echo "  logs      - Show container logs"
        echo "  status    - Show container status"
        echo "  connect   - Connect to MSSQL database (requires sqlcmd)"
        echo "  migrate   - Run Entity Framework migrations"
        echo "  reset     - Reset database (WARNING: Deletes all data!)"
        echo ""
        echo "Examples:"
        echo "  ./docker-manage.sh start"
        echo "  ./docker-manage.sh logs"
        echo "  ./docker-manage.sh migrate"
        ;;
esac
