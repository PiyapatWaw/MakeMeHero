# Make Me Hero

Unity 6 LTS 2D prototype workspace.

## Asset layout

- `Assets/Art/Board`: background, reusable tile borders, connector, and vortex animation sheet.
- `Assets/Art/Characters`: player and wolf sprite sheets.
- `Assets/Prompts`: reusable LLM content templates.

The board assets are independent PNGs. Compose tiles and connectors as separate GameObjects; rotate `connector-horizontal.png` for vertical links. The vortex sheet contains four 256px frames in a horizontal idle-loop sequence.

`Assets/Prompts/character-evolution-prompt.txt` uses `{0}` as the current-class placeholder. Append a unit's runtime experience/history as the `Unit context` section before calling an LLM.

`Assets/Prompts/player-spritesheet-prompt.txt` and `Assets/Prompts/monster-spritesheet-prompt.txt` are image-generation templates. Replace `{0}` with the player class or monster type before generating a sprite sheet.
