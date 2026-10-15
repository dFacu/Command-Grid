using UnityEngine;
using System.Collections.Generic;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(DrawPileManager))]

public class DrawPileManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DrawPileManager DrawPileManager = (DrawPileManager)target;
        if (GUILayout.Button("Draw Next Card"))
        {
            HandManager handManager = FindObjectOfType<HandManager>();
            if (handManager != null)
            {
                DrawPileManager.DrawCard(handManager);
            }
        }
    }
}
#endif
