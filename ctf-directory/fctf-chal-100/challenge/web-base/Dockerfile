# Sử dụng image PHP chính thức với Apache
FROM php:8.1-apache

# Cài đặt các tiện ích cần thiết
RUN apt-get update && apt-get install -y \
    unzip \
    nano \
    && docker-php-ext-install mysqli \
    && a2enmod rewrite

# Thiết lập ServerName để tránh cảnh báo
RUN echo "ServerName localhost" >> /etc/apache2/apache2.conf

# Thiết lập thư mục làm việc
WORKDIR /var/www/html

# Sao chép mã nguồn của bài thi vào container
COPY . /var/www/html

# Đảm bảo quyền truy cập cho thư mục uploads
RUN mkdir -p /var/www/html/uploads && \
    chmod -R 777 /var/www/html/uploads

# Expose port 80 để truy cập ứng dụng
EXPOSE 80

# Khởi động Apache
CMD ["apache2-foreground"]
