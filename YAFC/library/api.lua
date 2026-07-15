---@meta

-- api definitions functions

---Draws a sprite on the screen
---@param id integer Sprite ID in spritesheet
---@param x number X coordinate
---@param y number Y coordinate
---@param s? number Sprite scale (default 1)
---@param fx? boolean Flipping sprite by x
---@param fy? boolean Flipping sprite by y
function spr(id, x, y, s, fx, fy) end

---Clearing the screen with a specified RGB color 
---@param r integer Red color (0-255)
---@param g integer Green color (0-255)
---@param b integer Blue color (0-255)
function cls(r, g, b) end

---Prints text to the screen
---@param t string Text to print
---@param x number X coordinate
---@param y number Y coordinate
---@param size number Size of text
---@param r integer Red color (0-255)
---@param g integer Green color (0-255)
---@param b integer Blue color (0-255)
function print_text(t, x, y, size, r, g, b) end

---Checks if the button is pressed
---@param id integer Button ID
---@return boolean is_pressed
function btn(id) end

---Checks if the button was pressed in this frame
---@param id integer Button ID
---@return boolean is_just_pressed
function btnp(id) end

---Generates sound on one of four channels at a given frequency
---@param ch integer Sound channel. Channels 1 and 2 are pulse wave generators. Channel 3 is a triangle wave generator. Channel 4 is a noise generator
---@param freq number Sound frequency
---@param vol number Sound volume (0.0 - 1.0)
---@param duty? number Sound duty (default 0.5)
function sound(ch, freq, vol, duty) end