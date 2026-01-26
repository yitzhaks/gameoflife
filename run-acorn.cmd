@echo off
REM Acorn - a methuselah that takes 5206 generations to stabilize
dotnet run --project src/GameOfLife.Console -- --width 100 --height 60 --inject acorn@50,30 --start-autoplay
