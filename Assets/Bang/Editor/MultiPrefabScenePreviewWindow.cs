using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MultiPrefabScenePreviewWindow : EditorWindow
{
    public static List<GameObject> prefabsToPreview = new List<GameObject>();
    private static List<GameObject> spawnedObjects = new List<GameObject>();
    private static GameObject selectedParent;
    private static int selectedPrefabIndex = -1; // Ch? s? prefab ???c ch?n

    private Vector2 scrollPos;
    private const float cellSize = 100f;   // Kích th??c ô preview
    private const float padding = 10f;     // Kho?ng cách gi?a các ô

    [MenuItem("Tool/Multi Prefab Scene Preview")]
    public static void ShowWindow()
    {
        GetWindow<MultiPrefabScenePreviewWindow>("Multi Prefab Scene Preview");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Grid Preview", EditorStyles.boldLabel);

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        if (prefabsToPreview.Count == 0)
        {
            GUILayout.Label("No prefabs. Drag & drop or add prefab.");
        }
        else
        {
            float windowWidth = position.width - 20;
            int columns = Mathf.Max(1, Mathf.FloorToInt(windowWidth / (cellSize + padding)));

            int rowCount = Mathf.CeilToInt(prefabsToPreview.Count / (float)columns);
            int prefabIndex = 0;

            for (int row = 0; row < rowCount; row++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(padding);

                for (int col = 0; col < columns; col++)
                {
                    if (prefabIndex >= prefabsToPreview.Count) break;

                    GameObject prefab = prefabsToPreview[prefabIndex];
                    int currentIndex = prefabIndex;
                    prefabIndex++;

                    GUILayout.BeginVertical(GUILayout.Width(cellSize));

                    if (prefab != null)
                    {
                        // Preview texture
                        Texture2D previewTexture = AssetPreview.GetAssetPreview(prefab);
                        if (previewTexture == null) previewTexture = AssetPreview.GetMiniThumbnail(prefab);

                        Rect previewRect = GUILayoutUtility.GetRect(cellSize, cellSize);
                        GUI.Box(previewRect, GUIContent.none);

                        // N?u click vào preview thì ch?n prefab
                        if (Event.current.type == EventType.MouseDown && previewRect.Contains(Event.current.mousePosition))
                        {
                            selectedPrefabIndex = currentIndex;
                            Repaint();
                        }

                        // V? preview
                        GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit);

                        // Label v?i màu khác n?u ???c ch?n
                        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
                        labelStyle.alignment = TextAnchor.MiddleCenter;
                        labelStyle.normal.textColor = (selectedPrefabIndex == currentIndex) ? Color.green : Color.white;

                        GUILayout.Label(prefab.name, labelStyle, GUILayout.Width(cellSize));
                    }

                    GUILayout.EndVertical();
                    GUILayout.Space(padding);
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(padding);
            }
        }

        GUILayout.EndScrollView();

        GUILayout.Space(10);
        GUILayout.Label("Select Parent for Spawned Objects", EditorStyles.boldLabel);
        selectedParent = (GameObject)EditorGUILayout.ObjectField("Parent Object", selectedParent, typeof(GameObject), true);

        // Khu v?c Drag&Drop
        Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag Prefab Here");

        Event evt = Event.current;
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (var draggedObject in DragAndDrop.objectReferences)
                {
                    if (draggedObject is GameObject prefab)
                    {
                        prefabsToPreview.Add(prefab);
                    }
                }
            }
            evt.Use();
        }

        if (GUILayout.Button("Add Prefab"))
        {
            string path = EditorUtility.OpenFilePanel("Select Prefab", Application.dataPath, "prefab");
            if (!string.IsNullOrEmpty(path))
            {
                string assetPath = "Assets" + path.Substring(Application.dataPath.Length);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab != null)
                {
                    prefabsToPreview.Add(prefab);
                }
            }
        }

        if (GUILayout.Button("Clear Prefabs"))
        {
            prefabsToPreview.Clear();
            selectedPrefabIndex = -1;
        }

        if (GUILayout.Button("Clear Spawned Objects"))
        {
            ClearSpawnedObjects();
        }
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        bool isAltHeld = e.alt;

        if (isAltHeld)
        {
            // Alt + Left Click => Spawn selected prefab
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (selectedPrefabIndex >= 0 && selectedPrefabIndex < prefabsToPreview.Count)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Vector3 spawnPosition = hit.point;
                        spawnPosition.y = 0;
                        spawnPosition.x = Mathf.Round(spawnPosition.x);
                        spawnPosition.z = Mathf.Round(spawnPosition.z);

                        GameObject prefabToInstantiate = prefabsToPreview[selectedPrefabIndex];
                        if (prefabToInstantiate != null)
                        {
                            GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToInstantiate);
                            newObj.transform.position = spawnPosition;
                            Undo.RegisterCreatedObjectUndo(newObj, "Spawn Object");

                            if (selectedParent != null)
                                newObj.transform.SetParent(selectedParent.transform);

                            spawnedObjects.Add(newObj);
                        }
                    }
                }

                e.Use();
            }

            // Alt + Right Click => Delete object
            if (e.type == EventType.MouseDown && e.button == 1)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    GameObject hitObject = hit.collider.gameObject;
                    if (hitObject != null && spawnedObjects.Contains(hitObject))
                    {
                        Undo.DestroyObjectImmediate(hitObject);
                        spawnedObjects.Remove(hitObject);
                    }
                }

                e.Use();
            }
        }

        // Key E => Rotate nearest spawned object
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.E)
        {
            GameObject closestObject = GetClosestSpawnedObject(e.mousePosition);

            if (closestObject != null)
            {
                closestObject.transform.Rotate(0, 90, 0);
            }

            e.Use();
        }

        SceneView.RepaintAll();
    }

    static GameObject GetClosestSpawnedObject(Vector2 mousePosition)
    {
        GameObject closestObject = null;
        float closestDistance = Mathf.Infinity;

        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                Vector3 screenPos = SceneView.currentDrawingSceneView.camera.WorldToScreenPoint(obj.transform.position);
                float distance = Vector2.Distance(mousePosition, new Vector2(screenPos.x, screenPos.y));

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = obj;
                }
            }
        }

        return closestObject;
    }

    void ClearSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            Undo.DestroyObjectImmediate(obj);
        }
        spawnedObjects.Clear();
    }
}
