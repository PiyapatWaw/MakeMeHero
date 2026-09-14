# Phase 0 core class diagram

```mermaid
classDiagram
  class RunApplicationService {
    +CreateRun() Run
    +Recruit(runId, class)
    +Deploy(runId, heroId, tile)
    +StartDay(runId)
    +AdvanceTime(runId, seconds)
  }
  class IRunRepository
  class InMemoryRunRepository
  class Run {
    +RunPhase Phase
    +int Day
    +int Gold
    +decimal CityHp
    +Recruit(class) Hero
    +Deploy(heroId, tile)
    +StartDay()
    +Advance(seconds)
  }
  class Hero {
    +HeroClass Class
    +int RankStars
    +int SurvivedDays
    +int SkillSlots
  }
  class Wolf
  class CombatEvent
  class ExperienceSnapshot
  class IEncounterScalingPolicy
  class ExponentialEncounterScalingPolicy
  class IRandomSource
  class CombatTuning

  RunApplicationService --> IRunRepository
  InMemoryRunRepository ..|> IRunRepository
  RunApplicationService --> Run
  Run --> Hero
  Run --> Wolf
  Run --> CombatEvent
  Run --> ExperienceSnapshot
  Run --> IEncounterScalingPolicy
  Run --> IRandomSource
  Run --> CombatTuning
  ExponentialEncounterScalingPolicy ..|> IEncounterScalingPolicy
```

`Run` is the transaction boundary: it accepts only legal phase-specific commands and owns battle resolution. Unity adapters should call `RunApplicationService`; they must not mutate `Hero`, `Wolf`, or battlefield state directly. Configuration, scaling, randomness, and storage are interfaces so future content, deterministic replay, and persistence can replace them without changing combat rules.
