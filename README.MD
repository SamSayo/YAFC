# YAFC — Yet Another Fantasy Console

YAFC is a small retro-inspired fantasy console and game runtime built with C# and .NET 8.

Games are written in Lua and run inside a lightweight virtual console environment with pixel graphics, keyboard input, synthesized chiptune audio, tile-based collision, and self-contained game cartridges.

The project is designed around a simple idea: writing a small game should require only a Lua script and a small set of assets, without having to build a complete engine around it.

## Features

### Lua scripting

YAFC uses MoonSharp to execute game code inside a restricted Lua environment.

The runtime provides a small, purpose-built API instead of exposing the underlying C# implementation directly.

A game is built around three entry points:

```lua
function init()
    -- Called once when the game starts.
end

function update()
    -- Called once per frame.
end

function draw()
    -- Called once per frame after update().
end
```

`deltaTime` is available as a global value containing the elapsed time since the previous frame.

### Graphics

The virtual console uses a logical resolution of `256×192` pixels.

The final image is scaled to the window using integer scaling and nearest-neighbor filtering, preserving the pixel-art look while allowing the window to be resized.

Graphics are based around an `8×8` sprite grid.

Sprites are loaded from a PNG spritesheet and can be drawn with scaling and horizontal or vertical flipping.

YAFC also provides a simple tile API. Tiles are addressed by their position on the tile map rather than by screen coordinates.

The runtime uses a fixed 16-color palette. Non-transparent pixels from the loaded spritesheet are converted to the closest color from this palette.

### Input

Six virtual buttons are currently available:

|  ID | Key   |
| --: | ----- |
| `0` | Left  |
| `1` | Right |
| `2` | Up    |
| `3` | Down  |
| `4` | Z     |
| `5` | X     |

Two functions are provided:

```lua
btn(id)
```

Returns `true` while the button is held.

```lua
btnp(id)
```

Returns `true` on the frame when the button is pressed.

### Audio

YAFC contains a software audio synthesizer running at `44100 Hz`.

Four channels are currently available:

| Channel | Type     |
| ------: | -------- |
|     `1` | Pulse    |
|     `2` | Pulse    |
|     `3` | Triangle |
|     `4` | Noise    |

Pulse channels have configurable duty cycles. The audio stream is mixed into a single 16-bit mono output stream.

Audio can be controlled directly from Lua:

```lua
sound(channel, frequency, volume, duty)
```

Example:

```lua
sound(1, 440, 0.5, 0.5)
```

### Tile collision and movement

YAFC includes a small collision system intended for simple 2D games.

Tiles can be marked as solid:

```lua
set_tile_solid(tileX, tileY, true)
```

Their current state can be queried with:

```lua
get_tile_solid(tileX, tileY)
```

Movement is performed through:

```lua
result = move_and_slide(x, y, width, height, velocityX, velocityY)
```

The collision system performs axis-separated rectangular collision checks against solid tiles and returns information about the resulting movement.

The returned object contains:

```lua
result.FinalX
result.FinalY

result.HitWallLeft
result.HitWallRight
result.IsGrounded
result.HitCeiling
```

YAFC does not provide a complete character controller or physics simulation. Gravity, acceleration, jumping and other game-specific movement logic are intended to be implemented in Lua.

## Lua API

The current Lua API consists of:

### Graphics

```lua
spr(id, x, y, scale, flipX, flipY)
```

Draw an `8×8` sprite from the spritesheet.

```lua
tile(id, tileX, tileY)
```

Draw a tile at tile-map coordinates.

```lua
cls(r, g, b)
```

Clear the screen with the specified RGB color.

```lua
print_text(text, x, y, size, r, g, b)
```

Draw text using Raylib's default font.

### Input

```lua
btn(id)
btnp(id)
```

Read virtual button state.

### Audio

```lua
sound(channel, frequency, volume, duty)
```

Control one of the four synthesizer channels.

### Physics

```lua
move_and_slide(x, y, width, height, velocityX, velocityY)
```

Move a rectangular body and resolve collisions with solid tiles.

```lua
set_tile_solid(tileX, tileY, solid)
```

Change the collision state of a tile.

```lua
get_tile_solid(tileX, tileY)
```

Query the collision state of a tile.

## Example

A minimal game can look like this:

