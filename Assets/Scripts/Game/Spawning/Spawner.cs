using System.Collections.Generic;
using System.Linq;
using MakeMeHero.Core;
using UnityEngine;

namespace MakeMeHero.Game
{
    /// <summary>
    /// Scene adapter that creates the 3×3 tile grid and mirrors deployed core characters
    /// into prefab instances. It contains no combat, targeting, or pathfinding rules.
    /// </summary>
    public sealed class Spawner : MonoBehaviour
    {
        [Header("Run")]
        [SerializeField] private GameManager gameManager;

        [Header("Roots")]
        [SerializeField] private Transform tileRoot;
        [SerializeField] private Transform characterRoot;

        [Header("Board")]
        [SerializeField] private TileView tilePrefab;
        [SerializeField] private Vector2 tileSpacing = new Vector2(3f, 2.5f);

        [Header("Hero prefabs")]
        [SerializeField] private HeroView soldierPrefab;
        [SerializeField] private HeroView archerPrefab;
        [SerializeField] private HeroView magePrefab;
        [SerializeField] private HeroView healerPrefab;

        [Header("Monster prefabs")]
        [SerializeField] private MonsterView wolfPrefab;

        private readonly Dictionary<string, HeroView> _heroViews = new Dictionary<string, HeroView>();
        private readonly Dictionary<string, MonsterView> _monsterViews = new Dictionary<string, MonsterView>();
        private bool _boardBuilt;

        private void OnEnable()
        {
            if (gameManager == null) gameManager = GameManager.Instance;
            if (gameManager != null) gameManager.RunChanged += Synchronize;
        }

        private void Start()
        {
            BuildBoard();
            if (gameManager != null && gameManager.CurrentRun != null) Synchronize(gameManager.CurrentRun);
        }

        private void OnDisable()
        {
            if (gameManager != null) gameManager.RunChanged -= Synchronize;
        }

        public void BuildBoard()
        {
            if (_boardBuilt || tilePrefab == null) return;
            var root = tileRoot == null ? transform : tileRoot;
            for (var row = 0; row < 3; row++)
            {
                for (var column = 0; column < 3; column++)
                {
                    var tile = gameManager.Pooling.Get(tilePrefab, root);
                    tile.name = "Tile " + column + "," + row;
                    tile.Configure(column, row);
                    tile.transform.localPosition = ToWorldPosition(new GridPosition(column, row));
                }
            }
            _boardBuilt = true;
        }

        public void Synchronize(Run run)
        {
            if (run == null) return;
            BuildBoard();
            SynchronizeHeroes(run.Heroes);
            SynchronizeMonsters(run.Wolves);
        }

        private void SynchronizeHeroes(IEnumerable<Hero> heroes)
        {
            var activeIds = new HashSet<string>();
            foreach (var hero in heroes.Where(x => !x.IsDead && x.Position.HasValue))
            {
                activeIds.Add(hero.Id);
                HeroView view;
                if (!_heroViews.TryGetValue(hero.Id, out view))
                {
                    var prefab = HeroPrefabFor(hero.Class);
                    if (prefab == null) continue;
                    view = gameManager.Pooling.Get(prefab, CharacterRoot());
                    view.name = hero.DisplayName;
                    _heroViews.Add(hero.Id, view);
                }
                view.Bind(hero);
                view.transform.localPosition = ToWorldPosition(hero.Position.Value);
            }
            ReleaseMissingHeroes(activeIds);
        }

        private void SynchronizeMonsters(IEnumerable<Wolf> wolves)
        {
            var activeIds = new HashSet<string>();
            foreach (var wolf in wolves.Where(x => !x.IsDead && x.Position.HasValue))
            {
                activeIds.Add(wolf.Id);
                MonsterView view;
                if (!_monsterViews.TryGetValue(wolf.Id, out view))
                {
                    if (wolfPrefab == null) continue;
                    view = gameManager.Pooling.Get(wolfPrefab, CharacterRoot());
                    view.name = wolf.DisplayName;
                    _monsterViews.Add(wolf.Id, view);
                }
                view.Bind(wolf);
                view.transform.localPosition = ToWorldPosition(wolf.Position.Value);
            }
            ReleaseMissingMonsters(activeIds);
        }

        private HeroView HeroPrefabFor(HeroClass heroClass)
        {
            switch (heroClass)
            {
                case HeroClass.Soldier: return soldierPrefab;
                case HeroClass.Archer: return archerPrefab;
                case HeroClass.Mage: return magePrefab;
                case HeroClass.Healer: return healerPrefab;
                default: return null;
            }
        }

        private Transform CharacterRoot() { return characterRoot == null ? transform : characterRoot; }
        private Vector3 ToWorldPosition(GridPosition position) { return new Vector3(position.Column * tileSpacing.x, -position.Row * tileSpacing.y, 0f); }

        private void ReleaseMissingHeroes(HashSet<string> activeIds)
        {
            foreach (var id in _heroViews.Keys.Where(id => !activeIds.Contains(id)).ToList())
            {
                gameManager.Pooling.Release(_heroViews[id]);
                _heroViews.Remove(id);
            }
        }

        private void ReleaseMissingMonsters(HashSet<string> activeIds)
        {
            foreach (var id in _monsterViews.Keys.Where(id => !activeIds.Contains(id)).ToList())
            {
                gameManager.Pooling.Release(_monsterViews[id]);
                _monsterViews.Remove(id);
            }
        }
    }
}
