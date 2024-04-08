#!/bin/bash

ARCHIVE_NAME="SoftwareAgentService.tar.gz"
SERVICE_FILE="SoftwareAgentService.service"

INSTALL_PATH="/opt/SoftwareAgentService"

mkdir -p $INSTALL_PATH

tar -xzvf ./$ARCHIVE_NAME -C $INSTALL_PATH

sudo cp $INSTALL_PATH/$SERVICE_FILE /etc/systemd/system/

sudo systemctl daemon-reload

sudo systemctl enable SoftwareAgentService
sudo systemctl start SoftwareAgentService

echo "Installed."

# How to use
#chmod +x install_script.sh
#sudo ./install_script.sh