# Hero Defense — Core Architecture (Phase 0)

โค้ดใน `Assets/Scripts/Core` เป็น pure C# domain core: ไม่มี `MonoBehaviour`, `GameObject`, `Scene` หรือ Unity API. หน้าที่ของมันคือบังคับกติกาเกมให้ทดสอบได้ และส่ง state/event ให้ Unity แสดงผลภายหลัง

## ภาพรวม

```text
Unity MonoBehaviour / UI
        │ command
        ▼
RunApplicationService
        │ load / save
        ▼
Run ── owns ── Characters, World state, History
        │ uses
        ├─ Configuration
        ├─ Interfaces
        └─ Infrastructure implementations
```

`Run` คือ aggregate หลักของหนึ่ง run. ภายนอกไม่ควรแก้ HP, Gold, ตำแหน่ง Hero หรือ phase โดยตรง แต่ส่งคำสั่งผ่าน `RunApplicationService` หรือ `Run` method ที่ถูกต้องตาม phase

## Folder และ class

### `Types.cs`

enum ที่ใช้ข้ามหลาย domain:

- `RunPhase` — `Standby`, `Battle`, `EndOfDay`, `Lost`
- `HeroClass` — Soldier, Archer, Mage, Healer
- `SkillId` — Guard, Volley, ArcaneBurst, Heal
- `EventType` — ชนิดข้อมูลสำหรับ audit event

### `Characters/`

```text
Character (abstract)
├─ Hero (abstract)
│  ├─ Soldier
│  ├─ Archer
│  ├─ Mage
│  └─ Healer
└─ Monster (abstract)
   └─ Wolf
```

- `Character` — base class ของสิ่งที่ต่อสู้ได้: ID, display name, HP, normal attack, cooldown และตำแหน่ง
- `Hero` — เพิ่ม class, Skill, ดาว/rank, วันรอด และ future Skill slot
- `Monster` — เพิ่ม City Damage และ movement cooldown
- `Soldier`, `Archer`, `Mage`, `Healer`, `Wolf` — concrete type คนละไฟล์; จึงเพิ่ม class/monster ใหม่ได้โดยไม่ทำให้ hierarchy เดิมเปลี่ยน
- `HeroFactory` — สร้าง subclass ที่ตรงกับ `HeroClass` ตอนเริ่ม run หรือ recruit; `Run` ไม่ต้องรู้ constructor ของทุก class

### `Configuration/`

- `CombatTuning` — ค่า balance ที่เปลี่ยนได้: HP, attack, interval และ base Skill ของแต่ละ class รวมถึง Wolf
- `ExponentialEncounterScalingPolicy` — สูตรจำนวน Wolf `ceil(3 × 1.3^(day−1))` และ interval spawn 1 วินาที

ต่อไปค่านี้สามารถย้ายเป็น ScriptableObject/JSON adapter ได้ โดยคง `Run` และ combat rule เดิม

### `World/`

- `GridPosition` — value type ของตำแหน่งใน grid 3×3 พร้อม City Gate ซ้ายกลาง และ Spawn Gate ขวากลาง

เมื่อเริ่มทำ Unity จะเพิ่ม `Battlefield`, `Tile` และ graph/navigation policy ที่ folder นี้ได้

### `History/`

- `CombatEvent` — immutable, append-only record เช่น recruit, movement, attack, skill, damage, death, City Damage และ run lost
- `ExperienceSnapshot` — สรุปสถิติ Hero ที่รอดและลง Field ต่อวัน: damage dealt/taken, heal, kill, skill cast

สอง class นี้เป็นฐานข้อมูลให้ Run Timeline, Run Summary และ LLM-driven evolution ในอนาคต โดยไม่มี level หรือ XP score ใน Phase 0

### `Interfaces/`

- `IRunRepository` — วิธีเก็บ/หา run
- `IRandomSource` — การสุ่ม path tie ที่แทน implementation ได้และ test ได้
- `IEncounterScalingPolicy` — วิธีคำนวณ wave ต่อวัน

### `Infrastructure/`

- `InMemoryRunRepository` — implementation ชั่วคราวของ repository สำหรับ session ปัจจุบัน
- `SystemRandomSource` — random implementation ปกติ

`Infrastructure` เปลี่ยนเป็น save file, cloud save หรือ seeded replay random ได้ โดยไม่เปลี่ยน domain rule

### `Run.cs`

`Run` เป็นเจ้าของ state และ game rule ของหนึ่งรอบ:

- Run phase, day, Gold, City HP และ battle clock
- roster Hero, Wolf ที่กำลังอยู่ใน encounter, formation และ tile capacity
- 3×3 navigation, path tie แบบสุ่ม, City Damage และ Run Lost
- action priority: Skill ที่ valid ก่อน, normal attack รองลงมา, หนึ่ง action ต่อ timestamp
- simultaneous damage resolution, skill effects, hero permadeath, End-of-Day reward/heal/snapshot

คำสั่งที่ `Run` รับได้: `Recruit`, `Deploy`, `Undeploy`, `RankUp`, `StartDay`, `Advance`.

### `RunApplicationService.cs`

เป็น application/use-case layer ไม่ใช่ Unity GameController. มันหา `Run` ผ่าน repository, เรียก command ที่เหมาะสม และ save state กลับ

MonoBehaviour ภายหลังควรเรียก `CreateRun`, `Recruit`, `Deploy`, `StartDay`, `AdvanceTime` ที่ class นี้ แล้วอ่าน `Run` เพื่ออัปเดต HUD, sprite, animation และ tile GameObject

## Testing

`Tests/HeroDefense.Core.Specs` เป็น console smoke test ที่ link source จริงจาก `Assets/Scripts/Core` โดยตรง

```powershell
dotnet run --project Tests/HeroDefense.Core.Specs/HeroDefense.Core.Specs.csproj
```

ปัจจุบันตรวจค่าเริ่มต้น, recruit/formation เฉพาะ Standby, เงื่อนไข rank และ encounter ที่ Field ว่างจน City รับ damage/จบวันได้
