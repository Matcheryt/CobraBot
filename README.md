# CobraBot [![Build status](https://ci.appveyor.com/api/projects/status/so5g0icditw2ngl0?svg=true)](https://ci.appveyor.com/project/Matcheryt/cobrabot)

A Discord Bot built with [Discord.NET](https://github.com/RogueException/Discord.Net) wrapper using .NET Core (C#)

Feel free to add the original version of the bot to your server [clicking here](https://discordapp.com/api/oauth2/authorize?client_id=389534436099883008&permissions=8&redirect_uri=https://discordapp.com/&scope=bot).

The project is licensed under MIT license. You can check out <a href="https://github.com/Matcheryt/CobraBot/blob/master/LICENSE">LICENSE</a> for more information.

# Setting Up
In order to open/use CobraBot, you'll need to have [.Net Core SDK](https://www.microsoft.com/net/download/windows) installed. Currently it runs using the **newest version** of .NET Core **(v3.1)**.

Before running the bot, **don't forget to change the file botconfig.json accordingly.**
API Documentation can be found in APIModule.cs file.

After cloning/downloading this repo, you can run CobraBot using CLI with the following command:
```
dotnet run --project <Path to the .csproj file>
```

# Music
If you want CobraBot to play music, you'll need to create a role on your server called "DJ" (without quotes) or whatever you name it on MusicModule.cs. Also, you will need to **install the following files:**

## Windows
* [FFmpeg] (includes ffmpeg, ffplay and ffprobe)
* [youtube-dl.exe] (for playing youtube videos)
* libsodium.dll
* opus.dll

## Linux
If you are hosting the bot on Linux, install FFmpeg and youtube-dl via your package manager.
You'll also need to install libsodium and libopus, you can check [this tutorial](https://dsharpplus.emzi0767.com/articles/vnext_setup.html#gnulinux-1) for help.

[FFmpeg]: <https://www.ffmpeg.org/>
[youtube-dl.exe]: <https://youtube-dl.org/>
