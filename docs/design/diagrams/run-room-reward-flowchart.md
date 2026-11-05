# Run, room, and reward flow

> Epic: #31 · Story: #39 · Design task: #58
> Reference taken from Team Gameplay Design Brief document

How a player moves through a run: start, entering an Act, choosing and completing rooms, receiving rewards, choosing the next route, and ending.

## Diagram

Flowchart of a Project Rogue run: Start run, Initialize run, Enter Act, Reveal map and Choose a connected node, then one of five encounter groups, Encounter complete, and Receive rewards. Non-boss rooms reveal the next routes and loop back to the choice. A boss room leads to a Final Act check that either starts the next Act or ends the run. Defeat in any encounter also ends the run, and both endings lead to a Run summary. (diagrams/run-room-reward-flow.svg)

*Figure 1. Run, room, and reward flow.*

**How to read it:** solid lines are stated in the design doc and dashed lines are **PLANNED**: taken from the doc and needs team review. **Q#** tags that point to the questions below. Each arrow label is the action or condition that triggers the next stage.

**Editing the diagram:** [`diagrams/run-room-reward-flow.dot`] (using Graphviz)

## Design details

| Topic | Answer | Status |
| --- | --- | --- |
| **Run start** | Pick a class (Knight, Mage, Thief) and one of three starting passives. The class sets starting attributes, resources and equipment, and any class can use any type of weapon. |
| **Room types** | 12 types in 5 groups (table below). Which will be included in the prototype will be planned. |
| **Rewards** | XP, Gold, Mastery Points, loot, boons, secrets and alt exits. Attribute Points are awarded automatically on level-up. Fixed drop vs chooseable drops will be planned. |
| **Transitions** | Doors are reachable nodes and walking through one commits to that branch and gives up on other branches unless a Secret node connects them. Optional gates do not block the mandatory route and Exits unlock on completion.  |
| **Boss and Act progression** | Each Act ends at its boss while Acts 1 and 2 lead to the next Act. The Act 3 boss ending the run is assumed. |
| **Defeat** | Health reaching 0 ends the run (assumed). Meta currency and unlocks persist. XP/level, attributes, Gold, Mastery Points, weapons, boons and map progress will reset. |

### Room groups

| Group | Types | Finished when | Status |
| --- | --- | --- | --- |
| Combat | Standard Combat, Elite | Enemies cleared, waves survived or objective defended |
| Challenge | Puzzle, Traversal / Challenge, Trial / Shrine | Challenge finished |
| Loot and choice | Treasure, Mystery Event, Secret | Reward taken or choice made |
| Service | Shop, Rest / Sanctuary, Forge / Trainer | Player leaves |
| Boss | Boss | Boss defeated |

### When progression updates

| System | When | Status |
| --- | --- | --- |
| XP / Level | Per-kill XP during the encounter, plus a larger completion chunk at the rewards step (bigger for elites and bosses). Each level-up gives 3 Attribute Points, spent now or banked. | Brief / Planned |
| Gold | From enemies, encounter rewards, treasure and events. Spent in Service rooms. | Brief |
| Mastery Points | At the rewards step, for milestone encounters only (elite, boss, challenge, major puzzle, rare event or secret). Never per kill. | Brief |
| Meta currency | At the run summary. Some is earned on failed runs. | Brief / Planned |
| Milestone unlocks | When achievements, bosses or class completions happen. Added to future runs. | Brief |

## Questions to be answered

| # | Question |
| --- | --- |
| Q1 | Which of the room types will be included in the prototype? |
| Q2 | Are exits locked until the encounter is completed? Do Shop, Rest and Treasure rooms need a "done" action? |
| Q3 | Which rewards are fixed drops and which can be chosen by the player? Which rooms will offer boons or weapons? |
| Q4 | How are bosses picked and rewarded, and does the Act 3 boss signal the end of a run? |
| Q5 | What exactly persists and resets, and how much meta currency does a failed run pay? |
| Q6 | Where can unused Attribute Points be spent? |

