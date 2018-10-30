using System;
using UnityEngine;

[Serializable]
public struct GameObjectPoolDefinition
{
    public int Id { get { return Prefab ? Prefab.GetInstanceID() : 0; } }
    public string Name { get { return Prefab ? Prefab.name : "missing!"; } }
    public GameObject Prefab;
    public int Count;

    public GameObjectPoolDefinition(GameObject prefab)
    {
        Prefab = prefab;
        Count = 0;
    }

    public override string ToString()
    {
        return string.Format("\"{0}\" (x{1})", Prefab ? Prefab.name : "missing!", Count);
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("NomadLib/Print Object Id")]
    public static void PrintId()
    {
        var go = UnityEditor.Selection.activeGameObject;
        if (go)
        {
            Debug.Log(go.GetInstanceID());
        }
    }
#endif
}