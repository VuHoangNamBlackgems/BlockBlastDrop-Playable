using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
public class ToolEditor : EditorWindow
{
    private GameObject Parent;
    private GameObject Tile;
    private int x, y;
    bool showGrid = false;
    bool[,] selectedCells;
    Vector3 tileSize = Vector3.one; // m?c ??nh
    bool Full = false;
    Object prefab;
    Editor prefabEditor;
    bool spawnblock;
    [MenuItem("Tool/CreateGird")]
   public static void ShowWindow()
    {
        GetWindow<ToolEditor>("My Tool");
    }
    private void OnGUI()
    {
        GUILayout.Label("Parent");
        Parent = (GameObject)EditorGUILayout.ObjectField("Prefab", Parent, typeof(GameObject), true);
        GUILayout.Label("Tile");
        Tile = (GameObject)EditorGUILayout.ObjectField("Prefab", Tile, typeof(GameObject), false);
        GUILayout.Label("Color");
        x = EditorGUILayout.IntField("x", x);
        y = EditorGUILayout.IntField("y", y);
        Full = EditorGUILayout.Toggle("Full?", Full);
        if (GUILayout.Button("Create"))
        {
            showGrid = true;
            selectedCells = new bool[x, y];
            if (Tile != null)
            {
                if (Full && selectedCells != null)
                {
                    for (int row = 0; row < y; row++)
                    {
                        for (int col = 0; col < x; col++)
                        {
                            selectedCells[col, row] = true;
                        }
                    }
                }
                GameObject preview = (GameObject)PrefabUtility.InstantiatePrefab(Tile);
                Renderer renderer = preview.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    tileSize = renderer.bounds.size;
                }

                // Xóa b?n preview (t?m th?i t?o ?? l?y size)
                DestroyImmediate(preview);
            }
        }
        GUILayout.Space(10);
        if (showGrid)
        {
            for (int row = 0; row < y; row++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int col = 0; col < x; col++)
                {
                    Color originalColor = GUI.backgroundColor;
                    if (selectedCells[col, row])
                        GUI.backgroundColor = Color.green;

                    if (GUILayout.Button($"[{col},{row}]", GUILayout.Width(60), GUILayout.Height(30)))
                    {
                        selectedCells[col, row] = !selectedCells[col, row];
                    }

                    GUI.backgroundColor = originalColor;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        if (GUILayout.Button("Done"))
        {
            for (int row = 0; row < y; row++)
            {
                for (int col = 0; col < x; col++)
                {
                    if (selectedCells[col, row])
                    {
                        GameObject tileInstance = (GameObject)PrefabUtility.InstantiatePrefab(Tile);

                        // V? trí d?a trên kích th??c tile
                        float tileHeight = tileSize.y;
                        tileInstance.transform.position = new Vector3(
    RoundToNearest(col * tileSize.x, tileSize.x),
    0,
    RoundToNearest(row * tileSize.z, tileSize.z)
);

                        tileInstance.transform.SetParent(Parent.transform);
                        tileInstance.name = $"Tile_{col}_{row}";
                    }
                }
            }
        }

        //Spawn block
        spawnblock = EditorGUILayout.Toggle("SpawnBLock", spawnblock);
        prefab = EditorGUILayout.ObjectField("Block Spawn", prefab, typeof(GameObject), false);

        if (prefab != null)
        {
            if (prefabEditor == null || prefabEditor.target != prefab)
            {
                prefabEditor = Editor.CreateEditor(prefab);
            }

            if (prefabEditor != null)
            {
                prefabEditor.OnPreviewGUI(GUILayoutUtility.GetRect(256, 256), EditorStyles.whiteLabel);
            }
            
        }
        if (spawnblock == true)
        {

        }
    }
    float RoundToNearest(float value, float multiple)
    {
        return Mathf.Round(value / multiple) * multiple;

    }


}
#endif
