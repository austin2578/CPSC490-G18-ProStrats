# Project Proposal — Project Rogue

**Department of Computer Science**  
**CPSC 490 Undergraduate Seminar in Computer Science — Proposal for Capstone Project**

**Group 18 — ProStrats (Procrastination Strategies)** · Sponsor: independent  
Authors: Monroe, Austin (austin2578), Truong, Brady (Ztaco10), Brodersen, Kimberly (Kmbroders), Armenta, Maximiliano (MaxArmenta-04), Bumatay, Rebekah (r1beka)  
Date: 2026-10-04

> **This file is the proposal document, not a README.** Its section numbers,
> titles, and guidance are copied from the course Word template, so it
> converts cleanly for Canvas submission. Write continuous academic prose —
> no task lists, no emoji, no repo jargon.
>
> Each section below opens with the template's own guidance in a quote block.
> **Delete the quote blocks and every 〈bracket〉 before submitting.**
>
> **Getting this into the Word template for Canvas.** The template numbers
> its headings **automatically** (a multilevel list: top-level sections at
> level 1, *Related Work* and *Problem Statements* at level 2). The numbers
> typed below exist so the repo copy is readable and checkable — so when you
> move the text into Word, do not end up with both sets.
>
> The reliable route, and the one most teams should use: **open the course
> template and paste your prose section by section**, leaving Word's own
> numbering to do the numbering. Ten minutes, no surprises.
>
> If you prefer to convert, `pandoc` can do it (install with
> `winget install pandoc`):
>
>     pandoc proposal/proposal.md -o proposal.docx --reference-doc="CPSC 490 Project Proposal Template Fall 2026.docx"
>
> Then in Word: delete the typed `0.` / `1.` / `1.1` prefixes (Word re-adds
> them from the list), and set *Related Work* and *Problem Statements* to the
> template's level-2 heading so they number as 1.1 and 1.2. Check figure
> placement, then submit.
>
> **Formatting requirements — the submitted Word document is graded against
> these, explicitly:**
>
> - **Cover page: use the template's cover page, unchanged in layout.** Fill
>   in only its fields — project title, group number and name, sponsor,
>   authors, date — and keep the template's own placement, fonts and spacing
>   for it. The header block at the top of this file carries the same fields
>   so the paste is a transcription, not a redesign.
> - **Font: Times New Roman, 11-point.** Body text, headings and captions
>   take their size and style from the template's own styles — do not
>   restyle anything by hand.
> - **Line spacing: 1.5.** **Margins: 1.0 inch** on all four sides.
> - **Section format, numbering and indentation must match the Word template
>   exactly** — the multilevel-list numbering, heading levels, and paragraph
>   indentation are the template's, not yours. If your document's §1.1 looks
>   different from the template's §1.1, fix yours.
> - **Length: the Final Project Proposal Paper (due Sun Dec 20) must exceed
>   50 pages** under exactly this formatting — font, spacing and margins are
>   fixed above precisely so page count means the same thing for every team.
>   The Preview paper (due Sun Nov 29) is the same document part-way; it has
>   no minimum, but it is graded on the same formatting.
>
> A paste into the template inherits all of this automatically **if you paste
> as text and let Word's styles apply** (Home → Paste → *Keep Text Only*, or
> apply the template's styles after pasting). A pandoc conversion with
> `--reference-doc` inherits it too — but verify font, spacing and margins
> afterward rather than assuming.
>
> Either way, keep this Markdown copy current — it is what peer review and CI
> can actually read. If your team writes in Word instead, commit the `.docx`
> here as well.

---

## 0. Abstract
Project Rogue is a 2D platformer roguelite that explores how combat, equipment, and character builds work together to create different routes and how these decisions affect a run. Project Rogue draws mechanic and world building inspiration from other other games such as Dead Cells, Hades, Slay the Spire, and Salt and Sanctuary.
The project features flexible starting classes, run-specific attributes, boons, branching routes, and distinct weapon types with a goal of achieving character build variety and replayability through persistent progression each run. By combining structured randomness with unique encounters, Project Rogue seeks to provide varied runs while preserving previous combat, exploration, and platforming experience.
The primary goal of this porject is to develop a playable prototype and test how well these systmes can work together. The proposed systems / mechanics will be tested by playing the game ourselves to determine whether players will be able to understand different build options, make meaningful decisions, and adapt to new weapons and abilites that can be unlocked on different routes or playthroughs.

(add more once proposal finished)

