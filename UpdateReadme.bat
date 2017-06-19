@echo off

SET DIR=%~dp0%

powershell.exe -Command "& '%DIR%UpdateReadme.ps1' %*" 