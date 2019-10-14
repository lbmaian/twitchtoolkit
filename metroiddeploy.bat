set target=Debug

rm -r TwitchToolkit\bin\%target%\Assemblies
mkdir TwitchToolkit\bin\%target%\Assemblies
move /Y TwitchToolkit\bin\%target%\*.dll TwitchToolkit\bin\%target%\Assemblies
move /Y TwitchToolkit\bin\%target%\*.pdb TwitchToolkit\bin\%target%\Assemblies

xcopy /E /Y ".\TwitchToolkit\bin\%target%" "Z:\SteamLibrary\steamapps\common\RimWorld\Mods\TwitchToolKit\"