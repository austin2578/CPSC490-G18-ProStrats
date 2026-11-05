# Specification — Flexible character progression

> Epic: #29 · Stories: #32, #33, #34, #35 · Owner: Austin Monroe · Sprint: 2 · Status: draft

The exact objective wording is defined in proposal §2. If this specification and proposal §2 disagree, proposal §2 takes precedence.

## Scope

**In scope.** Starting classes and their initial character profiles; Vigor, Strength, Dexterity, Mind, and Constitution; Health, Stamina, and Mana; run-local leveling and Attribute Points; attribute respec; active weapon replacement; S/A/B/C/D/E weapon scaling; weapon-family Mastery; randomized weapon affixes; equipped active weapon skills; boons; status effects; effect-source tracking; and interactions between these systems during a run.

**Out of scope.** Final balancing values for every class, weapon, boon, skill, and status; complete content for every possible weapon family; permanent meta-progression between runs; final UI artwork; final audio/animation polish; and systems unrelated to character progression such as route-map generation and enemy encounter behavior. Those systems are covered by Goals 2 and 3 or later implementation work.

## Functional requirements

### Starting classes — Objective 1.1

| ID | Requirement | Story |
| --- | --- | --- |
| FR-1 | When a player begins a run, the system shall allow the player to select from at least three starting classes. | #32 |
| FR-2 | When a starting class is selected, the system shall initialize the character with that class's configured starting attribute profile. | #32 |
| FR-3 | When a starting class is selected, the system shall initialize the character with that class's configured starting resource profile. | #32 |
| FR-4 | When a class-specific traversal or utility capability is used, the system shall apply the capability associated with the selected class. | #32 |
| FR-5 | When the player attempts to equip a weapon family, the system shall allow the weapon regardless of the player's starting class. | #32 |

### Attributes, resources, and respec — Objective 1.2

| ID | Requirement | Story |
| --- | --- | --- |
| FR-6 | When a run begins, the system shall track Vigor, Strength, Dexterity, Mind, and Constitution as the player's five primary attributes. | #33 |
| FR-7 | When a run begins, the system shall track Health, Stamina, and Mana as player resources. | #33 |
| FR-8 | When the player gains a level during a run, the system shall award three Attribute Points. | #33 |
| FR-9 | When the player spends an Attribute Point, the system shall increase the selected attribute and reduce the player's unspent Attribute Points accordingly. | #33 |
| FR-10 | When the player performs a valid respec, the system shall return previously invested Attribute Points and allow them to be reassigned. | #33 |
| FR-11 | When an attribute or resource value changes, systems that depend on that value shall use the updated value during subsequent calculations. | #33 |

### Weapons, Mastery, affixes, and skills — Objective 1.3

| ID | Requirement | Story |
| --- | --- | --- |
| FR-12 | When the player equips a weapon, the system shall make that weapon the player's single active weapon. | #34 |
| FR-13 | When the player replaces the active weapon during a run, the system shall remove the previous active weapon from the active slot and activate the replacement. | #34 |
| FR-14 | When weapon damage or another scaled weapon value is calculated, the system shall apply the weapon's configured S/A/B/C/D/E attribute scaling. | #34 |
| FR-15 | When the player replaces a weapon with another weapon from the same weapon family, the system shall retain the player's Mastery progress for that family. | #34 |
| FR-16 | When a weapon is generated with randomized affixes, the system shall apply those affixes to the weapon instance without changing the base definition shared by other weapons. | #34 |
| FR-17 | When the player configures active weapon skills, the system shall allow no more than three active weapon skills to be equipped at one time. | #34 |

### Boons, statuses, and build interactions — Objective 1.4

| ID | Requirement | Story |
| --- | --- | --- |
| FR-18 | When the player gains a boon or effect, the system shall be able to modify attacks, resources, or status behavior according to that effect's definition. | #35 |
| FR-19 | When a status effect is applied, the system shall track the effect and its source for the affected actor. | #35 |
| FR-20 | When one effect triggers another effect, the system shall track proc depth so uncontrolled recursive effect loops can be stopped. | #35 |
| FR-21 | When the Goal 1 prototype is evaluated, the system shall demonstrate at least four distinct interactions involving weapons, attributes, boons, affixes, resources, or statuses. | #35 |

## Non-functional requirements

