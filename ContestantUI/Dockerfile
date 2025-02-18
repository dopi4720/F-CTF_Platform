# Stage 1: Build stage
FROM node:16 AS build

# Set working directory
WORKDIR /app

# Copy package files and install dependencies
COPY package*.json package-lock.json ./
RUN npm install

# Copy the rest of the application files
COPY . .

# Build the React application
RUN npm run build

EXPOSE 3000

# Start Nginx server
CMD ["node", "server.js"]