## 1. Introduction

Roguelite games are built around repeated runs where the player faces changing challenges, rewards, and opportunities to develop a character. In an action-focused roguelite, these decisions happen alongside real-time combat, movement, and exploration, so both player skill and character-building choices can influence whether a run succeeds. Replayability is an important part of this type of game because each attempt should give the player a reason to make different choices instead of simply repeating the same sequence. Weapons, upgrades, routes, and temporary abilities can all contribute to this variety, but having many options does not automatically mean that those options create meaningful decisions.

One problem Project Rogue addresses is that character progression can become restrictive when a starting class strongly determines which equipment or abilities remain useful for the rest of a run. A player may technically have several choices available while still being encouraged to follow a narrow build because changing direction would make earlier investments ineffective. Weapon progression can become similarly predictable when a new weapon is mainly judged by whether its damage number is larger than the current weapon. These systems can reduce the value of experimentation and make later runs feel similar even when different rewards appear.

Level structure creates a separate problem for a replayable action game. Fully randomized environments can provide variety, but too much randomness can reduce the quality of deliberate platforming, combat spaces, and encounter pacing. Fully authored levels can provide carefully designed encounters, but repeated play can make the same route increasingly predictable. Project Rogue therefore proposes separating the organization of a run from the physical design of its rooms. A run can change through branching routes, encounter types, rewards, and connections while the individual combat, traversal, puzzle, and event spaces remain handcrafted.

Project Rogue is a 2D action roguelite platformer designed around flexible character building, weapon-driven combat, and branching runs. Starting classes establish a player's initial direction without permanently restricting later weapon choices. During a run, players can develop a build through attributes, boons, weapon-family progression, skills, and randomized weapon properties. The active weapon can also be replaced, allowing a new weapon to create a strategic decision instead of functioning only as a direct upgrade. Changing weapons may encourage the player to reconsider attribute investment, skill choices, or which future rewards and routes are most useful.

The project is not based on the claim that any one of these mechanics is completely new. Classes, randomized rewards, branching routes, persistent progression, and weapon upgrades already appear throughout related games. The intended difference is how Project Rogue combines these systems so that choices in one part of a run can affect later decisions. A weapon can influence which attributes are valuable, upgrades can change how later rewards are evaluated, and route selection can determine which opportunities become available. The project will develop and test a playable prototype to determine whether these interacting systems can remain understandable while still giving players enough flexibility to create different builds and adapt during repeated runs.

### 1.1 Related Work
Action roguelites, roguelike deckbuilders, and action RPGs provide relevant precedents for Project Rogue through their approaches to combat and character development. The central design question is how combat, equipment, character identity, and route selection can produce meaningful decisions within a run. This survey compares Dead Cells, Hades, Salt and Sanctuary, and Slay the Spire along those dimensions.
The comparison uses developer descriptions rather than a controlled gameplay study; the strengths and limitations below are design interpretations relative to this project's goals, not measured judgments of game quality.
Dead Cells is the closest reference for the proposed moment-to-moment gameplay. Motion Twin describes it as a 2D action-platformer with pattern-based enemies, distinctive weapons and spells, nonlinear paths, and hidden passages [1]. These features connect combat execution with exploration: selecting a route changes the spaces and challenges the player encounters, while movement and enemy recognition determine whether the player survives.
This is a useful precedent for Project Rogue's combat rooms, traversal challenges, and secrets. However, the described approach does not by itself establish the proposed relationship between freely chosen attributes and persistent investments within a run's weapon-family skill trees. Project Rogue intends to make that relationship explicit, so replacing a weapon involves considering its moveset, scaling, and compatibility with previous investments rather than evaluating damage alone.
Hades provides a complementary reference for character builds. Supergiant Games describes weapons and selectable Olympian boons as sources of build variety, alongside permanent improvements available through the Mirror of Night [2]. Its relevance is the interaction between action combat and choices that modify the player's abilities. Project Rogue similarly proposes boons that affect attacks, statuses, and resources.
Its intended emphasis is on changing build direction during a run through weapon replacement, attribute redistribution, and weapon-family Mastery. This comparison does not imply that Hades lacks build depth; it identifies the additional progression structure that Project Rogue proposes to investigate. Supporting more opportunities to reconsider a build also creates a cost: the player must understand more dependencies before judging whether a new item is useful.
Salt and Sanctuary provides a relevant example of combining 2D action combat with RPG character customization. Ska Studios describes a system of discoverable, craftable, and upgradeable equipment within an interconnected world containing platforming challenges, secrets, and hidden shortcuts [3]. These features make it a useful reference for Project Rogue's relationship between equipment choices, combat, and room exploration.
The principal distinction is progression structure: Salt and Sanctuary emphasizes exploration of a connected world, whereas Project Rogue proposes branching routes through handcrafted encounters across three Acts. Project Rogue also intends to make weapon replacement a recurring strategic decision through run-local attributes, weapon-family Mastery, and respec opportunities.
This combination would require careful balancing so that changing weapons remains viable without making earlier investments feel meaningless.
Slay the Spire is relevant to strategic planning even though it centers on card combat rather than action-platforming. Mega Crit describes a changing layout, choices between safer and riskier paths, and interactions between cards and relics [4]. These systems make adaptation part of progression: the value of a reward depends on how it works with the current deck, while a route affects which opportunities become available.
Project Rogue applies a similar relationship to weapons, boons, and encounter selection. Its proposed branching map would lead to physical doors and handcrafted rooms, where players must execute the chosen strategy through movement and combat. Translating planning into real-time play adds a challenge absent from a purely card-based comparison: build information must remain understandable without disrupting the pace of combat.

