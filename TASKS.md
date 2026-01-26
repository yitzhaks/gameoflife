# Game of Life - Tasks & Bugs

## Bugs

### Board larger than console window causes display issues

**Status**: Open

**Description**: When the board dimensions exceed the console window size, the display becomes corrupted. The differential rendering assumes the board fits within the visible area.

**Steps to reproduce**:
1. Run with a large board size (e.g., `--width 200 --height 100`)
2. Observe display corruption when board exceeds terminal dimensions

**Expected behavior**: Either clip the board to fit the console window, or scroll/pan, or warn the user that the board is too large.

**Possible solutions**:
- Detect console window size and warn if board exceeds it
- Clip rendering to visible area
- Add viewport/scrolling support

---

## Backlog

(No pending tasks)
