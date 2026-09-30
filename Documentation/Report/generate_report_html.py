"""Build a self-contained HTML version of the development report from the same content as the PDF."""
import base64
import html
import os
import re

import generate_report as gr

OUTPUT = os.path.join(gr.HERE, "Drop_by_Drop_Development_Report.html")
headings = []


def slug(text):
    return re.sub(r"[^a-z0-9]+", "-", html.unescape(re.sub(r"<[^>]+>", "", text)).lower()).strip("-")


def image_src(filename):
    with open(os.path.join(gr.FIG, filename), "rb") as f:
        return "data:image/png;base64," + base64.b64encode(f.read()).decode("ascii")


def convert_markup(text):
    return (text.replace("<font face='Courier'>", "<code>")
                .replace("</font>", "</code>"))


def ph(text):
    return f'<span class="ph">[{text}]</span>'


def P(text, style="body"):
    return f'<p class="{style}">{convert_markup(text)}</p>'


def heading(text, level):
    anchor = slug(text)
    headings.append((level, text, anchor))
    tag = "h1" if level == 0 else "h2"
    return f'<{tag} id="{anchor}">{text}</{tag}>'


def bullets(items):
    return ["<ul>" + "".join(f"<li>{convert_markup(i)}</li>" for i in items) + "</ul>"]


def table(rows, widths, head=True):
    total = sum(widths)
    cols = "".join(f'<col style="width:{w / total * 100:.1f}%">' for w in widths)
    body = []
    for r_i, row in enumerate(rows):
        cell = "th" if head and r_i == 0 else "td"
        body.append("<tr>" + "".join(f"<{cell}>{convert_markup(str(c))}</{cell}>" for c in row) + "</tr>")
    return f'<table><colgroup>{cols}</colgroup>{"".join(body)}</table>'


def figure(filename, width_cm, caption):
    pct = min(100, width_cm / 16 * 100)
    return (f'<figure><img src="{image_src(filename)}" style="width:{pct:.0f}%" alt="">'
            f"<figcaption>{caption}</figcaption></figure>")


def figure_pair(left, right, width_cm, caption, labels=None):
    labels = labels or ("", "")
    cells = "".join(
        f'<div class="pair-cell">{f"<span class=pair-label>{label}</span>" if label else ""}'
        f'<img src="{image_src(fn)}" alt="{label}"></div>'
        for fn, label in zip((left, right), labels))
    return f'<figure><div class="pair">{cells}</div><figcaption>{caption}</figcaption></figure>'


gr.ph = ph
gr.P = P
gr.H1 = lambda text: heading(text, 0)
gr.H2 = lambda text: heading(text, 1)
gr.bullets = bullets
gr.table = table
gr.table_caption = lambda text: P(text, "caption")
gr.figure = figure
gr.figure_pair = figure_pair
gr.PageBreak = lambda: ""
gr.Spacer = lambda *a, **k: ""


def cover():
    meta = [
        "<b>Student:</b> Harriet Manda",
        f"<b>Student number:</b> {ph('insert student number')}",
        "<b>Team member:</b> Belinda Mtabane",
        f"<b>Lecturer:</b> {ph('insert lecturer name')}",
        "<b>Engine:</b> Unity 6 (6000.0.44f1), Universal Render Pipeline",
        "<b>Repository:</b> github.com/BelindaMtabane/ZurisMission",
        "<b>Date:</b> 30 September 2026",
    ]
    return (
        '<section class="cover">'
        f"<h1 class='cover-title'>{gr.TITLE.upper()}</h1>"
        "<p class='cover-sub'>Development Report: Tutorial Level, Game Engine Prototype and Full Game</p>"
        "<p class='cover-sub'>XBCGD7312 Task 2: Phase 3 and Phase 4</p>"
        f'<img src="{image_src("fig_start_screen.png")}" alt="Start screen">'
        + "".join(f"<p class='cover-meta'>{m}</p>" for m in meta)
        + "</section>"
    )


def contents():
    items = []
    for level, text, anchor in headings:
        cls = "toc0" if level == 0 else "toc1"
        items.append(f'<li class="{cls}"><a href="#{anchor}">{text}</a></li>')
    return '<nav class="toc"><h1>Table of Contents</h1><ul>' + "".join(items) + "</ul></nav>"


