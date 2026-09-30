"""Build the Drop by Drop: Zuri's Mission development report (Phase 3 and Phase 4) as a PDF."""
import os

from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_JUSTIFY, TA_LEFT
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import cm
from reportlab.platypus import (
    BaseDocTemplate,
    Frame,
    Image,
    KeepTogether,
    NextPageTemplate,
    PageBreak,
    PageTemplate,
    Paragraph,
    Spacer,
    Table,
    TableStyle,
)
from reportlab.platypus.tableofcontents import TableOfContents

HERE = os.path.dirname(os.path.abspath(__file__))
FIG = os.path.join(HERE, "figures")
OUTPUT = os.path.join(HERE, "Drop_by_Drop_Development_Report.pdf")

TITLE = "Drop by Drop: Zuri's Mission"
RUNNING_HEAD = "Drop by Drop: Zuri's Mission | Development Report"
ACCESSED = "30 September 2026"

ACCENT = colors.HexColor("#1f5f8b")
ACCENT_LIGHT = colors.HexColor("#e8f1f8")
GRID = colors.HexColor("#b7cfe0")
PLACEHOLDER_BG = "#fff2a8"

base = getSampleStyleSheet()
S = {
    "cover_title": ParagraphStyle("cover_title", parent=base["Title"], fontName="Helvetica-Bold",
                                  fontSize=28, leading=34, textColor=ACCENT, alignment=TA_CENTER),
    "cover_sub": ParagraphStyle("cover_sub", parent=base["Normal"], fontName="Helvetica",
                                fontSize=15, leading=20, alignment=TA_CENTER, textColor=colors.HexColor("#333333")),
    "cover_meta": ParagraphStyle("cover_meta", parent=base["Normal"], fontName="Helvetica",
                                 fontSize=11.5, leading=18, alignment=TA_CENTER),
    "h1": ParagraphStyle("h1", parent=base["Heading1"], fontName="Helvetica-Bold", fontSize=16,
                         leading=20, spaceBefore=6, spaceAfter=10, textColor=ACCENT, keepWithNext=1),
    "h2": ParagraphStyle("h2", parent=base["Heading2"], fontName="Helvetica-Bold", fontSize=12.5,
                         leading=16, spaceBefore=12, spaceAfter=6, textColor=colors.HexColor("#23445e"),
                         keepWithNext=1),
    "body": ParagraphStyle("body", parent=base["Normal"], fontName="Helvetica", fontSize=11,
                           leading=16.5, spaceAfter=8, alignment=TA_JUSTIFY),
    "bullet": ParagraphStyle("bullet", parent=base["Normal"], fontName="Helvetica", fontSize=11,
                             leading=15.5, leftIndent=16, bulletIndent=4, spaceAfter=3, alignment=TA_LEFT),
    "caption": ParagraphStyle("caption", parent=base["Normal"], fontName="Helvetica", fontSize=9.5,
                              leading=12, alignment=TA_CENTER, spaceBefore=3, spaceAfter=12,
                              textColor=colors.HexColor("#333333")),
    "pair_label": ParagraphStyle("pair_label", parent=base["Normal"], fontName="Helvetica", fontSize=9.5,
                                 leading=12, alignment=TA_CENTER, textColor=ACCENT),
    "cell": ParagraphStyle("cell", parent=base["Normal"], fontName="Helvetica", fontSize=9, leading=11.5),
    "cell_head": ParagraphStyle("cell_head", parent=base["Normal"], fontName="Helvetica-Bold", fontSize=9,
                                leading=11.5, textColor=colors.white),
    "ref": ParagraphStyle("ref", parent=base["Normal"], fontName="Helvetica", fontSize=10.5, leading=15,
                          leftIndent=22, firstLineIndent=-22, spaceAfter=7),
    "toc_title": ParagraphStyle("toc_title", parent=base["Heading1"], fontName="Helvetica-Bold",
                                fontSize=16, leading=20, textColor=ACCENT, spaceAfter=12),
}


def ph(text):
    """Highlighted placeholder the student must complete."""
    return f'<span backColor="{PLACEHOLDER_BG}">[{text}]</span>'


def P(text, style="body"):
    return Paragraph(text, S[style])


def H1(text):
    p = Paragraph(text, S["h1"])
    p.toc_level = 0
    return p


def H2(text):
    p = Paragraph(text, S["h2"])
    p.toc_level = 1
    return p


def bullets(items):
    return [Paragraph(i, S["bullet"], bulletText="\u2022") for i in items]


def table(rows, widths, head=True):
    data = []
    for r_i, row in enumerate(rows):
        style = "cell_head" if head and r_i == 0 else "cell"
        data.append([Paragraph(str(c), S[style]) for c in row])
    t = Table(data, colWidths=widths, hAlign="CENTER", repeatRows=1 if head else 0)
    cmds = [
        ("GRID", (0, 0), (-1, -1), 0.5, GRID),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("LEFTPADDING", (0, 0), (-1, -1), 5),
        ("RIGHTPADDING", (0, 0), (-1, -1), 5),
        ("TOPPADDING", (0, 0), (-1, -1), 4),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 4),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, ACCENT_LIGHT]),
    ]
    if head:
        cmds.append(("BACKGROUND", (0, 0), (-1, 0), ACCENT))
    t.setStyle(TableStyle(cmds))
    return t


def table_caption(text):
    return P(text, "caption")


def figure(filename, width_cm, caption):
    path = os.path.join(FIG, filename)
    img = Image(path)
    ratio = img.imageHeight / float(img.imageWidth)
    img.drawWidth = width_cm * cm
    img.drawHeight = width_cm * cm * ratio
    return KeepTogether([img, P(caption, "caption")])


def figure_pair(left, right, width_cm, caption, labels=None):
    """Two images side by side at a shared height, filling at most 2 * width_cm."""
    imgs = [Image(os.path.join(FIG, fn)) for fn in (left, right)]
    aspects = [img.imageWidth / float(img.imageHeight) for img in imgs]
    height = 2 * width_cm * cm / sum(aspects)
    for img, aspect in zip(imgs, aspects):
        img.drawHeight = height
        img.drawWidth = height * aspect
    rows = [imgs]
    if labels:
        rows.insert(0, [P(f"<b>{label}</b>", "pair_label") for label in labels])
    t = Table(rows, colWidths=[img.drawWidth + 0.3 * cm for img in imgs], hAlign="CENTER")
    t.setStyle(TableStyle([("VALIGN", (0, 0), (-1, -1), "BOTTOM"),
                           ("ALIGN", (0, 0), (-1, -1), "CENTER"),
                           ("BOTTOMPADDING", (0, 0), (-1, 0), 2 if labels else 3)]))
    return KeepTogether([t, P(caption, "caption")])


