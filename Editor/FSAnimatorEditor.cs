using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FSAnimator))]
public class FSAnimatorEditor : Editor
{
    private bool showRuntimeInfo = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FSAnimator animator = (FSAnimator)target;

        if (!Application.isPlaying)
            return;

        EditorGUILayout.Space(10);

        showRuntimeInfo = EditorGUILayout.Foldout(showRuntimeInfo, "Runtime Info / Debug Controls", true, EditorStyles.foldoutHeader);

        if (!showRuntimeInfo)
            return;

        using (new EditorGUI.DisabledScope(!animator.isPaused))
        {
            if (GUILayout.Button("Step Animation"))
            {
                animator.DebugStep();
            }
        }

        bool newPaused = EditorGUILayout.Toggle("Paused", animator.isPaused);

        if (newPaused != animator.isPaused)
        {
            if (newPaused)
                animator.Pause();
            else
                animator.UnPause();
        }

        int frameCount = animator.currentAnimation != null ? animator.currentAnimation.cels.Count : 0;
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.Toggle("Finished", animator.isFinished);
            EditorGUILayout.ObjectField("Current Animation", animator.currentAnimation, typeof(FSAnimator), false);
            EditorGUILayout.LabelField("Current Frame", $"{animator.currentFrame} / {Mathf.Max(0, frameCount - 1)}");
            EditorGUILayout.ObjectField("Pending Animation", animator.pendingAnim, typeof(FSAnimation), false);
            EditorGUILayout.ObjectField("Queued Animation", animator.queuedAnim, typeof(FSAnimation), false);
        }   
    }




}