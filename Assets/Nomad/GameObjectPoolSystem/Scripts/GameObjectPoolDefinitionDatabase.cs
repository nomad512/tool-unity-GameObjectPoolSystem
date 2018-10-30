using System.Collections.Generic;
using UnityEngine;

namespace Nomad
{
    [CreateAssetMenu(fileName = "New PoolObject Database", menuName = "Nomad/PoolObject Database")]
    public class GameObjectPoolDefinitionDatabase : ScriptableObject
    {
        public List<GameObjectPoolDefinition> ObjectPoolDefinitions = new List<GameObjectPoolDefinition>();

        public override string ToString()
        {
            return string.Format("{0} ({1} {2})", name, ObjectPoolDefinitions.Count, ObjectPoolDefinitions.Count > 1 ? "definitions" : "definition");
        }
    }
}