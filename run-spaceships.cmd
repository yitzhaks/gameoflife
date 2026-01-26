@echo off
REM A flotilla of spaceships: lightweight, middleweight, and heavyweight
dotnet run --project src/GameOfLife.Console -- --width 60 --height 30 -i lwss@5,5 -i mwss@5,12 -i hwss@5,20 --auto --delay 100
