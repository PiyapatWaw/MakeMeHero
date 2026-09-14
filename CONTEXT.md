# Hero Defense

Hero Defense is an auto-battle city-defense prototype whose units develop through survival, rank progression, and recorded gameplay experience.

## Combat

**Skill**:
A unit-owned auto-cast ability, separate from its normal attack. A Skill resolves its effect using its declared area; Phase 0 initially supports only the caster's own tile.
_Avoid_: Attack modifier, player-triggered ability

**Skill Area**:
The set of tiles affected when a Skill casts, calculated from the caster's tile as its anchor. Phase 0 supports only the Self area; adjacent and shaped areas are later extensions. Normal attacks also target only the combatant's current tile in Phase 0.
_Avoid_: Target range, free-target zone

**Target Relation**:
The relationship a Skill effect accepts within its affected tiles: Self, Ally, Enemy, or Any. It is separate from Skill Area so a tile shape and a target rule can evolve independently.
_Avoid_: Skill Area, target range

**Skill Cooldown**:
The elapsed combat time a Skill must wait after casting before it may auto-cast again. Phase 0 uses cooldown readiness only and has no mana or energy resource.
_Avoid_: Mana cost, player charge

**City HP**:
The city's remaining health for the current Run. A new Phase 0 Run starts at 25 HP. A monster reaching the city applies its own City Damage once and is then removed from the encounter.
_Avoid_: Immediate loss on arrival

**City Damage**:
The fixed damage value an individual monster applies to City HP when it reaches the city. It allows standard monsters and future bosses to share the same arrival rule.
_Avoid_: Global arrival damage

**Run Lost**:
The terminal Run state reached when City HP is zero. It freezes battle resolution, retains the completed Run's history and statistics, and opens the Run Summary.
_Avoid_: Reset battle, discard encounter

**Run Timeline**:
A chronological record for one Run showing each unit's recruitment, rank changes, skill gains, death or survival, and the experience accumulated between those milestones.
_Avoid_: Combat log, unit list

**Experience Evidence Log**:
An audit-oriented record of the gameplay events that explain how one unit reached its current Character Experience Profile. It is richer than the player-facing Run Timeline and supports research/debug inspection.
_Avoid_: Player timeline, opaque experience score

**Combat Event**:
An immutable, append-only fact from a Run that can change combat state, statistics, or future experience interpretation. Events carry the Run day, encounter, room context, source and target when applicable, and the relevant value. Phase 0 records recruitment, formation changes, rank changes, day start and end, spawn and movement, normal attack and Skill effects that occur, damage, healing, kills, death, City Damage, and Run Lost; it does not record cooldown ticks or actions without a valid target.
_Avoid_: Frame log, mutable combat record

**Run Repository**:
The boundary through which a Run is stored and retrieved. Phase 0 uses an in-memory implementation while preserving the option to add durable persistence later. A new Run always creates a new roster and characters; a completed Run remains an in-memory archive with its character profiles, timeline, and evidence for the current session.
_Avoid_: Save-file format, global game state

**Tile Capacity**:
A battlefield tile permits up to three heroes. Monster occupancy is not a fixed three-unit cap: when living heroes are present, a tile admits new monsters only until its monster count equals its living hero count; when no living heroes are present, monsters pass through without an occupancy limit from this rule. A hero death never ejects monsters already engaged in that tile; they remain until no living hero remains, then continue on their next movement tick.
_Avoid_: Single-unit tile, shared capacity

**Tile Engagement**:
Concurrent auto-battle among all living heroes and monsters on one tile. Every combatant acts independently by cooldown, and monsters cannot advance while living heroes remain on that tile.
_Avoid_: Turn queue, single duel

**Targeting Rule**:
The deterministic selection rule used by auto-combat within one Tile Engagement. Offensive actions choose the living enemy with lowest current HP; heal/support actions choose the living ally with lowest HP percentage, including the caster when applicable; CombatantId breaks ties.
_Avoid_: Random target, manual target selection

