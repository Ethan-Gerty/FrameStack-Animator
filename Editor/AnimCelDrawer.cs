using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AnimCel))]
public class AnimCelDrawer : PropertyDrawer
{
    public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(pos, label, property);

        int frameIndex = GetFrameIndex(property);

        SerializedProperty sprite = property.FindPropertyRelative("sprite");
        SerializedProperty events = property.FindPropertyRelative("events");

        float previewSize = 64f;
        float spacing = 5f;
        float lineHeight = EditorGUIUtility.singleLineHeight;

        // Frame label
        Rect labelRect = new Rect(pos.x, pos.y, pos.width, lineHeight);

        EditorGUI.LabelField(labelRect, $"Cel {frameIndex}", EditorStyles.boldLabel);

        // Everything else starts underneath the label
        float contentY = pos.y + lineHeight + spacing;

        // Sprite preview
        Rect previewRect = new Rect(pos.x, contentY, previewSize, previewSize);

        if (sprite.objectReferenceValue != null)
        {
            Sprite spriteAsset = sprite.objectReferenceValue as Sprite;
            Texture2D texture = AssetPreview.GetAssetPreview(spriteAsset);

            if (texture != null)
                GUI.DrawTexture(previewRect, texture, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUI.HelpBox(previewRect, "No Sprite", MessageType.None);
        }

        // Right-side controls
        Rect contentRect = new Rect(pos.x + previewSize + spacing, contentY, pos.width - previewSize - spacing, pos.height);

        Rect spriteRect = new Rect(contentRect.x, contentRect.y, contentRect.width, lineHeight);

        EditorGUI.PropertyField(spriteRect, sprite);

        Rect eventsRect = new Rect(contentRect.x,contentRect.y + lineHeight + spacing, contentRect.width, EditorGUI.GetPropertyHeight(events));

        EditorGUI.PropertyField(eventsRect, events, true);

        EditorGUI.EndProperty();
    }



    public override float GetPropertyHeight(
    SerializedProperty property,
    GUIContent label)
    {
        SerializedProperty events = property.FindPropertyRelative("events");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 5f;
        float bottomPadding = 10f;

        float eventsHeight = EditorGUI.GetPropertyHeight(events);

        float contentHeight = Mathf.Max(
            64f,
            lineHeight + spacing + eventsHeight
        );

        return lineHeight + spacing + contentHeight + bottomPadding;
    }


    private int GetFrameIndex(SerializedProperty property)
    {
        string path = property.propertyPath;

        int start = path.LastIndexOf('[') + 1;
        int end = path.LastIndexOf(']');

        if (start > 0 && end > start)
        {
            string indexString = path.Substring(start, end - start);

            if (int.TryParse(indexString, out int index))
                return index;
        }

        return -1;
    }
}