class ReportDoc(BaseDocTemplate):
    def __init__(self, filename, **kw):
        super().__init__(filename, pagesize=A4, leftMargin=2.5 * cm, rightMargin=2.5 * cm,
                         topMargin=2.3 * cm, bottomMargin=2.3 * cm, title=TITLE + " - Development Report",
                         author="Harriet Manda", subject="XBCGD7312 Task 2: Phase 3 and Phase 4", **kw)
        frame = Frame(self.leftMargin, self.bottomMargin, self.width, self.height, id="main")
        self.addPageTemplates([
            PageTemplate(id="cover", frames=[frame], onPage=self._cover_page),
            PageTemplate(id="body", frames=[frame], onPage=self._body_page),
        ])

    def _cover_page(self, canvas, doc):
        canvas.saveState()
        canvas.setFillColor(ACCENT)
        canvas.rect(0, A4[1] - 1.2 * cm, A4[0], 1.2 * cm, fill=1, stroke=0)
        canvas.rect(0, 0, A4[0], 0.8 * cm, fill=1, stroke=0)
        canvas.restoreState()

    def _body_page(self, canvas, doc):
        canvas.saveState()
        canvas.setFont("Helvetica", 8.5)
        canvas.setFillColor(colors.HexColor("#555555"))
        canvas.drawString(self.leftMargin, A4[1] - 1.5 * cm, RUNNING_HEAD)
        canvas.drawRightString(A4[0] - self.rightMargin, 1.3 * cm, f"Page {doc.page}")
        canvas.setStrokeColor(GRID)
        canvas.line(self.leftMargin, A4[1] - 1.65 * cm, A4[0] - self.rightMargin, A4[1] - 1.65 * cm)
        canvas.restoreState()

    def afterFlowable(self, flowable):
        level = getattr(flowable, "toc_level", None)
        if level is not None:
            text = flowable.getPlainText()
            key = f"h{level}-{self.seq.nextf('toc')}"
            self.canv.bookmarkPage(key)
            self.canv.addOutlineEntry(text, key, level=level, closed=level > 0)
            self.notify("TOCEntry", (level, text, self.page, key))


def cover():
    s = [Spacer(1, 3.2 * cm),
         P(TITLE.upper(), "cover_title"),
         Spacer(1, 0.5 * cm),
         P("Development Report: Tutorial Level, Game Engine Prototype and Full Game", "cover_sub"),
         Spacer(1, 0.3 * cm),
         P("XBCGD7312 Task 2: Phase 3 and Phase 4", "cover_sub"),
         Spacer(1, 1.2 * cm)]
    img = Image(os.path.join(FIG, "fig_start_screen.png"))
    ratio = img.imageHeight / float(img.imageWidth)
    img.drawWidth = 13 * cm
    img.drawHeight = 13 * cm * ratio
    s += [img, Spacer(1, 1.2 * cm)]
    meta = [
        "<b>Student:</b> Harriet Manda",
        f"<b>Student number:</b> {ph('insert student number')}",
        "<b>Team member:</b> Belinda Mtabane",
        f"<b>Lecturer:</b> {ph('insert lecturer name')}",
        "<b>Engine:</b> Unity 6 (6000.0.44f1), Universal Render Pipeline",
        "<b>Repository:</b> github.com/BelindaMtabane/ZurisMission",
        "<b>Date:</b> 30 September 2026",
    ]
    s += [P(m, "cover_meta") for m in meta]
    s += [NextPageTemplate("body"), PageBreak()]
    return s


def contents():
    toc = TableOfContents()
    toc.levelStyles = [
        ParagraphStyle("toc0", fontName="Helvetica-Bold", fontSize=11, leading=16, leftIndent=0,
                       firstLineIndent=0, spaceBefore=4),
        ParagraphStyle("toc1", fontName="Helvetica", fontSize=10, leading=14, leftIndent=16,
                       firstLineIndent=0),
    ]
    figures = [
        "Figure 1: The current Level 1 finish courtyard and Level Complete panel",
        "Figure 2: Before and after: early prototype (May 2026) and Level 2 now",
        "Figure 3: Before and after: early prototype canyon (May 2026) and Level 3 now",
        "Figure 4: Before and after: the start screen in August 2026 and now",
        "Figure 5: Before and after: Level 1 in August 2026 and now",
        "Figure 6: Green tank wrap texture used on the Level 2 tanks",
        "Figure 7: Before and after the ground fix for the grey void",
        "Figure 8: Before and after the roadside clearance band",
    ]
    tables = [
        "Table 1: Level 1 teaching phases",
        "Table 2: Auxiliary mechanics and game rules",
        "Table 3: Difficulty and environment gradient across the three stages",
        "Table 4: Contextual sound mapping",
        "Table 5: Summary of my changes and additions by period",
        "Table 6: Challenges and fixes",
    ]
    s = [P("Table of Contents", "toc_title"), toc, Spacer(1, 0.6 * cm),
         P("<b>List of Figures</b>", "body")]
    s += bullets(figures)
    s += [Spacer(1, 0.3 * cm), P("<b>List of Tables</b>", "body")]
    s += bullets(tables)
    s.append(PageBreak())
    return s


def section_intro():
    return [
        H1("1. Introduction"),
        P("<i>Drop by Drop: Zuri's Mission</i> is a third-person runner built in Unity 6 with the Universal "
          "Render Pipeline. The player guides Zuri, a young girl from a drought-stricken village, as she "
          "collects water and building materials and repairs the water infrastructure that her community "
          "depends on. The game is divided into three connected stages: the Dry Bushlands (Level 1), the "
          "Mudlands (Level 2) and the Pipe Repair Sprint (Level 3). Progress is shown on screen as a "
          "Village Restoration percentage that rises from 0% to 100% across the three stages."),
        P("This report documents my contribution to Task 2 of the module (The Independent Institute of "
          "Education, 2026). It covers Phase 3, which asks for the core mechanics prototype to be tested, "
          "a tutorial level to be built and a developed game engine prototype to be produced, and Phase 4, "
          "which asks for a complete game with a defined start and end that engages meaningfully with a "
          "community problem. The report explains what I worked on, what the team changed and added, the "
          "challenges we encountered and how they were fixed, and what the project achieved."),
        P("The game was developed in a team of two. My teammate, Belinda Mtabane, built much of the Level 2 "
          "and Level 3 gameplay logic, the shared runner services and the team game design document (GDD). "
          "My focus was the user interface and player feedback, environment art and level dressing, "
          "character animation, audio, the finish sequences and overall integration and bug fixing. The "
          "Git history of the project is used throughout as evidence of this work, and my commits are "
          "listed in Appendix B."),
    ]


