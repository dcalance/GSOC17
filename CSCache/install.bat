cd /d %~dp0 && mcs dependencies/Options.cs -t:library -out:Options.dll && mcs -r:System.Configuration.dll -r:Options.dll -t:library Config.cs ConsoleTools.cs CSCacheLib.cs FilesTools.cs MD5Tools.cs LibArgs.cs ParseTools.cs -out:CSCacheLib.dll && mcs -r:CSCacheLib.dll CSCache.cs && mcs install.cs && mono install.exe && del install.exe && pause
