@echo off

title Torch Updater

echo ----------Torch Update----------
C:/Applications/wget.exe https://build.torchapi.net/job/Torch/job/Torch/job/master/lastSuccessfulBuild/artifact/bin/torch-server.zip -O torch-server.zip
powershell Expand-Archive -Force torch-server.zip ./
del torch-server.zip

powershell move "Torch.Server.exe" "Sol.Server.exe" -force
powershell move "Torch.Server.exe.config" "Sol.Server.exe.config" -force
powershell move "Torch.Server.pdb" "Sol.Server.pdb" -force
powershell move "Torch.Server.xml" "Sol.Server.xml" -force

start Sol.Server.exe -autostart
exit