using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;

[CustomEditor(typeof(StoryContextManager))]
public class StoryContextManagerEditor : Editor
{
    private StoryContextManager storyContext;
    private bool[] foldouts = new bool[8]; // For different sections
    
    // Temporary variables for adding new items
    private string newClue = "";
    private string newSuspect = "";
    private string newNPCName = "";
    private string newNPCSecret = "";
    private string newKeyEvent = "";
    private string newPlayerChoice = "";
    
    // Search/filter variables
    private string clueFilter = "";
    private string suspectFilter = "";
    private string eventFilter = "";
    
    private Vector2 scrollPosition;

    private void OnEnable()
    {
        storyContext = (StoryContextManager)target;
        
        // Initialize foldouts
        for (int i = 0; i < foldouts.Length; i++)
        {
            foldouts[i] = true;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.Space(10);
        
        // Header
        EditorGUILayout.BeginVertical("box");
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 16;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Story Context Manager", headerStyle);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Current Scene Information
        DrawCurrentSceneSection();
        
        // Story Progress
        DrawStoryProgressSection();
        
        // Discovered Clues
        DrawDiscoveredCluesSection();
        
        // Known Suspects
        DrawKnownSuspectsSection();
        
        // NPC Secrets
        DrawNPCSecretsSection();
        
        // Important Events
        DrawImportantEventsSection();
        
        // Player Choices History
        DrawPlayerChoicesSection();
        
        // Context Preview
        DrawContextPreviewSection();
        
        // Utility Buttons
        DrawUtilitySection();
        
        EditorGUILayout.EndScrollView();
        
        serializedObject.ApplyModifiedProperties();
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(storyContext);
        }
    }

