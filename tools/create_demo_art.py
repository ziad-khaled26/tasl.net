from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import math

root = Path(__file__).resolve().parents[1] / "CinemaBooking/wwwroot/images"
root.mkdir(parents=True, exist_ok=True)
font_path = "C:/Windows/Fonts/segoeui.ttf"
bold_path = "C:/Windows/Fonts/segoeuib.ttf"
def font(size, bold=False):
    return ImageFont.truetype(bold_path if bold else font_path, size)
def gradient(width, height, top, bottom):
    image = Image.new("RGB", (width, height))
    draw = ImageDraw.Draw(image)
    for y in range(height):
        t = y / height
        colour = tuple(int(a * (1 - t) + b * t) for a, b in zip(top, bottom))
        draw.line((0, y, width, y), fill=colour)
    return image
def centred(draw, text, y, size, fill, width=600, bold=False):
    draw.text((width / 2, y), text, font=font(size, bold), fill=fill, anchor="mt")
titles = [("BEYOND", "THE NILE"), ("ORBIT", "NINE"), ("THE LAST", "TRAM"), ("SUMMER", "IN ALEX"), ("HIDDEN", "FREQUENCY"), ("PAPER", "LANTERNS")]
palettes = [((11, 47, 65), (31, 111, 118)), ((13, 17, 46), (59, 36, 101)), ((56, 33, 35), (151, 77, 55)), ((26, 98, 133), (112, 190, 193)), ((10, 30, 42), (25, 72, 71)), ((41, 28, 71), (125, 58, 101))]
for i, title in enumerate(titles):
    image = gradient(600, 900, *palettes[i])
    d = ImageDraw.Draw(image)
    d.rounded_rectangle((25, 25, 575, 875), 16, outline=(220, 226, 232), width=1)
    centred(d, "CINEMABOOKING  /  ORIGINAL DEMO", 60, 17, "#d4e5ef")
    accent = ["#e3b368", "#c2b5f5", "#efc99d", "#fff0c7", "#95e3cf", "#ffd095"][i]
    if i == 0:
        d.ellipse((335, 175, 510, 350), fill=accent)
        d.polygon([(0, 495), (160, 330), (310, 495)], fill="#244b56")
        d.polygon([(210, 510), (405, 350), (620, 510)], fill="#316570")
        for y in range(510, 660, 18):
            x = int(50 * math.sin(y / 60))
            d.line((110 + x, y, 480 + x, y), fill="#81b5b4", width=3)
    elif i == 1:
        for n in range(80):
            x, y = (n * 139 + 43) % 530 + 35, (n * 79) % 440 + 130
            d.ellipse((x, y, x + n % 3 + 1, y + n % 3 + 1), fill="#c5c6e2")
        d.ellipse((165, 225, 435, 495), fill="#806fa7")
        d.arc((80, 270, 520, 440), 10, 350, fill=accent, width=8)
        d.ellipse((260, 270, 340, 350), fill="#9b8ec1")
        d.line((300, 480, 300, 625), fill="#d6d1eb", width=4)
        d.polygon([(300, 530), (274, 595), (326, 595)], fill="#e5e0fa")
    elif i == 2:
        for n in range(9):
            x = n * 76 - 35
            d.rectangle((x, 240 + n % 3 * 25, x + 65, 530), fill="#593c3e")
            for y in range(280, 490, 40): d.rectangle((x + 12, y, x + 22, y + 18), fill="#b98563")
        d.rounded_rectangle((150, 340, 450, 580), 30, fill="#e3b480", outline="#ffe5c2", width=5)
        d.rounded_rectangle((180, 370, 420, 450), 10, fill="#40505a")
        for x in [205, 385]: d.ellipse((x - 17, 493, x + 17, 527), fill="#ffedb5")
        d.line((218, 580, 100, 665), fill="#dcb894", width=5)
        d.line((382, 580, 500, 665), fill="#dcb894", width=5)
    elif i == 3:
        d.ellipse((360, 170, 490, 300), fill=accent)
        for n in range(7):
            y = 380 + n * 35
            points = [(x, y + 10 * math.sin(x / 45 + n)) for x in range(20, 581, 4)]
            d.line(points, fill="#a5d9d8", width=3)
        d.line((200, 345, 200, 580), fill="#ffe9ba", width=7)
        d.pieslice((80, 280, 320, 470), 180, 360, fill="#e7b18a")
        d.polygon([(80, 375), (200, 280), (200, 375)], fill="#fae7c2")
    elif i == 4:
        for n in range(90):
            x = 65 + n * 5.2
            h = 20 + abs(math.sin(n * 0.41) * math.cos(n * 0.13)) * 135
            d.line((x, 425 - h, x, 425 + h), fill=accent, width=3)
        d.ellipse((235, 360, 365, 490), fill="#14333e", outline=accent, width=4)
        d.ellipse((275, 400, 325, 450), fill=accent)
        d.line((300, 480, 300, 610), fill=accent, width=4)
    else:
        for n in range(9):
            x, y = 80 + (n * 173) % 440, 180 + (n * 79) % 360
            d.line((x, y - 40, x, y), fill="#ab7c98", width=2)
            d.rounded_rectangle((x - 25, y, x + 25, y + 67), 18, fill=accent)
            d.line((x, y + 6, x, y + 61), fill="#f3aa78", width=2)
            d.line((x, y + 67, x, y + 82), fill=accent, width=3)
    centred(d, title[0], 675, 55, "#fff8ee", bold=True)
    centred(d, title[1], 739, 49, "#fff8ee", bold=True)
    centred(d, "ILLUSTRATED PLACEHOLDER | FICTIONAL FILM", 839, 16, "#e6e6ed")
    image.save(root / f"movie-{i + 1}.png", optimize=True)
