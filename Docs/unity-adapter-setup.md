# Unity adapter setup

The Phase 0 rules remain in `Assets/Scripts/Core`. The MonoBehaviour bridge is in `Assets/Scripts/Game`.

1. Create an empty scene object named `GameController`.
2. Add `GameManager`.
3. Optionally add `RunDebugView`, then drag `GameController` into its **Controller** field.
4. Connect UI buttons to `GameManager` methods: `StartDay`, `RecruitSoldier`, `RecruitArcher`, `RecruitMage`, `RecruitHealer`, or `CreateNewRun`.
5. A board/tile presenter can call `Deploy(heroId, column, row)` during Standby Phase.

`GameManager` creates the in-memory run in `Awake()` and advances battle through a coroutine while `Run.Phase` is `Battle`. Each loop waits for **Battle Tick Seconds** (1 second by default), then advances the core by that same amount. Its `RunChanged` event is the intended hook for future HUD, board, character sprite, and effect presenters.

`GameManager` is a singleton (`GameManager.Instance`) and persists across scene loads. Keep exactly one instance in the bootstrap scene.

No scene, prefab, UI, sprite, or effect presenter is included yet. This adapter is intentionally a thin Unity layer over the tested domain core.

## Prefab-ready view components

- Add `Game/World/TileView` to each tile prefab and set **Column** / **Row** from 0–2.
- Add `Game/Characters/HeroView` to every hero prefab. Assign its `SpriteRenderer` and optional `Animator`; bind it later with `Bind(Hero)`.
- Add `Game/Characters/MonsterView` to every monster prefab. Assign the same optional visual references; bind it later with `Bind(Monster)`.

These components hold only visual identity and core ID binding. A future board/spawn presenter will instantiate the correct prefab, call `Bind`, position it from `TileView.Position`, and react to `GameManager.RunChanged`.

## Board spawning

Add `Game/Spawning/Spawner` to an empty `Battlefield` object, then assign:

- the scene `GameManager`;
- a `TileView` prefab plus optional tile/character roots;
- one `HeroView` prefab for each of Soldier, Archer, Mage, Healer;
- one `MonsterView` Wolf prefab.

At `Start`, it creates the nine tiles. Whenever `GameManager.RunChanged` fires, it creates/removes Hero and Wolf prefab instances for characters currently deployed on the board and places them at the matching 3×3 coordinate. It intentionally does not animate movement or create effects yet.

`Spawner` acquires and releases every prefab through `GameManager.Instance.Pooling`. `GameManager` creates a `Pooling` child object, and `Pooling` creates one child pool per prefab type. Tile, Soldier, Archer, Mage, Healer, and Wolf instances are reused: on release, an instance is deactivated and re-parented beneath its own prefab pool rather than destroyed.

## Reusable health bar

`Assets/Art/UI/health-bar-spritesheet.png` contains four horizontal cells: frame, empty track, green fill, and red fill. Slice it into four sprites in Unity, then assemble a world-space Canvas prefab with `Image` components for frame, track, and a fill image set to **Filled / Horizontal**.

Add `Game/UI/HealthBarView` to that prefab. Its `SetHealth(currentHp, maximumHp)` or `SetNormalized(value)` method works unchanged for City, Hero, and Monster. Use the green fill for Hero/City and red fill for Monster.
