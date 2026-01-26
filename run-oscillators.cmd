@echo off
REM Various oscillators: blinker, toad, beacon, clock, pulsar, pentadecathlon
dotnet run --project src/GameOfLife.Console -- --width 50 --height 35 -i blinker@5,3 -i toad@15,3 -i beacon@25,3 -i clock@35,3 -i pulsar@5,12 -i pentadecathlon@30,15 --auto --delay 200
