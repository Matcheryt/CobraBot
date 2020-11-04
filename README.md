# CobraBot [![Build status](https://ci.appveyor.com/api/projects/status/so5g0icditw2ngl0?svg=true)](https://ci.appveyor.com/project/Matcheryt/cobrabot) 

<img align="right" width="100" height="100" src="https://i.imgur.com/0fFn8H0.png">

### A Discord Bot built with: 
* [Discord.NET (v2.2.0)](https://github.com/RogueException/Discord.Net) - .NET Discord API Wrapper.
* [.NET Core (v3.1)](https://dotnet.microsoft.com/learn/dotnet/what-is-dotnet) - Framework used.
* [Victoria (v5.1.9)](https://github.com/Yucked/Victoria) - .NET Lavalink wrapper

Feel free to add the original version of the bot to your server [clicking here](https://discordapp.com/api/oauth2/authorize?client_id=389534436099883008&permissions=8&redirect_uri=https://discordapp.com/&scope=bot).

The project is licensed under MIT license. You can check out <a href="https://github.com/Matcheryt/CobraBot/blob/master/LICENSE">LICENSE</a> for more information.

## Features
* API commands such as steam, openweathermap, oxford dictionary, covid19
* Music commands with queue, playlists support and much more
* Misc commands such as random number generator, polls, message cleaning, discord user info
* Documented code makes it easy for begginers to understand it and build their own version of the bot
* Updated frequently

## Setting Up
In order to open/use CobraBot, you'll need to have [.Net Core SDK](https://www.microsoft.com/net/download/windows) installed. Currently it runs using the **newest version** of .NET Core **(v3.1)**.

Before running the bot, **don't forget to change the file botconfig.json accordingly.**
API Documentation can be found in APIModule.cs file.

After cloning/downloading this repo, you can run CobraBot using CLI with the following command:
```
dotnet run --project <Path to the .csproj file>
```

## Music
If you want CobraBot to play music, you'll need to download and run a **Lavalink Server** on your machine. You can get it on [Lavalink repository](https://github.com/Frederikam/Lavalink).
After that, if you don't change the port on application.yml, the bot should automatically connect to your Lavalink Server.
