using System;
using System.Collections.Generic;
using System.IO;
using MakeMeHero.Core;
using UnityEngine;

namespace MakeMeHero.Game
{
    /// <summary>
    /// Unity entry point for one gameplay run. Attach this to a scene root; UI and views
    /// call its public methods, while combat rules remain inside the pure C# Run aggregate.
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField, Min(0.01f)] private float battleTickSeconds = 1f;
        [SerializeField] private bool startFirstDayOnAwake;

        private IRunRepository _repository;
        private RunApplicationService _service;
        private Run _run;
        private Coroutine _battleRoutine;
        private ResearchJsonExporter _researchExporter;
        private CharacterEvolutionJsonImporter _evolutionImporter;
        private CharacterEvolutionRequestJsonExporter _evolutionRequestExporter;
        private readonly HashSet<string> _exportedResearchRunIds = new HashSet<string>();

        public Run CurrentRun { get { return _run; } }
        public Pooling Pooling { get; private set; }
        public event Action<Run> RunChanged;
        public event Action<string> CommandRejected;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Pooling = new Pooling(transform);
            _researchExporter = new ResearchJsonExporter(Path.Combine(Application.persistentDataPath, "research_logs"));
            _evolutionImporter = new CharacterEvolutionJsonImporter();
            _evolutionRequestExporter = new CharacterEvolutionRequestJsonExporter();

            _repository = new InMemoryRunRepository();
            _service = new RunApplicationService(
                _repository,
                CombatTuning.Phase0(),
                new ExponentialEncounterScalingPolicy(),
                seed => new SystemRandomSource(seed),
                new SystemRunClock());
            CreateNewRun();
            if (startFirstDayOnAwake) StartDay();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnDisable()
        {
            StopBattleLoop();
        }

        public void CreateNewRun()
        {
            StopBattleLoop();
            _run = _service.CreateRun();
            NotifyRunChanged();
        }

        public void StartDay()
        {
            Execute(delegate { _service.StartDay(_run.Id); });
            if (_run != null && _run.Phase == RunPhase.Battle) StartBattleLoop();
        }
        public void RecruitSoldier() { Recruit(HeroClass.Soldier); }
        public void RecruitArcher() { Recruit(HeroClass.Archer); }
        public void RecruitMage() { Recruit(HeroClass.Mage); }
        public void RecruitHealer() { Recruit(HeroClass.Healer); }
        public void Recruit(HeroClass heroClass) { Execute(delegate { _service.Recruit(_run.Id, heroClass); }); }
        public void Deploy(string heroId, int column, int row) { Execute(delegate { _service.Deploy(_run.Id, heroId, new GridPosition(column, row)); }); }
        public void Undeploy(string heroId) { Execute(delegate { _service.Undeploy(_run.Id, heroId); }); }
        public void RankUp(string heroId) { Execute(delegate { _service.RankUp(_run.Id, heroId); }); }
        public void EndRun() { Execute(delegate { _service.EndRun(_run.Id); }); StopBattleLoop(); }
        public EvolutionDecisionResult ValidateEvolutionJson(string json)
        {
            CharacterEvolutionDecision decision;
            var parse = _evolutionImporter.TryImport(json, out decision);
            return parse.IsValid ? _service.ValidateEvolutionDecision(_run.Id, decision) : parse;
        }
        public EvolutionDecisionResult ApplyEvolutionJson(string json)
        {
            CharacterEvolutionDecision decision;
            var parse = _evolutionImporter.TryImport(json, out decision);
            if (!parse.IsValid) return parse;
            var result = _service.ApplyEvolutionDecision(_run.Id, decision);
            if (result.IsValid) NotifyRunChanged();
            return result;
        }
        public string ExportEvolutionRequestJson(string heroId)
        {
            return _evolutionRequestExporter.Serialize(_service.CreateEvolutionRequest(_run.Id, heroId));
        }

        private void Execute(Action command)
        {
            try { command(); NotifyRunChanged(); }
            catch (InvalidOperationException exception) { Reject(exception.Message); }
            catch (ArgumentOutOfRangeException exception) { Reject(exception.Message); }
        }

        private void NotifyRunChanged()
        {
            if (RunChanged != null) RunChanged(_run);
            ExportTerminalResearchLog();
        }

        private void ExportTerminalResearchLog()
        {
            if (_run == null || !_run.Metadata.EndedAtUtc.HasValue || _exportedResearchRunIds.Contains(_run.Id)) return;
            _researchExporter.Export(_service.ResearchLog(_run.Id));
            _exportedResearchRunIds.Add(_run.Id);
        }

        private void Reject(string message)
        {
            Debug.LogWarning(message, this);
            if (CommandRejected != null) CommandRejected(message);
        }

        private void StartBattleLoop()
        {
            StopBattleLoop();
            _battleRoutine = StartCoroutine(BattleLoop());
        }

        private void StopBattleLoop()
        {
            if (_battleRoutine == null) return;
            StopCoroutine(_battleRoutine);
            _battleRoutine = null;
        }

        private System.Collections.IEnumerator BattleLoop()
        {
            while (_run != null && _run.Phase == RunPhase.Battle)
            {
                yield return new WaitForSeconds(battleTickSeconds);
                if (_run != null && _run.Phase == RunPhase.Battle)
                    Execute(delegate { _service.AdvanceTime(_run.Id, (decimal)battleTickSeconds); });
            }
            _battleRoutine = null;
        }
    }
}
