@ECHO OFF
:: Claude Code startup script for Game of Life project
:: Sets up persistent task list and launches Claude

SET CLAUDE_CODE_TASK_LIST_ID=game-of-life
claude.exe %*
