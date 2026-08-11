using System.Linq;
using UnityEditor;
using UnityEngine;

public static class FSAnimationCreator
{
    [MenuItem("Assets/Create/FrameStackAnimator/Animation From Selected Sprites", false, 1)]
    private static void CreateAnimationFromSelectedSprites()
    {
        Sprite[] sprites = Selection.objects.OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogWarning("No sprites selected.");
            return;
        }


        FSAnimation anim = ScriptableObject.CreateInstance<FSAnimation>();

        foreach (Sprite sprite in sprites)
        {
            AnimCel cel = new AnimCel
            {
                sprite = sprite
            };

            anim.cels.Add(cel);
        }

        string path = GetCreationPath();

        path = AssetDatabase.GenerateUniqueAssetPath(path + "/New FSAnimation.asset");


        AssetDatabase.CreateAsset(anim, path);
        AssetDatabase.SaveAssets();

        Selection.activeObject = anim;
        EditorGUIUtility.PingObject(anim);
    }

    [MenuItem("Assets/Create/FrameStackAnimator/Animation From Selected Sprites", true)]
    private static bool ValidateCreateAnimationFromSelectedSprites()
    {
        return Selection.objects.Any(obj => obj is Sprite);
    }

    private static string GetCreationPath()
    {
        string path = "Assets";

        foreach (Object selectedObject in Selection.objects)
        {
            string selectedPath = AssetDatabase.GetAssetPath(selectedObject);

            if (string.IsNullOrEmpty(selectedPath))
                continue;

            if (AssetDatabase.IsValidFolder(selectedPath))
                return selectedPath;

            string directory = System.IO.Path.GetDirectoryName(selectedPath);

            if (!string.IsNullOrEmpty(directory))
                return directory.Replace("\\", "/");
        }

        return path;
    }
}