| ID | Requirement | Why it matters here |
| --- | --- | --- |
| NFR-1 | Character progression systems shall use shared stat, resource, effect, and weapon interfaces rather than creating separate incompatible implementations for each class. | Classes must remain flexible enough to use the same weapons and progression systems. |
| NFR-2 | Adding a new class, weapon definition, boon, affix, or status should primarily require adding configuration/data rather than rewriting existing character-progression logic. | The project requires an extensible prototype that can support additional content during later sprints. |
| NFR-3 | Progression state shall remain consistent when the active weapon changes or when Attribute Points are reassigned. | Weapon replacement and respec are central to Goal 1 and must not corrupt the current build. |
| NFR-4 | Proc-depth protection shall prevent an effect interaction from producing an uncontrolled infinite trigger chain. | Goal 1 intentionally allows interacting effects, so recursion must remain controlled. |
| NFR-5 | Goal 1 functionality shall operate in the team's shared Unity project version and shall not require a class-specific fork of the combat or resource systems. | All teammates must be able to integrate progression work into the same prototype. |

## Dependencies and system interactions

| Goal 1 system | Depends on / interacts with | Relationship |
| --- | --- | --- |
| Classes (#32) | Movement/traversal (#36) | Class-specific traversal or utility capabilities use the player movement foundation from Goal 2. |
| Attributes/resources (#33) | Combat/damage/abilities (#37) | Attributes and resources provide values used for damage scaling, costs, survivability, and abilities. |
| Weapons/Mastery (#34) | Combat/damage/abilities (#37) | Weapon attacks and active skills must use the shared Goal 2 combat pipeline rather than their own damage system. |
| Boons/statuses (#35) | Combat/damage/abilities (#37) | Effects modify or respond to attacks, resources, damage, and statuses through shared systems. |
| Progression systems (#32–#35) | Node/reward/run progression (#40) | Goal 3 rewards can grant progression resources or build options used by Goal 1. |
| Build choices (#32–#35) | Playtesting (#41) | Goal 3 playtests record class, weapon, and build decisions to evaluate whether Goal 1 produces meaningful variety. |

## Acceptance criteria (epic level)

- [ ] A player can begin a run using at least three distinct starting classes.
- [ ] Each starting class has a distinct starting attribute/resource profile and one class-specific traversal or utility capability.
- [ ] Every starting class can equip every supported weapon family.
- [ ] Vigor, Strength, Dexterity, Mind, Constitution, Health, Stamina, and Mana are represented through the shared progression systems.
- [ ] Leveling grants three Attribute Points per level.
- [ ] A player can invest Attribute Points and perform a respec that returns those points for reassignment.
- [ ] The player has one active weapon at a time and can replace that weapon during a run.
- [ ] Weapon scaling supports S/A/B/C/D/E grades.
- [ ] Weapon-family Mastery remains when replacing a weapon with another weapon from the same family.
- [ ] Randomized affixes can modify weapon instances.
- [ ] The player can equip up to three active weapon skills.
- [ ] Boons/effects can modify attacks, resources, or statuses.
- [ ] At least four distinct build interactions can be demonstrated.
- [ ] Effect-source and proc-depth tracking prevent uncontrolled recursive effects.
- [ ] Functional requirements FR-1 through FR-21 can be demonstrated or tested against their linked stories.
- [ ] `python .github/scripts/check_repo.py` passes.

## Open questions

| Question | Owner | Needed by |
| --- | --- | --- |
| What exact starting attribute and resource profiles will Knight, Mage, and Thief use? | Austin / team | Before #32 implementation is finalized |
| What traversal or utility capability will each starting class receive? | Austin / team | Before #32 implementation is finalized |
| Does respec cost Gold, another resource, or nothing? | Austin / team | Before #33 respec implementation |
| Where can the player perform a respec during a run? | Austin / team | Before #33 respec implementation |
| Which weapon families must be represented in the prototype? | Austin / team | Before #34 acceptance testing |
| How is weapon-family Mastery earned and how much is required for the prototype? | Austin / team | Before #34 implementation is finalized |
| How many affixes must exist beyond the minimum needed to demonstrate randomized affixes? | Austin / team | Before #34 acceptance testing |
| Which four boon/status/build interactions will be used to satisfy Objective 1.4? | Austin / team | Before #35 acceptance testing |

Open questions remain unresolved until the team makes an explicit decision; they should not be silently filled in during implementation.
