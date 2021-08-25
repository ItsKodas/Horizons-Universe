const fs = require('fs')
var config = require('./config.json')



var admins = `\n`
for (steamId of config.admins) { admins += `\t\t<unsignedLong>${steamId}</unsignedLong>\n` }


for (server in config.servers) {
    var name = server
    var server = config.servers[server]

    console.log(`${name}:`)


    var source = {
        "torch": {},
        "dedicated": {
            "file": fs.readFileSync(`${server.source}/Instance/SpaceEngineers-Dedicated.cfg`, 'utf8'),
            "settings": xml(fs.readFileSync(`${server.source}/Instance/SpaceEngineers-Dedicated.cfg`, 'utf8'), 'SessionSettings')
        },
        "sandbox_config": {
            "file": fs.readFileSync(`${server.source}/Instance/Saves/World/Sandbox_config.sbc`, 'utf8')
        },
        "sandbox": {
            "file": fs.readFileSync(`${server.source}/Instance/Saves/World/Sandbox.sbc`, 'utf8')
        },
        "mods": xml(fs.readFileSync(`${server.source}/Instance/Saves/World/Sandbox_config.sbc`, 'utf8'), 'Mods')
    };

    var local = {
        "torch": {
            "file": fs.readFileSync(`${server.local}/torch.cfg`, 'utf8')
        },
        "dedicated": {
            "file": fs.readFileSync(`${server.local}/Instance/SpaceEngineers-Dedicated.cfg`, 'utf8')
        },
        "sandbox_config": {
            "file": fs.readFileSync(`${server.local}/Instance/Saves/World/Sandbox_config.sbc`, 'utf8')
        },
        "sandbox": {
            "file": fs.readFileSync(`${server.local}/Instance/Saves/World/Sandbox.sbc`, 'utf8')
        }
    };



    //! Instance Title
    local.torch.file = xml(local.torch.file, 'InstanceName', name), console.log(`\t- Instance Title Set.`)

    //! Server Port
    local.dedicated.file = xml(local.dedicated.file, 'ServerPort', server.port), console.log(`\t- Server Port Set.`)

    //! Server Configurations
    if (server.transfer.configs) {
        local.dedicated.file = xml(local.dedicated.file, 'SessionSettings', source.dedicated.settings), console.log(`\t- SessionSettings Set.`)
        local.sandbox_config.file = xml(local.sandbox_config.file, 'Settings', source.dedicated.settings), console.log(`\t- Primary World Settings Set.`)
        local.sandbox.file = xml(local.sandbox.file, 'Settings', source.dedicated.settings), console.log(`\t- Secondary World Settings Set.`)
    }

    //! Mods
    if (server.transfer.mods) {
        local.sandbox_config.file = xml(local.sandbox_config.file, 'Mods', source.mods), console.log(`\t- Primary Mod Settings Set.`)
        local.sandbox.file = xml(local.sandbox.file, 'Mods', source.mods), console.log(`\t- Secondary Mod Settings Set.`)
    }

    //! Administrators
    local.dedicated.file = xml(local.dedicated.file, 'Administrators', admins), console.log(`\t- Administrators Set.`)


    //! Saving Files
    //? Torch Config
    fs.writeFileSync(`${server.local}/torch.cfg`, local.torch.file, 'utf8')
    console.log(`\t- torch.cfg has been saved.`)

    //? Dedicated Config
    fs.writeFileSync(`${server.local}/Instance/SpaceEngineers-Dedicated.cfg`, local.dedicated.file, 'utf8')
    console.log(`\t- Dedicated configs have been saved.`)

    //? Sandbox_config.sbc
    fs.writeFileSync(`${server.local}/Instance/Saves/World/Sandbox_config.sbc`, local.sandbox_config.file, 'utf8')
    console.log(`\t- Sandbox_config.sbc has been saved.`)

    //? Sandbox.sbc
    fs.writeFileSync(`${server.local}/Instance/Saves/World/Sandbox.sbc`, local.sandbox.file, 'utf8')
    console.log(`\t- Sandbox.sbc has been saved.`)


    //! Config Transfer
    if (server.transfer.files) {
        fs.readdirSync(`${server.local}/Instance`).forEach(file => {
            if (file.includes('.cfg') && !config.settings.blacklist.includes(file)) fs.unlinkSync(`${server.local}/Instance/${file}`)
        })
        fs.readdirSync(`${server.source}/Instance`).forEach(file => {
            if (file.includes('.cfg') && !config.settings.blacklist.includes(file)) fs.copyFileSync(`${server.source}/Instance/${file}`, `${server.local}/Instance/${file}`)
        })
        console.log(`\t- Config Files Replaced.`)
    }

    //! Plugin Transfer
    /*if (server.transfer.plugins) {
        fs.readdirSync(`${server.local}/Plugins`).forEach(file => {
            if (file.includes('.cfg')) fs.unlinkSync(`${server.local}/Plugins/${file}`)
        })
        fs.readdirSync(`${server.source}/Plugins`).forEach(file => {
            if (file.includes('.cfg')) fs.copyFileSync(`${server.source}/Plugins/${file}`, `${server.local}/Plugins/${file}`)
        })
        console.log(`\t- Plugins Replaced.`)
    }*/



    //? Nexus Settings
    var nexusCfg = fs.readFileSync(`${server.local}/Instance/NexusSync.cfg`, 'utf8')
    nexusCfg = xml(nexusCfg, 'ControllerIP', config.settings.controllerIP)
    nexusCfg = xml(nexusCfg, 'ServerID', server.id)
    fs.writeFileSync(`${server.local}/Instance/NexusSync.cfg`, nexusCfg, 'utf8')
    console.log(`\t- Nexus Settings Set.`)

    //? InfluxDB Settings
    if (fs.existsSync(`${server.local}/Instance/TorchInfluxDbPlugin.cfg`)) {
        var influxCfg = fs.readFileSync(`${server.local}/Instance/TorchInfluxDbPlugin.cfg`, 'utf8')
        influxCfg = xml(influxCfg, 'HostUrl', config.settings.influxDb)
        influxCfg = xml(influxCfg, 'Bucket', name)
        fs.writeFileSync(`${server.local}/Instance/TorchInfluxDbPlugin.cfg`, influxCfg, 'utf8')
        console.log(`\t- Influx Settings Set.`)
    }



    //! Server Edit Complate
    console.log('\n')
}



function xml(file, field, replace) {
    var extra = file.split(`<${field}`)[1].split(`>`)[0]
    var value = file.split(`<${field}${extra}>`)[1].split(`</${field}>`)[0]
    if (!replace) return value
    return file.replace(`<${field}${extra}>${value}</${field}>`, `<${field}${extra}>${replace}</${field}>`)
}