def section_problem():
    return [
        H1("2. Community Problem and Design Intent"),
        P("The community problem at the centre of the game is access to water. Globally, 1.8 billion people "
          "live in households that have to collect drinking water from sources off the premises, and in "
          "seven out of ten of those households women and girls are primarily responsible for carrying it "
          "(UNICEF and WHO, 2023). The burden is heaviest in sub-Saharan Africa, where nearly half of the "
          "population still relies on water collection and women are about four times as likely as men to "
          "fetch water (UNICEF and WHO, 2023). Time spent walking to and from water sources is time lost to "
          "school, work and rest. Zuri, a girl carrying a bucket through heat, mud and storms so that her "
          "village can have water, is a direct representation of this burden."),
        P("Bogost (2007) describes <i>procedural rhetoric</i>: games persuade through the rules and "
          "processes they model rather than through text alone. We applied this idea by making water a "
          "resource the player has to manage rather than a fact the player is told. Zuri's body hydration "
          "drains continuously and faster during heat waves, the bucket she carries spills when she is hit, "
          "and the level can only be completed if enough water and materials reach the village. The player "
          "experiences scarcity through play. Flanagan (2009) argues that games can be used to raise "
          "critical awareness of social issues when the play itself carries the message; our aim was for "
          "the player to feel the effort and risk involved in collecting water without the game becoming a "
          "lecture."),
        P("The brief also requires the game to carry the emotional weight of the problems faced by "
          "disadvantaged communities without necessarily evoking happy emotions (The Independent Institute "
          "of Education, 2026). The game balances pressure and hope: losing states are tied to dehydration, "
          "injury and running out of time, while success is shown visibly through houses, water tanks and a "
          "restoration percentage that grows as Zuri works. The team's four design pillars were <i>teach, "
          "then test</i>; <i>water is life</i>; <i>visible restoration</i>; and <i>fair difficulty</i>."),
    ]


