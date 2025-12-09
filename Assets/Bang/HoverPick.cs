#if UNITY_EDITOR
using System;
using Luna.Unity.FacebookInstantGames;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Progress;

public class HoverPick : EditorWindow
{
    private GameObject parentBlock;
    private GameObject hoveredObject;
    private ColorDataSO colorDataSO;
    private bool ChangeColor = false;
    [MenuItem("Tool/Hover Pick Tool")]
    public static void ShowWindow()
    {
        GetWindow<HoverPick>("Hover Picker");
    }

    void OnGUI()
    {
        if (hoveredObject != null)
        {
            GUILayout.Label("Object d??i chu?t: " + hoveredObject.name);
        }
        else
        {
            GUILayout.Label("Không có object nào d??i chu?t.");
        }
        GUILayout.Label("ParentBlock");
        parentBlock = (GameObject)EditorGUILayout.ObjectField("Prefab", parentBlock, typeof(GameObject), true);

        GUILayout.BeginVertical();
        GUILayout.Label("Color");
        ChangeColor = EditorGUILayout.Toggle("Change Color", ChangeColor);
        colorDataSO = (ColorDataSO)EditorGUILayout.ObjectField("ColorDataSO", colorDataSO, typeof(ScriptableObject), true);
        GUILayout.EndVertical();
        if (GUILayout.Button("Change color all"))
        {
            if(parentBlock != null)
    {
                var controllers = parentBlock.GetComponentsInChildren<BlockController>(true);

                foreach (var controller in controllers)
                {
                    Undo.RecordObject(controller, "Change ColorDataSO");
                    controller.ColorDataSo = colorDataSO;
                    EditorUtility.SetDirty(controller); 
                }

                Debug.Log("Change All Color");
            }
        }
        if (GUILayout.Button("Add Collider"))
        {
            var controllers = parentBlock.GetComponentsInChildren<BlockController>(true);

            foreach (var controller in controllers)
            {
                controller.AddComponent<BoxCollider>();
            }
        }  if (GUILayout.Button("Remove Collider"))
        {
            var controllers = parentBlock.GetComponentsInChildren<BlockController>(true);

            foreach (var controller in controllers)
            {
                DestroyImmediate(controller.GetComponent<BoxCollider>());
            }
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

        void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;

        if (e.type == EventType.MouseMove)
        {
            GameObject obj = HandleUtility.PickGameObject(e.mousePosition, false);

            if (obj != null && obj.GetComponent<BlockController>() != null) 
            {
                if (obj != hoveredObject)
                {
                    hoveredObject = obj;
                    Repaint();
                    Debug.Log(obj.name);

                }
            }
            else
            {
                if (hoveredObject != null)
                {
                    hoveredObject = null;
                    Repaint();
                }
            }
        }

        if (e.type == EventType.MouseDown)
        {
            GameObject obj = HandleUtility.PickGameObject(e.mousePosition, false);
            if (ChangeColor == true)
            {
                if (obj != null && obj.transform.parent != null && obj.transform.parent.parent != null)
                {
                    BlockController controller = obj.transform.parent.parent.GetComponent<BlockController>();
                    if (controller != null)
                    {
                        Undo.RecordObject(controller, "Change ColorDataSO");
                        controller.ColorDataSo = colorDataSO;
                        controller._colorRenderer.material = colorDataSO.BlockMaterial;
                        EditorUtility.SetDirty(controller);
                        hoveredObject = controller.gameObject;
                        Debug.Log("ChangeColor: " + obj.name);
                        Repaint();
                    }
                }
                else
                {
                    hoveredObject = null;
                    Repaint();
                }
            }
        }
    }
    
}
#endif