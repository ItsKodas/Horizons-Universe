const fs = require('fs');

var torch_cfg = fs.readFileSync('torch.cfg', 'utf8');
var currentInstancePath = torch_cfg.split('<InstancePath>')[1].split('</InstancePath>')[0];
currentInstancePath = `<InstancePath>${currentInstancePath}</InstancePath>`;
console.log(`Old Instance Path: ${currentInstancePath}`);

newInstancePath = `<InstancePath>${__dirname}\\Instance</InstancePath>`;
console.log(`Updated Instance Path: ${newInstancePath}`);

fs.writeFileSync('torch.cfg', torch_cfg.replace(currentInstancePath, newInstancePath));
console.log('Torch.cfg has been written!')



var dedicated_cfg = fs.readFileSync('./Instance/SpaceEngineers-Dedicated.cfg', 'utf8');
var currentLoadWorld = dedicated_cfg.split('<LoadWorld>')[1].split('</LoadWorld>')[0];
currentLoadWorld = `<LoadWorld>${currentLoadWorld}</LoadWorld>`;
console.log(`Old World Path: ${currentLoadWorld}`);

newLoadWorld = `<LoadWorld>${__dirname}\\Instance\\Saves\\World</LoadWorld>`;
console.log(`Updated World Path: ${newLoadWorld}`);

fs.writeFileSync('./Instance/SpaceEngineers-Dedicated.cfg', dedicated_cfg.replace(currentLoadWorld, newLoadWorld));
console.log('SpaceEngineers-Dedicated.cfg has been written!')