using System;
using System.Collections.Generic;
using UnityEngine;

namespace MakeMeHero.Game
{
    /// <summary>Game-wide prefab pool owned by GameManager. Every prefab receives its own inactive child pool.</summary>
    public sealed class Pooling
    {
        private readonly Transform _root;
        private readonly Dictionary<int, IInstancePool> _poolsByPrefabId = new Dictionary<int, IInstancePool>();
        private readonly Dictionary<int, IInstancePool> _poolsByInstanceId = new Dictionary<int, IInstancePool>();

        public Pooling(Transform owner)
        {
            var rootObject = new GameObject("Pooling");
            rootObject.transform.SetParent(owner, false);
            _root = rootObject.transform;
        }

        public TView Get<TView>(TView prefab, Transform activeRoot) where TView : Component
        {
            if (prefab == null) throw new ArgumentNullException("prefab");
            IInstancePool pool;
            if (!_poolsByPrefabId.TryGetValue(prefab.GetInstanceID(), out pool))
            {
                pool = new PrefabPool<TView>(prefab, _root);
                _poolsByPrefabId.Add(prefab.GetInstanceID(), pool);
            }
            var typedPool = (PrefabPool<TView>)pool;
            var instance = typedPool.Get(activeRoot);
            _poolsByInstanceId[instance.GetInstanceID()] = typedPool;
            return instance;
        }

        public void Release(Component instance)
        {
            if (instance == null) return;
            IInstancePool pool;
            if (_poolsByInstanceId.TryGetValue(instance.GetInstanceID(), out pool)) pool.Release(instance);
        }

        private interface IInstancePool { void Release(Component instance); }

        private sealed class PrefabPool<TView> : IInstancePool where TView : Component
        {
            private readonly Transform _root;
            private readonly TView _prefab;
            private readonly Stack<TView> _available = new Stack<TView>();
            private readonly HashSet<int> _availableIds = new HashSet<int>();

            public PrefabPool(TView prefab, Transform owner)
            {
                _prefab = prefab;
                var poolObject = new GameObject(prefab.name + " Pool");
                poolObject.transform.SetParent(owner, false);
                _root = poolObject.transform;
            }

            public TView Get(Transform activeRoot)
            {
                TView instance;
                if (_available.Count > 0)
                {
                    instance = _available.Pop();
                    _availableIds.Remove(instance.GetInstanceID());
                }
                else instance = Object.Instantiate(_prefab, _root);
                instance.transform.SetParent(activeRoot, false);
                instance.gameObject.SetActive(true);
                return instance;
            }

            public void Release(Component instance)
            {
                var typedInstance = instance as TView;
                if (typedInstance == null || _availableIds.Contains(typedInstance.GetInstanceID())) return;
                typedInstance.transform.SetParent(_root, false);
                typedInstance.gameObject.SetActive(false);
                _available.Push(typedInstance);
                _availableIds.Add(typedInstance.GetInstanceID());
            }
        }
    }
}
