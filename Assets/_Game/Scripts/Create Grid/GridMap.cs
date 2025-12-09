#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class GridMap : EditorWindow
{
    private int rows = 4;
    private int columns = 4;
    private GameObject prefabToSpawn;

    private Color[] colors = new Color[] { Color.white, Color.green }; // 0: Trắng, 1: Xanh
    private int[,] colorStates;

    [MenuItem("Window/Flexible Grid With Prefab")]
    public static void ShowWindow()
    {
        GetWindow<GridMap>("Grid & Prefab");
    }

    private void OnGUI()
    {
        GUILayout.Label("Thiết lập Grid và Prefab", EditorStyles.boldLabel);

        rows = EditorGUILayout.IntField("Số dòng (Rows)", rows);
        columns = EditorGUILayout.IntField("Số cột (Columns)", columns);

        rows = Mathf.Max(1, rows);
        columns = Mathf.Max(1, columns);

        prefabToSpawn = (GameObject)EditorGUILayout.ObjectField("Prefab để sinh", prefabToSpawn, typeof(GameObject), false);

        // Khởi tạo hoặc cập nhật mảng trạng thái màu
        if (colorStates == null || colorStates.GetLength(0) != columns || colorStates.GetLength(1) != rows)
        {
            colorStates = new int[columns, rows];
        }

        GUILayout.Space(10);

        // Hiển thị lưới từ dưới lên
        for (int y = rows - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < columns; x++)
            {
                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = colors[colorStates[x, y]];

                if (GUILayout.Button($"{x},{y}", GUILayout.Width(50), GUILayout.Height(30)))
                {
                    // Toggle giữa trắng và xanh
                    colorStates[x, y] = (colorStates[x, y] == 0) ? 1 : 0;
                }

                GUI.backgroundColor = oldColor;
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);

        // Nút: Đổi tất cả sang xanh
        if (GUILayout.Button("🟩 Đổi tất cả sang màu xanh"))
        {
            for (int x = 0; x < columns; x++)
                for (int y = 0; y < rows; y++)
                    colorStates[x, y] = 1;
        }

        // Nút: Đổi tất cả sang trắng
        if (GUILayout.Button("⬜ Đổi tất cả sang màu trắng"))
        {
            for (int x = 0; x < columns; x++)
                for (int y = 0; y < rows; y++)
                    colorStates[x, y] = 0;
        }

        GUILayout.Space(10);

        // Nút sinh object
        if (GUILayout.Button("Generate GameObjects in Scene"))
        {
            GenerateObjectsInScene();
        }
    }

    private void GenerateObjectsInScene()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogError("❌ Bạn chưa gán Prefab! Vui lòng kéo Prefab từ Project vào.");
            return;
        }

        // Xoá object cũ
        GameObject oldParent = GameObject.Find("Generated_Grid");
        if (oldParent != null)
        {
            DestroyImmediate(oldParent);
        }

        GameObject parent = new GameObject("Generated_Grid");

        int createdCount = 0;
        float spacing = 2F;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (colorStates[x, y] == 1) // Chỉ sinh màu xanh
                {
                    GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
                    obj.name = $"{prefabToSpawn.name}_{x}_{y}_Green";
                    obj.transform.position = new Vector3(x * spacing, 0, (rows - 1 - y) * spacing);
                    obj.transform.SetParent(parent.transform);

                    Renderer rend = obj.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.sharedMaterial = new Material(rend.sharedMaterial);
                        rend.sharedMaterial.color = Color.green;
                    }

                    createdCount++;
                }
            }
        }

        Debug.Log($"✅ Đã tạo {createdCount} object màu xanh từ Grid.");
    }
}
#endif
