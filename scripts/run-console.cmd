@echo off
setlocal
set baseDir=%~dp0..
set width=%1
set height=%2
if "%width%"=="" set width=20
if "%height%"=="" set height=10
dotnet run --project "%baseDir%\src\GameOfLife.Console\GameOfLife.Console.csproj" -- %width% %height% glider.txt@1,1 blinker.txt@10,4 toad.txt@5,6
