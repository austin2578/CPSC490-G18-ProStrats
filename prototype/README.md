# Project Rogue prototype

> Epic: #30 · Stories: #36, #37 · Documentation task: #59

Project Rogue is a 2D action roguelite with flexible starting classes and builds
that develop through weapons, attributes, elements, and run choices. Planned
classes supply starting stats/equipment and a persistent identity passive while
allowing investment in any attribute and use of any weapon family. This Unity prototype contains separate
combat and movement/traversal demos, rather than a complete run.

## Setup on a fresh machine

1. Install Unity Hub and Unity Editor **6000.3.25f1**, as recorded in
   [ProjectVersion.txt](ProjectRogue/ProjectSettings/ProjectVersion.txt).
2. Clone the repository and check out the branch under review (normally
   `develop` for the integrated prototype).
3. In Hub, select **Add > Add project from disk**, choose
   `prototype/ProjectRogue` (containing `Assets`, `Packages`, `ProjectSettings`),
   and open it with the pinned Editor version.
4. Let Unity import assets and restore packages. The first restore needs internet
   access. Versions are in [manifest.json](ProjectRogue/Packages/manifest.json)
   and [packages-lock.json](ProjectRogue/Packages/packages-lock.json), including
   URP 17.3.0 and Input System 1.20.0.
5. Resolve red Console errors before testing. The demos need no backend, API key,
   database, Python environment, or external game account.

## Run and test combat

Open `Assets/Scenes/Test_Combat.unity`, press **Play**, and click the Game view.
The scene contains `TestPlayer`, `TargetDummy`, ground, and a camera.

| Key | Action |
|---|---|
| J | Basic attack |
| K | Active ability |
| M | Cycle Sword, Bow, and Wand test loadouts; selection appears in the Console |

These temporary combat loadouts differ from the traversal weapon selector.
Keys 1/2/3 require an optional `IWeaponSource`; no concrete implementation is
present. N has a debug binding but no weapon-swap handler in this demo.

Manual checks (expected behavior from source; record actual results):

1. With Sword selected, press J/K. A target inside the melee hitbox should log
   damage and lose `Health.Current`, visible in the Inspector during Play mode.
2. Cycle to Bow and Wand with M. Their projectile abilities should spawn an
   arrow/bolt and damage a target on contact.
3. Inspect `TestPlayer` while using an ability with a resource cost. Stamina/mana
   should decrease and regenerate. Repeated input inside the cooldown should
   not repeatedly spend resources or attack.
4. Change the ability's scaling attribute in `AttributeSet` during Play mode and
   repeat the same attack. Expected damage is
   `(base damage + attribute * scalingPerPoint) * damageMultiplier`.
   The fallback base is 10 unless a weapon source supplies it.
5. Deplete the dummy's health: it should die and disappear when `destroyOnDeath`
   is enabled. Stop/restart Play mode to reset.

See the [starting playable class diagram](../docs/design/combat-class-diagram.md)
for the planned Knight/Mage/Thief/Archer profiles. These starting classes and their
passives are not implemented by the combat test loadout selector.

## Run and test traversal

Stop Play mode, open `Assets/Scenes/MovementFoundation.unity`, and press Play.
Use A/D or Left/Right to move, Space to jump, and Left Shift for the selected
weapon utility. Use the on-screen panel to select a weapon and element.

| Selection | Expected behavior or current limitation |
|---|---|
| Sword | Forward dash |
| Axe | Jump, then Shift for a downward slam |
| Bow | Shift on the ground for a backward hop |
| Fire / Water / Wind | Passive data placeholders; no combat damage or attack changes |

Reach the exit: its marker should turn green and the Console should report
`Traversal room complete: entrance to exit reached.` Falling below the room's
threshold should return the player to the entrance and increment `FallCount`.
Stop/restart Play mode to reset. This separate scene does not prove integration
with the combat demo.

## Windows standalone build check

In Unity, open **File > Build Profiles**, select Windows, and switch platform
if needed. Install Windows Build Support through Hub if the Editor requests it.
Add `Test_Combat` and `MovementFoundation` to the profile's scene list, enabling
only the scene you want to smoke-test as the first scene. Choose **Build and
Run** and select an output folder outside `Assets` and outside the repository.
Repeat with the other scene first to test both demos separately. Keep the
executable with its generated data/support files. Record the build result,
launch result, controls tested, and any errors; a Windows build has not been
verified as part of this documentation update.

## Current status and sprint milestone

As of November 3, 2026, documentation tasks #59 and #60 are assigned to **Sprint 2**.
This README prepares setup and review; human acceptance and a merged PR are still
needed before those tasks are considered done.

| Area | Checked-in state |
|---|---|
| Movement/traversal (#36) | Movement, jump, weapon utilities, handcrafted traversal room |
| Combat (#37) | Basic/active attacks, cooldowns, resources, scaling, melee/projectiles, damage/death |
| Attributes | Temporary `AttributeSet` stub, not completed progression |
| Equipment | `IWeaponSource` contract and temporary Sword/Bow/Wand combat loadouts |
| Elements | Selector and passive data; no status-effect or attack-speed integration |
| Full run | No demonstrated complete branching run, reward loop, enemy encounters, or five-session playtest study |

The Python `login.py`, `tests/test_login.py`, and `requirements.txt` remain course
scaffold files. Current CI runs those Python tests: a green **Prototype build &
tests** job does not verify Unity compilation or gameplay. For Unity evidence,
record the Editor version, scene, observed results, and Console errors from the
manual checks above.

## Troubleshooting

- Hub cannot open it: select `prototype/ProjectRogue`, not the repository root.
- Input does nothing: focus the Game view and confirm Play mode is active.
  Combat uses J/K/M; traversal uses a separate input component/action asset.
- J/K do nothing: inspect `CombatTestLoadouts` on `TestPlayer` and its ability
  references. The direct `PlayerCombat` ability fields start empty and are
  populated by the loadout component at runtime.
- Attacks miss: check facing, range, hit mask, target collider, and `Health`.
  Select the player to see its melee hitbox gizmo.
- Projectile warning: assign the projectile prefab on `AbilityData`. The code
  spends resources and starts cooldown before reporting a missing prefab.
- Pink materials or missing packages: finish package restoration and inspect
  the Console using the pinned Editor version.

Verification scope: checked against scripts, scene references, package files,
and CI configuration. These manual checks are instructions, not a report of a
new Unity runtime test.
