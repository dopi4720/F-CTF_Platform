FROM python:3.12-alpine

# Install packages
RUN apk update

# Upgrade pip
RUN python -m pip install --upgrade pip

# Set environment variables
ENV PYTHONUNBUFFERED=1

# Copy the source code into the container
COPY ./source /app/

# Set the working directory in the container
WORKDIR /app

# Copy the requirements file and install dependencies
RUN pip install --no-cache-dir -r requirements.txt
RUN pip install psycopg2-binary

# Adjust permissions as needed
RUN chmod -R 755 /app/*

# Command to run the Python application
CMD ["python", "server.py"]