**Simultaneous Combat Resolution**:
At one combat timestamp, every ready action is planned from the pre-resolution state and then all resulting effects are applied together. A unit may therefore deal its ready attack or skill even if effects at that same timestamp also kill it.
_Avoid_: Initiative advantage, sequential same-timestamp resolution

**Combat Readiness**:
Field heroes begin an encounter with normal attack and Skill cooldowns ready. A wolf begins its normal attack readiness when it spawns and schedules its first movement one second after spawning. A wolf in a tile with living heroes does not move out while that Tile Engagement remains active.
_Avoid_: Initial cooldown wait, immediate spawn movement, movement through engagement

**Action Priority**:
A unit resolves no more than one combat action at a timestamp. If its Skill is ready and has a valid target, it casts the Skill; otherwise it uses its ready normal attack. The cooldown of a ready action not selected remains ready.
_Avoid_: Skill plus normal attack burst, random ready-action selection

**Same-Skill Effect Replacement**:
An active effect does not stack with another active effect bearing the same Skill ID in the same scope. A later cast replaces the existing effect and starts the later cast's duration anew; for example, a second Soldier Guard replaces the first Guard on that tile.
_Avoid_: Same-skill stacking, strongest-effect selection

**Hero Permadeath**:
Only a hero deployed in Field can die from combat. On death, the hero is permanently removed from playable Field and Reserve roster capacity for that Run, while its Character Experience Profile, Run Timeline milestones, and Experience Evidence Log remain available for inspection.
_Avoid_: Reserve death, revivable Phase 0 hero, discarded history

**Path Tie**:
A choice between neighboring battlefield tiles with equal shortest-path distance to the city. Phase 0 resolves a Path Tie randomly rather than through a fixed directional priority.
_Avoid_: Direction priority

**Monster Navigation**:
On each movement tick, a monster considers the four orthogonally adjacent battlefield tiles and chooses an eligible tile that minimizes its shortest remaining path distance to the city. Equal-distance choices are Path Ties. A destination containing living heroes is ineligible when its monster occupancy already equals its living hero count. Each chosen destination, including a random Path Tie result, is retained as an immutable movement event for audit.
_Avoid_: Fixed lane, diagonal movement, deterministic direction priority

**Battlefield Topology**:
The Phase 0 battlefield is a 3×3 orthogonal grid. City is outside and connected only to the left-middle tile; Spawn is outside and connected only to the right-middle tile. All grid connections are horizontal or vertical, with no diagonal movement.
_Avoid_: Attached city/spawn tile, diagonal graph, fixed three-tile lane

**Encounter Scaling Policy**:
The replaceable rule that determines an encounter's monster count and spawn cadence from its day. The Phase 0 baseline spawns `ceil(3 × 1.3^(day−1))` wolves, one every one second; it therefore yields 3 wolves on Day 1, 4 on Day 2, 6 on Day 3, and 9 on Day 5.
_Avoid_: Fixed wave count, hard-coded encounter script

**Standby Phase**:
The player-controlled phase before an encounter. The Run is not resolving combat; the player may perform the permitted preparation actions such as recruitment and formation, then explicitly starts the next day. Starting an encounter is allowed even when Field is empty.
_Avoid_: Auto-start next day, battle pause

**End-of-Day Phase**:
The non-combat resolution after all monsters in an encounter are defeated. It grants that day's rewards, finalizes each relevant unit's experience records, restores every surviving hero to full HP, and then transitions the Run to Standby Phase. It does not occur after Run Lost.
_Avoid_: Immediate next encounter, Run Summary

**Day Reward**:
The Gold granted at End-of-Day Phase. Its Phase 0 amount is `10 + 5 × (day−1)`, so Day 1 grants 10 Gold and Day 2 grants 15. Phase 0 uses Gold as its only player-spendable day reward; experience is recorded for every relevant unit separately and is never treated as currency.
_Avoid_: Experience currency, mixed reward resource

