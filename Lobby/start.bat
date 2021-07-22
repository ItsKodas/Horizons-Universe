@echo off

title Torch Updater

echo ----------Torch Update----------
C:/Applications/wget.exe https://build.torchapi.net/job/Torch/job/Torch/job/master/lastSuccessfulBuild/artifact/bin/torch-server.zip -O torch-server.zip
powershell Expand-Archive -Force torch-server.zip ./
del torch-server.zip

echo powershell move "Torch.Server.exe" "Lobby.Server.exe" -force
echo powershell move "Torch.Server.exe.config" "Lobby.Server.exe.config" -force
echo powershell move "Torch.Server.pdb" "Lobby.Server.pdb" -force
echo powershell move "Torch.Server.xml" "Lobby.Server.xml" -force

start Torch.Server.exe -autostart
exit