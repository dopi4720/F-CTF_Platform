# Use the official PostgreSQL image as the base
FROM postgres:latest

# Set environment variables for PostgreSQL
ENV POSTGRES_DB=postgres \
    POSTGRES_USER=postgres \
    POSTGRES_PASSWORD=postgres

# Expose PostgreSQL default port
EXPOSE 5432

# Define a volume for data persistence
VOLUME ["/var/lib/postgresql/data"]

# Optionally, you can add initialization scripts here (if needed)
# Example:
# COPY init.sql /docker-entrypoint-initdb.d/

# Default command
CMD ["postgres"]
