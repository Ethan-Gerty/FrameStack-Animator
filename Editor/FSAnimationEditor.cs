using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FSAnimation))]
public class FSAnimationEditor : Editor
{
    private SerializedProperty cels;

    // Preview
    private bool previewPlaying = false;
    private int previewFrame = 0;
    private double lastPreviewTime;




    // OnEnable
    private void OnEnable()
    {
        if (target == null)
            return;

        cels = serializedObject.FindProperty("cels");

        lastPreviewTime = EditorApplication.timeSinceStartup;
        EditorApplication.update += PreviewUpdate;
    }

    // OnDisable
    private void OnDisable()
    {
        EditorApplication.update -= PreviewUpdate;
    }





    // Draws the Inspector GUI
    public override void OnInspectorGUI()
    {
        if (target == null)
            return;

        serializedObject.Update();

        DrawDefaultInspector();

        DrawSpriteDropArea();

        serializedObject.ApplyModifiedProperties();
    }





    // Drop Area Creation
    private void DrawSpriteDropArea()
    {
        Rect dropArea = GUILayoutUtility.GetRect(0f, 60f,GUILayout.ExpandWidth(true));

        GUI.Box(dropArea, "Drag Sprites Here To Add Cels");

        Event currentEvent = Event.current;

        if (!dropArea.Contains(currentEvent.mousePosition)) return;

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
                if (draggedObject is not Sprite sprite)
                    continue;

                int index = cels.arraySize;

                cels.arraySize++;

                SerializedProperty cel = cels.GetArrayElementAtIndex(index);

                cel.FindPropertyRelative("sprite").objectReferenceValue = sprite;

                cel.FindPropertyRelative("events").arraySize = 0;
            }

            currentEvent.Use();
        }
    }




    // Animation Preview
    public override bool HasPreviewGUI() { return (FSAnimation)target != null; }
    public override GUIContent GetPreviewTitle() { return new GUIContent("FS Animation Preview"); }

    public override void OnPreviewGUI(Rect rect, GUIStyle background)
    {
        FSAnimation animation = (FSAnimation)target;
        if (animation == null || animation.cels == null || animation.cels.Count == 0)
        {
            EditorGUI.LabelField(rect, "No cels to preview.", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        previewFrame = Mathf.Clamp(previewFrame, 0, animation.cels.Count - 1);

        Sprite sprite = animation.cels[previewFrame].sprite;
        if (sprite == null)
        {
            EditorGUI.LabelField(rect, $"Cel {previewFrame} contains no sprite.", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        Texture2D previewTexture = AssetPreview.GetAssetPreview(sprite);
        if (previewTexture != null)
        {
            GUI.DrawTexture(rect, previewTexture, ScaleMode.ScaleToFit);
        } else
        {
            Repaint();
        }
    }

    public override void OnPreviewSettings()
    {
        FSAnimation animation = (FSAnimation)target;
        if (animation == null || animation.cels == null || animation.cels.Count == 0) return;

        // Frame Step Back
        if (GUILayout.Button("<", EditorStyles.miniButton))
        {
            previewPlaying = false;

            previewFrame--;

            if (previewFrame < 0) previewFrame = animation.cels.Count - 1;

            Repaint();
        }

        // Play / Pause
        if (GUILayout.Button(previewPlaying ? "⏸" : "▶", EditorStyles.miniButton))
        {
            previewPlaying = !previewPlaying;
            lastPreviewTime = EditorApplication.timeSinceStartup;

            Repaint();
        }

        // Next Frame
        if (GUILayout.Button(">", EditorStyles.miniButton))
        {
            previewPlaying = false;

            previewFrame++;

            if (previewFrame >= animation.cels.Count)
                previewFrame = 0;

            Repaint();
        }

        GUILayout.Label($"{previewFrame} / {animation.cels.Count - 1}",EditorStyles.miniLabel);
    }



    // Preview Playback
    private void PreviewUpdate()
    {
        if (!previewPlaying) return;

        FSAnimation animation = (FSAnimation)target;

        if (animation == null || animation.cels == null || animation.cels.Count == 0)
        {
            previewPlaying = false;
            return;
        }

        int previewFps = animation.overrideFps > 0 ? animation.overrideFps : 24;

        // An override FPS of 0 means don't advance.
        if (animation.overrideFps == 0)
            return;

        double frameDuration = 1.0 / previewFps;
        double currentTime = EditorApplication.timeSinceStartup;

        if (currentTime - lastPreviewTime < frameDuration)
            return;

        lastPreviewTime += frameDuration;

        previewFrame++;

        if (previewFrame >= animation.cels.Count)
        {
            if (animation.loop)
            {
                previewFrame = 0;
            }
            else
            {
                previewFrame = animation.cels.Count - 1;
                previewPlaying = false;
            }
        }

        Repaint();
    }
}