```lua
local playerX = 120
local playerY = 88

function init()
end

function update()
    local speed = 60

    if btn(0) then
        playerX = playerX - speed * deltaTime
    end

    if btn(1) then
        playerX = playerX + speed * deltaTime
    end

    if btn(2) then
        playerY = playerY - speed * deltaTime
    end

    if btn(3) then
        playerY = playerY + speed * deltaTime
    end
end

function draw()
    cls(0, 0, 0)
    spr(0, playerX, playerY)
end
```

## Game cartridges

Games can be loaded either from a `.yafc` archive or directly from a directory.

A `.yafc` cartridge is a ZIP archive with a predefined layout:

```text
game.yafc
├── main.lua
├── spritesheet.png
├── menu.xm
└── meta.xml
```

### `main.lua`

The main Lua script executed by the runtime.

### `spritesheet.png`

The game's sprite sheet.

Sprites are addressed as `8×8` tiles starting from the top-left corner.

### `menu.xm`

XM tracker music file intended for cartridge music.

The cartridge interface already reserves this resource, but music playback is not currently integrated into the game runtime.

### `meta.xml`

Metadata file containing cartridge information such as:

```text
Id
Name
Description
Version
```

The current loader reads this resource through its metadata deserializer; the format and filename are subject to change as the cartridge system evolves.

## Running YAFC

### Requirements

* .NET 8 SDK
* A desktop platform supported by the underlying Raylib runtime

### Build

Clone the repository and build the solution:

```bash
dotnet build YAFC.slnx
```

### Start the cartridge menu

Run YAFC without arguments:

```bash
dotnet run --project YAFC
```

The menu allows a cartridge to be selected through a native file dialog.

### Run a cartridge directly

A cartridge can also be passed on the command line:

```bash
dotnet run --project YAFC -- path/to/game.yafc
```

A directory containing cartridge files can be passed in the same way during development.

## Runtime architecture

The project is intentionally split into small subsystems.

```text
YAFC/
├── Audio/
│   └── AudioSynthesizer
├── GameCarts/
│   ├── GameCartFactory
│   ├── ArchiveGameCart
│   ├── DirGameCart
│   ├── CartridgeManager
│   └── Metadata
├── Physics/
│   └── PhysEngine
├── Video/
│   └── VramResourceManager
├── ConsoleRuntime
├── VirtualConsoleApi
└── InputManager
```

`ConsoleRuntime` controls the application lifecycle, window, Lua state, rendering loop, input, audio and cartridge execution.

`VirtualConsoleApi` is the bridge between Lua and the virtual console subsystems.

`VramResourceManager` handles the spritesheet, palette conversion, sprite drawing and tile collision state.

`AudioSynthesizer` generates the four software audio channels.

`PhysEngine` handles basic rectangular movement and collision against solid tiles.

`GameCarts` abstracts cartridge storage and loading. The same interface can represent either a directory or a `.yafc` archive.

## Rendering loop

When a cartridge is started, YAFC performs the following steps:

1. Load the cartridge.
2. Read `main.lua` and the spritesheet.
3. Create the MoonSharp Lua state.
4. Register the virtual console API.
5. Call `init()`.
6. Run the main update/draw loop at 60 FPS.
7. Update the audio stream.
8. Render the 256×192 framebuffer to the window.

The logical game resolution remains independent from the size of the application window.

Press `F` during gameplay to toggle fullscreen mode.

## Dependencies

YAFC currently uses:

* [Raylib-cs](https://github.com/ChrisDill/Raylib-cs) for graphics, windowing and audio
* [MoonSharp](https://www.moonsharp.org/) for Lua scripting
* [rlImGui-cs](https://github.com/NotNotTech/rlImGui-cs) for the cartridge selection interface
* [NativeFileDialogSharp](https://github.com/mohamadfouladgar/NativeFileDialogSharp) for native file dialogs
* [Zio](https://github.com/xoofx/Zio) for a unified filesystem abstraction for cartridge directories and archives

## Project status

YAFC is a compact experimental runtime rather than a complete game development suite.

The current codebase focuses on the virtual console itself:

* Lua execution
* pixel graphics
* sprite and tile rendering
* keyboard input
* procedural chiptune audio
* basic tile collision
* cartridge loading

There is currently no dedicated visual editor, level editor, asset pipeline or full-featured debugger included in the repository.

The public API is deliberately small and may change as the console specification develops.

## License

YAFC is distributed under the MIT License.

See [LICENSE.txt](LICENSE.txt) for the full license text.
