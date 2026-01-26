@echo off
REM Gosper's Glider Gun - produces a stream of gliders
dotnet run --project src/GameOfLife.Console -- --width 80 --height 40 -i gosper-glider-gun@2,2 --auto --delay 80
