using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FSAnimation))]
public class FSAnimationEditor : Editor
{
    private SerializedProperty cels;

    private void OnEnable()
    {
        cels = serializedObject.FindProperty("cels");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        GUILayout.Space(10);

        Rect dropArea = GUILayoutUtility.GetRect(0f, 60f, GUILayout.ExpandWidth(true));

        GUI.Box(dropArea, "Drag Sprites Here To Add Cels");



        Event currentEvent = Event.current;

        if (dropArea.Contains(currentEvent.mousePosition))
        {
            if (currentEvent.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                currentEvent.Use();
            }

            if (currentEvent.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                foreach (Object draggedObject in DragAndDrop.objectReferences)
                {
                    if (draggedObject is Sprite sprite)
                    {
                        int index = cels.arraySize;
                        cels.arraySize++;

                        SerializedProperty cel =
                            cels.GetArrayElementAtIndex(index);

                        cel.FindPropertyRelative("sprite").objectReferenceValue = sprite;

                        SerializedProperty events =
                            cel.FindPropertyRelative("events");

                        events.arraySize = 0;
                    }
                }

                currentEvent.Use();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}