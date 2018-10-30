using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nomad
{
    [Serializable]
    public struct GameObjectPool
    {
        public int Count { get { return _instances.Count; } }
        public int Id { get; private set; }
        public int MaxCount { get; private set; }
        public string Name { get; private set; }
        public int OverflowCount { get; private set; }
        public int ActiveCount
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < _instances.Count; i++)
                {
                    if (_instances[i].activeSelf)
                        sum++;
                }
                return sum;
            }
        }
        private GameObjectPoolDefinition _definition;
        private List<GameObject> _instances;

        static bool CreatePoolOnInstantiateEnabled = true; // TOOD: make configurable


        public GameObjectPool(GameObjectPoolDefinition objectPoolDefinition)
        {
            Id = objectPoolDefinition.Id;
            MaxCount = objectPoolDefinition.Count;
            Name = objectPoolDefinition.Name;
            _definition = objectPoolDefinition;
            _instances = new List<GameObject>();
            OverflowCount = 0;
        }

        public static GameObject Instantiate(GameObject original)
        {
            return Instantiate(original, Vector3.zero, Quaternion.identity, Vector3.one);
        }

        public static GameObject Instantiate(GameObject original, Vector3 position)
        {
            return Instantiate(original, position, Quaternion.identity, Vector3.one);
        }

        public static GameObject Instantiate(GameObject original, Vector3 position, Quaternion rotation, Vector3 localScale)
        {
            var cacheOriginalActive = original.activeSelf;
            original.SetActive(false);
            GameObjectPool pool;
            GameObject clone;
            var id = original.GetInstanceID();
            if (GameObjectPoolManager.ActiveObjectPools.TryGetValue(id, out pool))
            {
                // Use Existing ObjectPool
                clone = pool.GetAvailableInstance();
                GameObjectPoolManager.ActiveObjectPools[id] = pool;
            }
            else if (CreatePoolOnInstantiateEnabled)
            {
                // Create a pool
                pool = new GameObjectPool(new GameObjectPoolDefinition(original));
                pool.OverflowCount = 1;
                pool.MaxCount = 1;
                pool.Populate();
                clone = pool.GetAvailableInstance();
                GameObjectPoolManager.ActiveObjectPools.Add(id, pool);
                Debug.LogFormat("[ObjectPool] Created a pool: {0}", pool);
            }
            else
            {
                // Normal Instantiate
                clone = Object.Instantiate(original);
                Debug.LogFormat("[ObjectPool] Instantiated an object without pooling: {0}", clone);
            }
            original.SetActive(cacheOriginalActive);
            clone.transform.position = position;
            clone.transform.rotation = rotation;
            clone.transform.localScale = localScale;
            clone.SetActive(true);
            return clone;
        }

        public void Populate() // TODO: return success/error state
        {
            Cleanup();

            var count = MaxCount - Count;
            var creating = (count > 0);
            if (creating)
            {
                // Make new instances
                var cachePrefabActive = _definition.Prefab.activeSelf;
                _definition.Prefab.SetActive(false);
                for (int i = 0; i < count; i++)
                {
                    _instances.Add(Object.Instantiate(_definition.Prefab));
                }
                _definition.Prefab.SetActive(cachePrefabActive);
            }
            else
            {
                // Destory extra instances
                for (int i = count; i < 0; i++)
                {
                    var instance = GetAvailableInstance();
                    _instances.Remove(instance);
                    Object.Destroy(instance);
                }
            }
        }

        public void Destroy()
        {
            // Destroy pool
            var activeCount = 0;
            for (int i = 0; i < Count; i++)
            {
                if (!_instances[i])
                {
                    continue;
                }
                if (_instances[i].activeSelf)
                {
                    // TODO: defer destruction of pooled objects that are still in use.
                    activeCount++;
                }
                Object.Destroy(_instances[i]);
            }
            _instances.Clear();
            if (activeCount > 0)
            {
                Debug.LogWarningFormat("[ObjectPool] Pooled objects were destroyed while still active: {0}", activeCount);
            }
        }

        public GameObjectPool ModifyMaxCount(int difference)
        {
            MaxCount += difference;
            return this;
        }

        public override string ToString()
        {
            return string.Format("\'{0}\' ({1}/{2})", Name, ActiveCount, MaxCount);
        }

        private void Cleanup()
        {
            var cache = new List<GameObject>(_instances);
            var errors = 0;
            for (int i = Count - 1; i > 0; i--)
            {
                if (_instances[i] == null)
                {
                    errors++;
                    cache.RemoveAt(i);
                }
            }
            if (errors > 0)
            {
                Debug.LogErrorFormat("[ObjectPool] Pool {0} was missing {1} instance(s). Pooled objects should only be destroyed by the ObjectPool.", this, errors);
                _instances = cache;
            }
        }

        private GameObject GetAvailableInstance()
        {
            var i = 0;
            while (i < _instances.Count)
            {
                if (_instances[i] && !_instances[i].activeSelf)
                {
                    return _instances[i];
                }
                i++;
            }

            // Overflow
            OverflowCount++;
            ModifyMaxCount(1);
            var cachePrefabActive = _definition.Prefab.activeSelf;
            _definition.Prefab.SetActive(false);
            var clone = Object.Instantiate(_definition.Prefab);
            _instances.Add(clone);
            _definition.Prefab.SetActive(cachePrefabActive);
            Debug.LogFormat("[ObjectPool] No active instance was available. Pool {0} count overflows by {1}. ", this, OverflowCount);
            return clone;
        }
    }
}