    private void DrawCurrentSceneSection()
    {
        foldouts[0] = EditorGUILayout.Foldout(foldouts[0], "Current Scene Information", true);
        if (foldouts[0])
        {
            EditorGUILayout.BeginVertical("box");
            
            storyContext.currentLocation = EditorGUILayout.TextField("Current Location", storyContext.currentLocation);
            storyContext.currentTime = EditorGUILayout.TextField("Current Time", storyContext.currentTime);
            
            EditorGUILayout.LabelField("Current Objective");
            storyContext.currentObjective = EditorGUILayout.TextArea(storyContext.currentObjective, GUILayout.Height(60));
            
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.Space(5);
    }

    private void DrawStoryProgressSection()
    {
        foldouts[1] = EditorGUILayout.Foldout(foldouts[1], "Story Progress", true);
        if (foldouts[1])
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            storyContext.storyProgressPercentage = EditorGUILayout.IntSlider("Progress", storyContext.storyProgressPercentage, 0, 100);
            EditorGUILayout.LabelField($"{storyContext.storyProgressPercentage}%", GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
            
            // Progress bar
            Rect progressRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(progressRect, storyContext.storyProgressPercentage / 100f, "Story Progress");
            
            storyContext.currentChapter = EditorGUILayout.TextField("Current Chapter", storyContext.currentChapter);
            
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.Space(5);
    }

    private void DrawDiscoveredCluesSection()
    {
        foldouts[2] = EditorGUILayout.Foldout(foldouts[2], $"Discovered Clues ({storyContext.discoveredClues.Count})", true);
        if (foldouts[2])
        {
            EditorGUILayout.BeginVertical("box");
            
            // Filter
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter:", GUILayout.Width(40));
            clueFilter = EditorGUILayout.TextField(clueFilter);
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
            {
                clueFilter = "";
            }
            EditorGUILayout.EndHorizontal();
            
            // Add new clue
            EditorGUILayout.BeginHorizontal();
            newClue = EditorGUILayout.TextField("New Clue:", newClue);
            if (GUILayout.Button("Add", GUILayout.Width(50)) && !string.IsNullOrEmpty(newClue))
            {
                storyContext.AddClue(newClue);
                newClue = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Display clues
            for (int i = storyContext.discoveredClues.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(clueFilter) || storyContext.discoveredClues[i].ToLower().Contains(clueFilter.ToLower()))
                {
                    EditorGUILayout.BeginHorizontal();
                    storyContext.discoveredClues[i] = EditorGUILayout.TextField(storyContext.discoveredClues[i]);
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        storyContext.discoveredClues.RemoveAt(i);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.Space(5);
    }

    private void DrawKnownSuspectsSection()
    {
        foldouts[3] = EditorGUILayout.Foldout(foldouts[3], $"Known Suspects ({storyContext.knownSuspects.Count})", true);
        if (foldouts[3])
        {
            EditorGUILayout.BeginVertical("box");
            
            // Filter
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter:", GUILayout.Width(40));
            suspectFilter = EditorGUILayout.TextField(suspectFilter);
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
            {
                suspectFilter = "";
            }
            EditorGUILayout.EndHorizontal();
            
            // Add new suspect
            EditorGUILayout.BeginHorizontal();
            newSuspect = EditorGUILayout.TextField("New Suspect:", newSuspect);
            if (GUILayout.Button("Add", GUILayout.Width(50)) && !string.IsNullOrEmpty(newSuspect))
            {
                storyContext.AddSuspect(newSuspect);
                newSuspect = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Display suspects
            for (int i = storyContext.knownSuspects.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(suspectFilter) || storyContext.knownSuspects[i].ToLower().Contains(suspectFilter.ToLower()))
                {
                    EditorGUILayout.BeginHorizontal();
                    storyContext.knownSuspects[i] = EditorGUILayout.TextField(storyContext.knownSuspects[i]);
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        storyContext.knownSuspects.RemoveAt(i);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.Space(5);
    }

    private void DrawNPCSecretsSection()
    {
        var npcNamesProperty = serializedObject.FindProperty("npcNames");
        var npcSecretsProperty = serializedObject.FindProperty("npcSecrets");
        
        foldouts[4] = EditorGUILayout.Foldout(foldouts[4], $"NPC Secrets ({npcNamesProperty.arraySize})", true);
        if (foldouts[4])
        {
            EditorGUILayout.BeginVertical("box");
            
            // Add new NPC secret
            EditorGUILayout.BeginHorizontal();
            newNPCName = EditorGUILayout.TextField("NPC Name:", newNPCName);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            newNPCSecret = EditorGUILayout.TextField("Secret:", newNPCSecret);
            if (GUILayout.Button("Add", GUILayout.Width(50)) && !string.IsNullOrEmpty(newNPCName) && !string.IsNullOrEmpty(newNPCSecret))
            {
                storyContext.RevealNPCInfo(newNPCName, newNPCSecret);
                newNPCName = "";
                newNPCSecret = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Display NPC secrets
            for (int i = npcNamesProperty.arraySize - 1; i >= 0; i--)
            {
                if (i < npcSecretsProperty.arraySize)
                {
                    EditorGUILayout.BeginVertical("helpbox");
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("NPC:", GUILayout.Width(35));
                    npcNamesProperty.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(npcNamesProperty.GetArrayElementAtIndex(i).stringValue);
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        npcNamesProperty.DeleteArrayElementAtIndex(i);
                        if (i < npcSecretsProperty.arraySize)
                        {
                            npcSecretsProperty.DeleteArrayElementAtIndex(i);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.LabelField("Secret:");
                    npcSecretsProperty.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextArea(npcSecretsProperty.GetArrayElementAtIndex(i).stringValue, GUILayout.Height(40));
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.Space(5);
    }

    private void DrawImportantEventsSection()
    {
        foldouts[5] = EditorGUILayout.Foldout(foldouts[5], $"Important Events ({storyContext.keyEvents.Count})", true);
        if (foldouts[5])
        {
            EditorGUILayout.BeginVertical("box");
            
            // Filter
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter:", GUILayout.Width(40));
            eventFilter = EditorGUILayout.TextField(eventFilter);
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
            {
                eventFilter = "";
            }
            EditorGUILayout.EndHorizontal();
            
            // Add new event
            EditorGUILayout.BeginHorizontal();
            newKeyEvent = EditorGUILayout.TextField("New Event:", newKeyEvent);
            if (GUILayout.Button("Add", GUILayout.Width(50)) && !string.IsNullOrEmpty(newKeyEvent))
            {
                storyContext.AddKeyEvent(newKeyEvent);
                newKeyEvent = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Display events (most recent first)
            for (int i = storyContext.keyEvents.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(eventFilter) || storyContext.keyEvents[i].ToLower().Contains(eventFilter.ToLower()))
                {
                    EditorGUILayout.BeginHorizontal();
                    storyContext.keyEvents[i] = EditorGUILayout.TextField(storyContext.keyEvents[i]);
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        storyContext.keyEvents.RemoveAt(i);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.Space(5);
    }

    private void DrawPlayerChoicesSection()
    {
        foldouts[6] = EditorGUILayout.Foldout(foldouts[6], $"Player Choices History ({storyContext.playerChoices.Count})", true);
        if (foldouts[6])
        {
            EditorGUILayout.BeginVertical("box");
            
            // Add new choice (for testing)
            EditorGUILayout.BeginHorizontal();
            newPlayerChoice = EditorGUILayout.TextField("Test Choice:", newPlayerChoice);
            if (GUILayout.Button("Add", GUILayout.Width(50)) && !string.IsNullOrEmpty(newPlayerChoice))
            {
                storyContext.RecordPlayerChoice(newPlayerChoice);
                newPlayerChoice = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Clear All Choices"))
            {
                storyContext.playerChoices.Clear();
            }
            
            EditorGUILayout.Space(5);
            
            // Display recent choices (most recent first)
            for (int i = storyContext.playerChoices.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(storyContext.playerChoices[i]);
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    storyContext.playerChoices.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.Space(5);
    }

    private void DrawContextPreviewSection()
    {
        foldouts[7] = EditorGUILayout.Foldout(foldouts[7], "Context Preview", true);
        if (foldouts[7])
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Full Context"))
            {
                string context = storyContext.GetContextString();
                EditorGUIUtility.systemCopyBuffer = context;
                Debug.Log("Full Context (copied to clipboard):\n" + context);
            }
            
            if (GUILayout.Button("Condensed Context"))
            {
                string context = storyContext.GetCondensedContextString();
                EditorGUIUtility.systemCopyBuffer = context;
                Debug.Log("Condensed Context (copied to clipboard):\n" + context);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Show condensed preview
            EditorGUILayout.LabelField("Condensed Preview:", EditorStyles.boldLabel);
            string preview = storyContext.GetCondensedContextString();
            EditorGUILayout.TextArea(preview, EditorStyles.helpBox, GUILayout.Height(80));
            
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.Space(5);
    }

    private void DrawUtilitySection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Reset All Data"))
        {
            if (EditorUtility.DisplayDialog("Reset Story Context", 
                "Are you sure you want to reset all story data? This cannot be undone.", 
                "Yes, Reset", "Cancel"))
            {
                storyContext.ResetStoryContext();
            }
        }
        
        if (GUILayout.Button("Save Asset"))
        {
            EditorUtility.SetDirty(storyContext);
            AssetDatabase.SaveAssets();
            Debug.Log("Story Context saved successfully!");
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Statistics
        EditorGUILayout.BeginVertical("helpbox");
        EditorGUILayout.LabelField("Statistics:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Total Clues: {storyContext.discoveredClues.Count}");
        EditorGUILayout.LabelField($"Total Suspects: {storyContext.knownSuspects.Count}");
        EditorGUILayout.LabelField($"NPC Secrets: {storyContext.npcRevealedInfo.Count}");
        EditorGUILayout.LabelField($"Key Events: {storyContext.keyEvents.Count}");
        EditorGUILayout.LabelField($"Player Choices: {storyContext.playerChoices.Count}");
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndVertical();
    }
}