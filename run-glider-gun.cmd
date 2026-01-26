@echo off
REM Gosper's Glider Gun - produces a stream of gliders
dotnet run --project src/GameOfLife.Console -- --width 80 --height 40 --inject gosper-glider-gun@2,2 --start-autoplay
