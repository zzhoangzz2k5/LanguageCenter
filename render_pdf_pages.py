import sys
from pathlib import Path

import fitz
from PIL import Image, ImageDraw, ImageFont

sys.stdout.reconfigure(encoding="utf-8")
pdf_path = Path(r"D:\LanguageCenter\BaoCao_WebPrograming_Part1_Part2.pdf")
output_dir = Path(r"D:\LanguageCenter\report_render")
output_dir.mkdir(exist_ok=True)

doc = fitz.open(pdf_path)
page_paths = []
for index, page in enumerate(doc):
    pixmap = page.get_pixmap(matrix=fitz.Matrix(1.4, 1.4), alpha=False)
    path = output_dir / f"page-{index + 1}.png"
    pixmap.save(path)
    page_paths.append(path)

thumb_width = 300
thumb_height = 388
cols = 4
rows = 4
font = ImageFont.truetype(r"C:\Windows\Fonts\arial.ttf", 18)
for sheet_index in range((len(page_paths) + cols * rows - 1) // (cols * rows)):
    subset = page_paths[sheet_index * cols * rows:(sheet_index + 1) * cols * rows]
    sheet = Image.new("RGB", (cols * thumb_width, rows * (thumb_height + 28)), "white")
    draw = ImageDraw.Draw(sheet)
    for item_index, path in enumerate(subset):
        image = Image.open(path).convert("RGB")
        image.thumbnail((thumb_width - 12, thumb_height - 12))
        x = (item_index % cols) * thumb_width + (thumb_width - image.width) // 2
        y = (item_index // cols) * (thumb_height + 28) + 24
        sheet.paste(image, (x, y))
        draw.text((x, 2 + (item_index // cols) * (thumb_height + 28)), f"Page {sheet_index * cols * rows + item_index + 1}", fill="black", font=font)
    sheet.save(output_dir / f"contact-sheet-{sheet_index + 1}.png")

print(f"pages={len(page_paths)}")
