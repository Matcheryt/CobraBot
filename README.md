# CobraBot [![Current Build](https://ci.appveyor.com/api/projects/status/hvkuxaantytsrku0?svg=true)](https://ci.appveyor.com/project/Matcheryt/cobrabot)
A Discord Bot built with [Discord.NET](https://github.com/RogueException/Discord.Net) wrapper using .NET Core (C#)

Feel free to add the original version of the bot to your server [clicking here](https://discordapp.com/api/oauth2/authorize?client_id=389534436099883008&permissions=8&redirect_uri=https://discordapp.com/&scope=bot).

The project is licensed under MIT license. You can check out <a href="https://github.com/Matcheryt/CobraBot/blob/master/LICENSE">LICENSE</a> for more information.

# Setting Up
In order to open/use CobraBot, you'll need to have [.Net Core SDK](https://www.microsoft.com/net/download/windows) installed. Currently it runs using 1.1 version of .Net Core, but you can change it to a recent one, however, some changes to the code will need to be made for it to function properly.

After cloning/downloading this repo, you can run CobraBot using CLI with the following command:
```
dotnet run --project <Path to the .csproj file>
```

# Music
If you want CobraBot to play music, you should download and place the following items in the runtime directory of the bot:
* [FFmpeg] (includes ffmpeg, ffplay and ffprobe)
* [youtube-dl.exe] (for playing youtube videos)
* libsodium.dll
* opus.dll

Also, you'll need to create a role on your server called "DJ" (without quotes) or whatever you name it on MusicModule.cs

[FFmpeg]: <https://www.ffmpeg.org/>
[youtube-dl.exe]: <https://youtube-dl.org/>
