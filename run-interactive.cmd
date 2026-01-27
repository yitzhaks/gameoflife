@ECHO OFF
:: Interactive mode - press Space/Enter to step through generations
dotnet run --project src/GameOfLife.Console -- --width 40 --height 26 --inject glider@5,5 --inject pulsar@20,8 --aspect-mode half-block
