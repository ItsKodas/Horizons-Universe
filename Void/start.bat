@echo off

title Torch Updater

echo ---------Updating Torch---------
C:/Applications/wget.exe https://build.torchapi.net/job/Torch/job/Torch/job/master/lastSuccessfulBuild/artifact/bin/torch-server.zip -O torch-server.zip
powershell Expand-Archive -Force torch-server.zip ./
del torch-server.zip

echo ---------Editing Files----------
node setdir.js
set local=%CD%
cd..
node sync.js
cd %local%

echo ---------Booting Server---------
Torch.Server.exe -autostart -nogui
exit