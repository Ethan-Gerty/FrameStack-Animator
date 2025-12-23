using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FSAnimation", menuName = "FrameStackAnimator/Animation", order = 1)]
public class FSAnimation : ScriptableObject
{
    public bool loop = false;
    public List<AnimCel> cels = new List<AnimCel>();

    public int overrideFps = -1;

    public FSAnimation transitionInto = null;
    public int transitionStartFrame = 0;

    [Header("For UI")]
    public bool ignoreTimeScale = false;
}

[System.Serializable]
public class AnimCel
{
    public Sprite sprite;
    public List<string> events = new List<string>();
}