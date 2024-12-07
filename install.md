
#### **1. Thiết lập `umask`**

`umask` được sử dụng để đặt quyền truy cập mặc định cho các file/directory.

**Thực hiện lệnh sau:**

```bash
umask a+rx
```

----------

#### **2. Cài đặt các công cụ cần thiết**

**Cài đặt  xxd, wget, curl, netcat**
	
	```bash
	sudo apt install xxd wget curl netcat -y
	```

----------

#### **3. Cài đặt Docker**

1.  **Tải script cài đặt Docker:**
    
    ```bash
    curl -fsSL https://get.docker.com -o get-docker.sh
    ```
    
2.  **Chạy script để cài đặt Docker:**
    
    ```bash
    sudo sh get-docker.sh
    ```
    
3.  **Thiết lập để Docker hoạt động không cần quyền `root`:**
    
    ```bash
    sudo usermod -aG docker $USER && newgrp docker
    ```
   
    
4.  **Kiểm tra cài đặt Docker:**
    
    ```bash
    echo 'Install docker done, start install kctf'
    
    ```
    

----------

#### **4. Cài đặt kCTF**

1.  **Kích hoạt `kernel.unprivileged_userns_clone`:**
    
    ```bash
    echo 'kernel.unprivileged_userns_clone=1' | sudo tee -a /etc/sysctl.d/00-local-userns.conf
    sudo service procps restart
    ```
    
2.  **Chỉnh sửa quyền cho thư mục `kctf`:**
    
    ```bash
    sudo chmod -R 755 ctf-directory/kctf
    ```
    
3.  **Kích hoạt môi trường kCTF:**
    
    ```bash
    source ctf-directory/kctf/activate
    ```
    
4.  **Tạo cluster kCTF:**
    
    ```bash
    kctf cluster create local-cluster --start --type kind
    ```
    
5.  **Deactive môi trường kCTF**
    
    ```bash
    deactivate
    
    ```
    

----------

#### **5. Install hệ thống quản lý cuộc thi (Management Platform)**

Hệ thống quản lý thách thức và nền tảng kCTF.

1.  **Cấp quyền thư mục và cd vào `ManagementPlatform`:**
    
    ```bash
    sudo chmod -R 777 ./ManagementPlatform
    cd ManagementPlatform
    ```
    
2.  **Khởi chạy hệ thống bằng Docker Compose:**
    
    ```bash
    docker compose --env-file ./ManagementPlatform/.env up --force-recreate -d
    ```
    

----------

#### **7. Cài đặt net 8.0 sdk**

**Dành cho cấu hình bổ sung hoặc sử dụng trực tiếp trên máy vật lý:**

1.  **Cài đặt các gói phụ trợ:**
    
    ``` bash
    sudo snap install dotnet-sdk --classic
    ```
  2. **Kiểm tra dotnet đã cài thành công hay chưa** 
	  ``` bash
	  donet --version
	 ```
	  Nếu lệnh trên trả về version của dotnet đã được cài đặt là 8.x.x thì đã cài đặt dotnet SDK thành công

#### 8. Build Challenge Hosting Platform:
    
    ```bash
    dotnet publish ./ControlCenterAndChallengeHosting/ChallengeManagementServer
    ```
    

----------