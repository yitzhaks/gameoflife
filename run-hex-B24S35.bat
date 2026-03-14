@ECHO OFF
:: Run Game of Life with hexagonal topology using B24S35 rules
:: B24S35: Birth with 2 or 4 neighbors, Survive with 3-5 neighbors
:: More active birth creates more dynamic patterns
:: Injects oscillator patterns (behavior may differ from B2S34)

ECHO Running hexagonal Game of Life (radius 16, B24S35 rules)
ECHO Patterns: triple-triangle, hex-ring, pairs, triangles
ECHO Press SPACE to step, P to play/pause, Q to quit
ECHO.

dotnet run --project src/GameOfLife.Console -- --topology hex --hex-radius 16 --hex-rules B24S35 --hex-inject triple-triangle@-8,-8 --hex-inject hex-ring@5,5 --hex-inject pair@-5,8 --hex-inject pair@8,-5 --hex-inject triangle@0,-12 --hex-inject line3@-10,0 --start-autoplay --max-fps 10
