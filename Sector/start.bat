@echo off

title Torch Updater

echo ----------Torch Update----------
C:/Applications/wget.exe https://build.torchapi.net/job/Torch/job/Torch/job/master/lastSuccessfulBuild/artifact/bin/torch-server.zip -O torch-server.zip
powershell Expand-Archive -Force torch-server.zip ./
del torch-server.zip

powershell move "Torch.Server.exe" "Sector.Server.exe" -force
powershell move "Torch.Server.exe.config" "Sector.Server.exe.config" -force
powershell move "Torch.Server.pdb" "Sector.Server.pdb" -force
powershell move "Torch.Server.xml" "Sector.Server.xml" -force

start Sector.Server.exe -autostart
exit