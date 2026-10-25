# Sprint 1 review — October 8–24, 2026

## Progress this sprint — the substance

### Proposal moved

PR #42 updated proposal/proposal.md §2 Goals and Objectives,
closed #28, received non-author approval, and was merged into develop.

The team developed and reviewed its gameplay design document and discussed
gameplay mechanics, enemy types, and reference games. The professor also
approved the change to the weapon-and-element direction. The proposal,
planning documents, and issues need to consistently reflect that approval.

### Prototype moved

PR #44 implemented shared movement and traversal, including horizontal
movement, jumping, ground detection, weapon-based traversal utilities,
and a handcrafted traversal room. Approved PR was
merged into develop on October 23, closing #36.

PR #45 implemented the shared combat pipeline, including damage handling,
basic attacks, active abilities, resource costs, scaling scaffolding,
and a combat test scene. Brady reviewed and merged the PR, closing #37.

These contributions established movement and combat foundations.
Their integration with the remaining progression, encounter, and run
systems is continuing work.

## Velocity

| Metric | Story points |
|---|---|---|
| Original planned commitment | 40 |
| Completed by the extended October 24 deadline | 13 |
| Carried over | 27 |

Scope was too big so many task from Sprint 1 were moved to Sprint 2 and Sprint 3

## Completed work

| Issue | Owner | Deliverable | PR |
|---|---|---|---|
| #28 | Austin | Proposal §2 Goals and Objectives | #42 |
| #36 | Brady | Movement, weapon traversal, and handcrafted traversal room | #44 |
| #37 | Max | Shared combat pipeline and combat test scene | #45 |

## Carried over — and why

Unfinished work was moved to later sprints after feedback that the
Sprint 1 commitment was too large. Moving an issue does not make it
completed; the table records the remaining work and revised destination.

| Issue | Remaining work | Destination / reason |
|---|---|---|
| #32 — Objective 1.1 | Complete the starting-loadout specification and implementation, aligned with the approved weapon-and-element direction | Sprint 2 — Sprint 1 scope exceeded team capacity |
| #33 — Objective 1.2 | Complete attributes, resources, leveling, and respec functionality | Sprint 2 — Sprint 1 scope exceeded team capacity |
| #34 — Objective 1.3 | Complete weapon progression, Mastery, affixes, and equipped skills | Sprint 2 — Sprint 1 scope exceeded team capacity |
| #38 — Objective 2.3 | Complete enemy encounters, including attacks, damage, defeat, encounter completion, and rewards | Sprint 2 — Sprint 1 scope exceeded team capacity |
| #39 — Objective 3.1 | Complete the branching three-Act route structure and handcrafted-room progression | Sprint 3 — Rescheduled after the Sprint 2 progression and encounter work |
| #40 — Objective 3.2 | Complete node/reward types and run-local progression tracking | Sprint 3 — Rescheduled after the Sprint 2 progression and encounter work |
| #41 — Objective 3.3 | Conduct five playtest sessions and analyze build variety and player decisions | Sprint 3 — Requires the relevant gameplay systems to be ready for testing |

Goal 1 (#29) and Goal 2 (#30) were moved to Sprint 2, and Goal 3 (#31) was moved to Sprint 3. These epics group the objective-level work above and are not counted as additional story points. Movement and combat (#36 and #37) remain completed Sprint 1 work; moving their parent Goal 2 does not change their completion status.

## Verification and limitations

PR #44 reports verification of movement physics, ground detection,
weapon traversal behavior, respawning, selector controls, and completion
of the mandatory traversal route by all nine supported weapon/element
combinations.

PR #45 reports verification of basic attacks, active abilities, resource
costs, cooldowns, damage scaling, projectile behavior, and target defeat.

These are author-reported checks documented in the PRs. They do not
establish that every teammate independently repeated the tests.

Known limitations include incomplete element-passive effects in the
movement prototype, temporary attribute scaffolding in combat, and
projectiles passing through walls. Combined-system behavior requires
further integration testing.

## Feedback we received, and our response

| Feedback | Response / current status | Evidence |
|---|---|---|
| Sprint scope was too large | Revising labels, estimates, and sprint allocation | [Sprint Board](https://github.com/users/austin2578/projects/1) |
| Stories lacked clear deliverables and criteria | Owners will fill active story bodies with deliverables, acceptance criteria, dependencies, and scope | |
| PR #44 lacked review and merge | Approved it; merged October 23; #36 closed | PR #44 |
| Contributions were unevenly distributed | Plan individually owned documentation and prototype contributions, with pairing where helpful | [Sprint Board](https://github.com/users/austin2578/projects/1) |
| PRs were large and arrived late | Propose smaller increments, earlier draft PRs, and named reviewers |  |


## Contribution snapshot

All five members attended every team meeting during October 8–21.
Attendance for any additional meetings through October 24 will be added separately.

| Member | Contributions |
|---|---|
| Austin | Organized much of the repository, authored the team gameplay design PDF, suggested Dead Cells as a reference, and completed proposal §2 through PR #42 |
| Max | Contributed gameplay direction ideas, suggested Salt and Sanctuary as a reference, prepared meeting notes, and completed the combat pipeline through PR #45 |
| Kim | Contributed gameplay mechanic and enemy-type ideas; is helping revise labels, estimates, and sprint allocation |
| Rebekah | Reviewed the gameplay design PDF, approved and merged PR #44, and is helping revise labels, estimates, and sprint allocation |
| Brady | Implemented and documented movement/traversal through PR #44 and reviewed and merged combat PR #45 |

Contributors will confirm these descriptions. Supporting gameplay
document and meeting-note links will be added where available.
Discussion and coordination are recorded separately from merged
deliverables.

## What we propose to change in Sprint 2

- Commit to a smaller workload based on demonstrated capacity.
- Include specifications and planning alongside prototype development.
- Give every active item an owner, deliverable, and acceptance criteria.
- Open draft PRs earlier and identify reviewers.
- Document blockers and carry-over decisions when they occur.
- Keep approved gameplay requirements consistent across documents and issues.
- Maintain the sprint review throughout the sprint.

The team will confirm these changes during Sprint 2 planning.