def section_phase3():
    s = [H1("3. Phase 3: Tutorial Level and Game Engine Prototype")]

    s += [
        H2("3.1 Testing the Core Mechanics Prototype and Responding to Feedback"),
        P("Before extending the game, the team audited the core mechanics prototype against the intended "
          "design in a gap analysis (August 2026). The audit found that movement was split across three "
          "competing scripts, that two separate objects owned the HUD and health values, that there was no "
          "game audio system at all, that a legacy spawner instantiated objects every frame, that the "
          "Level 3 pipe triggers stopped working after the first hit, and that the Level 3 scene was "
          "referred to by two different names. As the brief requires, a copy of the prototype was kept on "
          "a separate <font face='Courier'>backup</font> branch in Git so that the team could always revert."),
        P("My first round of testing (May 2026) fixed defects that made meaningful playtesting impossible. "
          "The legacy spawner was given a spawn interval so that it no longer created ten objects every "
          "frame; a null check in the HUD trigger handler was moved so that the HUD reference was actually "
          "resolved; ground detection was corrected so that the grounded flag was not reset immediately "
          "after being set; the speed-boost timer, which could never expire, was replaced with a coroutine; "
          "and the Level 3 pipe latch was reset when the player left a collider so that later repairs "
          "registered."),
        P("From that point, development followed an iterative, playtest-driven loop: I played the affected "
          "level in the Unity editor after each change, noted what felt wrong or looked broken, fixed it and "
          "played again. Fullerton (2019) describes this playcentric process, in which playtesting informs "
          "every iteration rather than being left until the end. Most of the issues described in Section 6 "
          "were found this way."),
        P(f"{ph('Summarise the lecturer feedback received on the core mechanics prototype here, and state which of the changes in this report respond to each point.')}"),
    ]

    phases = [
        ["Phase", "Run progress", "What the player learns"],
        ["Learn", "0 to 25%", "Moving across the path, jumping, cactus water and basic obstacles. No snakes or heat waves yet."],
        ["Practice", "25 to 50%", "Heat waves, snakes and managing body water against bucket water."],
        ["Combine", "50 to 75%", "Mixed hazards, rolling logs and recovery pickups."],
        ["Challenge", "75 to 100%", "Higher hazard frequency, always with a safe route and a recovery source nearby."],
    ]
    s += [
        H2("3.2 Tutorial Level Design"),
        P("Level 1, the Dry Bushlands, is the tutorial. It is structured in four phases so that one idea is "
          "introduced at a time before ideas are combined (Table 1). This follows the approach of "
          "<i>Portal</i> (Valve Corporation, 2007), which the brief uses as its example: each new mechanic "
          "is introduced in a safe situation and then tested in a harder one, so the player keeps learning "
          "without feeling that they are being taught."),
        table(phases, [2.6 * cm, 2.8 * cm, 10.6 * cm]),
        table_caption("Table 1: Level 1 teaching phases"),
        P("To avoid spoon-feeding, the tips are short and contextual. Each tip appears only when the "
          "relevant object is about to be met (for example, <i>Snake ahead. Dodge with A or D</i> appears "
          "as the first snake approaches), and the tips stop at 25% of the run, after which the player "
          "relies on what they have learnt and on the obstacle guide in the HUD. Hodent (2017) recommends "
          "this kind of onboarding: teaching in context and limiting how much new information the player "
          "has to hold at once. The level also does not start until the player presses a key, with Zuri "
          "standing in an idle carrying pose, so players begin when they are ready. Schell (2019) "
          "describes this as shaping the <i>interest curve</i>: a gentle hook, then rising tension with "
          "moments of relief."),
        P("The community problem is introduced from the first screen. The start screen invites the player "
          "to <i>help Zuri bring water to her village</i>, the first objective is to collect materials to "
          "build the village well, houses and water tanks line the road, and the Village Restoration "
          "percentage is visible throughout. The player therefore understands what the game is about "
          "before the first obstacle appears."),
    ]

    s += [
        H2("3.3 Ending the Tutorial on a Cliffhanger"),
        P("Level 1 ends with the village only 35% restored: the well is built, but the job is clearly "
          "unfinished. When Zuri reaches the finish she walks into a courtyard of houses arranged in a U "
          "shape around two red water tanks and performs a <i>Tender Placement</i> animation, setting her "
          "bucket down. Only after that animation completes does the Level Complete panel appear "
          "(Figure 1). The story text for each stage ending, including the news at the end of Level 2 that "
          "the village water pipes have burst, is written in an end-of-level dialogue component that leads "
          "into the Level 3 repair sprint. Koster (2013) argues that enjoyment in games comes from "
          "mastering patterns; ending each stage with the current pattern mastered and a new, unsolved "
          "problem announced gives the player a reason to continue."),
        figure_pair("now_level1_village.png", "now_level1_finish.png", 7.8,
                    "Figure 1: The current Level 1 finish. Zuri runs into the U-shaped courtyard of houses "
                    "with two red water tanks (left), and the Level Complete panel appears once her finish "
                    "animation has played (right). Source: own screenshots, 30 September 2026.",
                    labels=("Reaching the courtyard", "Level Complete panel")),
    ]

    mech = [
        ["Mechanic", "Stage", "Rule", "Purpose"],
        ["Body water and bucket water", "All", "Body water drains over time; the bucket spills when Zuri is hit.", "Makes scarcity felt through play."],
        ["Heat waves", "1", "A full-screen heat haze warns of the wave, which drains body water quickly.", "Teaches rationing and timing."],
        ["Cactus hydration and water springs", "1 and 2", "Passing a cactus or standing at a spring refills water; springs sprout when Zuri approaches.", "Recovery and reward."],
        ["Building materials", "1 and 2", "Sand bags, bricks and hammers are collected toward the well and infrastructure.", "Visible restoration."],
        ["Snakes, rolling logs, rocks and sand pits", "1", "Dodge sideways or jump.", "Core movement skills."],
        ["Wolf attacks", "All", "Wolves appear at random points, and one chases Zuri from behind when time is running out.", "Tension near the deadline."],
        ["Mud monsters", "2 and 3", "Monsters pop up from mud wallows and splash mud around them.", "Reaction and lane choice."],
        ["Acid rain, lightning and fog", "3", "Acid falls from random directions; lightning damages pipes and creates maintenance work.", "The build-and-maintain theme."],
        ["Pipe and tank repair", "3", "Materials are spent at repair stacks to fill three tanks.", "The final restoration goal."],
        ["Level timer", "All", "A countdown runs; reaching zero ends the run.", "Pressure and pacing."],
        ["Win threshold", "1 and 2", "The stage is won once water, materials and hydration pass 60%.", "Reduced frustration from the original 100% rule."],
    ]
    s += [
        H2("3.4 Auxiliary Mechanics and Game Rules"),
        P("Phase 3 required all auxiliary mechanics and rules that were missing from the prototype to be "
          "implemented. Table 2 lists the mechanics now in the game. Salen and Zimmerman (2004) describe a "
          "game's rules as a formal system that produces the player's experience; each rule below was "
          "chosen because it reinforces the water theme or the pacing of the run."),
        table(mech, [3.4 * cm, 1.5 * cm, 6.4 * cm, 4.7 * cm]),
        table_caption("Table 2: Auxiliary mechanics and game rules"),
        P("The win threshold was lowered from 100% to 60% of the collected water, materials and hydration "
          "because requiring every resource to be full felt punishing in our testing, and a single "
          "mistake near the finish could undo a whole run. The lower threshold keeps the goal meaningful "
          "while allowing players to recover from errors."),
    ]

    grad = [
        ["Stage", "Environment", "Difficulty", "Objective"],
        ["1: Dry Bushlands", "Dry, sparse desert with little green", "Easy", "Collect water and building materials"],
        ["2: Mudlands", "Medium green with more plants and animals", "Harder", "Collect more water and more materials"],
        ["3: Pipe Repair Sprint", "Greenest, with fog and storms", "Hardest", "Build and maintain the water system"],
    ]
    s += [
        H2("3.5 Level Design"),
        P("Early versions of the levels placed most props by hand, which crowded the scenes and made the "
          "game stall. I moved the roadside scenery to runtime streaming, so that trees, rocks, houses and "
          "plants are spawned with each ground tile and removed behind the player. Scatter rules keep every "
          "prop outside a clearance band around the playable path, and an overlap resolver moves any prop "
          "that intersects another. The path was widened and the player can move freely across it, which "
          "gives players a variety of ways to handle each obstacle, as the brief requires."),
        P("The three stages follow a deliberate gradient in environment, difficulty and objective "
          "(Table 3). The environment becomes greener as the village's water situation improves, so the "
          "world itself reflects the player's progress."),
        table(grad, [3.6 * cm, 5.0 * cm, 2.2 * cm, 5.2 * cm]),
        table_caption("Table 3: Difficulty and environment gradient across the three stages"),
        P("Totten (2019) describes how architecture can guide players without instructions. Houses line "
          "the road and face it, framing the route, and each stage ends in a U-shaped courtyard of houses "
          "facing the finish, which draws the eye to the goal. Each house has a water tank beside it, "
          "because storing water is part of the problem the game represents. The tanks are wrapped with a "
          "branded label (red in Level 1, green in Level 2 and blue in Level 3, with three green tanks at "
          "the final finish) so that they read as real household water tanks rather than plain cylinders."),
        P("Figures 2 and 3 compare the early prototype environments from May 2026 with Levels 2 and 3 as "
          "they look now. The flat orange blocks and bare cliff walls have been replaced by a dressed road "
          "lined with houses, branded tanks, trees and hazards, and each stage has its own obstacle guide."),
        figure_pair("before_prototype_may_a.png", "now_level2_run.png", 7.8,
                    "Figure 2: Before and after. An early prototype build in May 2026 with block cliffs and "
                    "flat colour strips (left), and Level 2, the Mudlands, now, with green tanks, houses, a "
                    "rolling-log warning and the mud and warthog obstacle guide (right). "
                    "Source: own screenshots.",
                    labels=("Before: May 2026", "After: 30 September 2026")),
        figure_pair("before_prototype_may_b.png", "now_level3_later.png", 7.8,
                    "Figure 3: Before and after. An early prototype canyon from May 2026 (left), and Level 3, "
                    "the Pipe Repair Sprint, now, with fog, blue tanks, pipe repair stacks, the tank progress "
                    "panel and the pipes-to-fix counter (right). Source: own screenshots.",
                    labels=("Before: May 2026", "After: 30 September 2026")),
    ]

    s += [
        H2("3.6 Basic User Interface and Interaction"),
        P("The prototype's UI was hard to read and parts of it overlapped. I rebuilt the gameplay HUD around "
          "a consistent wood-and-parchment style using the Adventure UI Kit, with white text, outlines and "
          "fixed font sizes so that labels no longer shrink or disappear. The main elements are:"),
    ]
    s += bullets([
        "a status panel with icon meters for health, body water, bucket water and materials, and a Village "
        "Restoration bar;",
        "a countdown timer in a wood badge at the top right;",
        "an <b>obstacle guide</b> at the bottom left that shows images of the hazards in the current stage, "
        "each of which changes colour when Zuri is hit by that hazard, so the player sees the cause of the "
        "damage immediately;",
        "a Level 3 status panel showing health, materials used and gained, the number of pipes that need "
        "repairing after lightning strikes, and the progress of each tank at the top right;",
        "short feedback messages under the status panel for events such as collecting water or heat waves;",
        "a sound on/off button on the gameplay screen and a volume slider in the settings menu;",
        "pause, victory and loss screens that state the reason for a loss and offer Main Menu and level buttons;",
        "a new start screen (Figure 4), replacing an earlier image with a spelling error in the title.",
    ])
    s += [
        P("Hodent (2017) stresses that clear, immediate feedback reduces confusion and helps players learn "
          "from their mistakes. The obstacle guide and loss-reason messages were designed with that in mind."),
        figure_pair("before_start_screen.png", "now_start_screen.png", 7.8,
                    "Figure 4: Before and after. The August 2026 start screen, with the misspelt title "
                    "<i>IROP by DROP: Zuri's Journey</i>, a single New Game button and a loading bar over the "
                    "artwork (left), and the current start screen with the corrected title and a menu panel "
                    "for New Game, Level 2, Level 3, Settings and Quit (right). Both backgrounds were generated "
                    "with Google Gemini (Google, 2026). Source: own screenshots.",
                    labels=("Before: August 2026", "After: 30 September 2026")),
    ]

    s += [
        H2("3.7 Refined Assets and Animations"),
        P("Phase 3 asked for the Task 1 assets to be refined and animated. The primitive shapes of the "
          "prototype were replaced with low-poly asset packs: cacti, logs, rocks, bushes, flowers and trees "
          "from the Pandazole Nature pack, maple trees with a wind animation, roadside cliffs and boulders, "
          "desert village houses, and animated animals and enemies (Figure 5). Zuri herself was replaced: the "
          "prototype character in a pink top with a bucket on her head became a textured, animated model "
          "carrying the bucket in her hands. A full list of third-party assets is given in Appendix A."),
        figure_pair("fig_level1_august.png", "now_level1_run.png", 7.8,
                    "Figure 5: Before and after. Level 1 in August 2026 with capsule cacti, plain cliffs and "
                    "the prototype character (left), and Level 1 now with low-poly cacti, trees and rocks, "
                    "the new Zuri model, the objectives panel, status panel, timer and obstacle guide (right). "
                    "Source: own screenshots.",
                    labels=("Before: August 2026", "After: 30 September 2026")),
        P("Zuri now has four animations: idle carrying, running carrying, walking carrying and Tender "
          "Placement. All four share Zuri's own humanoid avatar, which lets Unity retarget the clips "
          "cleanly onto the skeleton (Unity Technologies, 2026e). The bucket is held in her hands and shows "
          "a live water level that rises and falls with the bucket value, spilling droplets when she is hit. "
          "Other animated elements include the mud monsters' bounce and pop-up, the wolves' run and attack, "
          "water springs that burst into bubbles, mist and spray when Zuri approaches, and a green splash "
          "when she hydrates at a cactus."),
        P("The branded tank wraps needed their own mesh. Unity's default cylinder stretches a texture around "
          "the full circumference, which made the lettering wide and unreadable. I generated a cylinder "
          "mesh in code (Unity Technologies, 2026d) whose texture coordinates keep the image's aspect ratio "
          "and centre the lettering on the side facing the road (Figure 6)."),
        figure("fig_yoyo_green.png", 9.5,
               "Figure 6: Green tank wrap texture used on the Level 2 tanks. The custom mesh wraps it around "
               "the tank without stretching the lettering."),
    ]

    s += [
        H2("3.8 Basic Game Systems"),
        P("The game systems track and run the rules at runtime. A single player-resources component owns "
          "health, body water, bucket water and materials, which removed the duplicated HUD and health "
          "values found in the gap analysis. A village progress service converts those resources into the "
          "restoration percentage for each stage, and a run-state manager controls the Playing, Paused, Dead "
          "and Victory states and moves the player between scenes. A scene bootstrapper adds the correct "
          "systems to each gameplay scene, and a layout director for each level places pickups and hazards "
          "at planned points in the run instead of spawning them randomly every frame. Nystrom (2014) "
          "recommends keeping a single, clearly owned instance of each global service and building behaviour "
          "from small components; following that approach made the systems easier to debug. The Level 3 "
          "tank repair system spends materials at repair stacks and fills three tanks, which is the "
          "final win condition."),
    ]

    sounds = [
        ["Game event", "Sound"],
        ["Menus and gameplay music; Level 3 music", "game sound; third level sound"],
        ["Button presses and moving to the next level", "buttonClick; Nextlevelbutton"],
        ["Collecting water or materials", "watercollection pickup; pickup"],
        ["Interacting with a water source or hydrating at a cactus", "bouncing-on-the-water"],
        ["Water springs (positional loop heard near each spring)", "spring water sound"],
        ["Mud patches and mud monsters", "mud monster sound"],
        ["Obstacle hits, warthog and wolf attacks", "obstacle sound; wathog attack sound; wolf attack sound"],
        ["Pipe repair and maintenance", "maintainance sound"],
        ["Lightning strikes in Level 3", "thunderstorm_pouring-rain"],
        ["Fog in Level 3 (ambient loop)", "wind-through-the-leaves"],
    ]
    s += [
        H2("3.9 Camera, Lighting, Audio and Other Global Systems"),
        P("Audio was missing entirely from the prototype. I added a game audio manager with separate "
          "channels for music, effects, ambience and storms. Each event in the game is mapped to a clip "
          "named for its purpose (Table 4). Short cooldowns stop clips from repeating too quickly, spring "
          "sounds play in 3D so that they grow louder as Zuri approaches, and the storm sound fades out "
          "after each lightning strike. The mute button and volume slider control the global listener "
          "volume (Unity Technologies, 2026a), so a single setting affects every sound. Collins (2008) "
          "notes that game sound works as feedback as well as atmosphere; here it tells the player when "
          "water has been collected, when danger is near and when the weather is changing."),
        table(sounds, [9.0 * cm, 7.0 * cm]),
        table_caption("Table 4: Contextual sound mapping"),
        P("Particle systems provide the heat haze in Level 1, mud splashes in Level 2, and rain, acid rain, "
          "lightning and roadside fog in Level 3, together with the spring bubbles, mist and spray on water "
          "sources. The heat effect was redesigned twice: a solid red tint and then a gradient were both "
          "rejected, and the final version is a soft full-screen haze. The camera follows behind Zuri. "
          "Turning on Rigidbody interpolation (Unity Technologies, 2026f) removed a visible jitter between "
          "physics and rendering, which matters because smooth, responsive motion is central to how a game "
          "feels (Swink, 2009). Most art, audio and UI assets are loaded at runtime through Unity's "
          "Resources system (Unity Technologies, 2026c), which let the game build its levels in code."),
    ]

    s += [
        H2("3.10 The Developed Game Engine Prototype"),
        P("With these systems in place, the developed prototype can be played from the start screen through "
          "the tutorial and into the later stages. The player moves Zuri across the path, jumps, collects "
          "water and materials, manages hydration against the heat, dodges animals and hazards, sees the "
          "cause of every hit in the obstacle guide, adjusts or mutes sound, pauses and restarts, and watches "
          "the village visibly improve. This prototype formed the base for the complete game described in "
          "Section 4."),
    ]
    return s


