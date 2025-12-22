using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FSAnimation", menuName = "FrameStackAnimator/Animation", order = 1)]
public class FSAnimation : ScriptableObject
{
    public bool loop = false;
    public List<Sprite> sprites = new List<Sprite>();

    public FSAnimation transitionInto = null;
    public int transitionStartFrame;
}
