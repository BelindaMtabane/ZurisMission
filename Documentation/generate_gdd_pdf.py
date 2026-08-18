"""Generate Drop by Drop: Flow of Hope — Game Design Document PDF."""
from reportlab.lib import colors
from reportlab.lib.pagesizes import letter
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import inch
from reportlab.platypus import (
    HRFlowable,
    PageBreak,
    Paragraph,
    SimpleDocTemplate,
    Spacer,
    Table,
    TableStyle,
)

OUTPUT = "Drop_by_Drop_Flow_of_Hope_GDD.pdf"
TITLE = "Drop by Drop: Flow of Hope"
SUBTITLE = "Game Design Document — Full Game Flow & Systems Reference"

styles = getSampleStyleSheet()
styles.add(ParagraphStyle(name="H1", parent=styles["Heading1"], fontSize=18, spaceAfter=10, textColor=colors.HexColor("#1a5276")))
styles.add(ParagraphStyle(name="H2", parent=styles["Heading2"], fontSize=13, spaceBefore=14, spaceAfter=6, textColor=colors.HexColor("#2874a6")))
styles.add(ParagraphStyle(name="H3", parent=styles["Heading3"], fontSize=11, spaceBefore=10, spaceAfter=4, textColor=colors.HexColor("#34495e")))
styles.add(ParagraphStyle(name="Body", parent=styles["Normal"], fontSize=10, leading=14, spaceAfter=6))
styles.add(ParagraphStyle(name="GDD_Bullet", parent=styles["Normal"], fontSize=10, leading=13, leftIndent=18, spaceAfter=3))
styles.add(ParagraphStyle(name="Caption", parent=styles["Normal"], fontSize=8, textColor=colors.grey))


def P(text, style="Body"):
    return Paragraph(text.replace("\n", "<br/>"), styles[style])


def H(text, level=2):
    return P(text, f"H{level}")


def B(text):
    return P(f"• {text}", "GDD_Bullet")


def table(data, col_widths=None):
    t = Table(data, colWidths=col_widths, hAlign="LEFT")
    t.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#2874a6")),
        ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
        ("FONTNAME", (0, 0), (-1, 0), "Helvetica-Bold"),
        ("FONTSIZE", (0, 0), (-1, -1), 9),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#ebf5fb")]),
        ("GRID", (0, 0), (-1, -1), 0.5, colors.HexColor("#aed6f1")),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("LEFTPADDING", (0, 0), (-1, -1), 6),
        ("RIGHTPADDING", (0, 0), (-1, -1), 6),
        ("TOPPADDING", (0, 0), (-1, -1), 4),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 4),
    ]))
    return t