**Recruit Cost**:
The Phase 0 Gold price to recruit one hero in Standby Phase. It is fixed at 10 Gold.
_Avoid_: Dynamic shop price, experience cost

**Starting Roster**:
A new Phase 0 Run begins with one Soldier, one Archer, one Mage, and one Healer, plus 100 Gold. These four heroes are distinct character instances, not reusable class templates.
_Avoid_: Empty starting roster, class-as-character

**Hero Display Name**:
Until a naming feature exists, every recruited hero receives an automatic display name made from its class and unique sequence, such as `Soldier #1` or `Mage #2`. The immutable character ID remains the audit key behind that display name.
_Avoid_: Class name as identity, mutable display name as audit key

**Direct Recruitment**:
During Standby Phase, the player chooses one of the available hero classes to recruit, paying its Recruit Cost. Recruitment follows a direct build-menu model; it is not a random class draw.
_Avoid_: Gacha recruitment, random class draw

**Reserve Capacity**:
Up to 30 living, undeployed heroes may be held in Reserve. It is separate from Field capacity, which follows the 3 heroes per tile limit across the 3×3 battlefield (27 maximum).
_Avoid_: Combined roster cap, unlimited reserve

**Formation**:
Only during Standby Phase, the player may recruit a hero into Reserve and freely move living heroes between Reserve and Field or reposition them on Field, provided a tile holds no more than three heroes. Formation cannot change during an encounter.
_Avoid_: Mid-battle repositioning, direct recruit-to-field placement

**Phase 0 Class Roles**:
Soldier is durable and protects allies, Archer attacks quickly, Mage has high damage, and Healer restores the ally with the lowest HP percentage. Each class has one auto-cast Skill that resolves only inside its caster's tile in Phase 0.
_Avoid_: Identical class stats, cross-tile Phase 0 skills

**Phase 0 Combat Tuning**:
The replaceable baseline configuration is: Soldier 45 HP, 6 attack damage every 1.0s, with a tile-wide 50% hero damage reduction for 3s on an 8s cooldown; Archer 28 HP, 8 damage every 0.8s, with a 12-damage hit to every enemy in tile on a 5s cooldown; Mage 24 HP, 10 damage every 1.2s, with an 18-damage hit to every enemy in tile on a 6s cooldown; Healer 32 HP, 3 damage every 1.1s, with a 14-HP heal to the lowest-HP-percentage hero in tile on a 5s cooldown. Wolf has 24 HP, deals 5 damage every 1.3s, moves every 1s, and deals 1 City Damage on arrival. Core HP and damage use decimal values without combat-time rounding.
_Avoid_: Combat constants embedded in resolution code, final balance values

**Rank Up**:
A hero starts at one star and becomes eligible for one additional star at each five accumulated survived days (5, 10, 15, and so on). In Standby Phase, the player may manually increase that hero's rank by one star, up to seven stars. Each rank-up is a timeline and evidence milestone and grants one additional Skill slot. In Phase 0, added slots remain empty rather than assigning a new skill; they reserve space for later content-driven evolution.
_Avoid_: Automatic rank-up, unbounded rank, unrecorded progression

**Unit Experience Record**:
The persistent per-unit experience data finalized at End-of-Day Phase from the Run's Combat Events and milestones. Only heroes deployed in Field that survive the encounter receive that day's record and one survived day. Phase 0 stores daily aggregates for damage dealt and taken, healing, kills, skill casts, and days survived, backed by the immutable evidence events. It feeds the player timeline and the deeper Experience Evidence Log; it does not create a level or a single XP score.
_Avoid_: Disposable match score, spendable XP, automatic level

**Character Experience Profile**:
The audit-backed, per-character history and statistical summary available to a future interpretation policy or LLM. It enables a later system to propose an appropriate evolution direction or Skill based on how that individual actually played, without modifying the original evidence.
_Avoid_: Predefined experience tree, opaque LLM output