def section_phase4():
    return [
        H1("4. Phase 4: The Complete Game"),
        H2("4.1 A Defined Start and End"),
        P("The brief asks for a full single-level game with a defined start and end. We treated the three "
          "stages as one continuous mission. The flow is: Start Screen, Level 1 (tutorial), Level 2, "
          "Level 3, and a Mission Complete screen that returns the player to the Start Screen. The game has "
          "several endings: the player can lose through dehydration, loss of health or running out of time, "
          "or by reaching a finish without enough resources, and each loss screen states the reason. The "
          "victory ending confirms that clean water now reaches every home in the village."),
        H2("4.2 Is the Game a Complete Experience?"),
        P("The game has an introduction (start screen and tutorial), rising difficulty across three stages, "
          "a climax (the timed repair sprint with storms, fog and monsters) and a resolution (the Mission "
          "Complete screen). Every stage has clear objectives, visible progress, feedback for success and "
          "failure, sound, and a finish sequence. In Schell's (2019) terms, the experience has a complete "
          "interest curve, from a gentle opening to a final peak and resolution."),
        H2("4.3 Does it Make the Player Aware of a Meaningful Problem?"),
        P("The problem is communicated mainly through play, as Bogost (2007) recommends. The player spends "
          "the whole game carrying water, rationing hydration, protecting a bucket that spills, and turning "
          "collected resources into wells, tanks and pipes. The Village Restoration percentage and the "
          "houses and tanks along the road link the player's effort to the community's benefit. The "
          "setting and protagonist reflect the finding that the burden of water collection falls mostly on "
          "women and girls (UNICEF and WHO, 2023)."),
        H2("4.4 Hosting on itch.io"),
        P("The brief requires the game to be hosted on itch.io and to receive a community score. The plan is "
          "to create a Unity build (WebGL for browser play, or a zipped Windows build), upload it to an "
          "itch.io project page and, for WebGL, mark it as playable in the browser (itch.io, 2026)."),
        P(f"<b>itch.io page:</b> {ph('insert itch.io URL')}<br/>"
          f"<b>Community score and number of ratings:</b> {ph('insert score and number of ratings')}<br/>"
          f"<b>Summary of community comments:</b> {ph('summarise player comments and any changes made in response')}"),
    ]