cinemas = ["NILE SCREEN", "CITY LIGHTS", "PICTURE HOUSE"]
for i, name in enumerate(cinemas):
    image = gradient(900, 560, (19 + i * 8, 34, 65 + i * 10), (55, 91 + i * 12, 120))
    d = ImageDraw.Draw(image)
    for n in range(30):
        x, y = (n * 139) % 860 + 20, (n * 61) % 160 + 20
        d.ellipse((x, y, x + 2, y + 2), fill="#b5c8df")
    d.rectangle((90, 180, 810, 510), fill="#182b44")
    d.rectangle((65, 175, 835, 280), fill="#d4b281")
    for x in range(80, 830, 26): d.ellipse((x, 185, x + 6, 191), fill="#fff0c1")
    centred(d, name, 202, 40, "#192c49", width=900, bold=True)
    for x in [150, 370, 590]:
        d.rounded_rectangle((x, 320, x + 155, 505), 8, fill="#315474", outline="#6588a6", width=3)
        d.line((x + 77, 325, x + 77, 500), fill="#6588a6", width=2)
    centred(d, "ILLUSTRATED CINEMA PLACEHOLDER", 525, 16, "#d9e4f4", width=900)
    image.save(root / f"cinema-{i + 1}.png", optimize=True)
names = ["Adam Farid", "Layla Nasser", "Omar Selim", "Maya Adel", "Youssef Amin", "Nour Khalil", "Karim Hassan", "Salma Fathi"]
for i, name in enumerate(names):
    image = gradient(400, 440, (35 + i * 9, 56 + i * 6, 88 + i * 5), (17, 31, 56))
    d = ImageDraw.Draw(image)
    skin = ["#c69b7a", "#dbb395", "#ad8268", "#d5a789"][i % 4]
    d.ellipse((115, 65, 285, 235), fill=skin)
    d.pieslice((112, 50, 288, 210), 180, 360, fill="#242b37")
    d.rounded_rectangle((165, 200, 235, 275), 15, fill=skin)
    d.ellipse((55, 245, 345, 535), fill=["#6e8b9d", "#8c748c", "#5d8d91"][i % 3])
    d.rectangle((0, 355, 400, 440), fill="#10213a")
    centred(d, name, 369, 24, "#ffffff", width=400, bold=True)
    centred(d, "FICTIONAL ACTOR | PLACEHOLDER", 408, 13, "#a7c8ec", width=400)
    image.save(root / f"actor-{i + 1}.png", optimize=True)
print("Created 17 original, clearly labelled demo PNG illustrations")

favicon = Image.new("RGB", (64, 64), "#2c7be5")
d = ImageDraw.Draw(favicon)
d.rounded_rectangle((12, 15, 52, 49), 5, outline="white", width=3)
for x in [18, 28, 38, 46]:
    d.rectangle((x, 17, x + 3, 22), fill="white")
    d.rectangle((x, 42, x + 3, 47), fill="white")
favicon.save(root.parent / "favicon.ico", sizes=[(16, 16), (32, 32), (64, 64)])
