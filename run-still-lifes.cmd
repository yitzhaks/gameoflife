@echo off
REM Still lifes - stable patterns that don't change
dotnet run --project src/GameOfLife.Console -- --width 40 --height 20 --inject block@5,5 --inject beehive@15,5 --inject loaf@25,5 --inject boat@5,12 --inject tub@15,12 --start-autoplay --generations 5
