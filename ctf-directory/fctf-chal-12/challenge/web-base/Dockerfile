# Base image
FROM ubuntu:16.04

# Update package list and upgrade system
RUN apt update && apt upgrade -y

# Install Apache and PHP
RUN apt install -y \
    apache2 \
    php \
    libapache2-mod-php

# Configure Apache
RUN echo "ServerName localhost" >> /etc/apache2/apache2.conf

# Copy PHP source code to the web server root directory
COPY ./source /var/www/html

# Set correct permissions for the source code
RUN chown -R www-data:www-data /var/www/html && chmod -R 755 /var/www/html

# Expose port 80 for HTTP traffic
EXPOSE 80

# Start Apache in the foreground
CMD ["apachectl", "-D", "FOREGROUND"]
