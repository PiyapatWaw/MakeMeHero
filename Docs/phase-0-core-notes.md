# Phase 0 core

The gameplay core is deliberately Unity-independent. `Assets/Scripts/Core/Run.cs` owns the Run aggregate and combat simulation; primitives, history, combatants/tuning, contracts, and application service each live in their own responsibility-focused source file.

Unity MonoBehaviours should later call `RunApplicationService` and render its state; they must not own combat rules or mutate units directly.

Run the no-dependency smoke specifications from a machine with .NET SDK:

```powershell
dotnet run --project Tests/HeroDefense.Core.Specs/HeroDefense.Core.Specs.csproj
```

The test project links the source file rather than copying it, so it validates the same core Unity will compile.
