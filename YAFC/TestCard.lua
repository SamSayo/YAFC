-- Переменные игрока
local player_x = 100
local player_y = 100
local is_flipped = false

function init()
    -- Вызывается один раз при старте
end

function update()
    -- Обработка ввода через наше абстрактное API (0 = Влево, 1 = Вправо)
    if btn(0) then
        player_x = player_x - 4
        is_flipped = true
    end
    if btn(1) then
        player_x = player_x + 4
        is_flipped = false
    end
    if btn(2) then player_y = player_y - 4 end
    if btn(3) then player_y = player_y + 4 end
end

function draw()
    cls(40, 44, 52) -- Очистка экрана темно-серым цветом
    
    -- Рисуем спрайт #0 из атласа на позиции игрока с масштабом 3x
    spr(0, player_x, player_y, 3.0, is_flipped, false)
    
    print_text("USE ARROWS TO MOVE", 10, 10, 20, 255, 255, 255)
end