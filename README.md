# Offscreen Fixer

Windows multi-monitor window fixer for the classic problem where an app reopens on a monitor that is no longer connected.

If you use multiple monitors and one of them gets switched off, unplugged, or moved in Windows, some programs remember that old layout and reopen completely outside your visible screen area. The app is running, but the window is effectively lost. Offscreen Fixer exists to fix exactly that frustration in a fast, obvious way.

## What This Does

This tool scans every active monitor and every visible top-level window, then checks whether the center point of a window is outside the current monitor layout. If it is, the window gets restored, moved back onto the primary monitor, and brought to the foreground so you can see it immediately.

It is designed for people searching for things like:

- move off-screen windows back to desktop
- bring hidden windows back into view
- fix apps opening on a disconnected monitor
- restore windows after monitor disconnect
- Windows 11 multi-monitor window recovery
- offscreen fixer
- window recovery tool for multi-monitor setups
- bring windows back from a disconnected monitor

## Why People Need This

Windows remembers window placement per monitor configuration. That is useful until it becomes annoying: you close a monitor, start an app later, and the window opens where that monitor used to be. This tool is the quick reset button for that situation.

Instead of hunting around with keyboard shortcuts or dragging invisible windows by guesswork, you just run the tool and let it pull the problem windows back where they belong.

## Included Files

- [OffscreenFixer.cs](OffscreenFixer.cs): the C# source used to build the real Windows executable.
- [OffscreenFixer.exe](OffscreenFixer.exe): the ready-to-run console app for people who just want to double-click.
- [OffscreenFixer.bat](OffscreenFixer.bat): a small launcher that starts the executable.

## How It Works

- Enumerates monitors through the Windows API.
- Enumerates all visible top-level windows through `EnumWindows`.
- Skips empty windows and tiny utility windows.
- Checks each window center against the current monitor layout.
- Restores minimized windows before moving them.
- Moves off-screen windows onto the primary monitor with a small cascade.
- Brings recovered windows to the foreground.
- Prints clear debug output to the console.

## How To Use

1. Download or clone the repo.
2. Double-click `OffscreenFixer.exe` for the simplest experience.
3. If you prefer the launcher, run `OffscreenFixer.bat`.
4. Watch the console output and let the tool finish.

## Build Notes

- The repo uses only built-in Windows APIs.
- The executable is a console app on purpose, so you can see exactly what was moved and why.
- The batch file is just a convenience launcher for people who want a double-click entry point.
- The code is meant to be small, practical, and easy to share on GitHub.