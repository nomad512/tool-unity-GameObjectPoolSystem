using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Nomad
{
    [CustomEditor(typeof(GameObjectPoolDefinitionDatabase))]
    public class GameObjectPoolDatabaseEditor : Editor
    {
        private GameObjectPoolDefinitionDatabase _database;

        private void OnEnable()
        {
            if (!_database)
            {
                _database = target as GameObjectPoolDefinitionDatabase;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Undo.RecordObject(target, "Changed GameObjectPoolDefinitionDatabase");

            var count = _database.ObjectPoolDefinitions.Count;
            var listProp = serializedObject.FindProperty("ObjectPoolDefinitions");
            GUILayout.Label(new GUIContent(string.Format("{0} {1}:", count, count > 1 ? "Definitions" : "Definition")));
            var i = 0;
            while (i < count)
            {
                var definitionProp = listProp.GetArrayElementAtIndex(i);
                var prefabProp = definitionProp.FindPropertyRelative("Prefab");
                var countProp = definitionProp.FindPropertyRelative("Count");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(prefabProp, GUIContent.none);
                var cacheLableWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 90;
                EditorGUILayout.PropertyField(countProp, new GUIContent("Pool Count"));
                countProp.intValue = Mathf.Max(countProp.intValue, 0);
                EditorGUIUtility.labelWidth = cacheLableWidth;
                var delete = GUILayout.Button("X");
                EditorGUILayout.EndHorizontal();

                if (delete)
                {
                    _database.ObjectPoolDefinitions.RemoveAt(i);
                    return;
                }
                i++;
            }

            GUILayout.Space(10);

            // Edit list
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var sortAZ = GUILayout.Button("Sort A-Z");
            var add = GUILayout.Button("+", GUILayout.MinWidth(40));
            if (add)
            {
                _database.ObjectPoolDefinitions.Add(new GameObjectPoolDefinition());
            }
            if (sortAZ)
            {
                _database.ObjectPoolDefinitions =
                    _database.ObjectPoolDefinitions.OrderBy(x => x.Name)
                    .OrderBy(x => x.Prefab == null ? 1 : 0)
                    .ToList();
            }
            GUILayout.EndHorizontal();

            // Apply changes
            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(10);

            // Drag and Drop prefabs
            var e = Event.current;
            var position = EditorGUILayout.GetControlRect(false, 30);
            var dragAndDropStyle = new GUIStyle(GUI.skin.box);
            dragAndDropStyle.alignment = TextAnchor.MiddleCenter;
            dragAndDropStyle.normal.textColor = Color.gray;
            GUI.Box(position, "Drop Prefabs Here", dragAndDropStyle);

            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:

                    if (!position.Contains(e.mousePosition))
                    {
                        break;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (e.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var draggedObject in DragAndDrop.objectReferences)
                        {
                            var go = draggedObject as GameObject;
                            if (go && PrefabUtility.GetPrefabType(go) == PrefabType.Prefab)
                            {
                                _database.ObjectPoolDefinitions.Add(new GameObjectPoolDefinition(go));
                                serializedObject.ApplyModifiedProperties();
                            }
                        }
                    }

                    Event.current.Use();
                    break;
            }

            GUILayout.Space(20);

            // Meta Data
            var isLoaded = GameObjectPoolManager.IsLoaded(_database);
            GUILayout.Label(new GUIContent(string.Format("Is Loaded: {0}", isLoaded)));
            GUILayout.BeginHorizontal();
            var selectPrefabs = GUILayout.Button("Select Prefabs");
            var doLoad = GUILayout.Button(isLoaded ? "Unload" : "Load");
            GUI.enabled = isLoaded;
            GUILayout.EndHorizontal();

            GUI.enabled = true;


            if (selectPrefabs)
            {
                Selection.objects = _database.ObjectPoolDefinitions.Select(x => x.Prefab as Object).Where(x => x != null).ToArray();
            }
            if (doLoad)
            {
                if (isLoaded)
                    GameObjectPoolManager.UnloadDatabase(_database);
                else
                    GameObjectPoolManager.LoadDatabase(_database);
            }
        }
    }
}