CSS = """
body { background:#eef2f5; margin:0; font-family: Arial, Helvetica, sans-serif; color:#1b1b1b; }
main { max-width: 820px; margin: 24px auto; background:#fff; padding: 56px 72px; box-shadow: 0 2px 12px rgba(0,0,0,.12); }
h1 { color:#1f5f8b; font-size: 1.55em; margin: 1.6em 0 .6em; border-bottom: 2px solid #b7cfe0; padding-bottom: 4px; }
h2 { color:#23445e; font-size: 1.15em; margin: 1.4em 0 .4em; }
p.body { font-size: 1em; line-height: 1.6; text-align: justify; margin: 0 0 .8em; }
ul { line-height: 1.5; margin: .2em 0 1em; }
li { margin-bottom: .3em; }
table { width:100%; border-collapse: collapse; font-size: .85em; margin: 1em 0 .2em; }
th { background:#1f5f8b; color:#fff; text-align:left; }
th, td { border:1px solid #b7cfe0; padding: 5px 7px; vertical-align: top; }
tr:nth-child(even) td { background:#e8f1f8; }
p.caption, figcaption { text-align:center; font-size:.85em; color:#333; margin: .3em 0 1.2em; }
figure { margin: 1.2em 0; text-align:center; }
figure img { max-width:100%; }
.pair { display:flex; gap:10px; justify-content:center; align-items:flex-end; }
.pair-cell { flex: 1 1 0; display:flex; flex-direction:column; align-items:center; }
.pair-cell img { width: 100%; }
.pair-label { font-size:.85em; font-weight:bold; color:#1f5f8b; margin-bottom:4px; }
p.ref { padding-left: 2em; text-indent: -2em; line-height: 1.5; margin: 0 0 .6em; word-break: break-word; }
.ph { background:#fff2a8; padding: 0 2px; }
code { font-family: Consolas, monospace; background:#f2f2f2; padding: 0 3px; }
.cover { text-align:center; padding: 30px 0 50px; border-bottom: 6px solid #1f5f8b; margin-bottom: 30px; }
.cover-title { border:none; font-size: 2.2em; margin-top: 0; }
.cover-sub { font-size: 1.15em; margin: .4em 0; color:#333; }
.cover img { width: 85%; margin: 26px 0; }
.cover-meta { margin: .25em 0; }
.toc ul { list-style:none; padding-left:0; }
.toc li.toc0 { font-weight:bold; margin-top:.5em; }
.toc li.toc1 { padding-left: 1.4em; font-size:.95em; }
.toc a { color:#1b1b1b; text-decoration:none; }
.toc a:hover { color:#1f5f8b; text-decoration:underline; }
@media (max-width: 760px) {
  main { margin: 0; padding: 18px 16px; box-shadow: none; }
  p.body { text-align: left; }
  .cover img { width: 100%; }
  .pair { flex-direction: column; align-items: center; }
  .pair-cell { width: 100%; }
  table { font-size: .78em; }
}
@media print {
  body { background:#fff; }
  main { box-shadow:none; margin:0; max-width:none; padding: 0; }
  h1 { page-break-before: always; }
  .cover-title, .toc h1 { page-break-before: auto; }
  table, figure { page-break-inside: avoid; }
}
"""


def build():
    body = []
    for section in (gr.section_intro, gr.section_problem, gr.section_phase3, gr.section_phase4,
                    gr.section_changes, gr.section_challenges, gr.section_successes,
                    gr.section_limitations, gr.section_conclusion, gr.references,
                    gr.appendix_assets, gr.appendix_commits):
        body += [part for part in section() if part]
    page = (
        "<!DOCTYPE html><html lang='en'><head><meta charset='utf-8'>"
        f"<title>{gr.TITLE} - Development Report</title><style>{CSS}</style></head>"
        f"<body><main>{cover()}{contents()}{''.join(body)}</main></body></html>"
    )
    with open(OUTPUT, "w", encoding="utf-8") as f:
        f.write(page)
    print("Wrote", OUTPUT)


if __name__ == "__main__":
    build()