def section_changes():
    rows = [
        ["Period", "What I worked on"],
        ["May 2026",
         "Fixed gameplay bugs: the spawner creating objects every frame, a HUD null check, ground detection, the "
         "speed-boost timer and the Level 3 pipe latch. Added gameplay UI systems, lane warning effects, pit "
         "obstacles, a game-over screen and a Zuri-voiced information feed. Fixed the UI overlay system and "
         "level flow so that singletons re-register on every scene load."],
        ["August 2026",
         "Moved UI art into Resources and wrote an editor tool that slices the UI sheet into sprites. Added the "
         "first player run animation by retargeting a humanoid rig. Scattered wells, boreholes, houses and NPCs "
         "across all three levels. Gave the start, victory, loss and end-of-level screens a wood-panel look. "
         "Added the level countdown timer and redesigned the status panel with icon meters. Added the live bucket "
         "water level with droplet spills and removed a broken heat particle effect. Looped desert ambience "
         "through Level 1."],
        ["September 2026",
         "Replaced primitive props with low-poly asset packs across all levels and added runtime roadside "
         "streaming with lane clearance. Created water sources as small dams with spring, mist and spill effects. "
         "Added the new Zuri character and her idle, run, walk and Tender Placement animations, with the bucket "
         "in her hands. Added wolves, mud monsters and animals. Rebuilt the HUD with the Adventure UI Kit, the "
         "obstacle guide, the Level 3 status panel and the sound controls. Added the game audio manager and all "
         "contextual sounds. Built U-shaped village finishes with branded water tanks. Added Level 3 rain, acid "
         "rain, lightning and fog. Fixed scene loading, animation and material problems (Section 6)."],
    ]
    return [
        H1("5. Summary of Changes and Additions"),
        P("Table 5 summarises my work by period. The full commit list is in Appendix B."),
        table(rows, [2.8 * cm, 13.2 * cm]),
        table_caption("Table 5: Summary of my changes and additions by period"),
    ]


def section_challenges():
    rows = [
        ["Challenge", "Cause", "Fix"],
        ["HUD text was invisible, tiny or stretched, and panels overlapped.",
         "Text auto-sizing shrank labels until they looked blank, some labels kept a near-black default colour, and "
         "mixing stretch anchors with fixed sizes collapsed text boxes to zero height.",
         "Fixed font sizes, a shared text style (white with outlines) and a layout helper with consistent anchors "
         "for every HUD panel."],
        ["New props, trees and monsters rendered magenta or pink.",
         "The asset packs used Built-in pipeline shaders, which URP cannot render.",
         "Materials were converted to URP Lit (Unity Technologies, 2026b), with natural colour fallbacks where "
         "a texture was missing."],
        ["Boulders and houses blocked the lanes, and some pickups spawned inside rocks.",
         "Houses started inside the boulder band, and wide meshes spilled into the path.",
         "A clearance band around the path, footprint limits on large rocks and an overlap resolver that pushes "
         "props apart (Figure 8)."],
        ["The grey Unity void was visible beside the road.",
         "The ground and roadside dressing did not extend far enough (Figure 7).",
         "Wider ground coloured to match each biome, and denser trees, cliffs and houses streamed along both sides."],
        ["Zuri's animation jittered, her feet bent and her legs sometimes disappeared.",
         "The clips used separate avatars, loop blending was off, Rigidbody interpolation was off, and foot IK "
         "over-corrected the retargeted skeleton.",
         "One shared Zuri avatar for all clips, loop blending, Rigidbody interpolation (Unity Technologies, "
         "2026f), and foot IK disabled."],
        ["The next-level screen cut off the Tender Placement animation.",
         "The screen appeared after a fixed 1.8-second wait, but the clip is longer.",
         "The finish sequence now waits for the animation to finish and holds the final pose before showing the "
         "screen."],
        ["Level 3 would not load, or froze on the loading screen.",
         "Asynchronous scene activation (Unity Technologies, 2026g) stalled, and the scene did too much "
         "environment work on load.",
         "A reliable scene-loading path with the loading screen kept, and a lighter environment start-up."],
        ["The heat overlay drew a hard line across the screen.",
         "A top-of-screen gradient ended at a visible edge.",
         "A soft full-screen haze that fades in and out with the heat wave."],
        ["The lettering on the tanks was stretched and unreadable.",
         "Unity's default cylinder wraps the image around the full circumference.",
         "A custom cylinder mesh with aspect-correct texture coordinates (Unity Technologies, 2026d)."],
        ["Pushing roughly 700 MB of new assets to GitHub.",
         "Large asset packs, including one 64 MB texture above GitHub's 50 MB recommendation.",
         "The work was committed in logical stages and pushed in two batches. Git LFS is recommended for very "
         "large files (GitHub, Inc., 2026)."],
    ]
    return [
        H1("6. Challenges and Fixes"),
        P("Table 6 summarises the main challenges, their causes and how they were fixed. Most were found "
          "during the playtesting loop described in Section 3.1."),
        table(rows, [4.4 * cm, 5.8 * cm, 5.8 * cm]),
        table_caption("Table 6: Challenges and fixes"),
        figure_pair("fig_grey_void.png", "now_level2_later.png", 7.8,
                    "Figure 7: Before and after the ground fix. An early roadside test on 16 September 2026 "
                    "with the grey Unity void exposed beyond the road edge (left), and Level 2 now, where "
                    "biome-coloured ground, houses, trees and rocks continue to the horizon (right). "
                    "Source: own screenshots.",
                    labels=("Before: 16 September 2026", "After: 30 September 2026")),
        figure_pair("fig_cliff_intrusion.png", "now_level1_midrun.png", 7.8,
                    "Figure 8: Before and after the clearance band. A roadside cliff intruding into the "
                    "playable path on 16 September 2026 (left), and Level 1 now, where cliffs and houses sit "
                    "outside the path and only intended obstacles, such as the log ahead, block the way "
                    "(right). The warm tint is the heat-wave haze, and the status panel shows the critical "
                    "water warning. Source: own screenshots.",
                    labels=("Before: 16 September 2026", "After: 30 September 2026")),
    ]


