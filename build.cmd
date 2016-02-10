@SET PowerShellCmd=%windir%\system32\WindowsPowerShell\v1.0\powershell.exe -ExecutionPolicy Unrestricted -Command
@SET BuildCmd=%PowerShellCmd% .\build.ps1 -environmentName %3 -configuration %2 -command %1
%BuildCmd%