const fs = require('fs');
var servers = require('./servers.json');

for (server in servers) {
    var name = server;
    var server = servers[server];

    var source = {
        "torch": {},
        "dedicated": {},
        "sandbox_config": {},
        "sandbox": {}
    };

    var local = {
        "torch": {},
        "dedicated": {},
        "sandbox_config": {},
        "sandbox": {}
    };


    //! Source Files
    source.dedicated['file'] = fs.readFileSync(`${server.source}/Instance/SpaceEngineers-Dedicated.cfg`, 'utf8');
    source.sandbox_config['file'] = fs.readFileSync(`${server.source}/Instance/Saves/World/Sandbox_config.sbc`, 'utf8');
    source.sandbox['file'] = fs.readFileSync(`${server.source}/Instance/Saves/World/Sandbox.sbc`, 'utf8');
    
    source.dedicated['settings'] = source.dedicated['file'].split('<SessionSettings>')[1].split('</SessionSettings>')[0];
    source.sandbox_config['mods'] = source.sandbox_config['file'].split('<Mods>')[1].split('</Mods>')[0];



    //! Local Files
    local.torch['file'] = fs.readFileSync(`${server.local}/torch.cfg`, 'utf8');
    local.dedicated['file'] = fs.readFileSync(`${server.local}/Instance/SpaceEngineers-Dedicated.cfg`, 'utf8');
    local.sandbox_config['file'] = fs.readFileSync(`${server.local}/Instance/Saves/World/Sandbox_config.sbc`, 'utf8');
    local.sandbox['file'] = fs.readFileSync(`${server.local}/Instance/Saves/World/Sandbox.sbc`, 'utf8');

    local.torch['title'] = local.torch['file'].split('<InstanceName>')[1].split('</InstanceName>')[0]
    local['port'] = local.dedicated['file'].split('<ServerPort>')[1].split('</ServerPort>')[0];
    local.dedicated['settings'] = local.dedicated['file'].split('<SessionSettings>')[1].split('</SessionSettings>')[0];
    local.sandbox_config['settings'] = local.sandbox_config['file'].split('<Settings xsi:type="MyObjectBuilder_SessionSettings">')[1].split('</Settings>')[0];
    local.sandbox['settings'] = local.sandbox['file'].split('<Settings xsi:type="MyObjectBuilder_SessionSettings">')[1].split('</Settings>')[0];
    local.sandbox_config['mods'] = local.sandbox_config['file'].split('<Mods>')[1].split('</Mods>')[0];
    local.sandbox['mods'] = local.sandbox['file'].split('<Mods>')[1].split('</Mods>')[0];



    //! Instance Title
    local.torch['file'] = local.torch['file'].replace(`<InstanceName>${local.torch['title']}</InstanceName>`, `<InstanceName>${name}</InstanceName>`);

    //! Server Port
    local.dedicated['file'] = local.dedicated['file'].replace(`<ServerPort>${local['port']}</ServerPort>`, `<ServerPort>${server.port}</ServerPort>`);
    
    //! Server Configurations
    local.dedicated['file'] = local.dedicated['file'].replace(`<SessionSettings>${local.dedicated['settings']}</SessionSettings>`, `<SessionSettings>${source.dedicated['settings']}</SessionSettings>`);
    local.sandbox_config['file'] = local.sandbox_config['file'].replace(`<Settings xsi:type="MyObjectBuilder_SessionSettings">${local.sandbox_config['settings']}</Settings>`, `<Settings xsi:type="MyObjectBuilder_SessionSettings">${source.dedicated['settings']}</Settings>`);
    local.sandbox['file'] = local.sandbox['file'].replace(`<Settings xsi:type="MyObjectBuilder_SessionSettings">${local.sandbox['settings']}</Settings>`, `<Settings xsi:type="MyObjectBuilder_SessionSettings">${source.dedicated['settings']}</Settings>`);

    //! Mods
    local.sandbox_config['file'] = local.sandbox_config['file'].replace(`<Mods>${local.sandbox_config['mods']}</Mods>`, `<Mods>${source.sandbox_config['mods']}</Mods>`);
    local.sandbox['file'] = local.sandbox['file'].replace(`<Mods>${local.sandbox['mods']}</Mods>`, `<Mods>${source.sandbox_config['mods']}</Mods>`);



    //? Torch Config
    fs.writeFileSync(`${server.local}/torch.cfg`, local.torch['file'], 'utf8');
    console.log(`${name} torch.cfg has been synced.`);

    //? Dedicated Config
    fs.writeFileSync(`${server.local}/Instance/SpaceEngineers-Dedicated.cfg`, local.dedicated['file'], 'utf8');
    console.log(`${name} port has been changed to ${server.port} and the dedicated config has been synced.`);

    //? Sandbox_config.sbc
    fs.writeFileSync(`${server.local}/Instance/Saves/World/Sandbox_config.sbc`, local.sandbox_config['file'], 'utf8');
    console.log(`${name} Sandbox_config.sbc has been synced`);
    
    //? Sandbox.sbc
    fs.writeFileSync(`${server.local}/Instance/Saves/World/Sandbox.sbc`, local.sandbox['file'], 'utf8');
    console.log(`${name} Sandbox.sbc has been synced\n`);
}