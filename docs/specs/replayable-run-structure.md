# Specification — Replayable Run Structure

> Epic: #31 · Stories: #39, #40, #41
> Owner: Kim Brodersen · Sprint: 2 · Status: draft

## Scope

**In scope.** Branching three-Act route graph with at least two route choices in each Act, where selected nodes load handcrafted rooms (3.1, #39). Node/reward/run progression (3.2, #40). Playtest build variety/decision-making - Conduct at least five playtest sessions that record run information, then summarize whether testers produced distinct builds in response to rewards or route options (3.3, #41). The exact objective wording is in proposal §2.

**Out of scope**: movement (2.1), combat (2.2), enemies (2.3).

**Depends on.** (1.2, #33) level and attribute progression is tied to XP gain, (1.3, #34) weapons are potential rewards, boons (1.4, #35).

## Functional requirements

| ID | Requirement | Story |
|---|---|---|
| FR-1 | When the player starts an Act, the system shall display the map and prompt the player to select a route. | #39 |
| FR-2 | When the player selects a new room node, the system shall load a handcrafted room. | #39 |
| FR-3 | When the player exits a room, the system shall not allow the player to return to it. | #39 |
| FR-4 | When the player earns a reward, the system shall offer a selection of rewards for the player to choose from. | #40 |
| FR-5 | When the player earns or spends gold, the system shall update and keep track of the player's total gold in that run. | #40 |
| FR-6 | When the player levels up, the system shall apply level-up rewards. | #40 |

## Non-functional requirements

| ID | Requirement | Why it matters here |
|---|---|---|
| NFR-1 | .... | .... |
| NFR-2 | .... | .... |
| NFR-3 | .... | .... |

## Acceptance criteria (epic level)

- [ ] (3.1) A run is divided into three Acts.
- [ ] (3.1) Each Act provides at least two route choices.
- [ ] (3.1) Route choices are represented as connected nodes.
- [ ] (3.1) Selected nodes load handcrafted rooms
- [ ] (3.1) Completed rooms advance the run through the selected route.
- [ ] (3.1) Fast-travel backtracking to completed nodes is not supported.
- [ ] (3.2) At least four distinct node or reward types are defined.
- [ ] (3.2) Gold is tracked during a run.
- [ ] (3.2) XP and Attribute Points are tracked during a run.
- [ ] (3.2) Mastery progression is tracked during a run.
- [ ] (3.2) Completing nodes can grant configured rewards.
- [ ] (3.2) Rewards can influence later build or route decisions.
- [ ] (3.3) At least five playtest sessions are completed.
- [ ] (3.3) Starting class is recorded for each session.
- [ ] (3.3) Weapon choices and weapon replacements are recorded.
- [ ] (3.3) Route choices are recorded.
- [ ] (3.3) Major build choices are recorded.
- [ ] (3.3) Changes made in response to rewards or route options are recorded.
- [ ] (3.3) Results are summarized to evaluate build variety and decision-making.
- [ ] FR-1 … FR-7 each have a test that fails if the behaviour is removed.
- [ ] `python .github/scripts/check_repo.py` is green and the prototype's
      tests pass.

## Open questions

| Question | Owner | Needed by |
|---|---|---|
| ...? | 〈name〉 | Sprint 2 planning |
| ...? | 〈name〉 | before FR-5 is implemented |
