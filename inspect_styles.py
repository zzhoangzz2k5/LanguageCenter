import sys

from docx import Document
from docx.oxml.ns import qn

sys.stdout.reconfigure(encoding="utf-8")
doc = Document(r"D:\LanguageCenter\BaoCao_WebPrograming_working.docx")

for idx in [6, 7, 29, 30, 31, 45, 58]:
    p = doc.paragraphs[idx]
    print(idx, p.text, p.alignment, p.style.name)
    for run in p.runs:
        if run.text.strip():
            print(
                "  ",
                repr(run.text),
                run.font.name,
                run.font.size.pt if run.font.size else None,
                run.bold,
                run.italic,
                run._element.rPr.rFonts.get(qn("w:ascii")) if run._element.rPr is not None and run._element.rPr.rFonts is not None else None,
            )

section = doc.sections[0]
print(
    "SECTION",
    section.page_width.inches,
    section.page_height.inches,
    section.top_margin.inches,
    section.right_margin.inches,
    section.bottom_margin.inches,
    section.left_margin.inches,
)
