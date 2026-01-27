@ECHO OFF
:: A single glider traveling across the grid
dotnet run --project src/GameOfLife.Console -- --width 30 --height 20 --inject glider@2,2 --start-autoplay --aspect-mode half-block
