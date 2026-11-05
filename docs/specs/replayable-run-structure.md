# Specification — Replayable Run Structure

> Epic: #31 · Stories: #39, #40, #41
> Owner: Kim Brodersen · Sprint: 2 · Status: draft

## Scope

**In scope.** Branching three-Act route graph with at least two route choices in each Act, where selected nodes load handcrafted rooms (3.1, #39). Node/reward/run progression (3.2, #40). Playtest build variety/decision-making - Conduct at least five playtest sessions that record run information, then summarize whether testers produced distinct builds in response to rewards or route options (3.3, #41). The exact objective wording is in proposal §2.

**Out of scope**: movement (2.1), combat (2.2), enemies (2.3).

**Depends on.** (1.2, #33) level and attribute progression is tied to XP gain, (1.3, #34) weapons are potential rewards, boons (1.4, #35).

## Functional requirements

Written in EARS form — "when 〈trigger〉, the system shall 〈response〉" — because
each one then becomes a test almost verbatim.

| ID | Requirement | Story |
|---|---|---|
| FR-1 | When the player exits a room, the system shall display the map and prompt for next room node selection. | #39 |
| FR-2 | When the player selects a new room node, the system shall load a handcrafted room. | #39 |
| FR-3 | When the player..., the system shall .... | #39 |
| FR-4 | When the player earns a reward, the system shall offer a selection of rewards for the player to choose from. | #40 |
| FR-5 | When the player earns or spends gold, the system shall update and keep track of the player's total gold in that run. | #40 |
| FR-5 | When the player levels up, the system shall apply level-up rewards. | #40 |
| FR-6 | When <>, the system shall <>. | #41 |

## Non-functional requirements

| ID | Requirement | Why it matters here |
|---|---|---|
| NFR-1 | Passwords are stored only as salted hashes, never in plaintext or reversible form. | The prototype will be read by the sponsor and the instructor. |
| NFR-2 | Failure responses for unknown-account and wrong-password are byte-identical. | Distinguishing them leaks which addresses are registered (see FR-4). |
| NFR-3 | Sign-in responds in under 500 ms on the demo dataset. | The Week-8 check is a live demo. |

## Acceptance criteria (epic level)

- [ ] A member can register, sign in, and sign out through the served
      interface — by tapping, not by calling functions in a console.
- [ ] FR-1 … FR-5 each have a test that fails if the behaviour is removed.
- [ ] No plaintext password appears anywhere in the repository or its history.
- [ ] `python .github/scripts/check_repo.py` is green and the prototype's
      tests pass.

## Open questions

| Question | Owner | Needed by |
|---|---|---|
| Does the sponsor require campus SSO instead of local accounts? | 〈name〉 | Sprint 2 planning |
| Is a 24-hour session acceptable, or does the sponsor mandate shorter? | 〈name〉 | before FR-5 is implemented |

*Open questions belong in the document, not in someone's memory. An
assistant asked to "finish the spec" will invent answers to these — that is
exactly the fabrication the review gate is looking for.*
