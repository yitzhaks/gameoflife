@echo off
REM A flotilla of spaceships: lightweight, middleweight, and heavyweight
dotnet run --project src/GameOfLife.Console -- --width 60 --height 30 --inject lwss@5,5 --inject mwss@5,12 --inject hwss@5,20 --start-autoplay
