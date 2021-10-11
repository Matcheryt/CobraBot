# CobraBot [![Build status](https://ci.appveyor.com/api/projects/status/so5g0icditw2ngl0/branch/master?svg=true)](https://ci.appveyor.com/project/Matcheryt/cobrabot)  [![Discord Bots](https://top.gg/api/widget/status/389534436099883008.svg)](https://top.gg/bot/389534436099883008) [![Discord Shield](https://discordapp.com/api/guilds/785982202169131008/widget.png?style=shield)](https://discord.gg/pbkdG7gYeu)


<img align="right" width="100" height="100" src="https://i.imgur.com/0fFn8H0.png">

### A Discord Bot built with: 
* [Discord.NET Labs](https://github.com/Discord-Net-Labs/Discord.Net-Labs) - .NET Discord API Wrapper.
* [.NET 6](https://docs.microsoft.com/en-us/dotnet/core/compatibility/6.0) - Framework used.

Feel free to add the original version of the bot to your server [clicking here](https://discord.com/api/oauth2/authorize?client_id=389534436099883008&permissions=8&redirect_uri=https%3A%2F%2Fdiscordapp.com%2F&scope=bot).

If you need any help, support is available on [Cobra's Discord server](https://discord.gg/pbkdG7gYeu).

The project is licensed under AGPL-3.0 license. You can check out <a href="https://github.com/Matcheryt/CobraBot/blob/master/LICENSE">LICENSE</a> for more information.

## Features
* Web services such as steam, openweathermap, oxford dictionary, covid19 and open movie database
* Moderation commands, auto role and mod cases support
* Custom prefix and user join/left messages
* Ability to let users create their own private voice chats
* Utilities such as random number generator, polls, discord user info, currency conversion
* Fun and NSFW commands and the ability to get a random post from any subreddit
* Documented code makes it easy for beginners to understand it and build their own version of the bot

## Setting Up
In order to open/use CobraBot, you'll need to have the [.Net SDK](https://www.microsoft.com/net/download/windows) installed. Currently it runs using the **newest version** of .NET **(v6.0)**.

Before running the bot, **don't forget to change the file botconfig.json accordingly.**
API Documentation can be found in APIModule.cs file.

### Database setup
Database is managed through EF Core, you'll need to create a migration first before being able to use database functionality.
If you're new to EF Core refer to [this](https://docs.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli).

## Running the bot

After cloning/downloading this repo, you can run CobraBot using CLI with the following command:
```
dotnet run --project <Path to the .csproj file>
```