# notory

**English | [Türkçe](README.tr.md)**

A lightweight Windows quick-note scratchpad.

notory lives quietly in your system tray. Press a hotkey and a small note pops up
wherever you are — jot something down, press the hotkey again to tuck it away.
Whatever you type is saved automatically and restored next time, so it's always
the same note waiting for you.

<p align="center">
  <img src="docs/screenshot.png" alt="notory's quick note" width="360" />
</p>

## Features

- **Always a keystroke away** — global hotkey (`Ctrl + Shift + N`) shows or hides
  the note from any app.
- **Auto-saves** — every keystroke is written to disk; nothing to remember to save.
- **Survives restarts** — your note is restored exactly as you left it.
- **Dark or light** — pick a **System**, **Dark**, or **Light** theme from the
  menu. Defaults to **System**, following your Windows setting.
- **Start with Windows** — optional, toggled from the menu.
- **English & Turkish** — switch the interface language from the menu.
- **Private by design** — everything stays on your machine; nothing is uploaded.

## Run it

notory isn't published as a prebuilt download yet, so for now you run it from
source. You'll need the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
(the SDK, not just the runtime) on Windows.

```bash
git clone https://github.com/volkanturhan/notory.git
cd notory
dotnet run --project notory/notory.csproj
```

notory starts quietly in the system tray — **no window pops up**. That's normal;
press the hotkey (or double-click the tray icon) to open your note.

## How to use

1. Launch notory — it starts quietly in the system tray.
2. Press **`Ctrl + Shift + N`** (or double-click the tray icon) to open the note.
3. Type anything — it saves as you go. **Clear** empties it.
4. Press **`Ctrl + Shift + N`** again to hide the note; it's still there next time.

Right-click the tray icon for **Open note**, **Start with Windows**, language, and
**Quit**.

## Where your data lives

Your note is stored locally at `%APPDATA%\notory\note.txt` and never leaves your
machine; preferences live next to it in `settings.json`.

## Build a shareable exe

Want a standalone `.exe` you can hand to someone without the SDK? Build it
yourself — the output isn't checked into the repo:

```bash
# Builds into dist/ (self-contained notory.exe + lite build)
pwsh tools/publish.ps1
```

## Tech

- C# / WPF on .NET 8 (Windows)
- No third-party dependencies

## License

MIT — see [LICENSE](LICENSE).
