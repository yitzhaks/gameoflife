@echo off
REM Various oscillators: blinker, toad, beacon, clock, pulsar, pentadecathlon
dotnet run --project src/GameOfLife.Console -- --width 50 --height 35 --inject blinker@5,3 --inject toad@15,3 --inject beacon@25,3 --inject clock@35,3 --inject pulsar@5,12 --inject pentadecathlon@30,15 --start-autoplay
