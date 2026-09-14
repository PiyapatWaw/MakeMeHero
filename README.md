# Make Me Hero

Unity 6 LTS 2D prototype workspace.

## Asset layout

- `Assets/Art/Board`: background, reusable tile borders, connector, and vortex animation sheet.
- `Assets/Art/Characters`: player and wolf sprite sheets.

The board assets are independent PNGs. Compose tiles and connectors as separate GameObjects; rotate `connector-horizontal.png` for vertical links. The vortex sheet contains four 256px frames in a horizontal idle-loop sequence.
