using UnityEngine;

namespace Nomad
{
    public class GameObjectPoolDemo : MonoBehaviour
    {
        [SerializeField]
        private GameObjectPoolDefinitionDatabase _poolableDatabase1;
        [SerializeField]
        private GameObjectPoolDefinitionDatabase _poolableDatabase2;
        [SerializeField]
        private GameObject[] _prefabs;

        private int _prefabSelection;
        private string _logText;
        private float _lastLogTime;


        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            var load = GUILayout.Button("Load Database 1");
            var unload = GUILayout.Button("Unload Database 1");
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            var load2 = GUILayout.Button("Load Database 2");
            var unload2 = GUILayout.Button("Unload Database 2");
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            var log = GUILayout.Button("Log Pool");
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(320, 10, 300, 300));
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(_prefabSelection == 0, "Prefab 1"))
            {
                _prefabSelection = 0;
            }
            if (GUILayout.Toggle(_prefabSelection == 1, "Prefab 2"))
            {
                _prefabSelection = 1;
            }
            if (GUILayout.Toggle(_prefabSelection == 2, "Prefab 3"))
            {
                _prefabSelection = 2;
            }
            GUILayout.EndHorizontal();

            GUI.color = new Color(0.8f, 0.8f, 0f);
            GUILayout.Label("Click on the plane to instantiate an object through the pool syste. Click an object to return it to the pool.");
            GUILayout.EndVertical();
            GUILayout.EndArea();

            if (load)
            {
                GameObjectPoolManager.LoadDatabase(_poolableDatabase1);
                GameObjectPoolManager.PopulateAll();
            }
            if (unload)
            {
                GameObjectPoolManager.UnloadDatabase(_poolableDatabase1);
                GameObjectPoolManager.PopulateAll();
            }
            if (load2)
            {
                GameObjectPoolManager.LoadDatabase(_poolableDatabase2);
                GameObjectPoolManager.PopulateAll();
            }
            if (unload2)
            {
                GameObjectPoolManager.UnloadDatabase(_poolableDatabase2);
                GameObjectPoolManager.PopulateAll();
            }
            if (log)
            {
                _logText = GameObjectPoolManager.Log();
                _lastLogTime = Time.time;
            }

            if (Time.time - _lastLogTime < 5f)
            {
                GUILayout.BeginArea(new Rect(10, 320, 300, Screen.height - 320));
                GUI.color = new Color(0.8f, 0.8f, 0f);
                GUILayout.Label(_logText);
                GUILayout.EndArea();
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        GameObjectPool.Instantiate(_prefabs[_prefabSelection], hit.point + new Vector3(0, 0.5f, 0));
                    }
                    else
                    {
                        hit.collider.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
