@echo off
REM Chaos - multiple methuselahs creating interesting interactions
dotnet run --project src/GameOfLife.Console -- --width 120 --height 60 --inject r-pentomino@30,30 --inject diehard@80,30 --inject acorn@55,45 --start-autoplay
