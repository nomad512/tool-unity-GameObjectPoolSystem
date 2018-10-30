using System.Collections.Generic;
using UnityEngine;

namespace Nomad
{
    public static class GameObjectPoolManager
    {
        public static Dictionary<int, GameObjectPool> ActiveObjectPools = new Dictionary<int, GameObjectPool>();

        private static List<GameObjectPoolDefinitionDatabase> _loadedDatabases = new List<GameObjectPoolDefinitionDatabase>();
        private static List<GameObjectPool> _unloadingObjectPools = new List<GameObjectPool>();


        public static void LoadDatabase(GameObjectPoolDefinitionDatabase database)
        {
            if (!database)
                return;
            if (_loadedDatabases.Contains(database))
                return;
            _loadedDatabases.Add(database);
            foreach (var def in database.ObjectPoolDefinitions)
            {
                var id = def.Id;
                GameObjectPool duplicate;
                if (ActiveObjectPools.TryGetValue(id, out duplicate))
                {
                    ActiveObjectPools[id] = duplicate.ModifyMaxCount(def.Count);
                    continue;
                }
                var p = new GameObjectPool(def);
                ActiveObjectPools.Add(id, p);
            }
        }

        public static void UnloadDatabase(GameObjectPoolDefinitionDatabase database)
        {
            if (!database)
                return;
            if (!_loadedDatabases.Contains(database))
                return;
            foreach (var def in database.ObjectPoolDefinitions)
            {
                var id = def.Id;
                GameObjectPool pool;
                if (!ActiveObjectPools.TryGetValue(id, out pool))
                {
                    // No pool for this object exists to unload.
                    continue;
                }
                ActiveObjectPools[id] = pool.ModifyMaxCount(-def.Count);
                if (pool.MaxCount <= 0)
                {
                    if (pool.MaxCount < 0)
                    {
                        Debug.LogErrorFormat("[ObjectPoolManager] Max pool count for object \'{0}\' was less than zero after unloading database \'{1}\'", pool, database);
                    }
                    // Pool count equals zero...proceed to unload the pool.
                }
                else if (pool.MaxCount == pool.OverflowCount)
                {
                    // Only overflow remain...proceed to unload the pool.
                }
                else
                {
                    continue;
                }
                _unloadingObjectPools.Add(pool);
                ActiveObjectPools.Remove(id);
            }
            _loadedDatabases.Remove(database);
        }

        public static void PopulateAll()
        {
            foreach (var kvp in ActiveObjectPools)
            {
                kvp.Value.Populate();
            }
            foreach (var pool in _unloadingObjectPools)
            {
                pool.Destroy();
            }
            _unloadingObjectPools.Clear();
        }

        public static bool IsLoaded(GameObjectPoolDefinitionDatabase database)
        {
            return _loadedDatabases.Contains(database);
        }

        public static string Log()
        {
            var b = new System.Text.StringBuilder("");
            b.AppendFormat("[ObjectPoolManager] Databases Loaded: {0}   ActivePools: {1}  UnloadingPools: {2} \n", _loadedDatabases.Count, ActiveObjectPools.Count, _unloadingObjectPools.Count);
            foreach (var kvp in ActiveObjectPools)
            {
                b.AppendFormat("{0}, ", kvp.Value.ToString());
            }
            Debug.Log(b.ToString());
            return b.ToString();
        }
    }
}
