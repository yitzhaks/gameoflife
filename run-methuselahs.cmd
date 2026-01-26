@echo off
REM Methuselahs - small patterns that evolve for many generations
REM R-pentomino stabilizes after 1103 generations!
dotnet run --project src/GameOfLife.Console -- --width 80 --height 50 --inject r-pentomino@40,25 --start-autoplay
