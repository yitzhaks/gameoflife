@echo off
REM Interactive mode - press Space/Enter to step through generations
dotnet run --project src/GameOfLife.Console -- --width 40 --height 25 -i glider@5,5 -i pulsar@20,8
