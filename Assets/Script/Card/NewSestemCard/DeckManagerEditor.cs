using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(DeckManager))]
public class DeckManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DeckManager deckManager = (DeckManager)target;
        if (GUILayout.Button("Draw Nex Card"))
        {
            HandManager handManager = FindObjectOfType<HandManager>();
            if (handManager != null)
                deckManager.DrawCard(handManager);
        }
    }
}
#endif