| Existing approach | What it does | Pros relative to project goals | Cons or tradeoffs relative to project goals | Why Project Rogue differs |
|---|---|---|---|---|
| Dead Cells [1] | Combines 2D combat, distinctive equipment, nonlinear routes, and secrets. | Closely matches the desired connection between movement, combat, and exploration. | Does not serve as a direct specification for the proposed attribute and weapon-family Mastery relationship. | Proposes one active weapon with run-local family skill investments and attribute-based build changes. |
| Hades [2] | Combines action combat, weapons, boon choices, and permanent upgrades. | Shows how ability modifiers can support varied combat builds across repeated attempts. | Adopting boon choices alone would leave the project's respec and Mastery systems undefined. | Proposes coordinated changes to weapons, attributes, and family skill trees during a run. |
| Salt and Sanctuary [3] | Combines 2D action combat with customizable equipment, crafting, upgrades, and an interconnected world. | Connects character customization with combat, platforming, secrets, and exploration. | Its interconnected campaign structure provides a different progression model from the proposed branching, run-based encounters. | Project Rogue proposes run-local attributes and weapon-family Mastery, with replaceable weapons and branching routes through handcrafted rooms. |
| Slay the Spire [4] | Combines changing routes with card selection and relic interactions. | Links reward evaluation and risk management to the current build. | Card-based combat does not directly address movement, timing, or action readability. | Couples branching route decisions with handcrafted action, puzzle, and traversal rooms. |

Together, these works establish that equipment variety, temporary build modifiers, persistent progression, and strategic routing are existing approaches rather than new inventions. Project Rogue's proposed contribution is their particular combination: flexible starting classes, a replaceable active weapon, separate attribute and Mastery investments, and a branching route through authored encounters.
For example, a useful Staff drop could encourage a Knight to reconsider attributes and seek a respec service, making a combat reward influence the next route choice. The value of this combination remains a hypothesis. Prototyping and playtesting should determine whether players can understand these interactions, recognize viable build changes, and find those decisions worthwhile.
The gameplay design brief supplies this intended direction; it does not establish that the proposed systems have already been implemented or validated.

*Draft source entries for eventual integration into §8, numbered in order of first citation. These sources were consulted while preparing this draft; the team should read them before submission. They are kept here temporarily so this edit remains limited to §1.1.*

[1] Motion Twin. “Dead Cells.” Official game website. https://dead-cells.com/. Accessed October 2, 2026.  
[2] Supergiant Games. “Hades.” Developer-published game description on Steam, “About This Game.” https://store.steampowered.com/app/1145360/Hades/. Accessed October 2, 2026.

[3] Ska Studios. “Salt and Sanctuary.” Official game website. https://ska-studios.com/games/salt-and-sanctuary/. Accessed October 2, 2026.

[4] Mega Crit. “Slay the Spire.” Developer-published game description on Steam, “About This Game.” https://store.steampowered.com/app/646570/Slay_the_Spire/. Accessed October 2, 2026.

### 1.2 Problem Statements

> Briefly state the problem to solve in this project.

