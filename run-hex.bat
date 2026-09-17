@ECHO OFF
:: Run Game of Life with hexagonal topology
:: Uses B2S34 rules optimized for 6-neighbor hex grids
:: Injects oscillator patterns

ECHO Running hexagonal Game of Life (radius 16, B2S34 rules)
ECHO Patterns: triple-triangle (period 12), hex-ring (period 3), pairs (period 2)
ECHO Press SPACE to step, P to play/pause, Q to quit
ECHO.

dotnet run --project src/GameOfLife.Console -- --topology hex --hex-radius 16 --hex-rules B2S34 --hex-inject triple-triangle@-8,-8 --hex-inject hex-ring@5,5 --hex-inject pair@-5,8 --hex-inject pair@8,-5 --hex-inject triangle@0,-12 --hex-inject line3@-10,0 --start-autoplay --max-fps 10
