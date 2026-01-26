@echo off
REM Chaos - multiple methuselahs creating interesting interactions
dotnet run --project src/GameOfLife.Console -- --width 120 --height 60 -i r-pentomino@30,30 -i diehard@80,30 -i acorn@55,45 --auto --delay 20