P1. Build variety in roguelites can collapse when class choice locks players into a narrow set of options or when a few weapon builds dominate, which weakens replayability.  
P2. Fully random level generation tends to lose the quality of designed encounters, while fully authored levels lose variety; a run needs structured randomness that preserves both.  
P3. Weapon progression that relies on a single mechanism becomes predictable, so progression needs several interacting layers that remain understandable and balanceable.

**Every problem here must connect to the goals and objectives in §2, and
every goal in §2 must trace back to a problem here.** A goal with no problem
behind it is scope you invented; a problem with no goal is a problem you are
not actually solving. Check both directions before you submit — this mapping
is what the final project report is graded against.

| Problem | Addressed by |
|---|---|
| P1 Build Variety and class lock-in | 〈Goal 1 (#n)〉 |
| P2 Structured randomness versus authored quality | 〈Goal 2 (#n)〉 |
| P3 Predictable weapon progression | 〈Goal 3 (#n)〉 |

## 2. Goals and Objectives
> Describe goals and objectives. Goals are general statements of what you are
> trying to accomplish with the project or problems to solve. Objectives are
> specific, measurable statements of what you want to complete to reach the
> project goals. Most projects have 2-3 goals.
>
> List the objectives for each goal. To write objectives, look at the goal
> statement and list what you need to complete using action words like use
> case names in order to meet the goal.
>
> Note that the goals and objectives in a proposal will be an important
> metric to evaluate whether or not you successfully finished your project
> when you turn in your final project report.

Each **goal** is tracked as an **Epic** issue and each **objective** as a
**User Story** issue in the team repository (see the setup guide's *Epics and user stories* section).
**Every epic and user story in the repository is linked from this section** —
CI gate G8 fails if one exists that this section does not link. That is what
keeps the goals in this document and the work on the board from drifting
apart.

Write each objective the way the guidance above asks — **an action word plus
the measure that says it is done**, not a role-play sentence:

- **Goal 1: 〈e.g. Secure account management〉** (Epic #〈n〉)
  - Objective 1.1: 〈Implement member registration and login with hashed
    credentials, session expiry, and rejection of malformed input.〉 (#〈n〉)
  - Objective 1.2: 〈Demonstrate the login round-trip in a runnable prototype
    at the Week-8 in-class check.〉 (#〈n〉)
- **Goal 2: 〈your second goal〉** (Epic #〈n〉)
  - Objective 2.1: 〈Action word + what you will complete + how it will be
    measured〉 (#〈n〉)

〈Replace the brackets with your own 2–3 goals and their objectives, and put
the **real issue numbers** in as you file them — gate G8 checks that every
epic and story in your repository is linked from this section. A fully worked
version of this, with live issues and a populated board, is in the course
example repository.〉

## 3. Proposed Approaches

> Describe your proposed approach to solve the problem, specifying how you
> will achieve the stated goals. List some possible strategies.

〈Your approach — **clear and concise**. State the strategy you chose, the
alternatives you considered, and the reasoning that decided between them.
Think of this as the argument, not the manual: a reader should finish this
section understanding *what* you will do and *why that* rather than the
alternatives.〉

**Keep the details out of this section.** Tooling, platforms, frameworks,
DBMS choices, environment setup, diagrams, and the work breakdown all belong
in §4 (Required Environment, Resources, and Planned Activities). If a
sentence here names a version number, a library, or a configuration, it
probably belongs in §4 — leave a pointer instead ("the implementation stack
is detailed in §4").

〈A few paragraphs, or a short list of candidate strategies with one line of
trade-off each. If it runs past a page, you are writing §4.〉

## 4. Required Environment, Resources, and Planned Activities

> Review the required and available resources and environment to complete
> your project. For example, server, platform, software tools, operating
> systems, DBMS, or any required skills.
>
> Describe the expected activities to achieve the stated goals, e.g.,
> software development process.

〈Your environment, resources, and planned activities.〉

**Diagrams belong in this section.** Include at minimum a high-level
architecture diagram and a system (context) diagram; add the ER/EER model and
a data-flow diagram where they help the reader understand what you are
building and what it depends on. Draw them with any graphical tool
(Lucidchart, draw.io, Miro, Mermaid, ERDPlus, Figma), keep the authoritative
copies in `docs/design/` with both editable source and exported image, and
reference them here.

〈Number every figure, caption it, and point at it from the prose — "Figure 1
shows the three deployment tiers and the trust boundary between them." A
figure the text never mentions is decoration. See `docs/design/DIAGRAMS.md`
for tools, conventions, and the rule that every box and arrow must be
verified against reality.〉

### Specification and design documents

**Every specification and design document the team writes is listed here**
with the objective it serves. This section is the index of the project's
technical detail: §3 holds the argument, §4 holds the documents that make it
buildable. CI gate G9 fails if a document exists in `docs/specs/` or
`docs/design/` that this section does not link.

| Document | Kind | Covers | Issues |
|---|---|---|---|
| 〈docs/specs/account-management.md〉 | specification | 〈account management requirements〉 | 〈#n, #n〉 |
| 〈docs/design/architecture.md〉 | design | 〈system architecture + data model〉 | 〈#n〉 |

〈The scaffold ships `docs/specs/example-spec.md` and
`docs/design/example-design.md` as worked examples — read them, then delete
them once you have your own, and list yours here.〉

〈Replace these rows with your own. Each document names its epic and stories
in its own first lines too (gate G2), so the trail runs both ways.〉

### Planned activities — the work items

The goals and objectives live in §2 as epics and user stories. **This section
links every *other* work item: features, enhancements, bugs, tasks, and
sub-tasks** — the concrete activities that deliver those objectives. CI gate
G8 fails if such an issue exists that this section does not link.

| Issue | Type | Activity | Parent | Owner | Sprint |
|---|---|---|---|---|---|
| 〈#n〉 | 〈task〉 | 〈stand up the prototype login endpoint〉 | 〈#story〉 | 〈owner〉 | 〈Sprint 1〉 |
| 〈#n〉 | 〈feature/enhancement/bug/task/sub-task〉 | 〈…〉 | 〈#story〉 | 〈…〉 | 〈…〉 |

〈Replace these rows with your own, and keep the table current as you file new
issues — with §2 it gives a reader every planned activity in one place, each
traceable to the objective it serves.〉

## 5. Project Outcomes

> Describe the outcomes or deliverables, e.g., final project report, user
> manuals, source code, data or database files, etc.
>
> Note: the deliverables always include the team GitHub repository, which
> must already contain prototype v0 (a thin end-to-end proof-of-concept,
> however small, running when this proposal is submitted). Briefly describe
> what your v0 demonstrates and how to run it.

〈**One or two paragraphs** explaining the project outcome overall — what will
exist when the project is finished, and what it will let someone do. Keep it
prose, not a checklist; name the deliverables inside the paragraphs, and say
briefly what prototype v0 demonstrates today and how to run it.〉

## 6. Project Timeline

> Identifies tasks (project objectives) to be performed, milestones to be
> met, and the estimated number of hours for each task.

〈**This is the plan for CPSC 491 next semester — the implementation timeline,
not this semester's proposal work.** Identify the tasks (your objectives from
§2), the milestones, and the estimated hours for each, in the order they will
be built. State the assumptions it rests on (sponsor availability, data
access, hardware).〉

| Task (objective) | Milestone | Owner | Est. hours | Spring phase |
|---|---|---|---|---|
| 〈…〉 | 〈…〉 | 〈…〉 | 〈…〉 | 〈…〉 |
| 〈…〉 | 〈…〉 | 〈…〉 | 〈…〉 | 〈…〉 |

〈Do **not** put this fall's four proposal sprints here — those live on the
project board and in `docs/sprint-reviews/`. This section answers "how does
the system actually get built next semester?"〉

## 7. AI Usage

> Per the course AI policy (see the syllabus, Use of AI Tools), disclose the
> AI tools used in preparing this proposal and the prototype: which tools,
> for what tasks (e.g., code generation, test writing, debugging,
> diagramming), and approximately what fraction of each artifact was
> AI-assisted.
>
> Reminder: the prose of this proposal must be your own writing. You remain
> fully responsible for the correctness of all AI-assisted work, including
> the prototype code.

〈Your disclosure. Naming the tool is not disclosure — name what it drafted,
what fraction of each artifact was AI-assisted, and how you verified it.〉

## 8. References

> [1] Burges, C. J. C. Tutorial on Support Vector Machines for Pattern
> Recognition. Kluwer Academic Publishers, 1998.
> [2] Chen, P., Fan, R., and Lin, C. A study on SMO-type decomposition
> methods for support vector machines. IEEE Transactions on Neural Networks,
> 2006.
> [3] For Wikipedia, specify the URL here
> [4] For a web source, specify the URL here plus date accessed

〈Number references in the order first cited and cite them in the text as
[1], [2]. Every entry must be a source a team member has actually read and
can produce on request.〉
