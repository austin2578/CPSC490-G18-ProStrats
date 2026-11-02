# Playtest Protocol

> Epic: #31 · Story: #41 · Task: #48

## Purpose and Questions

This protocol prepares a repeatable study of build variety and player
decision-making for Objective 3.3.

The study will investigate:
- Do testers produce distinct weapon-and-element builds?
- Do rewards or route options cause testers to change their plans?
- Can testers explain their choices and understand their effects?

## Readiness Requirements

Before the full study begins, the observer must verify that:

- The tested build is identified by its Git commit.
- Players can select or receive a starting weapon and element.
- Players can make and observe changes to their build.
- Rewards provide choices that can affect later decisions.
- Players encounter route choices and can select a route.
- The observer can record available options, player choices,
  and the player's reasons.
- Controls and starting conditions are documented and used
  consistently across sessions.

Record whether each requirement passes, fails, or is unavailable,
with supporting observations.

If required functionality is missing or a bug prevents a session
task, record the limitation. Treat that session as a pilot rather
than evidence that the full study is complete.

A pilot checks whether the instructions and recording template
work. It does not establish build variety or meaningful decisions.

## Tester Instructions and Session Tasks

### Observer Preparation

Before each session:

1. Assign a session ID and record the build commit.
2. Check the readiness requirements.
3. Record the controls, starting conditions, available choices,
   and session endpoint.
4. Use the same instructions and task sequence for every tester.
5. Record any differences in conditions or assistance provided.

### Instructions Read to Every Tester

"We are testing how understandable the game's choices are.
We are not testing your skill.

Play using the provided controls. At weapon, element, reward,
or route choices, explain what you intend to choose and why.
Tell us if something is unclear. You may keep your current
build or change it; neither outcome is preferred."

### Session Tasks

1. Review the controls and identify your starting weapon
   and element.
2. Describe your initial build or strategy.
3. Play until a reward or build choice is available.
   Explain the options and your reason for your selection.
4. At a route choice, explain which route you select and why.
5. Continue to the documented session endpoint, explaining
   any later choices or changes to your original plan.
6. Answer the follow-up questions.

### Observer Conduct

- Record available options, selections, and stated reasons.
- Ask neutral prompts such as "What influenced that choice?"
- Do not recommend a build, reward, or route.
- Distinguish observed behavior from the tester's explanation.
- Record bugs, interruptions, assistance, and incomplete tasks.
- Do not require a tester to change builds merely to produce
  a changed decision.

## Session Recording Template

Copy this template for each future session. Leave fields blank
until testing occurs; use "not available" or "not observed"
where appropriate.

### Session Details

- Session ID:
- Anonymous tester ID:
- Date:
- Observer:
- Session type: pilot / full study
- Build commit:
- Any uncommitted build changes:
- Readiness check results and evidence:
- Controls provided:
- Starting conditions:
- Planned session endpoint:
- Actual endpoint and reason for stopping:
- Starting weapon:
- Starting element:
- Initial build, including available attributes or upgrades:
- Player's initial plan:
- Final build:
- Bugs, interruptions, or assistance:
- Tasks completed / incomplete:

### Decision Log

Repeat this entry for each weapon, element, build, reward,
or route choice, including choices to keep the current build.

- Decision number:
- Task or location:
- Available options:
- Build before the choice:
- Player's intended choice and stated reason:
- Actual selection:
- Reward received or route selected:
- Build after the choice:
- Did the player report changing their earlier plan?
- Player's explanation of what influenced the decision:
- Observer notes:
- Missing evidence or uncertainty:

### Follow-Up Questions

Record the player's answers separately from observer interpretation.

1. How would you describe your final build and how it works?
2. Which options or effects were clear, and which were confusing?
3. What influenced your reward and route choices?
4. Did you change your original plan? Why or why not?
5. Were there alternatives you considered but rejected? Why?
6. What information would have helped you make your choices?

## Analysis Plan and Definitions

### Comparing Sessions

Compare at least five completed full-study sessions. Report pilot
and incomplete sessions separately, including why they were excluded.

Use one row per full-study session in a comparison table containing:

- Session ID and anonymous tester ID.
- Build commit and differences in testing conditions.
- Starting weapon, element, and initial plan.
- Final build and changes made during play.
- Rewards and routes selected.
- Decisions changed and the player's stated reasons.
- Clarity problems, assistance, and missing evidence.

Report the number of unique testers and any repeat participation.
Prefer the same build commit and starting conditions. If these differ,
group results by version or condition and explain how this limits
comparison.

### Distinct Builds

For this study, describe a build using its weapon family, element,
and available gameplay-affecting upgrades, attributes, or abilities.

Classify two builds as distinct when at least one recorded difference
changes an available action or gameplay effect. Cosmetic differences
alone do not count.

Record the specific difference and its effect. If the effect cannot
be verified, classify the distinction as uncertain.

Different routes alone do not establish distinct builds.

### Changed Decisions

Count a changed decision when the recorded choice differs from the
player's previously stated plan and the player identifies a reward
or route option that influenced the change.

Link each classification to its decision-log entry. A build change
alone does not prove that the player's plan changed.

Classify each relevant decision as:

- Changed: a prior plan, different choice, and stated influence
  are recorded.
- Maintained: the player explicitly keeps the earlier plan.
- Unclear: evidence is missing or contradictory.

### Reporting Findings

Summarize:

- How many distinct builds were observed and their differences.
- How many sessions contained changed, maintained, or unclear
  decisions.
- Which rewards or route options players said influenced choices.
- Common points of confusion and suggested improvements.

Support findings with session IDs and decision-log entries.
Report counts with their denominators. Do not treat missing
observations as evidence that no change occurred.

## Limitations and Pilot Testing

Pilot sessions rehearse the instructions and recording process.
Revise the protocol after pilots and before the full study.

At least five full-study sessions are required for the planned
comparison. Five sessions do not necessarily mean five unique testers;
report both counts.

A small sample cannot establish long-term replayability, overall
balance, or results for all players. Prior experience, repeated
participation, observer prompts, random rewards, and build-version
differences may influence results.

Unavailable mechanics cannot be evaluated. Separate player statements
from observed behavior, and report uncertainty rather than inventing
results.