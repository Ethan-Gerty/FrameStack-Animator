using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FSAnimation", menuName = "FrameStackAnimator/Animation", order = 1)]
public class FSAnimation : ScriptableObject
{
    [Header("Animation Variables")]
    public bool loop = false;

    public int overrideFps = -1;

    public FSAnimation transitionInto = null;
    public int transitionStartFrame = 0;

    [Header("For UI")]
    public bool ignoreTimeScale = false;

    [Header("Frames")]
    public List<AnimCel> cels = new List<AnimCel>();
}

[System.Serializable]
public class AnimCel
{
    public Sprite sprite;
    public List<string> events = new List<string>();
}