def section_successes():
    return [
        H1("7. Successes"),
    ] + bullets([
        "<b>A playable mission from start to finish.</b> The game runs from the start screen through all three "
        "stages to a Mission Complete ending, with clear win and loss states.",
        "<b>A tutorial that teaches through play.</b> Level 1 introduces one mechanic at a time with short, "
        "contextual tips, supported by the obstacle guide.",
        "<b>A readable, consistent interface.</b> The HUD, menus and end screens share one visual style, and "
        "the obstacle guide shows the cause of every hit.",
        "<b>A world that reflects progress.</b> The environment becomes greener and the village visibly more "
        "complete from stage to stage, with houses and branded tanks along the road and at each finish.",
        "<b>Sound as feedback.</b> The game went from silent to having music, ambience, weather and contextual "
        "effects, with a mute button and volume control.",
        "<b>A believable protagonist.</b> Zuri has a textured model, smooth animations, a bucket in her hands "
        "that shows its water level, and a finish animation that closes each stage.",
        "<b>A clean, staged version history.</b> All work is in GitHub in logical commits, with a backup branch "
        "of the earlier prototype.",
    ])


def section_limitations():
    return [
        H1("8. Limitations and Future Work"),
    ] + bullets([
        "The Level 2 introduction text still describes a river crossing with floodgates and cars, which no "
        "longer matches the Mudlands stage and should be rewritten.",
        "The end-of-level story dialogue is written but not yet triggered at the finish, which currently "
        "shows a general Level Complete message (Figure 1). Connecting it would make each cliffhanger explicit.",
        "There are no mid-run checkpoints or saved progress, both of which the gap analysis identified; a "
        "failed run restarts the stage.",
        "The playtesting described here was carried out by the team. Structured playtests with outside players, "
        "and the itch.io community score, will give stronger evidence of whether the game communicates its "
        "message.",
        "Some intermediate commits were grouped by file rather than by feature, so only the final version was "
        "compiled and tested as a whole.",
        "The 64 MB water particle texture should be moved to Git LFS or compressed to keep the repository "
        "healthy (GitHub, Inc., 2026).",
    ])


def section_conclusion():
    return [
        H1("9. Conclusion"),
        P("Across Phase 3 and Phase 4, the project grew from a core mechanics prototype with no audio, "
          "competing systems and primitive shapes into a complete three-stage mission with a tutorial, a "
          "consistent interface, animated characters, a streamed and dressed environment, contextual sound "
          "and a defined start and end. My work focused on the parts the player sees, hears and touches: "
          "the HUD and feedback, the environment and village, Zuri's animation, audio, and the finish "
          "sequences, together with the bug fixing and integration that made them work together. The "
          "central design choice, letting the rules of the game carry the message about water scarcity "
          "(Bogost, 2007), guided these decisions. Hosting on itch.io and gathering community feedback are "
          "the final steps."),
        PageBreak(),
    ]


def references():
    refs = [
        "Bogost, I., 2007. <i>Persuasive games: The expressive power of videogames</i>. Cambridge, MA: MIT Press.",
        "Collins, K., 2008. <i>Game sound: An introduction to the history, theory, and practice of video game "
        "music and sound design</i>. Cambridge, MA: MIT Press.",
        "Flanagan, M., 2009. <i>Critical play: Radical game design</i>. Cambridge, MA: MIT Press.",
        "Fullerton, T., 2019. <i>Game design workshop: A playcentric approach to creating innovative games</i>. "
        "4th ed. Boca Raton: CRC Press.",
        "GitHub, Inc., 2026. <i>About Git Large File Storage</i>. [online] Available at: "
        "&lt;https://docs.github.com/en/repositories/working-with-files/managing-large-files/"
        f"about-git-large-file-storage&gt; [Accessed {ACCESSED}].",
        "Google, 2026. <i>Gemini</i>. [generative AI image tool] Available at: &lt;https://gemini.google.com&gt; "
        f"[Accessed {ACCESSED}].",
        "Hodent, C., 2017. <i>The gamer's brain: How neuroscience and UX can impact video game design</i>. "
        "Boca Raton: CRC Press.",
        "itch.io, 2026. <i>Uploading HTML5 games</i>. [online] Available at: "
        f"&lt;https://itch.io/docs/creators/html5&gt; [Accessed {ACCESSED}].",
        "Koster, R., 2013. <i>A theory of fun for game design</i>. 2nd ed. Sebastopol: O'Reilly Media.",
        "Nystrom, R., 2014. <i>Game programming patterns</i>. [s.l.]: Genever Benning.",
        "Salen, K. and Zimmerman, E., 2004. <i>Rules of play: Game design fundamentals</i>. Cambridge, MA: "
        "MIT Press.",
        "Schell, J., 2019. <i>The art of game design: A book of lenses</i>. 3rd ed. Boca Raton: CRC Press.",
        "Swink, S., 2009. <i>Game feel: A game designer's guide to virtual sensation</i>. Burlington: Morgan "
        "Kaufmann.",
        "The Independent Institute of Education, 2026. <i>Module manual: XBCGD7312</i>. Sandton: The "
        "Independent Institute of Education.",
        "Totten, C.W., 2019. <i>An architectural approach to level design</i>. 2nd ed. Boca Raton: CRC Press.",
        "UNICEF and WHO, 2023. <i>Progress on household drinking water, sanitation and hygiene 2000-2022: "
        "Special focus on gender</i>. [pdf] New York: United Nations Children's Fund and World Health "
        f"Organization. Available at: &lt;https://washdata.org/report/jmp-2023-wash-households&gt; [Accessed {ACCESSED}].",
        "Unity Technologies, 2026a. <i>AudioListener.volume</i>. [online] Available at: "
        "&lt;https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AudioListener-volume.html&gt; "
        f"[Accessed {ACCESSED}].",
        "Unity Technologies, 2026b. <i>Convert shaders to URP with the Render Pipeline Converter</i>. [online] "
        "Available at: &lt;https://docs.unity3d.com/6000.0/Documentation/Manual/urp/upgrading-your-shaders.html&gt; "
        f"[Accessed {ACCESSED}].",
        "Unity Technologies, 2026c. <i>Introduction to the Resources system</i>. [online] Available at: "
        "&lt;https://docs.unity3d.com/6000.0/Documentation/Manual/LoadingResourcesatRuntime.html&gt; "
        f"[Accessed {ACCESSED}].",
        "Unity Technologies, 2026d. <i>Mesh</i>. [online] Available at: "
        f"&lt;https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Mesh.html&gt; [Accessed {ACCESSED}].",
        "Unity Technologies, 2026e. <i>Retarget Humanoid animations</i>. [online] Available at: "
        f"&lt;https://docs.unity3d.com/6000.0/Documentation/Manual/Retargeting.html&gt; [Accessed {ACCESSED}].",
        "Unity Technologies, 2026f. <i>Rigidbody.interpolation</i>. [online] Available at: "
        "&lt;https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Rigidbody-interpolation.html&gt; "
        f"[Accessed {ACCESSED}].",
        "Unity Technologies, 2026g. <i>SceneManagement.SceneManager.LoadSceneAsync</i>. [online] Available at: "
        "&lt;https://docs.unity3d.com/6000.0/Documentation/ScriptReference/"
        f"SceneManagement.SceneManager.LoadSceneAsync.html&gt; [Accessed {ACCESSED}].",
        "Valve Corporation, 2007. <i>Portal</i>. [digital game] PC. Bellevue: Valve Corporation. Available at: "
        f"&lt;https://store.steampowered.com/app/400/Portal/&gt; [Accessed {ACCESSED}].",
    ]
    return [H1("Reference List")] + [P(r, "ref") for r in refs] + [PageBreak()]