def build_story():
    s = []

    # Cover
    s.append(Spacer(1, 1.2 * inch))
    s.append(P(TITLE, "H1"))
    s.append(P(SUBTITLE, "H3"))
    s.append(Spacer(1, 0.3 * inch))
    s.append(P("<b>Genre:</b> Four-lane endless runner with resource survival and village restoration", "Body"))
    s.append(P("<b>Platform:</b> Unity (PC)", "Body"))
    s.append(P("<b>Document purpose:</b> Give the team a single reference for game flow, mechanics, level design, and technical architecture.", "Body"))
    s.append(Spacer(1, 0.5 * inch))
    s.append(P("<i>Generated from the live codebase — reflects current implementation.</i>", "Caption"))
    s.append(PageBreak())

    # 1. High concept
    s.append(H("1. High Concept", 1))
    s.append(P(
        "The player guides Zuri through three runner levels in a drought-stricken village. "
        "Each level teaches and tests a different survival skill while physically restoring "
        "the community's water infrastructure. Progress is measured as <b>Village Restoration %</b> "
        "displayed on the HUD.", "Body"))
    s.append(Spacer(1, 6))
    s.append(table([
        ["Milestone", "Village %", "Narrative meaning"],
        ["Level 1 complete", "0 → 35%", "Build the village well — collect materials, fill the bucket"],
        ["Level 2 complete", "35 → 65%", "Create the water bowl — survive mudlands hazards"],
        ["Level 3 complete", "65 → 100%", "Repair three water tanks before time runs out"],
    ], [1.4 * inch, 1.0 * inch, 3.8 * inch]))
    s.append(Spacer(1, 10))
    s.append(P("<b>Core fantasy:</b> Every drop of water collected, every material gathered, and every pipe repaired visibly restores hope to the village.", "Body"))

    # 2. Scene flow
    s.append(H("2. Scene Flow & Progression", 1))
    s.append(P("Scenes are defined in <b>SceneCatalog.cs</b>. The player journey:", "Body"))
    s.append(Spacer(1, 4))
    s.append(P("<b>StartScreen</b> → <b>MainGame (Level 1)</b> → <b>Level2</b> → <b>Level3</b> → Victory / End", "Body"))
    s.append(Spacer(1, 6))
    s.append(table([
        ["Scene", "Unity name", "Role"],
        ["Start Screen", "StartScreen", "Main menu — no gameplay bootstrap"],
        ["Level 1", "MainGame", "Desert tutorial + water survival"],
        ["Level 2", "Level2", "Mudlands hazards + warthogs"],
        ["Level 3", "Level3", "Timed pipe-repair sprint"],
        ["Info", "StarterInfor", "Optional tutorial/info screen"],
    ], [1.2 * inch, 1.3 * inch, 3.7 * inch]))
    s.append(Spacer(1, 8))
    s.append(H("Level transitions", 2))
    s.append(B("Victory panel appears when win conditions are met at the finish gate."))
    s.append(B("Player clicks <b>Next Level</b> → RunStateManager.GoToNextScene() loads the next scene."))
    s.append(B("Pause menu offers Restart, Main Menu, and dev shortcuts to Level 2/3."))
    s.append(B("Death pauses the game (timeScale = 0) and shows the lose panel with reason text."))

    # 3. Core mechanics
    s.append(H("3. Core Player Mechanics", 1))
    s.append(H("3.1 Movement (PlayerController)", 2))
    s.append(table([
        ["Input", "Action"],
        ["A / ←", "Move one lane left"],
        ["D / →", "Move one lane right"],
        ["Space", "Jump (force 15, 0.18s lock)"],
    ], [1.5 * inch, 4.7 * inch]))
    s.append(Spacer(1, 6))
    s.append(P("Four lanes (indices 0–3). Default X positions: −6, −2, 2, 6 (re-centered on ground at runtime). Player starts in lane 2. Forward speed is set per level by pacing scripts (~18–24 m/s).", "Body"))

    s.append(H("3.2 Resources (PlayerResources)", 2))
    s.append(table([
        ["Resource", "Max", "L1 start", "L2 start", "L3 start"],
        ["Health", "100", "100", "100", "100"],
        ["Player Water (body)", "100", "100", "100", "100"],
        ["Bucket Water", "100", "15", "0", "25"],
        ["Materials", "100", "0", "0", "0"],
    ], [1.4 * inch, 0.7 * inch, 0.8 * inch, 0.8 * inch, 0.8 * inch]))
    s.append(Spacer(1, 6))
    s.append(P("<b>Player Water</b> = hydration for the character. <b>Bucket Water</b> = water carried for the village. Both matter for survival and village progress.", "Body"))

    s.append(H("3.3 Village Progress (VillageProgressService)", 2))
    s.append(B("Level 1: ((materialNorm + bucketNorm) / 2) × 35 — capped at 35%"))
    s.append(B("Level 2: Lerp(35, 65, (materialNorm + bucketNorm) / 2)"))
    s.append(B("Level 3: 65 + average(tank1, tank2, tank3) × 0.35"))
    s.append(P("L1 bucketNorm uses growth from starting 15 → 100. Materials and bucket both contribute equally.", "Body"))

    s.append(H("3.4 Win & Lose (RunStateManager + HUDControls)", 2))
    s.append(table([
        ["Condition", "Levels", "Result"],
        ["Health ≤ 0", "All", "Death"],
        ["Player water = 0 for 18s", "L1 primary", "Death (low-water grace)"],
        ["Time limit expires", "L3", "Death"],
        ["Finish gate without full materials + bucket", "L1, L2", "Death with reason"],
        ["Finish gate without all tanks at 100%", "L3", "Death with reason"],
        ["All win checks pass at gate", "All", "Victory panel → next level"],
    ], [1.8 * inch, 1.0 * inch, 3.4 * inch]))

    s.append(PageBreak())

    # 4. Level 1
    s.append(H("4. Level 1 — Desert / Dry Bushlands (MainGame)", 1))
    s.append(P("<b>Purpose:</b> Teach four-lane movement, jumping, water survival, and resource collection. Village progress caps at 35%.", "Body"))
    s.append(P("<b>Run length:</b> ~1,015 m | <b>Target time:</b> ~3:30 | <b>Layout:</b> Level1LayoutDirector", "Body"))
    s.append(Spacer(1, 6))

    s.append(H("4.1 Teaching Phases", 2))
    s.append(table([
        ["Phase", "Progress", "Player learns"],
        ["Learn", "0–25%", "Lanes, jump, cactus water, basic obstacles — no snakes/heat yet"],
        ["Practice", "25–50%", "Heat waves, snakes, water management"],
        ["Combine", "50–75%", "Mix hazards + rolling logs + recovery"],
        ["Challenge", "75–100%", "Higher frequency, always a safe lane + recovery nearby"],
    ], [1.0 * inch, 1.0 * inch, 4.2 * inch]))

    s.append(H("4.2 Key Systems", 2))
    s.append(table([
        ["System", "When", "Effect"],
        ["Heat wave", "25% progress", "3–5s bursts, −20 player water/sec, 7s cooldown"],
        ["Snakes", "From ~28%", "Lane warning → charge; −2 player water on hit"],
        ["Cactus (water source)", "Throughout", "Pass-through: +18 body, +10 bucket / 1s"],
        ["Water springs", "14 placed pools", "+20 body, +12 bucket / 0.9s while standing"],
        ["Rolling logs", "From ~52%", "Span 2 lanes — jump to avoid; −5 health on hit"],
        ["Obstacles (rock/sand)", "Throughout", "Bucket damage; sand/rock slow + material loss"],
        ["Black pits", "Mid/late", "−5 health + slow"],
    ], [1.3 * inch, 1.2 * inch, 3.7 * inch]))

    s.append(H("4.3 Win Condition", 2))
    s.append(B("Reach finish gate (Ender1) with Health > 0, Player Water > 0, Materials = 100, Bucket = 100."))
    s.append(B("Victory message: \"LEVEL 1 COMPLETE\" → option to start Level 2."))

    s.append(H("4.4 Tutorial & Feedback", 2))
    s.append(B("Level1TutorialUI — tips until 25%: A/D lanes, materials, cactus, jump, heat, snakes, logs."))
    s.append(B("Level1FeedbackUI — toasts for water collect, damage, heat, snakes, low water."))
    s.append(B("Level1LowWaterMonitor — warns at ≤35% and ≤15% player water."))

    # 5. Level 2
    s.append(H("5. Level 2 — Mudlands (Level2)", 1))
    s.append(P("<b>Purpose:</b> Apply L1 skills under mud, poison, tracking enemies, and environmental hazards. Village progress 35% → 65%.", "Body"))
    s.append(P("<b>Run length:</b> ~1,414 m | <b>Target time:</b> ~3:00 | <b>Layout:</b> Level2LayoutDirector", "Body"))
    s.append(Spacer(1, 6))

    s.append(H("5.1 Layout Bands", 2))
    s.append(table([
        ["Band", "Progress", "Content"],
        ["Tutorial", "0–15%", "Mud, rocks, cactus, water pickups"],
        ["Warthogs intro", "15–30%", "First warthog crossings (speed 36)"],
        ["Logs + poison", "30–45%", "Rolling logs, poison plants, warthogs (38)"],
        ["Mid-run", "45–60%", "Mud monsters, denser hazard mix"],
        ["Mixed pressure", "60–75%", "Warthogs 40–42, bubble shields"],
        ["Finish stretch", "75–100%", "Warthogs 43–45, extra recovery pickups"],
    ], [1.2 * inch, 1.0 * inch, 4.0 * inch]))

    s.append(H("5.2 Hazards", 2))
    s.append(table([
        ["Hazard", "Avoidance", "Penalty"],
        ["Warthog", "Jump (homes onto your lane)", "−10 health"],
        ["Mud puddle", "Jump or lane change", "Slow ×0.65 for 2s"],
        ["Mud monster", "Dodge mud balls; bubble shield blocks", "−5 health, slow ×0.45"],
        ["Poison plant", "Avoid 2-lane gas zone", "Health + water drain over time"],
        ["Rolling log", "Jump", "−5 health"],
        ["Cactus", "Avoid collision", "−10 health, −5 player water"],
        ["Rock", "Jump or dodge", "−10 health"],
    ], [1.2 * inch, 2.0 * inch, 2.0 * inch]))

    s.append(H("5.3 Pickups", 2))
    s.append(table([
        ["Pickup", "Values"],
        ["Water droplet", "+15 player water, +15 bucket"],
        ["Water pool", "+10 player, +25 bucket"],
        ["Baobab", "+20 player water"],
        ["Material", "+15 (Pipe / Nails / Hammer cycle)"],
        ["Health fruit", "12 / 15 / 18 / 20 (rotating)"],
        ["Bubble shield", "9s — blocks mud balls and poison"],
    ], [1.5 * inch, 4.7 * inch]))

    s.append(H("5.4 Win Condition", 2))
    s.append(B("Same as Level 1: 100/100 materials, 100/100 bucket, health and player water > 0 at Ender2."))

    s.append(PageBreak())

    # 6. Level 3
    s.append(H("6. Level 3 — Pipe Repair Sprint (Level3)", 1))
    s.append(P("<b>Purpose:</b> Timed sprint while spending materials to repair three water tanks. Village progress 65% → 100%.", "Body"))
    s.append(P("<b>Run length:</b> ~7,182 m | <b>Hard time limit:</b> 180 seconds (3:00)", "Body"))
    s.append(Spacer(1, 6))

    s.append(H("6.1 Primary Objective — Tank Repair", 2))
    s.append(table([
        ["Tank", "Repairs needed", "Pipe chances", "Material/repair", "Progress/repair"],
        ["Tank 1", "5", "8", "4", "+20%"],
        ["Tank 2", "10", "15", "5", "+10%"],
        ["Tank 3", "2", "5", "6", "+50%"],
    ], [0.8 * inch, 1.1 * inch, 1.1 * inch, 1.2 * inch, 1.2 * inch]))
    s.append(Spacer(1, 6))
    s.append(P("Tank HUD positions along run: ~40%, ~76%, ~90%. Player presses repair key at pipe spawn points while carrying enough materials.", "Body"))

    s.append(H("6.2 Wave Schedule (Level3WaveDirector)", 2))
    s.append(table([
        ["Progress", "Wave mode"],
        ["0–20%", "Intro hazards"],
        ["20–50%", "Snake wave"],
        ["50–75%", "Warthog wave"],
        ["75–90%", "Snake wave"],
        ["90–95%", "Combined hazards"],
        ["95–100%", "CombinedHard + boss sequence"],
    ], [1.5 * inch, 4.7 * inch]))

    s.append(H("6.3 Hazards (selected)", 2))
    s.append(B("Lightning: 2s warning → −5 health, −5% materials"))
    s.append(B("Acid rain: 2s warning → −10 health, −5% materials"))
    s.append(B("Snake: −3 health | Warthog: −5 health | Rolling log: −6 health, −5 materials"))
    s.append(B("Mud: −5 materials | Tree: −10 materials, −5% bucket"))

    s.append(H("6.4 Win / Lose", 2))
    s.append(B("Win: All three tanks reach 100% (can finish before or at end gate)."))
    s.append(B("Lose: Time expires, or finish reached with incomplete tanks."))
    s.append(B("Level3FeedbackUI shows tank percentages, time warnings, and wave transitions."))

    # 7. Architecture
    s.append(H("7. Technical Architecture (for developers)", 1))
    s.append(H("7.1 Bootstrap Pipeline (SceneBootstrapper)", 2))
    s.append(P("On every gameplay scene (not StartScreen/StarterInfor), execution order −100:", "Body"))
    s.append(B("RunStateManager — pause/death/victory state"))
    s.append(B("PlayerController — disables legacy PlayerMovement scripts"))
    s.append(B("RunnerPlayerSetup — lane alignment, forward speed, jump feel"))
    s.append(B("HUDControls + PlayerResources + VillageProgressService"))
    s.append(B("GroundSpawnner — streaming ground tiles"))
    s.append(B("Level-specific LayoutDirector + FeedbackUI (+ L1 TutorialUI, LowWaterMonitor)"))

    s.append(H("7.2 Layout Directors", 2))
    s.append(P("Each level builds all gameplay objects at runtime — no broken legacy Spawner/BucketSpawner. Scene-placed pickups are disabled; fresh layout is generated from progress-percent beats.", "Body"))
    s.append(table([
        ["Director", "Scene", "Spawns"],
        ["Level1LayoutDirector", "MainGame", "Materials, cactus, springs, snakes, logs, obstacles, finish gate"],
        ["Level2LayoutDirector", "Level2", "Mud, warthogs, poison, monsters, logs, pickups, finish gate"],
        ["Level3LayoutDirector", "Level3", "Pipes, hazards, waves, pickups, tank markers"],
    ], [1.5 * inch, 1.0 * inch, 3.7 * inch]))

    s.append(H("7.3 Key Scripts Reference", 2))
    s.append(table([
        ["Script", "Responsibility"],
        ["PlayerController", "Lane movement, jump, speed modifiers"],
        ["PlayerResources", "Health, water, bucket, materials — single source of truth"],
        ["HUDControls", "UI display, win/lose checks, resource API for pickups"],
        ["RunStateManager", "Playing/Paused/Dead/Victory states, scene transitions"],
        ["VillageProgressService", "Village % formulas per level"],
        ["SceneCatalog", "Canonical scene names and helpers"],
        ["Level1/2/3FinishGate", "End trigger → LevelProgress()"],
    ], [1.8 * inch, 4.4 * inch]))

    s.append(PageBreak())

    # 8. Design pillars
    s.append(H("8. Design Pillars & Player Experience Goals", 1))
    s.append(table([
        ["Pillar", "How it shows up"],
        ["Teach, then test", "L1 phases introduce one mechanic at a time before combining"],
        ["Water is life", "Body water + bucket water both matter; heat and hazards drain hydration"],
        ["Visible restoration", "Village % rises as player collects and repairs infrastructure"],
        ["Fair difficulty", "Every hazard beat has reaction time, open lanes, or recovery nearby"],
        ["Four-lane readability", "All objects placed in lanes 0–3; patterns use lane choice as gameplay"],
        ["No random unfairness", "Layout directors place intentional beats — not frame-by-frame spawning"],
    ], [1.5 * inch, 4.7 * inch]))

    s.append(Spacer(1, 12))
    s.append(H("9. Quick Playtest Checklist", 1))
    s.append(B("L1: Player learns A/D and Space; cactus refills water; heat manageable; village caps at 35%; Level 2 loads."))
    s.append(B("L2: Warthogs require jump; mud/poison readable; materials + bucket reach 100 at gate; Level 3 loads."))
    s.append(B("L3: All three tanks repairable within 3 minutes; time warnings appear; village reaches 100%."))
    s.append(B("All levels: No console errors; pause/restart/menu work; death shows clear reason."))

    s.append(Spacer(1, 20))
    s.append(HRFlowable(width="100%", thickness=0.5, color=colors.grey))
    s.append(P("<i>Drop by Drop: Flow of Hope — GDD v1.0 | Generated from Unity project codebase</i>", "Caption"))

    return s


def main():
    doc = SimpleDocTemplate(
        OUTPUT,
        pagesize=letter,
        rightMargin=0.75 * inch,
        leftMargin=0.75 * inch,
        topMargin=0.75 * inch,
        bottomMargin=0.75 * inch,
        title=TITLE,
        author="Zuri's Mission Team",
    )
    doc.build(build_story())
    print(f"Created: {OUTPUT}")


if __name__ == "__main__":
    main()
