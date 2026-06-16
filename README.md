# 7DTD-Server-Essentials
Advanced 7 Days to Die dedicated server mod featuring player commands, Discord integration, claim protection, anti-cheat, events, automation and quality-of-life improvements.
# 7DTD Server Essentials

An advanced server-side mod for 7 Days to Die dedicated servers.

This project extends the default game experience with a large collection of administration tools, player commands, Discord integration, anti-griefing systems, automated server events, pet companions, vehicle management and various quality-of-life improvements.

## Main Features

### Player Commands

* `/day7` - Blood Moon and server statistics information
* `/home` - Teleport to your bedroll
* `/info` - Display mod information
* `/time` - Show current real-world time
* `/settele <name>` - Save a custom teleport location
* `/tele <name>` - Teleport to a saved location
* `/deltele <name>` - Delete a saved teleport
* `/listtele` - List saved teleports
* `/halal` - Teleport to your last death location
* `/horda` - Spawn a horde event
* `/hozd` - Summon your vehicles from long distances
* `/jarmuinfo` - Display vehicle information
* `/vote` - Redeem voting rewards
* `/pet` - Pet system commands
* `/homerun` - Start the Homerun Derby event
* `/eject <player>` - Remove a passenger from your vehicle
* `/goto <ally>` - Teleport to an ally
* `/javitas` - Toggle automatic claim block repair
* `/radio` - Toggle online radio integration

### Administration Commands

* Teleport players
* Entity management
* Chunk repair tools
* POI reset tools
* Claim management
* Player statistics editing
* Sound playback system
* Manual cron execution
* Voting API testing

### Anti-Grief Protection

#### POI Protection

Warns players when entering a Point of Interest currently occupied by another player who is not a friend or party member. All incidents are logged.

#### Claim Protection

Warns players when entering another player's protected claim area and sends Discord notifications for suspicious activity.

#### Workstation Protection

Unauthorized players cannot access workstations such as:

* Dew Collector
* Chemistry Station
* Forge
* Workbench
* Cement Mixer

All access attempts are logged.

### Discord Integration

* Player join notifications
* Player leave notifications
* Server start notifications
* Server stop notifications
* Quest completion logging
* Full chat bridge between Discord and game chat

### Additional Features

* Automatic block repair inside active claims
* Trader door auto-close system
* Anti-cheat system
* Cooldown framework
* Zombie protection zones
* Built-in cron scheduler
* Player backup data storage
* Vehicle tracking system
* Pet companion system
* Colored admin ranks in chat
* HUD vehicle speed display
* Claim expiration tracking

### Events

#### Zombie Hunt

Kill a target number of zombies within a limited time.

#### Marathon

Travel a specified distance on foot before the timer expires.

#### Mini Blood Moon

A smaller Blood Moon event with custom balancing.

#### Deadeye

Land a number of headshot kills within the time limit.
