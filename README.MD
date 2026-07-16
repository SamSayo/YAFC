# **YAFC - Yet Another Fantasy Console**

A retro-inspired fantasy console emulator built with C# and .NET 8, featuring Lua scripting, sprite graphics, and chiptune audio synthesis.

## 🎮 Overview

YAFC is a virtual console platform that allows you to create and play retro-style games. It provides a pixel-art graphics API, input handling, and audio synthesis capabilities, all powered by Lua scripts packaged in `.yafc` cartridge files.

## ✨ Features

- **Lua Scripting Engine** – Games are written in Lua using the MoonSharp interpreter
- **Graphics System** – Sprite drawing, text rendering, and a classic 16-color palette
- **Audio Synthesis** – 4-channel chiptune audio generation (Pulse, Triangle, Noise waveforms)
- **Input Management** – Keyboard controls mapped to 6 buttons (arrow keys + Z/X)
- **Cartridge Format** – ZIP-based `.yafc` files containing Lua code and sprite sheets
- **VRAM Management** – Efficient sprite and resource management with tile-based rendering
- **Retro Aesthetic** – resizable window, classic gaming feel

## 🏗️ Architecture

### Core Components

- **`ConsoleRuntime`** – Main runtime manager; handles window, audio, input, and Lua lifecycle
- **`CartridgeManager`** – Loads and extracts game cartridge files (ZIP archives)
- **`VirtualConsoleApi`** – Lua API bindings for graphics, input, and audio
- **`VramResourceManager`** – Sprite sheet management and tile rendering
- **`AudioSynthesizer`** – Waveform generation and audio playback
- **`InputManager`** – Keyboard input polling and button state tracking

### Game Lifecycle

1. Launch with a `.yafc` cartridge file or use menu mode
2. Load Lua code and sprite sheet from cartridge
3. Call Lua functions: `init()`, `update()`, `draw()` each frame
4. Render graphics and play audio through Raylib

## 🚀 Getting Started

### Requirements
- .NET 8 SDK or later
- Windows, macOS, or Linux

### Building

```powershell
dotnet build YAFC.slnx
dotnet run --project YAFC
```

### Running a Game

```powershell
dotnet run --project YAFC -- path/to/game.yafc
```

Or launch menu mode:
```powershell
dotnet run --project YAFC
```

## 🎮 Lua API Reference

### Graphics
```lua
spr(id, x, y, scale, flipX, flipY)  -- Draw sprite
cls(r, g, b)                         -- Clear screen
print(text, x, y, size, r, g, b)     -- Draw text
```

### Input
```lua
btn(id)   -- Check if button held (0-5: left/right/up/down/Z/X)
btnp(id)  -- Check if button pressed this frame
```

### Audio
```lua
sound(channel, frequency, volume, duty)  -- Play tone on channel 1-4
```

### Game Structure
```lua
function init()
  -- Called once at startup
end

function update()
  -- Called every frame
end

function draw()
  -- Called every frame after update
end
```

## 📦 Cartridge Format

`.yafc` files are ZIP archives containing:
- `game.lua` – Main game code
- `sprites.png` – Sprite sheet (indexed PNG with 16-color palette)

## Roadmap

- [ ] Start screen
- [ ] Implement DPCM (NES-style compressed audio) support
- [ ] Add support for multiple BIOS regions (2-3 regional variants) 

## 🛠️ Dependencies

- **Raylib-cs** – Graphics and audio rendering
- **MoonSharp** – Lua scripting
- **NativeFileDialogSharp** – File dialogs

## 📄 License

See LICENSE.txt for details.
