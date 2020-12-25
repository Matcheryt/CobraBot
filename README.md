# CobraBot [![Build status](https://ci.appveyor.com/api/projects/status/so5g0icditw2ngl0/branch/master?svg=true)](https://ci.appveyor.com/project/Matcheryt/cobrabot)  [![Discord Bots](https://top.gg/api/widget/status/389534436099883008.svg)](https://top.gg/bot/389534436099883008) [![Discord Shield](https://discordapp.com/api/guilds/785982202169131008/widget.png?style=shield)](https://discord.gg/pbkdG7gYeu)


<img align="right" width="100" height="100" src="https://i.imgur.com/0fFn8H0.png">

### A Discord Bot built with: 
* [Discord.NET (2.3.0-dev-20201223.7)](https://github.com/RogueException/Discord.Net) - .NET Discord API Wrapper.
* [Discord Interactivity Addon (2.2.0-rc8)](https://github.com/Playwo/Discord.InteractivityAddon) - Discord.NET interactivity addon
* [.NET 5](https://docs.microsoft.com/en-us/dotnet/core/dotnet-five) - Framework used.
* [Victoria (v5.1.11)](https://github.com/Yucked/Victoria) - .NET Lavalink wrapper.

I'm currently using the nightly version of Discord.NET as it fixes some reconnecting issues that the stable version has.

Feel free to add the original version of the bot to your server [clicking here](https://discord.com/api/oauth2/authorize?client_id=389534436099883008&permissions=8&redirect_uri=https%3A%2F%2Fdiscordapp.com%2F&scope=bot).

If you need any help, support is available on [Cobra's Discord server](https://discord.gg/pbkdG7gYeu).

The project is licensed under MIT license. You can check out <a href="https://github.com/Matcheryt/CobraBot/blob/master/LICENSE">LICENSE</a> for more information.

## Features
* API commands such as steam, openweathermap, oxford dictionary, covid19
* Moderation commands and auto give role when user joins server
* Custom prefix and user join/left messages
* Music commands with queue, playlists support and much more
* Misc commands such as random number generator, polls, discord user info, currency conversion
* Documented code makes it easy for beginners to understand it and build their own version of the bot
* Updated frequently

## Setting Up
In order to open/use CobraBot, you'll need to have the [.Net SDK](https://www.microsoft.com/net/download/windows) installed. Currently it runs using the **newest version** of .NET **(v5.0)**.

Before running the bot, **don't forget to change the file botconfig.json accordingly.**
API Documentation can be found in APIModule.cs file.

### Database setup
Database is managed through EF Core, you'll need to create a migration first before being able to use database functionality.
If you're new to EF Core refer to [this](https://docs.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli).

### Music setup
If you want CobraBot to play music, you'll need to download and run a **Lavalink Server** on your machine. You can get it on [Lavalink repository](https://github.com/Frederikam/Lavalink).
After that, if you don't change the port on application.yml, the bot should automatically connect to your Lavalink Server.

## Running the bot

After cloning/downloading this repo, you can run CobraBot using CLI with the following command:
```
dotnet run --project <Path to the .csproj file>
```

If you choose to use the nightly version of Discord.NET, don't forget to add the nuget source before running the bot:
```
dotnet nuget add source https://www.myget.org/F/discord-net/api/v3/index.json
```
