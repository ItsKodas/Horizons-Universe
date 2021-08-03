@echo off

title Torch Updater

echo ----------Torch Update----------
C:/Applications/wget.exe https://build.torchapi.net/job/Torch/job/Torch/job/master/lastSuccessfulBuild/artifact/bin/torch-server.zip -O torch-server.zip
powershell Expand-Archive -Force torch-server.zip ./
del torch-server.zip

start Torch.Server.exe -autostart
exit