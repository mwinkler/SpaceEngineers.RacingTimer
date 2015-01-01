@echo off
FOR /D %%p IN ("%USERPROFILE%\AppData\Roaming\SpaceEngineers\Mods\RacingTimer\*.*") DO rmdir "%%p" /s /q
xcopy *.cs %USERPROFILE%\AppData\Roaming\SpaceEngineers\Mods\RacingTimer /s /y
xcopy modinfo.sbmi %USERPROFILE%\AppData\Roaming\SpaceEngineers\Mods\RacingTimer /y
rmdir %USERPROFILE%\AppData\Roaming\SpaceEngineers\Mods\RacingTimer\obj /q /s
rem pause