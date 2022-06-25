# steam-icon-fix-linux

## Problem

Steam only downloads icons to the icon-cache during install, so if you move games around or share a drive between machines, you may be missing icons.

## Fix

This parses a shortcut folder and takes missing steam icons in linux, downloads them via the steam API, and sets them in the shortcut files.

TODO: Figure out higher resolution icons? (but even steam is giving me low-res 16x16 icons atm...)

> dotnet run [STEAM_API_KEY](https://steamcommunity.com/dev/apikey) [STEAM_ID](https://www.google.com/url?sa=t&rct=j&q=&esrc=s&source=web&cd=&ved=2ahUKEwiWr_yW28n4AhXaEEQIHUHECdEQFnoECAoQAQ&url=https%3A%2F%2Fwww.steamidfinder.com%2F&usg=AOvVaw0bXyXz2-U3xxyv9lcmDiUj)

Using api: https://partner.steamgames.com/doc/webapi/IPlayerService

![](.img/Screenshot%20from%202022-06-25%2017-05-18.png) ![](.img/Screenshot%20from%202022-06-25%2017-06-04.png)

![](.img/Screenshot%20from%202022-06-25%2016-50-55.png)

![](.img/Screenshot%20from%202022-06-25%2016-51-38.png)