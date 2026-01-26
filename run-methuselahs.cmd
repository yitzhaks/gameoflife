@echo off
REM Methuselahs - small patterns that evolve for many generations
REM R-pentomino stabilizes after 1103 generations!
dotnet run --project src/GameOfLife.Console -- --width 80 --height 50 -i r-pentomino@40,25 --auto --delay 50
