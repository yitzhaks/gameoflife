@echo off
REM A single glider traveling across the grid
dotnet run --project src/GameOfLife.Console -- --width 30 --height 20 -i glider@2,2 --auto --delay 150
