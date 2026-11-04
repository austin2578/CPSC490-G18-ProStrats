# Specification — Action gameplay
 
> Epic: #30 · Stories: #36, #37, #38
> Owner: Max Armenta · Sprint: 2 · Status: draft
 
## Scope
 
**In scope.** Movement and traversal in one handcrafted room (2.1, #36). A
shared combat pipeline for basic attacks and active abilities, shown with a
sword (melee), a bow, and a wand (projectiles) (2.2, #37.
Three enemy encounter configurations whose enemies fight through that
same pipeline (2.3, #38). The exact objective wording is in proposal §2.
 
**Out of scope**
: the classes themselves (1.1), attributes, leveling, and respec (1.2),
weapon progression (1.3), boons and statuses (1.4), the route map and reward
economy (Goal 3), final art and audio, balance tuning.
 
**Depends on.** 1.1 (#32) for each class's traversal utility. 1.2 (#33) for
attribute values (a stand-in is used until it lands). 1.3 (#34) for weapon
damage and skills, through one interface. 1.4 (#35) hooks in after damage is
resolved. 3.1 (#39) loads rooms and encounters. 3.2 (#40) supplies reward
contents.
 
## Functional requirements
 
| ID | Requirement | Story |
|---|---|---|
| FR-1 | When the player holds a left or right input, the system shall move the player horizontally in that direction. | #36 |
| FR-2 | When the player presses jump while standing on the ground, the system shall make the player jump. | #36 |
| FR-3 | While the player is in the air, the system shall apply gravity, report the player as not grounded, and not allow a normal jump. | #36 |
| FR-4 | When the player uses a class traversal utility, the system shall perform that class's capability. | #36 |
| FR-5 | When a player uses only these mechanics, the handcrafted traversal room shall let them travel from its entrance to its exit. | #36 |
| FR-6 | When the player uses a basic attack or an active ability, the system shall resolve it through one shared pipeline: cooldown check, resource cost, scaled damage, then hit. An attack used before its cooldown ends shall be ignored. | #37 |
| FR-7 | When an attack is used, the system shall compute damage as (base damage + attribute value × the ability's per-point scaling) × the ability's multiplier. | #37 |
| FR-8 | When an attack costs more Stamina or Mana than the player has, the system shall not perform the attack and shall not spend any resource. | #37 |
| FR-9 | When an attack hits a target that can take damage, the system shall reduce its health by the computed damage, and when health reaches zero the system shall mark it defeated. | #37 |
| FR-10 | When a melee attack is used, the system shall hit each damageable target inside its hitbox once; when a projectile attack is used, the system shall launch a projectile that damages the first valid target it touches and then disappears, or disappears after its lifetime. | #37 |
| FR-11 | The system shall provide at least three enemy encounter configurations, each playable from start to completion. | #38 |
| FR-12 | When the player is within an enemy's engage range, the enemy shall engage the player. | #38 |
| FR-13 | When an enemy attacks, the system shall resolve the attack through the same combat pipeline the player uses (FR-6 to FR-9). | #38 |
| FR-14 | When the player's attack damages an enemy, the system shall reduce its health, and when it reaches zero the enemy shall die. | #38 |
| FR-15 | When every enemy in an encounter has died, the system shall mark the encounter complete and trigger its reward. | #38 |
 
## Non-functional requirements
 
| ID | Requirement | Why it matters here |
|---|---|---|
| NFR-1 | Every source of damage shall go through the one shared damage function; there shall be no separate damage path for any weapon or enemy. | Weapon scaling (1.3), boons (1.4), and enemy attacks all plug in at one place. |
| NFR-2 | Attack values (damage multiplier, scaling, cost, cooldown, hitbox) shall be editable data assets, not hard-coded. | The team can tune combat in the Inspector without code changes. |
| NFR-3 | Goal 2 code shall compile and run without the weapon system (1.3) or the final attribute system (1.2) present. | The stories ship in separate pull requests, so each must work alone. |
| NFR-4 | An attack input shall produce its effect (a hit, or a spawned projectile) on the frame it is pressed. | Combat has to feel responsive in the live demo. |
 
## Acceptance criteria (epic level)
 
- [ ] (2.1) A tester completes the handcrafted traversal room, entrance to
      exit, using the keyboard only.
- [ ] (2.1) Pressing jump in mid-air does not start a second jump.
- [ ] (2.1) Each class's traversal utility works in the room (needs #32).
- [ ] (2.2) The player defeats a target using a basic attack, and separately
      using an active ability.
- [ ] (2.2) Raising an attribute changes the same attack's damage by the
      expected amount, and an attack the player cannot afford does nothing and
      spends nothing.
- [ ] (2.2) Sword, bow, and wand attacks each hit a target.
- [ ] (2.3) Each of the three encounter configurations can be completed, and
      completing one triggers its reward.
- [ ] (2.3) Enemy attacks damage the player through the same shared damage
      function as the player's attacks (confirmed in code review).
- [ ] A player moves through the traversal room, fights an encounter using a
      basic attack and an ability that spend resources, and the encounter
      reports completion.
- [ ] FR-1 … FR-15 each have a test or a recorded manual check in a pull
      request that would fail if the behaviour were removed.
- [ ] `python .github/scripts/check_repo.py` is green.
## Open questions
 
| Question | Owner | Needed by |
|---|---|---|
| Which traversal utility does each class get, and does it cost a resource? | Austin | before FR-4 is implemented |
| How is an encounter configured (data asset, scene, or script)? | Kim | before FR-11 is implemented |
| What are the enemy types, engage ranges, and attack patterns, and do enemies use the same ability assets and resource pools as the player? | Kim | before FR-12 is implemented |
| Is the encounter reward real (3.2) or a placeholder for the prototype demo? | Rebeka | before FR-15 is implemented |