@echo off
REM Still lifes - stable patterns that don't change
dotnet run --project src/GameOfLife.Console -- --width 40 --height 20 -i block@5,5 -i beehive@15,5 -i loaf@25,5 -i boat@5,12 -i tub@15,12 --auto --delay 500 --generations 5
