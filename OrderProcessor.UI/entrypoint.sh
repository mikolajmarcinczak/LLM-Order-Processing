#!/bin/bash

set -e

# Set all necessary environment variables for EF migrations
export MYSQL_SERVER=mysql
export MYSQL_PORT=3306
export MYSQL_DATABASE=${MYSQL_DATABASE}
export MYSQL_USERNAME=root
export MYSQL_ROOT_PASSWORD=${MYSQL_ROOT_PASSWORD}

# Set the connection string from environment variables
export ConnectionStrings__DefaultConnection="Server=mysql;Port=3306;Database=${MYSQL_DATABASE};Uid=root;Pwd=${MYSQL_ROOT_PASSWORD};"

echo "Waiting for MySQL to be ready..."
echo "Connection string: Server=mysql;Port=3306;Database=${MYSQL_DATABASE};Uid=root;Pwd=***;"

# Wait for MySQL to be ready, then apply migrations
until /root/.dotnet/tools/dotnet-ef database update --project /app/OrderProcessor.Infrastructure --startup-project /app/OrderProcessor.UI;
do
    echo "Waiting for database to come online..."
    sleep 5
done

echo "Database migrations applied. Starting application..."

# Execute the application
exec dotnet OrderProcessor.UI.dll 