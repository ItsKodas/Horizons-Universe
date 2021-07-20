@echo off

title Torch Updater

echo ----------Torch Update----------
C:/Applications/wget.exe https://build.torchapi.net/job/Torch/job/Torch/job/master/lastSuccessfulBuild/artifact/bin/torch-server.zip -O torch-server.zip
powershell Expand-Archive -Force torch-server.zip ./
del torch-server.zip

powershell move "Torch.Server.exe" "Trade.Server.exe" -force
powershell move "Torch.Server.exe.config" "Trade.Server.exe.config" -force
powershell move "Torch.Server.pdb" "Trade.Server.pdb" -force
powershell move "Torch.Server.xml" "Trade.Server.xml" -force

echo start Trade.Server.exe -autostart
exit