def appendix_assets():
    rows = [
        ["Asset", "Used for"],
        ["Pandazole Ultimate Pack (Pandazole Nature Environment Pack)", "Cacti, logs, rocks, bushes, grass, flowers and trees"],
        ["HQ Autumn Dry Maple Trees", "Roadside maple trees with wind animation"],
        ["AGWYN's Low Poly Cliffs", "Roadside cliffs"],
        ["GameDev Starter Kit - Farming (AssetHunts!)", "Roadside boulders and the grain silo"],
        ["Desert Village (PolyRonin)", "Village houses and clay jars"],
        ["Big Water Tower", "Level 3 water towers"],
        ["AQUIS Water Toon Shader (AureDevGames)", "Water surfaces on the springs and dams"],
        ["Stylized Water Effects (NamuFX)", "Spring bubbles, mist, spray and splashes"],
        ["FX Lightning II Free (FX_Kandol_Pack)", "Level 3 lightning"],
        ["Fog Particles", "Level 3 roadside and road fog"],
        ["RPG Monster DUO PBR Polyart", "Mud monsters"],
        ["Realistic Furry Wolf (free sample)", "Wolf enemies"],
        ["Pig Funny and Fried Lite", "Village animals"],
        ["City People (Denys Almaral)", "Village NPCs"],
        ["Low Poly Casual Bucket", "Zuri's bucket"],
        ["Adventure UI Kit", "HUD panels, buttons, bars and icons"],
        ["Zuri character model and animations", f"Player character {ph('add source of model and animations')}"],
        ["Sound effects and music", f"All game audio {ph('add source and licence of the sound clips')}"],
        ["Start screen background", "Generated with Google Gemini (Google, 2026)"],
    ]
    return [
        H1("Appendix A: Third-Party Assets"),
        P("The following third-party assets, mostly from the Unity Asset Store, were used under their "
          "respective licences. Custom scripts, level layouts, the tank wrap textures and all integration "
          "work were produced by the team."),
        table(rows, [8.0 * cm, 8.0 * cm]),
        Spacer(1, 0.4 * cm),
    ]


def appendix_commits():
    commits = [
        ["Date", "Commit message"],
        ["26 May 2026", "Fix gameplay bugs and scope AI lane control to gameplay levels"],
        ["26 May 2026", "Add gameplay UI systems, warning effects and Zuri-voiced info feed"],
        ["28 May 2026", "Fix UI overlay system, level flow, and Level 3 pipe-repair interaction"],
        ["29 May 2026", "UI assets (merged into main through pull request #1)"],
        ["19 Aug 2026", "Fix PitObstacle compile error and clean up remaining warnings"],
        ["19 Aug 2026", "Move UI art into Resources and add sprite-sheet slicing tool"],
        ["19 Aug 2026", "Add player run animation by retargeting the city NPC rig"],
        ["19 Aug 2026", "Scatter wells, boreholes, houses and NPCs across all three levels"],
        ["19 Aug 2026", "Give the start screen a consistent wood-panel look"],
        ["19 Aug 2026", "Give Lose, Victory and end-of-level screens a wood-panel look"],
        ["19 Aug 2026", "Add a level countdown timer and redesign the status panel with icon meters"],
        ["19 Aug 2026", "Fix end-screen canvas ordering and enlarge lose/victory text"],
        ["19 Aug 2026", "Add live bucket water-level and droplet-spill feedback, remove broken heat particles"],
        ["19 Aug 2026", "Loop desert ambience audio through Level 1"],
        ["19 Aug 2026", "Give the gameplay feedback toast a wood panel and move it under the status HUD"],
        ["30 Sep 2026", "Add UI kit, fog, FX, Pandazole and monster asset packs"],
        ["30 Sep 2026", "Add wolf, pig, warthog and animal asset packs"],
        ["30 Sep 2026", "Add NamuFX stylized water effects pack"],
        ["30 Sep 2026", "Add Zuri character animations and editor setup tools"],
        ["30 Sep 2026", "Add Zuri character, animal and enemy prefabs with wolf attacks"],
        ["30 Sep 2026", "Dress roadside environment with nature pack trees, flowers and boulders"],
        ["30 Sep 2026", "Add stylized water sources, spring dam and cactus hydration effects"],
        ["30 Sep 2026", "Add village houses, finish courtyards and yoyo wrapped water tanks"],
        ["30 Sep 2026", "Add game audio manager with music, effects and ambience clips"],
        ["30 Sep 2026", "Restyle HUD with adventure UI panels, obstacle guide and sound toggle"],
        ["30 Sep 2026", "Update level 1 pickups, obstacles, heat wave and finish tanks"],
        ["30 Sep 2026", "Update level 2 mud monsters, water pools and obstacle sounds"],
        ["30 Sep 2026", "Update level 3 storms, lightning, pipe repair and monster waves"],
        ["30 Sep 2026", "Update run state, scene bootstrapping, main scene and project settings"],
    ]
    return [
        H1("Appendix B: My Commit History"),
        P("Selected commits authored by me in the team repository (github.com/BelindaMtabane/ZurisMission)."),
        table(commits, [3.0 * cm, 13.0 * cm]),
    ]


def build():
    doc = ReportDoc(OUTPUT)
    story = []
    story += cover()
    story += contents()
    story += section_intro()
    story += section_problem()
    story += section_phase3()
    story += section_phase4()
    story += section_changes()
    story += section_challenges()
    story += section_successes()
    story += section_limitations()
    story += section_conclusion()
    story += references()
    story += appendix_assets()
    story += appendix_commits()
    doc.multiBuild(story)
    print("Wrote", OUTPUT)


if __name__ == "__main__":
    build()
