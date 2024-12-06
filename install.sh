# Kiểm tra xem script có chạy với quyền sudo hay không
if [ "$EUID" -ne 0 ]; then
  echo "Plesae run with sudo permission!"
  exit 1
fi

#set umask
umask a+rx

#install tools
sudo apt install xxd wget curl netcat -y


#get docker install script
curl -fsSL https://get.docker.com -o get-docker.sh

#execute docker install script
sudo sh get-docker.sh

#set docker command execute without root
sudo usermod -aG docker $USER #&& newgrp docker

echo 'Install docker done, start install kctf'

echo 'kernel.unprivileged_userns_clone=1' | sudo tee -a /etc/sysctl.d/00-local-userns.conf

sudo service procps restart

sudo chmod -R 755 ctf-directory/kctf

source ctf-directory/kctf/activate

echo 'Install kctf done, start create Cluster'

kctf cluster create local-cluster --start --type kind

echo 'Install Cluster done'

sudo chmod -R 777 ./ManagementPlatform

docker compose --env-file ./ManagementPlatform/.env -f ./ManagementPlatform/docker-compose.yml up --force-recreate -d

########################################################### doan nay dang can nhac xem publish physic machine hay docker
sudo apt install -y wget apt-transport-https software-properties-common

wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb

sudo dpkg -i packages-microsoft-prod.deb

sudo apt update

#build Challenge Hosting Platform
dotnet publish ./ControlCenterAndChallengeHosting/ChallengeManagementServer
###########################################################